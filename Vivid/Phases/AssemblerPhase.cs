using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class AssemblerPhase : Phase
{
	public override Status Execute(Bundle bundle)
	{
		if (!bundle.Get(ConfigurationPhase.ASSEMBLER_FLAG, false)) return Status.OK;

		var files = bundle.Get(FilePhase.OUTPUT, new List<SourceFile>());
		if (files.Count == 0) return Status.Error("Nothing to assembly");

		var link = bundle.Get(ConfigurationPhase.LINK_FLAG, false);

		// Determine the output name of the object file
		var output_name = bundle.Get(ConfigurationPhase.OUTPUT_NAME, ConfigurationPhase.DEFAULT_OUTPUT);

		// Initialize the target architecture
		Instructions.X64.Initialize();
		Keywords.Values.Clear();
		Operators.Map.Remove(Operators.AND.Identifier);
		Operators.Map.Remove(Operators.OR.Identifier);

		var succeeded = true;

		// Mesh all assembly files into one large code chunk
		var assembly = string.Join('\n', files.Select(i => i.Content));

		try
		{
			var parser = new AssemblyParser();
			parser.Parse(assembly);

			var text_section_output = InstructionEncoder.Encode(parser.Instructions);
			var text_section = text_section_output.Section;
			var data_sections = parser.Sections.Values.Select(i => i.Export()).ToList();
			var debug_frames_section = text_section_output.Frames.Export();
			var debug_lines_section = text_section_output.Lines.Export();

			var sections = new List<BinarySection>() { text_section };
			sections.Add(debug_frames_section);
			sections.AddRange(data_sections);
			sections.Add(debug_lines_section);

			if (link)
			{
				var object_file = ElfFormat.Create(sections);
				var result = Linker.Link(new List<BinaryObjectFile> { object_file }, "_V4initv_rx");

				File.WriteAllBytes(output_name, result);
			}
			else
			{
				var object_file = ElfFormat.Build(sections);
				File.WriteAllBytes(output_name + AssemblyPhase.ObjectFileExtension, object_file);
			}
		}
		catch (Exception e)
		{
			Console.Error.WriteLine(e.Message);
			succeeded = false;
		}

		if (succeeded) Environment.Exit(0);

		return Status.Error("Assembler failed");
	}
}