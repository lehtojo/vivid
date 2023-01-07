using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

public class ConfigurationPhase : Phase
{
	private List<string> Folders { get; set; } = new List<string>();
	private List<string> Libraries { get; set; } = new List<string>();
	private List<string> Files { get; set; } = new List<string>();
	private List<string> Objects { get; set; } = new List<string>();

	private bool IsOptimizationEnabled { get; set; } = false;

	private void Initialize()
	{
		Folders.Clear();
		Folders.Add(Environment.CurrentDirectory.Replace('\\', '/') + '/');

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
			Folders.AddRange(path.Split(':').Where(i => !string.IsNullOrEmpty(i)).Select(i => i.Replace('\\', '/')));
		}
		else
		{
			var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
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

		Keywords.Initialize();
		Operators.Initialize();
	}

	private void Collect(DirectoryInfo folder, bool recursive = true)
	{
		foreach (var item in folder.GetFiles())
		{
			if (item.Extension == Settings.VIVID_EXTENSION)
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

			filename = folder + AssemblyPhase.LIBRARY_PREFIX + library;
			if (File.Exists(filename)) return filename;

			filename = folder + library + AssemblyPhase.StaticLibraryExtension;
			if (File.Exists(filename)) return filename;

			filename = folder + library + AssemblyPhase.SharedLibraryExtension;
			if (File.Exists(filename)) return filename;

			filename = folder + AssemblyPhase.LIBRARY_PREFIX + library + AssemblyPhase.StaticLibraryExtension;
			if (File.Exists(filename)) return filename;
			
			filename = folder + AssemblyPhase.LIBRARY_PREFIX + library + AssemblyPhase.SharedLibraryExtension;
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

	private Status Configure(string option, Queue<string> parameters)
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
					new() { Command = "-q / -quiet",										Description = "Suppresses the console output" },
					new() { Command = "-v / -verbose",									Description = "Outputs more information about the compilation" },
					new() { Command = "-t / -time",										Description = "Displays information about the length of the compilation" },
					new() { Command = "-x64",												Description = "Compile for architecture x64" },
					new() { Command = "-arm64",											Description = "Compile for architecture arm64" },
					new() { Command = "-version",											Description = "Outputs the version of the compiler" },
					new() { Command = "-s",													Description = "Creates a compiler service which waits for code analysis input from a local socket" },
					new() { Command = "-objects",											Description = "Outputs all compiled source files as object files" },
					new() { Command = "-binary",											Description = "Outputs a raw executable binary file" }
				};

				Console.WriteLine
				(
					"Usage: v [options] <folders / files>" + "\n" +
					"Options:" + "\n" +
					string.Join(null, options)
				);

				Environment.Exit(1);
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

				Settings.IsDebuggingEnabled = true;
				return Status.OK;
			}

			case "-o":
			case "-output":
			{
				if (!parameters.TryDequeue(out var output) || IsOption(output))
				{
					return Status.Error("Missing or invalid value for option '{0}'", option);
				}

				Settings.OutputName = output;
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

			case "-link":
			{
				Settings.LinkObjects = true;
				return Status.OK;
			}

			case "-a":
			case "-assembly":
			{
				Settings.IsAssemblyOutputEnabled = true;
				return Status.OK;
			}

			case "-dynamic":
			case "-shared":
			case "-dll":
			{
				Settings.OutputType = BinaryType.SHARED_LIBRARY;
				return Status.OK;
			}

			case "-static":
			{
				Settings.OutputType = BinaryType.STATIC_LIBRARY;
				return Status.OK;
			}

			case "-t":
			case "-time":
			{
				Settings.Time = true;
				return Status.OK;
			}

			case "-q":
			case "-quiet":
			{
				Settings.IsVerboseOutputEnabled = false;
				return Status.OK;
			}

			case "-v":
			case "-verbose":
			{
				Settings.IsVerboseOutputEnabled = true;
				return Status.OK;
			}

			case "-O":
			case "-O1":
			{
				if (Settings.IsDebuggingEnabled)
				{
					return Status.Error("Optimization and debugging can not be enabled at the same time");
				}

				IsOptimizationEnabled = true;
				Analysis.IsMathematicalAnalysisEnabled = true;

				return Status.OK;
			}

			case "-O2":
			{
				if (Settings.IsDebuggingEnabled)
				{
					return Status.Error("Optimization and debugging can not be enabled at the same time");
				}

				IsOptimizationEnabled = true;
				Analysis.IsMathematicalAnalysisEnabled = true;

				return Status.OK;
			}

			case "-x64":
			{
				Settings.Architecture = Architecture.X64;
				return Status.OK;
			}

			case "-arm64":
			{
				Settings.Architecture = Architecture.Arm64;
				return Status.OK;
			}

			case "-version":
			{
				Console.WriteLine($"Vivid version {Settings.VERSION}");
				Environment.Exit(0);

				return Status.OK;
			}

			case "-s":
			{
				Settings.Service = true;
				return Status.OK;
			}

			case "-objects":
			{
				Settings.OutputType = BinaryType.OBJECTS;
				return Status.OK;
			}

			case "-binary":
			{
				Settings.OutputType = BinaryType.RAW;
				return Status.OK;
			}

			case "-use-legacy-assembly":
			{
				Size.WORD.Allocator = ".short";
				Size.DWORD.Allocator = ".long";
				Size.QWORD.Allocator = ".quad";
				Size.XWORD.Identifier = "xmmword";
				Size.YWORD.Identifier = "ymmword";

				Settings.IsLegacyAssemblyEnabled = true;
				Assembler.SectionDirective = ".section";
				Assembler.SectionRelativeDirective = ".secrel";
				Assembler.ExportDirective = ".global";
				Assembler.TextSectionIdentifier = ".text";
				Assembler.DataSectionIdentifier = ".data";
				Assembler.DebugFileDirective = ".file";
				Assembler.CharactersAllocator = ".ascii";
				Assembler.ByteAlignmentDirective = ".balign";
				Assembler.PowerOfTwoAlignment = ".align";
				Assembler.ZeroAllocator = ".zero";
				Assembler.MemoryAddressExtension = " ptr ";
				Assembler.RelativeSymbolSpecifier = "rip+";
				Assembler.DebugFunctionStartDirective = ".cfi_startproc";
				Assembler.DebugFrameOffsetDirective = ".cfi_def_cfa_offset";
				Assembler.DebugFunctionEndDirective = ".cfi_endproc";

				Debug.DebugAbbreviationTable = ".debug_abbrev";
				Debug.DebugInformationTable = ".debug_info";
				Debug.DebugLineTable = ".debug_line";
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

	public override Status Execute()
	{
		Initialize();

		var arguments = Settings.Arguments;
		var parameters = new Queue<string>(arguments);

		while (parameters.Count > 0)
		{
			var element = parameters.Dequeue();

			if (IsOption(element))
			{
				var status = Configure(element, parameters);

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
					if (file.Extension == Settings.VIVID_EXTENSION)
					{
						Files.Add(file.FullName);
						continue;
					}
					else if (file.Extension == Settings.ASSEMBLY_EXTENSION)
					{
						Files.Add(file.FullName);
						Settings.TextualAssembly = true;
						continue;
					}
					else if (file.Extension == AssemblyPhase.ObjectFileExtension)
					{
						Objects.Add(file.FullName);
						continue;
					}
					else
					{
						return Status.Error($"Source file must have '{Settings.VIVID_EXTENSION}' extension");
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

		Settings.Target = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OSPlatform.Linux : OSPlatform.Windows;

		Settings.Filenames = Files.Distinct().ToList();
		Settings.UserImportedObjectFiles = Objects.Distinct().ToList();
		Settings.Libraries = Libraries;

		if (!Settings.IsX64 && !Settings.IsArm64)
		{
			return Status.Error("This compiler only supports architectures x64 and arm64");
		}

		return Status.OK;
	}
}
