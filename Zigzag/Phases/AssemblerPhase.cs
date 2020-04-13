using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public class AssemblerPhase : Phase
{
	private const int EXIT_CODE_OK = 0;

	private const string COMPILER = "yasm";
	private const string LINKER = "link";

	private const string COMPILER_DEBUG_ARGUMENT = "-g cv8";
	private const string COMPILER_PLATFORM = "-f win32";

	private const string LINKER_SUBSYSTEM = "/subsystem:console";
	private const string LINKER_DEFAULT_LIB = "/nodefaultlib";
	private const string LINKER_ENTRY = "/entry:main";
	private const string LINKER_DEBUG = "/debug";
	private const string LINKER_LIBRARY_PATH = "/libpath:\"C:\\Program Files (x86)\\Windows Kits\\10\\Lib\\10.0.18362.0\\um\\x86\"";
	private const string LINKER_STANDARD_LIBRARY = "libz.obj";

	private const string ERROR = "Internal assembler failed";

	private Status Run(string executable, List<string> arguments)
	{
		ProcessStartInfo configuration = new ProcessStartInfo()
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

			return process.ExitCode == EXIT_CODE_OK ? Status.OK : Status.Error(ERROR + "\n" + process.StandardError.ReadToEnd());
		}
		catch
		{
			return Status.Error(ERROR);
		}
	}

	private Status Compile(Bundle bundle, string input, string output)
	{
		var debug = bundle.Get("debug", false);
		var delete = !bundle.Get("assembly", false);

		var arguments = new List<string>();

		if (debug)
		{
			arguments.Add(COMPILER_DEBUG_ARGUMENT);
		}

		arguments.AddRange(new string[]
		{
			COMPILER_PLATFORM,
			$"-o {output}",
			input
		});

		var status = Run(COMPILER, arguments);

		if (delete)
		{
			try
			{
				File.Delete(input);
			}
			catch
			{
				Console.WriteLine("Warning: Couldn't remove generated assembly file");
			}
		}

		return status;
	}

	private Status Link(Bundle bundle, string input, string output)
	{
		List<string> arguments = new List<string>()
		{
			$"/out:{output + ".exe"}",
			LINKER_SUBSYSTEM,
			LINKER_DEFAULT_LIB,
			LINKER_ENTRY,
			LINKER_DEBUG,
			LINKER_LIBRARY_PATH,
			"kernel32.lib",
			"user32.lib",
			input,
			LINKER_STANDARD_LIBRARY
		};

		var libraries = bundle.Get("libraries", new string[] { });

		foreach (string library in libraries)
		{
			arguments.Add("-l" + library);
		}

		return Run(LINKER, arguments);
	}

	public override Status Execute(Bundle bundle)
	{
		if (!bundle.Contains("parse"))
		{
			return Status.Error("Nothing to assemble");
		}

		var parse = bundle.Get<Parse>("parse");
		var only_assembly = bundle.Get("assembly", false);

		var output_file = bundle.Get("output", ConfigurationPhase.DEFAULT_OUTPUT);
		var source_file = output_file + ".asm";
		var object_file = output_file + ".obj";

		var context = parse.Context;
		var assembly = Assembler.Build(context).TrimEnd();

		try
		{
			File.WriteAllText(source_file, assembly);
		}
		catch
		{
			return Status.Error("Couldn't move generated assembly into a file");
		}

		if (only_assembly)
		{
			return Status.OK;
		}

		Status status;

		if ((status = Compile(bundle, source_file, object_file)).IsProblematic)
		{
			return status;
		}

		if ((status = Link(bundle, object_file, output_file)).IsProblematic)
		{
			return status;
		}

		return Status.OK;
	}
}