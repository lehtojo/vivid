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
		Instructions.X64.Initialize();
		Keywords.Definitions.Clear();
		Operators.Initialize();
		Operators.Definitions.Remove(Operators.AND.Identifier);
		Operators.Definitions.Remove(Operators.OR.Identifier);

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
	
		// Now check the relocations as well
		Assert.AreEqual(3, output.Relocations.Count);

		Assert.AreEqual(-4, output.Relocations[0].Addend);
		Assert.AreEqual(4, output.Relocations[0].Bytes);
		Assert.AreEqual(179, output.Relocations[0].Offset);
		Assert.AreEqual("L4", output.Relocations[0].Symbol.Name);
		Assert.AreEqual(BinaryRelocationType.PROGRAM_COUNTER_RELATIVE, output.Relocations[0].Type);

		Assert.AreEqual(-4, output.Relocations[1].Addend);
		Assert.AreEqual(4, output.Relocations[1].Bytes);
		Assert.AreEqual(370, output.Relocations[1].Offset);
		Assert.AreEqual("CL4", output.Relocations[1].Symbol.Name);
		Assert.AreEqual(BinaryRelocationType.PROGRAM_COUNTER_RELATIVE, output.Relocations[1].Type);

		Assert.AreEqual(-4, output.Relocations[2].Addend);
		Assert.AreEqual(4, output.Relocations[2].Bytes);
		Assert.AreEqual(411, output.Relocations[2].Offset);
		Assert.AreEqual("DL4", output.Relocations[2].Symbol.Name);
		Assert.AreEqual(BinaryRelocationType.PROGRAM_COUNTER_RELATIVE, output.Relocations[2].Type);
	}
}