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
		Keywords.Definitions.Clear();
		Operators.Definitions.Remove(Operators.AND.Identifier);
		Operators.Definitions.Remove(Operators.OR.Identifier);

		var succeeded = true;

		try
		{
			if (link)
			{
				var object_files = new List<BinaryObjectFile>();

				foreach (var file in files)
				{
					var parser = new AssemblyParser();
					parser.Parse(file.Content);

					var text_section_output = InstructionEncoder.Encode(parser.Instructions, parser.DebugFile);
					var text_section = text_section_output.Section;
					var data_sections = parser.Sections.Values.Select(i => i.Export()).ToList();
					var debug_frames_section = text_section_output.Frames?.Export();
					var debug_lines_section = text_section_output.Lines?.Export();

					var sections = new List<BinarySection>() { text_section };
					if (debug_frames_section != null) sections.Add(debug_frames_section);
					sections.AddRange(data_sections);
					if (debug_lines_section != null) sections.Add(debug_lines_section);

					object_files.Add(ElfFormat.Create(sections, parser.Exports));
				}

				var result = Assembler.IsTargetWindows
					? PeFormat.Link(object_files, new List<string>(), Assembler.DefaultEntryPoint, output_name, true)
					: Linker.Link(object_files, Assembler.DefaultEntryPoint, true);

				File.WriteAllBytes(output_name, result);
			}
			else
			{
				foreach (var file in files)
				{
					var parser = new AssemblyParser();
					parser.Parse(file.Content);

					var text_section_output = InstructionEncoder.Encode(parser.Instructions, parser.DebugFile);
					var text_section = text_section_output.Section;
					var data_sections = parser.Sections.Values.Select(i => i.Export()).ToList();
					var debug_frames_section = text_section_output.Frames?.Export();
					var debug_lines_section = text_section_output.Lines?.Export();

					var sections = new List<BinarySection>() { text_section };
					if (debug_frames_section != null) sections.Add(debug_frames_section);
					sections.AddRange(data_sections);
					if (debug_lines_section != null) sections.Add(debug_lines_section);

					var object_file = Assembler.IsTargetWindows
						? PeFormat.Build(sections, parser.Exports)
						: ElfFormat.Build(sections, parser.Exports);

					File.WriteAllBytes(output_name + AssemblyPhase.ObjectFileExtension, object_file);
				}
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