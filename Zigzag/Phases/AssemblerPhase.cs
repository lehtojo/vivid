using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Linq;

public class AssemblerPhase : Phase
{
    private const int EXIT_CODE_OK = 0;

    private const string COMPILER = "yasm";

    private const string WINDOWS_ASSEMBLER_FORMAT = "-f win";
    private const string LINUX_ASSEMBLER_FORMAT = "-f elf";

    private const string WINDOWS_ASSEMBLER_DEBUG_ARGUMENT = "-g cv8";
    private const string LINUX_ASSEMBLER_DEBUG_ARGUMENT = "-g dwarf2";

    private const string WINDOWS_LINKER = "link";
    private const string LINUX_LINKER = "ld";

    private const string WINDOWS_LINKER_SUBSYSTEM = "/subsystem:console";
    private const string WINDOWS_LINKER_DEFAULT_LIB = "/nodefaultlib";
    private const string WINDOWS_LINKER_ENTRY = "/entry:function_run";
    private const string WINDOWS_LINKER_DEBUG = "/debug";
    private const string WINDOWS_LARGE_ADDRESS_UNAWARE = "/largeaddressaware:no";

    private const string LINUX_SHARED_LIBRARY_FLAG = "--shared";
    private const string LINUX_STATIC_LIBRARY_FLAG = "--static";

    private const string WINDOWS_SHARED_LIBRARY_FLAG = "/dll";

    private const string STANDARD_LIBRARY = "libz";

    private const string ERROR = "Internal assembler failed";

    private bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    private string AssemblerFormat =>
        (IsLinux ? LINUX_ASSEMBLER_FORMAT : WINDOWS_ASSEMBLER_FORMAT) + Assembler.Size.Bits;

    private string AssemblerDebugArgument =>
        IsLinux ? LINUX_ASSEMBLER_DEBUG_ARGUMENT : WINDOWS_ASSEMBLER_DEBUG_ARGUMENT;

    private string ObjectFileExtension => IsLinux ? ".o" : ".obj";
    private string SharedLibraryExtension => IsLinux ? ".so" : ".dll";
    private string StaticLibraryExtension => IsLinux ? ".a" : ".lib";
    private string StandardLibrary => STANDARD_LIBRARY + Assembler.Size.Bits + ObjectFileExtension;

    /// <symmary>
    /// Runs the specified executable with the given arguments
    /// </summary>
    private Status Run(string executable, List<string> arguments)
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

            string output;

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
            return Status.Error(ERROR);
        }
    }

    /// <summary>
    /// Compiles the specified input file and exports the result with the specified output filename
    /// </summary>
    private Status Compile(Bundle bundle, string input_file, string output_file)
    {
        var debug = bundle.Get("debug", false);
        var keep_assembly = bundle.Get("assembly", false);

        var arguments = new List<string>();

        if (debug)
        {
            arguments.Add(AssemblerDebugArgument);
        }

        // Add assembler format and output filename
        arguments.AddRange(new string[]
        {
            AssemblerFormat,
            $"-o {output_file}",
            input_file
        });

        var status = Run(COMPILER, arguments);

        if (keep_assembly) return status;

        try
        {
            File.Delete(input_file);
        }
        catch
        {
            Console.WriteLine("Warning: Couldn't remove generated assembly file");
        }

        return status;
    }

    /// <summary>
    /// Links the specified input file with necessary system files and produces an executable with the specified output filename
    /// </summary>
    private Status Windows_Link(Bundle bundle, string input_file, string output_name)
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
        var library_paths = path.Split(';').Where(p => !string.IsNullOrEmpty(p)).Select(p => $"/libpath:\"{p}\"")
            .Select(p => p.Replace('\\', '/'));

        var arguments = new List<string>()
        {
            $"/out:{output_name + output_extension}",
            WINDOWS_LINKER_SUBSYSTEM,
            WINDOWS_LINKER_ENTRY,
            WINDOWS_LINKER_DEBUG,
            WINDOWS_LARGE_ADDRESS_UNAWARE,
            "kernel32.lib",
            "user32.lib",
            input_file,
            StandardLibrary
        };

        switch (output_type)
        {
            case BinaryType.SHARED_LIBRARY:
                arguments.Add(WINDOWS_SHARED_LIBRARY_FLAG);
                break;
            case BinaryType.STATIC_LIBRARY:
                return Status.Error("Static libraries on Windows are not supported yet");
        }

        arguments.AddRange(library_paths);

        var libraries = bundle.Get("libraries", new string[] { });

        foreach (var library in libraries)
        {
            arguments.Add(library);
        }

        var result = Run(WINDOWS_LINKER, arguments);

        try
        {
            File.Delete(input_file);
        }
        catch
        {
            Console.WriteLine("Warning: Couldn't remove generated object file");
        }

        return result;
    }

    /// <summary>
    /// Links the specified input file with necessary system files and produces an executable with the specified output filename
    /// </summary>
    private Status Linux_Link(Bundle bundle, string input_file, string output_file)
    {
        var output_type = bundle.Get("output_type", BinaryType.EXECUTABLE);

        List<string>? arguments;

        if (output_type != BinaryType.EXECUTABLE)
        {
            var extension = output_type == BinaryType.SHARED_LIBRARY ? SharedLibraryExtension : StaticLibraryExtension;

            var flag = output_type == BinaryType.SHARED_LIBRARY ? LINUX_SHARED_LIBRARY_FLAG : LINUX_STATIC_LIBRARY_FLAG;

            arguments = new List<string>()
            {
                flag,
                $"-o {output_file}{extension}",
                input_file,
                StandardLibrary
            };
        }
        else
        {
            arguments = new List<string>()
            {
                $"-o {output_file}",
                input_file,
                StandardLibrary
            };
        }

        var libraries = bundle.Get("libraries", new string[] { });

        foreach (var library in libraries)
        {
            arguments.Add("-l" + library);
        }

        var result = Run(LINUX_LINKER, arguments);

        try
        {
            File.Delete(input_file);
        }
        catch
        {
            Console.WriteLine("Warning: Couldn't remove generated object file");
        }

        return result;
    }

    public override Status Execute(Bundle bundle)
    {
        if (!bundle.Contains("parse"))
        {
            return Status.Error("Nothing to assemble");
        }

        var parse = bundle.Get<Parse>("parse");

        var output_file = bundle.Get("output", ConfigurationPhase.DEFAULT_OUTPUT);
        var source_file = output_file + ".asm";
        var object_file = output_file + ObjectFileExtension;

        var context = parse.Context;
        string? assembly;

        try
        {
            assembly = Assembler.Assemble(context).TrimEnd();
        }
        catch (Exception e)
        {
            return Status.Error(e.Message);
        }

        try
        {
            File.WriteAllText(source_file, assembly);
        }
        catch
        {
            return Status.Error("Couldn't move generated assembly into a file");
        }

        Status status;

        if ((status = Compile(bundle, source_file, object_file)).IsProblematic)
        {
            return status;
        }

        return IsLinux ? Linux_Link(bundle, object_file, output_file) : Windows_Link(bundle, object_file, output_file);
    }
}