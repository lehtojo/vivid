using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.IO;
using System;

[TestClass]
public class DataEncoderTests
{
	[TestMethod]
	public void Run()
	{
		// Initialize the target architecture
		Instructions.X64.Initialize();
		Keywords.Definitions.Clear();
		Operators.Initialize();
		Operators.Definitions.Remove(Operators.AND.Identifier);
		Operators.Definitions.Remove(Operators.OR.Identifier);

		// Support custom working folder for testing
		if (Environment.GetEnvironmentVariable("UNIT_TEST_FOLDER") != null)
		{
			Environment.CurrentDirectory = Environment.GetEnvironmentVariable("UNIT_TEST_FOLDER")!;
		}

		var assembly = File.ReadAllText("./Tests/Data.asm");
		var file = new SourceFile("./Tests/Data.asm", assembly, 0);

		var parser = new AssemblyParser();
		parser.Parse(file, assembly);

		var expected_data_section = File.ReadAllBytes("./Tests/Expected-Data-Section.bin");

		var data_section = parser.Sections[".data"].Export();
		var other_section = parser.Sections[".other"].Export();

		// Compare the binary output
		Assert.True(data_section.Data.SequenceEqual(expected_data_section));
		Assert.True(other_section.Data.All(i => i == 0));

		var data = data_section.Symbols["data"];
		var foo = data_section.Symbols["foo"];
		var bar = data_section.Symbols["bar"];
		var something = data_section.Symbols["something"];

		// Data section symbols:
		Assert.AreEqual(data_section.Symbols.Count, 4);

		Assert.AreEqual(data.Name, "data");
		Assert.AreEqual(data.Offset, 0);
		Assert.AreEqual(data.Export, false);
		Assert.AreEqual(data.External, false);

		Assert.AreEqual(foo.Name, "foo");
		Assert.AreEqual(foo.Offset, 4);
		Assert.AreEqual(foo.Export, false);
		Assert.AreEqual(foo.External, false);

		Assert.AreEqual(bar.Name, "bar");
		Assert.AreEqual(bar.Offset, 16);
		Assert.AreEqual(bar.Export, false);
		Assert.AreEqual(bar.External, false);

		Assert.AreEqual(something.Name, "something");
		Assert.AreEqual(something.Offset, 0);
		Assert.AreEqual(something.Export, false);
		Assert.AreEqual(something.External, true);

		// Data section relocations:
		Assert.True(ReferenceEquals(data_section.Relocations[0].Symbol, something));
		Assert.AreEqual(data_section.Relocations[0].Offset, 108);
		Assert.AreEqual(data_section.Relocations[0].Type, BinaryRelocationType.ABSOLUTE32);
		Assert.AreEqual(data_section.Relocations[0].Bytes, 4);

		var other = other_section.Symbols["other"];
		var start = other_section.Symbols["start"];
		foo = other_section.Symbols["foo"];
		bar = other_section.Symbols["bar"];
		var baz = other_section.Symbols["baz"];

		// Other section symbols:
		Assert.AreEqual(other_section.Symbols.Count, 5);

		Assert.AreEqual(other.Name, "other");
		Assert.AreEqual(other.Offset, 0);
		Assert.AreEqual(other.Export, false);
		Assert.AreEqual(other.External, false);

		Assert.AreEqual(start.Name, "start");
		Assert.AreEqual(start.Offset, 0);
		Assert.AreEqual(start.Export, false);
		Assert.AreEqual(start.External, false);

		Assert.AreEqual(foo.Name, "foo");
		Assert.AreEqual(foo.Offset, 0);
		Assert.AreEqual(foo.Export, false);
		Assert.AreEqual(foo.External, true);

		Assert.AreEqual(bar.Name, "bar");
		Assert.AreEqual(bar.Offset, 0);
		Assert.AreEqual(bar.Export, false);
		Assert.AreEqual(bar.External, true);

		Assert.AreEqual(baz.Name, "baz");
		Assert.AreEqual(baz.Offset, 16);
		Assert.AreEqual(baz.Export, false);
		Assert.AreEqual(baz.External, false);

		// Other section relocations:
		Assert.True(ReferenceEquals(other_section.Relocations[0].Symbol, foo));
		Assert.AreEqual(other_section.Relocations[0].Offset, 0);
		Assert.AreEqual(other_section.Relocations[0].Type, BinaryRelocationType.ABSOLUTE32);
		Assert.AreEqual(other_section.Relocations[0].Bytes, 4);

		Assert.True(ReferenceEquals(other_section.Relocations[1].Symbol, bar));
		Assert.AreEqual(other_section.Relocations[1].Offset, 16);
		Assert.AreEqual(other_section.Relocations[1].Type, BinaryRelocationType.ABSOLUTE64);
		Assert.AreEqual(other_section.Relocations[1].Bytes, 8);

		// Other section exports:
		Assert.True(parser.Exports.Contains("baz"));
		Assert.AreEqual(parser.Exports.Count, 1);
	}
}