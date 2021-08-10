using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

public class ConfigurationPhase : Phase
{
	public const string COMPILER_VERSION = "1.0";

	public const string ARGUMENTS = "arguments";
	public const string FILES = "filenames";
	public const string OBJECTS = "objects";
	public const string LIBRARIES = "libraries";

	public const string OUTPUT_NAME = "output_name";
	public const string OUTPUT_TYPE = "output_type";

	public const string OUTPUT_TIME = "time";

	public const string REBUILD_FLAG = "rebuild";

	public const string SERVICE_FLAG = "service";

	public const string VIVID_EXTENSION = ".v";
	public const string DEFAULT_OUTPUT = "v";

	private List<string> Folders { get; set; } = new List<string>();
	private List<string> Libraries { get; set; } = new List<string>();
	private List<string> Files { get; set; } = new List<string>();
	private List<string> Objects { get; set; } = new List<string>();

	private static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

	private bool IsOptimizationEnabled { get; set; } = false;

	private void Initialize()
	{
		Folders.Clear();
		Folders.Add(Environment.CurrentDirectory.Replace('\\', '/') + '/');

		if (IsLinux)
		{
			// Get all folders registered to the environment variable 'PATH'
			var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
			Folders.AddRange(path.Split(':').Where(i => !string.IsNullOrEmpty(i)).Select(i => i.Replace('\\', '/')));
		}
		else
		{
			// Get all folders registered to the environment variable 'Path'
			var path = Environment.GetEnvironmentVariable("Path") ?? string.Empty;
			Folders.AddRange(path.Split(';').Where(i => !string.IsNullOrEmpty(i)).Select(i => i.Replace('\\', '/')));
		}

		for (var i = 0; i < Folders.Count; i++)
		{
			var folder = Folders[i];

			if (!folder.EndsWith('/'))
			{
				Folders[i] = folder + '/';
			}
		}
	}

	private void Collect(DirectoryInfo folder, bool recursive = true)
	{
		foreach (var item in folder.GetFiles())
		{
			if (item.Extension == VIVID_EXTENSION)
			{
				Files.Add(item.FullName);
			}
		}

		if (!recursive)
		{
			return;
		}

		foreach (var item in folder.GetDirectories())
		{
			Collect(item);
		}
	}

	private string? FindLibrary(string library)
	{
		foreach (var folder in Folders)
		{
			var filename = folder + library;

			if (File.Exists(filename)) return filename;

			filename = folder + AssemblerPhase.LIBRARY_PREFIX + library;
			if (File.Exists(filename)) return filename;

			filename = folder + library + AssemblerPhase.StaticLibraryExtension;
			if (File.Exists(filename)) return filename;

			filename = folder + library + AssemblerPhase.SharedLibraryExtension;
			if (File.Exists(filename)) return filename;

			filename = folder + AssemblerPhase.LIBRARY_PREFIX + library + AssemblerPhase.StaticLibraryExtension;
			if (File.Exists(filename)) return filename;
			
			filename = folder + AssemblerPhase.LIBRARY_PREFIX + library + AssemblerPhase.SharedLibraryExtension;
			if (File.Exists(filename)) return filename;
		}

		return null;
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
					new() { Command = "-help",												Description = "Displays this information" },
					new() { Command = "-r <folder> / -recursive <folder>",		Description = "Includes source files (.v) from the specified folder and its subfolders"},
					new() { Command = "-d / -debug",										Description = "Generates the output binary with debug information" },
					new() { Command = "-o <filename> / -output <filename>",		Description = "Sets the output filename (Default: v.exe, v, v.dll, v.so, v.lib, v.a)" },
					new() { Command = "-l <library> / -library <library>",		Description = "Includes a library to the compilation process" },
					new() { Command = "-a / -assembly",									Description = "Exports the generated assembly to a file" },
					new() { Command = "-shared / -dynamic / -dll",					Description = "Sets the output type to shared library (.dll or .so)" },
					new() { Command = "-static",											Description = "Sets the output type to static library (.lib or .a)"},
					new() { Command = "-st / -single-thread",							Description = "Compiles on a single thread instead of multiple threads" },
					new() { Command = "-q / -quiet",										Description = "Suppresses the console output" },
					new() { Command = "-v / -verbose",									Description = "Outputs more information about the compilation" },
					new() { Command = "-f / -force / -rebuild",						Description = "Forces the compiler to compile all the source files again" },
					new() { Command = "-t / -time",										Description = "Displays information about the length of the compilation" },
					new() { Command = "-O, -O1, -O2",									Description = "Optimizes the output" },
					new() { Command = "-x64",												Description = "Compile for architecture x64" },
					new() { Command = "-arm64",											Description = "Compile for architecture Arm64" },
					new() { Command = "-version",											Description = "Outputs the version of the compiler" },
					new() { Command = "-s",													Description = "Creates a compiler service which waits for code analysis input from a local socket" }
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
				if (!parameters.TryDequeue(out var folder) || IsOption(folder))
				{
					return Status.Error("Missing or invalid value for option '{0}'", option);
				}

				if (!Directory.Exists(folder))
				{
					return Status.Error("Could not find folder '{0}'", folder);
				}

				Collect(new DirectoryInfo(folder), true);

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
				if (!parameters.TryDequeue(out var output) || IsOption(output))
				{
					return Status.Error("Missing or invalid value for option '{0}'", option);
				}

				bundle.Put(OUTPUT_NAME, output);
				return Status.OK;
			}

			case "-l":
			case "-library":
			{
				if (!parameters.TryDequeue(out var library) || IsOption(library))
				{
					return Status.Error("Missing or invalid value for option '{0}'", option);
				}

				var filename = FindLibrary(library);

				if (filename == null)
				{
					return Status.Error($"Can not find the specified library '{library}'. If the library name is correct, make sure the library is visible to this compiler.");
				}

				Libraries.Add(filename);
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
				bundle.Put(OUTPUT_TYPE, BinaryType.SHARED_LIBRARY);
				return Status.OK;
			}

			case "-static":
			{
				bundle.Put(OUTPUT_TYPE, BinaryType.STATIC_LIBRARY);
				return Status.OK;
			}

			case "-st":
			case "-single-thread":
			{
				bundle.PutBool("multithreaded", false);
				return Status.OK;
			}

			case "-t":
			case "-time":
			{
				bundle.PutBool(OUTPUT_TIME, true);
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
				Analysis.IsFunctionInliningEnabled = false;

				return Status.OK;
			}

			case "-O2":
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
				Analysis.IsFunctionInliningEnabled = true;

				return Status.OK;
			}

			case "-x64":
			{
				Assembler.Architecture = Architecture.X64;
				return Status.OK;
			}

			case "-arm64":
			{
				Assembler.Architecture = Architecture.Arm64;
				return Status.OK;
			}

			case "-version":
			{
				Console.WriteLine($"Vivid version {COMPILER_VERSION}");
				Environment.Exit(0);

				return Status.OK;
			}

			case "-s":
			{
				bundle.PutBool(SERVICE_FLAG, true);
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
		Initialize();

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
					if (file.Extension == VIVID_EXTENSION)
					{
						Files.Add(file.FullName);
						continue;
					}
					else if (file.Extension == AssemblerPhase.ObjectFileExtension)
					{
						Objects.Add(file.FullName);
						continue;
					}
					else
					{
						return Status.Error($"Source file must have '{VIVID_EXTENSION}' extension");
					}
				}

				var directory = new DirectoryInfo(element);

				if (directory.Exists)
				{
					Collect(directory, false);
					continue;
				}

				return Status.Error("Invalid source file or folder '{0}'", element);
			}
		}

		Assembler.Target = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OSPlatform.Linux : OSPlatform.Windows;

		bundle.Put(FILES, Files.Distinct().ToArray());
		bundle.Put(OBJECTS, Objects.Distinct().ToArray());
		bundle.Put(LIBRARIES, Libraries.ToArray());

		if (!bundle.Contains(OUTPUT_NAME))
		{
			bundle.Put(OUTPUT_NAME, DEFAULT_OUTPUT);
		}

		if (!Assembler.IsX64 && !Assembler.IsArm64)
		{
			return Status.Error("This compiler only supports architectures x64 and Arm64");
		}

		return Status.OK;
	}
}