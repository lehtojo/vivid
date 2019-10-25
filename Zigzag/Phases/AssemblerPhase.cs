using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System;

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

	private Status Run (string executable, List<string> arguments)
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
			Process process = Process.Start(configuration);
			process.WaitForExit();

			return process.ExitCode == EXIT_CODE_OK ? Status.OK : Status.Error(ERROR);

		}
		catch
		{
			return Status.Error(ERROR);
		}
	}

	private Status Compile(Bundle bundle, string input, string output)
	{
		bool debug = bundle.Get("debug", false);
		bool delete = !bundle.Get("assembly", false);

		List<string> arguments = new List<string>();

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

		Console.WriteLine("Starting...");

		Status status = Run(COMPILER, arguments);

		Console.WriteLine("Finished...");

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

		string[] libraries = bundle.Get("libraries", new string[] { });

		foreach (string library in libraries)
		{
			arguments.Add("-l" + library);
		}

		return Run(LINKER, arguments);
	}

	public override Status Execute(Bundle bundle)
	{
		Parse parse = bundle.Get<Parse>("parse", null);

		if (parse == null)
		{
			return Status.Error("Nothing to assemble");
		}

		string output = bundle.Get("output", ConfigurationPhase.DEFAULT_OUTPUT);
		string source = output + ".asm";
		string @object = output + ".obj";

		Node node = parse.Node;
		Context context = parse.Context;

		string assembly = Assembler.Build(node, context);

		try
		{
			File.WriteAllText(source, assembly);
		}
		catch
		{
			return Status.Error("Couldn't move generated assembly into a file");
		}

		if (Compile(bundle, source, @object).IsProblematic)
		{
			return Status.Error(ERROR);
		}

		if (Link(bundle, @object, output).IsProblematic)
		{
			return Status.Error(ERROR);
		}

		return Status.OK;
	}
}