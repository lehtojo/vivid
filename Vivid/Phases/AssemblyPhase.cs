using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;

public class AssemblyPhase : Phase
{
	private const int EXIT_CODE_OK = 0;

	private const string X64_ASSEMBLER = "x64-as";
	private const string ARM64_ASSEMBLER = "arm64-as";

	private const string X64_LINKER = "x64-ld";
	private const string ARM64_LINKER = "arm64-ld";

	private const string ASSEMBLY_OUTPUT_EXTENSION = ".asm";

	private const string SHARED_LIBRARY_FLAG = "--shared";
	private const string STATIC_LIBRARY_FLAG = "--static";

	private const string STANDARD_LIBRARY = "libv";

	private const string ERROR = "Internal assembler failed";

	public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
	public static string ObjectFileExtension => IsLinux ? ".o" : ".obj";
	public static string SharedLibraryExtension => IsLinux ? ".so" : ".dll";
	public static string StaticLibraryExtension => IsLinux ? ".a" : ".lib";
	public static string ExecutableExtension => IsLinux ? string.Empty : ".exe";

	public static string StandardLibrary => STANDARD_LIBRARY + '_' + Enum.GetName(typeof(Architecture), Settings.Architecture)!.ToLowerInvariant() + ObjectFileExtension;
	public static string ImportedStandardLibraryObjectFile => "v." + StandardLibrary;

	private const string RED = "\x1B[1;31m";
	private const string GREEN = "\x1B[1;32m";
	private const string CYAN = "\x1B[1;36m";
	private const string RESET = "\x1B[0m";

	public const string LIBRARY_PREFIX = "lib";

	/// <summary>
	/// Returns the executable name of the assembler based on the current settings
	/// </summary>
	private static string GetAssembler()
	{
		return Settings.IsArm64 ? ARM64_ASSEMBLER : X64_ASSEMBLER;
	}

	/// <summary>
	/// Returns the executable name of the linker based on the current settings
	/// </summary>
	private static string GetLinker()
	{
		return Settings.IsArm64 ? ARM64_LINKER : X64_LINKER;
	}

	/// <summary>
	/// Returns whether the specified program is installed
	/// </summary>
	private static bool Linux_IsInstalled(string program)
	{
		// Execute the 'which' command to check whether the specified program exists
		var process = Process.Start("which", program);
		process.WaitForExit();

		// Which-command exits with code 0 when the specified program exists
		return process.ExitCode == 0;
	}

	/// <summary>
	/// Runs the specified executable with the given arguments
	/// </summary>
	private static Status Run(string executable, List<string> arguments)
	{
		var configuration = new ProcessStartInfo()
		{
			FileName = executable,
			Arguments = string.Join(' ', arguments),
			WorkingDirectory = Environment.CurrentDirectory,
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};

		try
		{
			var process = Process.Start(configuration) ?? throw new ApplicationException($"Could not start process '{executable}'");
			process.WaitForExit();

			var output = string.Empty;

			var standard_output = process.StandardOutput.ReadToEnd();
			var standard_error = process.StandardError.ReadToEnd();

			if (string.IsNullOrEmpty(standard_output) || string.IsNullOrEmpty(standard_error))
			{
				output = standard_output + standard_error;
			}
			else
			{
				output = $"Output:\n{standard_output}\n\n\nError(s):\n{standard_error}";
			}

			return process.ExitCode == EXIT_CODE_OK ? Status.OK : Status.Error(ERROR + "\n" + output);
		}
		catch
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || !Linux_IsInstalled(executable))
			{
				return Status.Error($"Is the application '{executable}' installed and visible to this application?");
			}

			return Status.Error(ERROR);
		}
	}

	/// <summary>
	/// Compiles the specified input file and exports the result with the specified output filename
	/// </summary>
	private static Status Compile(string input_file, string output_file)
	{
		var arguments = new List<string>();

		// Add output file, input file and enable debug information using the arguments
		arguments.AddRange(new string[]
		{
			$"-o {output_file}",
			input_file,
			"--gdwarf2"
		});

		var status = Run(GetAssembler(), arguments);

		if (!Settings.IsAssemblyOutputEnabled)
		{
			try
			{
				File.Delete(input_file);
			}
			catch
			{
				Console.WriteLine("Warning: Could not remove generated assembly file");
			}
		}

		return status;
	}

	/// <summary>
	/// Links the specified input file with necessary system files and produces an executable with the specified output filename
	/// </summary>
	private static Status Windows_Link(IEnumerable<string> input_files, string output_name)
	{
		var output_type = Settings.OutputType;
		var output_extension = output_type switch
		{
			BinaryType.SHARED_LIBRARY => SharedLibraryExtension,
			BinaryType.STATIC_LIBRARY => StaticLibraryExtension,
			_ => ".exe"
		};

		// Provide all folders in PATH to linker as library paths
		var path = Environment.GetEnvironmentVariable("Path") ?? string.Empty;
		var library_paths = path.Split(';').Where(i => !string.IsNullOrEmpty(i)).Select(p => $"-L \"{p}\"").Select(i => i.Replace('\\', '/'));

		var arguments = new List<string>()
		{
			$"-o {output_name + output_extension}",
			"-lkernel32",
			"-luser32",
			StandardLibrary
		};

		arguments.AddRange(input_files);

		if (output_type == BinaryType.SHARED_LIBRARY)
		{
			arguments.Add("-e main"); // Set the entry point to be the main function
			arguments.Add(SHARED_LIBRARY_FLAG);
		}
		else if (output_type == BinaryType.STATIC_LIBRARY)
		{
			return Status.Error("Static libraries should be passed to the link function");
		}
		else
		{
			arguments.Add("-e main"); // Set the entry point to be the main function
		}

		arguments.AddRange(library_paths);

		foreach (var library in Settings.Libraries)
		{
			arguments.Add(library);
		}

		return Run(GetLinker(), arguments);
	}

	/// <summary>
	/// Links the specified input file with necessary system files and produces an executable with the specified output filename
	/// </summary>
	private static Status Linux_Link(IEnumerable<string> input_files, string output_file)
	{
		var output_type = Settings.OutputType;

		if (output_type == BinaryType.STATIC_LIBRARY)
		{
			return Status.Error("Static libraries should be passed to the link function");
		}

		List<string>? arguments;

		if (output_type != BinaryType.EXECUTABLE)
		{
			var extension = output_type == BinaryType.SHARED_LIBRARY ? SharedLibraryExtension : StaticLibraryExtension;

			var flag = output_type == BinaryType.SHARED_LIBRARY ? SHARED_LIBRARY_FLAG : STATIC_LIBRARY_FLAG;

			arguments = new List<string>()
			{
				flag,
				$"-o {output_file}{extension}",
				StandardLibrary
			};
		}
		else
		{
			arguments = new List<string>()
			{
				$"-o {output_file}",
				StandardLibrary
			};
		}

		arguments.AddRange(input_files);

		// Provide all folders in PATH to linker as library paths
		var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
		var library_paths = path.Split(':').Where(p => !string.IsNullOrEmpty(p)).Select(p => $"-L \"{p}\"").Select(p => p.Replace('\\', '/'));

		arguments.AddRange(library_paths);

		foreach (var library in Settings.Libraries)
		{
			arguments.Add("-l" + library);
		}

		return Run(GetLinker(), arguments);
	}

	/// <summary>
	/// Creates the object file name which will be produced from the specified source file with output name of the current binary
	/// </summary>
	public static string GetObjectFileName(SourceFile source_file, string output_name)
	{
		return output_name + '.' + source_file.GetFilenameWithoutExtension() + ObjectFileExtension;
	}

	public override Status Execute()
	{
		Translator.TotalInstructions = 0;

		var parse = Settings.Parse ?? throw new ApplicationException("Missing parse");
		var files = Settings.SourceFiles;
		var objects = Settings.UserImportedObjectFiles;
		var imports = Settings.Libraries;

		var output_name = Settings.OutputName;
		var output_type = Settings.OutputType;

		var context = parse.Context;
		var assemblies = (Dictionary<SourceFile, string>?)null;
		var exports = new Dictionary<SourceFile, List<string>>();

		try
		{
			assemblies = Assembler.Assemble(context, files, imports, exports, output_name, output_type);
		}
		catch (Exception e)
		{
			return Status.Error(e.Message);
		}

		try
		{
			if (Settings.IsAssemblyOutputEnabled)
			{
				foreach (var file in files)
				{
					var assembly = assemblies[file];
					var assembly_file = output_name + '.' + file.GetFilenameWithoutExtension() + ASSEMBLY_OUTPUT_EXTENSION;

					File.WriteAllText(assembly_file, assembly);
				}
			}
		}
		catch
		{
			return Status.Error("Could not move generated assembly into a file");
		}

		// Skip using the legacy system if it is not required
		if (!Settings.IsLegacyAssemblyEnabled) return Status.OK;

		if (Settings.IsVerboseOutputEnabled)
		{
			Console.WriteLine("Total Instructions: " + Translator.TotalInstructions);
		}

		Status status;

		foreach (var file in files)
		{
			var assembly_file = output_name + '.' + file.GetFilenameWithoutExtension() + ASSEMBLY_OUTPUT_EXTENSION;
			var object_file = GetObjectFileName(file, output_name);

			if ((status = Compile(assembly_file, object_file)).IsProblematic)
			{
				return status;
			}
		}

		if (output_type == BinaryType.STATIC_LIBRARY)
		{
			return StaticLibraryFormat.Export(context, output_name, exports);
		}

		var object_files = files.Select(i => GetObjectFileName(i, output_name)).Concat(objects).ToList();

		if (IsLinux)
		{
			return Linux_Link(object_files, output_name);
		}
		else
		{
			return Windows_Link(object_files, output_name);
		}
	}
}