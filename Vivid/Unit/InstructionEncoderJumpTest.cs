using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System;

[TestClass]
public class InstructionEncoderJumpTests
{
	[TestMethod]
	public void Test()
	{
		// Initialize the target architecture
		#warning These settings might break things in the other tests
		Instructions.X64.Initialize();
		Keywords.Values.Clear();
		Operators.Map.Remove(Operators.AND.Identifier);
		Operators.Map.Remove(Operators.OR.Identifier);

		Environment.CurrentDirectory = "/home/lehtojo/vivid/Vivid/";
		var assembly = File.ReadAllText("./Tests/Instructions/Jumps.asm");

		var parser = new AssemblyParser();
		parser.Parse(assembly);

		// Load the expected binary
		var expected = File.ReadAllBytes("./Tests/Instructions/Jumps-Expected.bin");

		// Encode the parsed instructions and then compare the produced binary to the expected one
		var output = InstructionEncoder.Encode(parser.Instructions, null);
		var actual = output.Section.Data;

		var fails = expected.Length != actual.Length;

		for (var i = 0; i < Math.Min(expected.Length, actual.Length); i++)
		{
			if (expected[i] == actual[i]) continue;
			Assert.Fail($"Expected and actual binaries differ at index {i}");
		}

		if (fails) Assert.Fail($"Expected and actual binaries are identical in the shared range, but one is longer than the other");
	}
}