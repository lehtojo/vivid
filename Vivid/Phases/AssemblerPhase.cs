using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Linq;

public class AssemblerPhase : Phase
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

	private static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
	private static string ObjectFileExtension => IsLinux ? ".o" : ".obj";
	private static string SharedLibraryExtension => IsLinux ? ".so" : ".dll";
	private static string StaticLibraryExtension => IsLinux ? ".a" : ".lib";
	
	
	private static string StandardLibrary => STANDARD_LIBRARY + '_' + Enum.GetName(typeof(Architecture), Assembler.Architecture)!.ToLowerInvariant() + ObjectFileExtension;

	private static string RED = "\x1B[1;31m";
	private static string GREEN = "\x1B[1;32m";
	private static string CYAN = "\x1B[1;36m";
	private static string RESET = "\x1B[0m";

	/// <summary>
	/// Returns the executable name of the assembler based on the current settings
	/// </summary>
	private static string GetAssembler()
	{
		return Assembler.IsArm64 ? ARM64_ASSEMBLER : X64_ASSEMBLER;
	}

	/// <summary>
	/// Returns the executable name of the linker based on the current settings
	/// </summary>
	private static string GetLinker()
	{
		return Assembler.IsArm64 ? ARM64_LINKER : X64_LINKER;
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

	/// <symmary>
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
			var process = Process.Start(configuration);
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
	private static Status Compile(Bundle bundle, string input_file, string output_file)
	{
		var debug = bundle.Get("debug", false);
		var keep_assembly = bundle.Get("assembly", false);

		var arguments = new List<string>();

		// Add assembler format and output filename
		arguments.AddRange(new string[]
		{
			$"-o {output_file}",
			input_file,
			"--gdwarf2"
		});

		var status = Run(GetAssembler(), arguments);

		if (!keep_assembly)
		{
			try
			{
				System.IO.File.Delete(input_file);
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
	private static Status Windows_Link(Bundle bundle, IEnumerable<string> input_files, string output_name)
	{
		var output_type = bundle.Get<BinaryType>("output_type", BinaryType.EXECUTABLE);
		var output_extension = output_type switch
		{
			BinaryType.SHARED_LIBRARY => SharedLibraryExtension,
			BinaryType.STATIC_LIBRARY => StaticLibraryExtension,
			_ => ".exe"
		};

		// Provide all folders in PATH to linker as library paths
		var path = Environment.GetEnvironmentVariable("Path") ?? string.Empty;
		var library_paths = path.Split(';').Where(p => !string.IsNullOrEmpty(p)).Select(p => $"-L \"{p}\"").Select(p => p.Replace('\\', '/'));

		var arguments = new List<string>()
		{
			$"-o {output_name + output_extension}",
			"-e main",
			"-lkernel32",
			"-luser32",
			StandardLibrary
		};

		arguments.AddRange(input_files);

		if (output_type == BinaryType.SHARED_LIBRARY)
		{
			arguments.Add(SHARED_LIBRARY_FLAG);
		}
		else if (output_type == BinaryType.STATIC_LIBRARY)
		{
			return Status.Error("Static libraries on Windows are not supported yet");
		}

		arguments.AddRange(library_paths);

		var libraries = bundle.Get("libraries", Array.Empty<string>());

		foreach (var library in libraries)
		{
			arguments.Add(library);
		}

		return Run(GetLinker(), arguments);
	}

	/// <summary>
	/// Links the specified input file with necessary system files and produces an executable with the specified output filename
	/// </summary>
	private static Status Linux_Link(Bundle bundle, IEnumerable<string> input_files, string output_file)
	{
		var output_type = bundle.Get<BinaryType>("output_type", BinaryType.EXECUTABLE);

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

		var libraries = bundle.Get("libraries", Array.Empty<string>());

		foreach (var library in libraries)
		{
			arguments.Add("-l" + library);
		}

		return Run(GetLinker(), arguments);
	}

	public static File[] GetModifiedFiles(Bundle bundle, File[] files)
	{
		// If the bundle contains a flag, which forces to rebuild, just return the specified files
		if (bundle.Get(ConfigurationPhase.REBUILD_FLAG, false))
		{
			return files;
		}

		var output_name = bundle.Get(ConfigurationPhase.OUTPUT_NAME, ConfigurationPhase.DEFAULT_OUTPUT);
		var modified = new List<File>();

		foreach (var file in files)
		{
			var object_file = output_name + '.' + file.GetFilenameWithoutExtension() + ObjectFileExtension;

			if (!System.IO.File.Exists(object_file))
			{
				if (Assembler.IsVerboseOutputEnabled)
				{
					Console.WriteLine($"{file.Fullname} {CYAN}is a new file{RESET}");
				}
				
				modified.Add(file);
				continue;
			}

			var source_file_last_write = System.IO.File.GetLastWriteTime(file.Fullname);
			var object_file_last_write = System.IO.File.GetLastWriteTime(object_file);

			if (source_file_last_write < object_file_last_write)
			{
				if (Assembler.IsVerboseOutputEnabled)
				{
					Console.WriteLine($"{file.Fullname} {GREEN}has not changed{RESET}");
				}

				continue;
			}

			if (Assembler.IsVerboseOutputEnabled)
			{
				Console.WriteLine($"{file.Fullname} {RED}has changed{RESET}");
			}

			modified.Add(file);
		}

		return modified.ToArray();
	}

	public override Status Execute(Bundle bundle)
	{
		if (!bundle.Contains("parse"))
		{
			return Status.Error("Nothing to assemble");
		}

		var parse = bundle.Get<Parse>("parse");
		var files = bundle.Get(FilePhase.OUTPUT, Array.Empty<File>());
		var output_name = bundle.Get(ConfigurationPhase.OUTPUT_NAME, ConfigurationPhase.DEFAULT_OUTPUT);

		// Filter out files that have not changed since the last compilation
		var modified = GetModifiedFiles(bundle, files);

		var context = parse.Context;
		var assemblies = new Dictionary<File, string>();

		try
		{
			assemblies = Assembler.Assemble(context, modified);
		}
		catch (Exception e)
		{
			return Status.Error(e.Message);
		}

		try
		{
			foreach (var file in modified)
			{
				var assembly = assemblies[file];
				var assembly_file = output_name + '.' + file.GetFilenameWithoutExtension() + ASSEMBLY_OUTPUT_EXTENSION;

				System.IO.File.WriteAllText(assembly_file, assembly);
			}
		}
		catch
		{
			return Status.Error("Could not move generated assembly into a file");
		}

		Status status;

		foreach (var file in modified)
		{
			var assembly_file = output_name + '.' + file.GetFilenameWithoutExtension() + ASSEMBLY_OUTPUT_EXTENSION;
			var object_file = output_name + '.' + file.GetFilenameWithoutExtension() + ObjectFileExtension;

			if ((status = Compile(bundle, assembly_file, object_file)).IsProblematic)
			{
				return status;
			}
		}

		var object_files = files.Select(i => output_name + '.' + i.GetFilenameWithoutExtension() + ObjectFileExtension).ToArray();

		if (IsLinux)
		{
			return Linux_Link(bundle, object_files, output_name);
		}
		else
		{
			return Windows_Link(bundle, object_files, output_name);
		}
	}
}