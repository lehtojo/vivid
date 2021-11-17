using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.IO;
using System;

public class ExpanderInstance
{
	public int Start { get; set; }
	public string Expander { get; set; }
	public int End => Start + Expander.Length;

	public ExpanderInstance(int index, string expander)
	{
		Start = index;
		Expander = expander;
	}
}

[TestClass]
public class InstructionEncoderCoreTest
{
	public const string EXPECTED_ASSEMBLY_OUTPUT = "Instructions-Expected.asm";
	public const string EXPECTED_OBJECT_FILE = "Instructions-Expected.o";
	public const string EXPECTED_DISASSEMBLY_FILE = "Instructions-Expected.disassembly";
	
	public const string ACTUAL_ASSEMBLY_OUTPUT = "Instructions-Actual.asm";
	public const string ACTUAL_DISASSEMBLY_FILE = "Instructions-Actual.disassembly";
	public const string ACTUAL_BINARY_OUTPUT = "Instructions-Actual.bin";

	public const string REGISTER8_EXPANDER = "$register8";
	public const string REGISTER16_EXPANDER = "$register16";
	public const string REGISTER32_EXPANDER = "$register32";
	public const string REGISTER64_EXPANDER = "$register64";
	public const string REGISTER_EXPANDER = "$register";

	public const string ADDRESS8_EXPANDER = "$address8";
	public const string ADDRESS16_EXPANDER = "$address16";
	public const string ADDRESS32_EXPANDER = "$address32";
	public const string ADDRESS64_EXPANDER = "$address64";
	public const string ADDRESS_EXPANDER = "$address";

	public const string CONSTANT8_EXPANDER = "$constant8";
	public const string CONSTANT16_EXPANDER = "$constant16";
	public const string CONSTANT32_EXPANDER = "$constant32";
	public const string CONSTANT64_EXPANDER = "$constant64";
	public const string CONSTANT_MAX_32_EXPANDER = "$constant-max-32";
	public const string CONSTANT_EXPANDER = "$constant";

	public const string SYMBOL_EXPANDER = "$symbol";
	public const string SIGN_EXPANDER = "$sign";
	public const string POTENTIAL_MINUS = "$minus?";
	public const string MEMORY_ADDRESS_EXTENSION = "$ptr";
	public const string EVALUATION_MULTIPLIER_EXPANDER = "$multiplier";

	public static readonly string[] Expanders = new[]
	{
		REGISTER8_EXPANDER,
		REGISTER16_EXPANDER,
		REGISTER32_EXPANDER,
		REGISTER64_EXPANDER,
		REGISTER_EXPANDER,
		ADDRESS8_EXPANDER,
		ADDRESS16_EXPANDER,
		ADDRESS32_EXPANDER,
		ADDRESS64_EXPANDER,
		ADDRESS_EXPANDER,
		CONSTANT8_EXPANDER,
		CONSTANT16_EXPANDER,
		CONSTANT32_EXPANDER,
		CONSTANT64_EXPANDER,
		CONSTANT_MAX_32_EXPANDER,
		CONSTANT_EXPANDER,
		SYMBOL_EXPANDER,
		SIGN_EXPANDER,
		POTENTIAL_MINUS,
		MEMORY_ADDRESS_EXTENSION,
		EVALUATION_MULTIPLIER_EXPANDER,
	};

	public static string[] AddressExpansionsWithoutSizes = new[]
	{
		"[$register64]",
		"[$register64 $sign $constant-max-32]",
		"[$register64 * $multiplier]",

		"[$register64 * $multiplier $sign $constant-max-32]",
		"[$register64 * $multiplier + $register64]",

		"[$register64 * $multiplier + $register64 $sign $constant-max-32]",
		"[$register64 * $multiplier + $register64 $sign $constant-max-32]",

		"[$symbol]",
		"[$symbol $sign $constant-max-32]",
	};

	public static string[] GetExpansions(string expander)
	{
		switch (expander)
		{
			case REGISTER8_EXPANDER: return new[] { "cl", "r14b" };
			case REGISTER16_EXPANDER: return new[] { "cx", "r14w" };
			case REGISTER32_EXPANDER: return new[] { "ecx", "r14d" };
			case REGISTER64_EXPANDER: return new[] { "rcx", "r14" };
			case REGISTER_EXPANDER:
			return GetExpansions(REGISTER8_EXPANDER)
						.Concat(GetExpansions(REGISTER16_EXPANDER))
						.Concat(GetExpansions(REGISTER32_EXPANDER))
						.Concat(GetExpansions(REGISTER64_EXPANDER))
						.ToArray();

			case ADDRESS8_EXPANDER: return AddressExpansionsWithoutSizes.Select(i => Size.BYTE.Identifier + Assembler.MemoryAddressExtension + i).ToArray();
			case ADDRESS16_EXPANDER: return AddressExpansionsWithoutSizes.Select(i => Size.WORD.Identifier + Assembler.MemoryAddressExtension + i).ToArray();
			case ADDRESS32_EXPANDER: return AddressExpansionsWithoutSizes.Select(i => Size.DWORD.Identifier + Assembler.MemoryAddressExtension + i).ToArray();
			case ADDRESS64_EXPANDER: return AddressExpansionsWithoutSizes.Select(i => Size.QWORD.Identifier + Assembler.MemoryAddressExtension + i).ToArray();
			case ADDRESS_EXPANDER:
			return GetExpansions(ADDRESS8_EXPANDER)
						.Concat(GetExpansions(ADDRESS16_EXPANDER))
						.Concat(GetExpansions(ADDRESS32_EXPANDER))
						.Concat(GetExpansions(ADDRESS64_EXPANDER))
						.ToArray();

			case CONSTANT8_EXPANDER: return new[] { "10" };
			case CONSTANT16_EXPANDER: return new[] { "1000" };
			case CONSTANT32_EXPANDER: return new[] { "100000" };
			case CONSTANT64_EXPANDER: return new[] { "10000000000" };
			case CONSTANT_MAX_32_EXPANDER:
			return GetExpansions(CONSTANT8_EXPANDER)
						.Concat(GetExpansions(CONSTANT16_EXPANDER))
						.Concat(GetExpansions(CONSTANT32_EXPANDER))
						.ToArray();
			case CONSTANT_EXPANDER:
			return GetExpansions(CONSTANT8_EXPANDER)
						.Concat(GetExpansions(CONSTANT16_EXPANDER))
						.Concat(GetExpansions(CONSTANT32_EXPANDER))
						.Concat(GetExpansions(CONSTANT64_EXPANDER))
						.ToArray();

			case SYMBOL_EXPANDER: return new[] { "Fo0_B4r_Baz" };
			case SIGN_EXPANDER: return new[] { "+", "-" };
			case POTENTIAL_MINUS: return new[] { "", "-" };
			case MEMORY_ADDRESS_EXTENSION: return new[] { Assembler.MemoryAddressExtension };
			case EVALUATION_MULTIPLIER_EXPANDER: return new[] { "1", "2", "4", "8" };

			default: throw new ApplicationException($"Unknown expander {expander}");
		}
	}
	
	/// <summary>
	/// Expands the first found expander in the specified line.
	/// If no expander is found, null is returned.
	/// </summary>
	public static List<string>? ExpandFirst(string line)
	{
		// Find the first expander in the specified line
		var instance = Expanders.Select(i => new ExpanderInstance(line.IndexOf(i), i)).Where(i => i.Start >= 0).FirstOrDefault();

		// If there are no expanders in the line, the specified lines are expanded already
		if (instance == null) return null;

		var expansions = GetExpansions(instance.Expander);
		var expanded = new List<string>();

		foreach (var expansion in expansions)
		{
			// Replace the expander with the generated expansion
			expanded.Add(line[0..(instance.Start)] + expansion + line[(instance.End)..]);
		}

		return expanded;
	}

	public static List<string> Expand(string line)
	{
		var lines = new List<string>() { line };
		var complete = new List<string>();

		for (var i = 0; i < lines.Count; i++)
		{
			line = lines[i];

			var expansions = ExpandFirst(line);

			// If no expansions were created, the line is complete
			if (expansions == null)
			{
				complete.Add(line);
				continue;
			}

			lines.AddRange(expansions);
		}

		return complete;
	}

	public static string Simplify(string assembly)
	{
		assembly = assembly.Replace(" * 1", string.Empty);
		assembly = assembly.Replace("*1", string.Empty);
		assembly = assembly.Replace(" + 0", string.Empty);
		assembly = assembly.Replace("+0", string.Empty);
		assembly = assembly.Replace(" - 0", string.Empty);
		assembly = assembly.Replace("-0", string.Empty);
		return assembly;
	}

	public static string SimplifyDisassembly(string disassembly)
	{
		var complete = new List<string>();

		foreach (var line in disassembly.Split('\n'))
		{
			// The actual instruction starts after two tabs:
			// If the current line does not have two tabs, it does not have an instruction
			var index = line.IndexOf('\t');
			if (index < 0) continue;

			index = line.IndexOf('\t', index + 1);
			if (index < 0) continue;

			var simplified = line[(index + 1)..];

			// Remove comments
			var comment = simplified.IndexOf('#');
			if (comment >= 0) { simplified = simplified[0..(comment)]; }

			complete.Add(simplified);
		}

		return string.Join('\n', complete);
	}

	private static string? Run(string executable, string arguments)
	{
		var process = new Process();
		process.StartInfo.FileName = executable;
		process.StartInfo.Arguments = arguments;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.RedirectStandardOutput = true;

		var output = new StringBuilder();

		process.OutputDataReceived += new DataReceivedEventHandler((_, information) =>
		{
			if (string.IsNullOrEmpty(information.Data)) return;
			output.AppendLine(information.Data);
		});

		process.Start();
		process.BeginOutputReadLine();
		process.WaitForExit(); 
		process.Close();

		return output.ToString();
	}

	public static string ExpandAll(string text)
	{
		// Generate assembly for the built-in assembler
		Assembler.MemoryAddressExtension = " ";

		var assembly = string.Join("\n", text.Split('\n').SelectMany(i => Expand(i)));
		File.WriteAllText(ACTUAL_ASSEMBLY_OUTPUT, assembly);

		// Generate assembly for an external assembler
		Assembler.MemoryAddressExtension = " ptr ";

		AddressExpansionsWithoutSizes = new[]
		{
			"[$register64]",
			"[$register64 $sign $constant-max-32]",
			"[$register64 * $multiplier]",

			"[$register64 * $multiplier $sign $constant-max-32]",
			"[$register64 * $multiplier + $register64]",

			"[$register64 * $multiplier + $register64 $sign $constant-max-32]",
			"[$register64 * $multiplier + $register64 $sign $constant-max-32]",

			"[rip + $symbol]",
			"[rip + $symbol $sign $constant-max-32]",
		};

		// Export the expected assembly, so that it can be encoded into an object file using an external assembler
		var expected_assembly = Simplify(string.Join("\n", text.Split('\n').SelectMany(i => Expand(i))));
		File.WriteAllText(EXPECTED_ASSEMBLY_OUTPUT, Assembler.LEGACY_ASSEMBLY_SYNTAX_SPECIFIER + '\n' + expected_assembly + '\n');

		Assembler.MemoryAddressExtension = " ";

		// Initialize the target architecture
		Instructions.X64.Initialize();
		Keywords.Definitions.Clear();
		Operators.Initialize();
		Operators.Definitions.Remove(Operators.AND.Identifier);
		Operators.Definitions.Remove(Operators.OR.Identifier);

		// Now parse the generated assembly and then encode it
		var parser = new AssemblyParser();
		parser.Parse(assembly);

		// Encode the parsed instructions
		var output = InstructionEncoder.Encode(parser.Instructions, parser.DebugFile);
		var actual = output.Section.Data;

		// Export the generated binary
		File.WriteAllBytes(ACTUAL_BINARY_OUTPUT, actual);

		// Encode the expected textual assembly into an object file
		if (Run("as", $"-o {EXPECTED_OBJECT_FILE} {EXPECTED_ASSEMBLY_OUTPUT}") == null)
		{
			throw new ApplicationException("Failed to produce the expected object file");
		}

		var expected_disassembly = Run("objdump", $"-d {EXPECTED_OBJECT_FILE} -M intel");

		// Disassemble the generated object file, so that it can be compared with the disassembly of the actual binary
		if (expected_disassembly == null)
		{
			throw new ApplicationException("Failed to produce the expected disassembly file");
		}

		File.WriteAllText(EXPECTED_DISASSEMBLY_FILE, expected_disassembly);

		var actual_disassembly = Run("objdump", $"-b binary -m i386:x86-64 -D {ACTUAL_BINARY_OUTPUT} -M intel");

		// Disassemble the generated binary and then compare it with the disassembly of the expected assembly
		if (actual_disassembly == null)
		{
			throw new ApplicationException("Failed to produce the actual disassembly file from the created binary file");
		}

		File.WriteAllText(ACTUAL_DISASSEMBLY_FILE, actual_disassembly);

		expected_disassembly = SimplifyDisassembly(expected_disassembly);
		actual_disassembly = SimplifyDisassembly(actual_disassembly);

		File.WriteAllText(ACTUAL_DISASSEMBLY_FILE + ".reduced", actual_disassembly);
		File.WriteAllText(EXPECTED_DISASSEMBLY_FILE + ".reduced", expected_disassembly);

		if (expected_disassembly != actual_disassembly)
		{
			Assert.Fail("Expected disassembly did not match the actual disassembly");
		}

		return string.Empty;
	}

	[TestMethod]
	public void Run()
	{
		// Support custom working folder for testing
		if (Environment.GetEnvironmentVariable("UNIT_TEST_FOLDER") != null)
		{
			Environment.CurrentDirectory = Environment.GetEnvironmentVariable("UNIT_TEST_FOLDER")!;
		}

		ExpandAll(System.IO.File.ReadAllText("./Tests/Instructions.asm"));
	}
}