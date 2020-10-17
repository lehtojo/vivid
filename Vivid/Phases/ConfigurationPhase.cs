using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;

public class ConfigurationPhase : Phase
{
	public const string EXTENSION = ".v";
	public const string DEFAULT_OUTPUT = "v";

	private List<string> Libraries { get; set; } = new List<string>();
	private List<string> Files { get; set; } = new List<string>();

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
		switch (option)
		{
			case "-help":
			case "--help":
			{
				var options = new Option[]
				{
					new Option() { Command = "-help / --help",                       Description = "Displays this information" },
					new Option() { Command = "-r <folder> / --recursive <folder>",   Description = "Includes source files (.z) from the given folder and its subfolders"},
					new Option() { Command = "-d / --debug",                           Description = "Generates the output binary with debug info" },
					new Option() { Command = "-o <filename> / --output <filename>", Description = "Sets the output filename (Default: z.asm, z.o, z, ...)" },
					new Option() { Command = "-l <library> / --library <library>",   Description = "Includes a library to the compilation process" },
					new Option() { Command = "--asm",                                 Description = "Exports the generated assembly to a file" },
					new Option() { Command = "--shared",                               Description = "Sets the output type to shared library (.dll or .so)" },
					new Option() { Command = "--static",                               Description = "Sets the output type to static library (.lib or .a)"},
					new Option() { Command = "-st / --single-thread",                 Description = "Compiles on a single thread instead of multiple threads" },
					new Option() { Command = "-q / --quiet",                           Description = "Suppresses the console output" },
					new Option() { Command = "-f <bits> / --format <bits>",         Description = "Specifies the bitmode to use" }
				};

				Console.WriteLine
				(
					"Usage: zz [options] <folders / files>" + "\n" +
					"Options:" + "\n" +
					string.Join(null, options)
				);

				Abort();

				return Status.OK;
			}

			case "-r":
			case "--recursive":
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
			case "--debug":
			{
				bundle.PutBool("debug", true);
				return Status.OK;
			}

			case "-o":
			case "--output":
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
			case "--lib":
			case "--library":
			{
				var library = parameters.Dequeue();

				if (library == null || IsOption(library))
				{
					return Status.Error("Missing or invalid value for option '{0}'", option);
				}

				Libraries.Add(library);
				return Status.OK;
			}

			case "--asm":
			{
				bundle.PutBool("assembly", true);
				return Status.OK;
			}

			case "--shared":
			{
				bundle.Put("output_type", BinaryType.SHARED_LIBRARY);
				return Status.OK;
			}

			case "--static":
			{
				bundle.Put("output_type", BinaryType.STATIC_LIBRARY);
				return Status.OK;
			}

			case "-st":
			case "--single-thread":
			{
				bundle.PutBool("multithreaded", false);
				return Status.OK;
			}

			case "--time":
			{
				bundle.PutBool("time", true);
				return Status.OK;
			}

			case "-f":
			case "--format":
			{
				if (int.TryParse(parameters.Dequeue(), out int bits) && (bits == 64 || bits == 32))
				{
					Lexer.Size = Size.FromBytes(bits / 8);
					Parser.Size = Size.FromBytes(bits / 8);
					Assembler.Size = Size.FromBytes(bits / 8);
					return Status.OK;
				}
				else
				{
					return Status.Error("Invalid format argument");
				}
			}

			case "--disable_mathematical_optimization":
			{
				Analysis.IsMathematicalOptimizationEnabled = false;
				return Status.OK;
			}

			default:
			{
				return Status.Error("Unknown option");
			}
		}
	}

	private static bool IsOption(string element)
	{
		return element[0] == '-';
	}

	public override Status Execute(Bundle bundle)
	{
		if (!bundle.Contains("arguments"))
		{
			return Status.Error("Could not configure settings");
		}

		var arguments = bundle.Get<string[]>("arguments");
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

				return Status.Error("Invalid source file/folder '{0}'", element);
			}
		}

		Assembler.Target = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OSPlatform.Linux : OSPlatform.Windows;

		bundle.Put("input_files", Files.ToArray());
		bundle.Put("libraries", Libraries.ToArray());

		if (!bundle.Contains("output"))
		{
			bundle.Put("output", DEFAULT_OUTPUT);
		}

		return Status.OK;
	}
}