using System;
using System.IO;
using System.Collections.Generic;

public class AssemblerPhase : Phase
{
	public override Status Execute(Bundle bundle)
	{
		if (!bundle.Get(ConfigurationPhase.ASSEMBLER_FLAG, false)) return Status.OK;

		var files = bundle.Get(FilePhase.OUTPUT, new List<SourceFile>());

		if (files.Count == 0)
		{
			return Status.Error("Nothing to assembly");
		}

		// Initialize the target architecture
		Instructions.X64.Initialize();
		Keywords.Values.Clear();
		Operators.Map.Remove(Operators.AND.Identifier);
		Operators.Map.Remove(Operators.OR.Identifier);

		var succeeded = true;

		foreach (var file in files)
		{
			try
			{
				var parser = new AssemblyParser();
				parser.Parse(file.Content);

				var text_section_output = EncoderX64.Encode(parser.Instructions);
				var binary_text_section = text_section_output.Section;
				var binary_data_section = parser.Data.Export();

				var object_file = ElfFormat.BuildObjectX64(new List<BinarySection> { binary_text_section, binary_data_section });
				File.WriteAllBytes(file.Fullname + AssemblyPhase.ObjectFileExtension, object_file);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e.Message);
				succeeded = false;
			}
		}

		if (succeeded) Environment.Exit(0);

		return Status.Error("Assembler failed");
	}
}