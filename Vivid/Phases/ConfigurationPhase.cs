using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

public class ConfigurationPhase : Phase
{
	public const string ARGUMENTS = "arguments";
	public const string FILES = "filenames";
	public const string LIBRARIES = "libraries";

	public const string OUTPUT_NAME = "output";
	public const string REBUILD_FLAG = "rebuild";

	public const string EXTENSION = ".v";
	public const string DEFAULT_OUTPUT = "v";

	private List<string> Libraries { get; set; } = new List<string>();
	private List<string> Files { get; set; } = new List<string>();

	private bool IsOptimizationEnabled { get; set; } = false;

	private void Collect(Bundle bundle, DirectoryInfo folder, bool recursive = true)
	{
		foreach (var item in folder.GetFiles())
		{
			if (item.Extension == EXTENSION)
			{
				Files.Add(item.FullName);
			}
		}

		if (recursive)
		{
			foreach (var item in folder.GetDirectories())
			{
				Collect(bundle, item);
			}
		}
	}

	private struct Option
	{
		public string Command { get; set; }
		public string Description { get; set; }

		public override string ToString()
		{
			return $"\t{Command,-40}{Description}\n";
		}
	};

	private Status Configure(Bundle bundle, string option, Queue<string> parameters)
	{
		// Replace sequential option characters with one
		option = Regex.Replace(option, "-{2,}", "-");

		switch (option)
		{
			case "-help":
			{
				var options = new Option[]
				{
					new Option() { Command = "-help",											Description = "Displays this information" },
					new Option() { Command = "-r <folder> / -recursive <folder>",		Description = "Includes source files (.v) from the specified folder and its subfolders"},
					new Option() { Command = "-d / -debug",									Description = "Generates the output binary with debug information" },
					new Option() { Command = "-o <filename> / -output <filename>",		Description = "Sets the output filename (Default: v.asm, v.o, v, ...)" },
					new Option() { Command = "-l <library> / -library <library>",		Description = "Includes a library to the compilation process" },
					new Option() { Command = "-a / -assembly",								Description = "Exports the generated assembly to a file" },
					new Option() { Command = "-shared / -dynamic / -dll",					Description = "Sets the output type to shared library (.dll or .so)" },
					new Option() { Command = "-static",											Description = "Sets the output type to static library (.lib or .a)"},
					new Option() { Command = "-st / -single-thread",						Description = "Compiles on a single thread instead of multiple threads" },
					new Option() { Command = "-q / -quiet",									Description = "Suppresses the console output" },
					new Option() { Command = "-v / -verbose",									Description = "Outputs more information about the compilation" },
					new Option() { Command = "-f / -force / -rebuild",						Description = "Forces the compiler to compile all the source files again" },
					new Option() { Command = "-time",											Description = "Displays information about the length of the compilation" },
					new Option() { Command = "-O, -O1, -optimize",							Description = "Optimizes the output" }
				};

				Console.WriteLine
				(
					"Usage: v [options] <folders / files>" + "\n" +
					"Options:" + "\n" +
					string.Join(null, options)
				);

				Abort();

				return Status.OK;
			}

			case "-r":
			case "-recursive":
			{
				var folder = parameters.Dequeue();

				if (folder == null || IsOption(folder))
				{
					return Status.Error("Missing or invalid value for option '{0}'", option);
				}

				if (!Directory.Exists(folder))
				{
					return Status.Error("Could not find folder '{0}'", folder);
				}

				Collect(bundle, new DirectoryInfo(folder), true);

				return Status.OK;
			}

			case "-d":
			case "-debug":
			{
				if (IsOptimizationEnabled)
				{
					return Status.Error("Optimization and debugging can not be enabled at the same time");
				}

				Assembler.IsDebuggingEnabled = true;
				bundle.PutBool("debug", true);
				return Status.OK;
			}

			case "-o":
			case "-output":
			{
				var output = parameters.Dequeue();

				if (output == null || IsOption(output))
				{
					return Status.Error("Missing or invalid value for option '{0}'", option);
				}

				bundle.Put("output", output);
				return Status.OK;
			}

			case "-l":
			case "-library":
			{
				var library = parameters.Dequeue();

				if (library == null || IsOption(library))
				{
					return Status.Error("Missing or invalid value for option '{0}'", option);
				}

				Libraries.Add(library);
				return Status.OK;
			}

			case "-a":
			case "-assembly":
			{
				bundle.PutBool("assembly", true);
				return Status.OK;
			}

			case "-dynamic":
			case "-shared":
			case "-dll":
			{
				bundle.Put("output_type", BinaryType.SHARED_LIBRARY);
				return Status.OK;
			}

			case "-static":
			{
				bundle.Put("output_type", BinaryType.STATIC_LIBRARY);
				return Status.OK;
			}

			case "-st":
			case "-single-thread":
			{
				bundle.PutBool("multithreaded", false);
				return Status.OK;
			}

			case "-time":
			{
				bundle.PutBool("time", true);
				return Status.OK;
			}

			case "-q":
			case "-quiet":
			{
				Assembler.IsVerboseOutputEnabled = false;
				return Status.OK;
			}

			case "-v":
			case "-verbose":
			{
				Assembler.IsVerboseOutputEnabled = true;
				return Status.OK;
			}

			case "-f":
			case "-force":
			case "-rebuild":
			{
				bundle.PutBool(REBUILD_FLAG, true);
				return Status.OK;
			}

			case "-O":
			case "-O1":
			case "-optimize":
			{
				if (Assembler.IsDebuggingEnabled)
				{
					return Status.Error("Optimization and debugging can not be enabled at the same time");
				}

				IsOptimizationEnabled = true;

				Analysis.IsInstructionAnalysisEnabled = true;
				Analysis.IsMathematicalAnalysisEnabled = true;
				Analysis.IsRepetitionAnalysisEnabled = true;
				Analysis.IsUnwrapAnalysisEnabled = true;

				return Status.OK;
			}

			default:
			{
				return Status.Error($"Unknown option '{option}'");
			}
		}
	}

	private static bool IsOption(string element)
	{
		return element[0] == '-';
	}

	public override Status Execute(Bundle bundle)
	{
		if (!bundle.Contains(ARGUMENTS))
		{
			return Status.Error("Could not configure settings");
		}

		var arguments = bundle.Get<string[]>(ARGUMENTS);
		var parameters = new Queue<string>(arguments);

		while (parameters.Count > 0)
		{
			var element = parameters.Dequeue();

			if (IsOption(element))
			{
				var status = Configure(bundle, element, parameters);

				if (status.IsProblematic)
				{
					return status;
				}
			}
			else
			{
				var file = new FileInfo(element);

				if (file.Exists)
				{
					if (file.Extension == EXTENSION)
					{
						Files.Add(file.FullName);
						continue;
					}
					else
					{
						return Status.Error($"Source file must have '{EXTENSION}' extension");
					}
				}

				var directory = new DirectoryInfo(element);

				if (directory.Exists)
				{
					Collect(bundle, directory, false);
					continue;
				}

				return Status.Error("Invalid source file or folder '{0}'", element);
			}
		}

		Assembler.Target = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OSPlatform.Linux : OSPlatform.Windows;

		bundle.Put(FILES, Files.Distinct().ToArray());
		bundle.Put(LIBRARIES, Libraries.ToArray());

		if (!bundle.Contains("output"))
		{
			bundle.Put("output", DEFAULT_OUTPUT);
		}

		return Status.OK;
	}
}