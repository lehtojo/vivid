using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class AssemblerPhase : Phase
{
	public override Status Execute()
	{
		if (!Settings.TextualAssembly) return Status.OK;

		var files = Settings.SourceFiles;
		if (files.Count == 0) return new Status("Nothing to assembly");

		// Initialize the target architecture
		Instructions.X64.Initialize();
		Keywords.All.Clear();
		Operators.All.Remove(Operators.AND.Identifier);
		Operators.All.Remove(Operators.OR.Identifier);

		var succeeded = true;

		try
		{
			if (Settings.LinkObjects)
			{
				var object_files = new List<BinaryObjectFile>();

				foreach (var file in files)
				{
					var parser = new AssemblyParser();
					parser.Parse(file, file.Content);

					var text_section_output = InstructionEncoder.Encode(parser.Instructions, parser.DebugFile);
					var text_section = text_section_output.Section;
					var data_sections = parser.Sections.Values.Select(i => i.Export()).ToList();
					var debug_frames_section = text_section_output.Frames?.Export();
					var debug_lines_section = text_section_output.Lines?.Export();

					var sections = new List<BinarySection>() { text_section };
					if (debug_frames_section != null) sections.Add(debug_frames_section);
					sections.AddRange(data_sections);
					if (debug_lines_section != null) sections.Add(debug_lines_section);

					object_files.Add(ElfFormat.Create(file.Filename, sections, parser.Exports));
				}

				var result = Settings.IsTargetWindows
					? PeFormat.Link(object_files, new List<string>(), Assembler.DefaultEntryPoint, Settings.OutputName, true)
					: Linker.Link(object_files, Assembler.DefaultEntryPoint, true);

				File.WriteAllBytes(Settings.OutputName, result);
			}
			else
			{
				foreach (var file in files)
				{
					var parser = new AssemblyParser();
					parser.Parse(file, file.Content);

					var text_section_output = InstructionEncoder.Encode(parser.Instructions, parser.DebugFile);
					var text_section = text_section_output.Section;
					var data_sections = parser.Sections.Values.Select(i => i.Export()).ToList();
					var debug_frames_section = text_section_output.Frames?.Export();
					var debug_lines_section = text_section_output.Lines?.Export();

					var sections = new List<BinarySection>() { text_section };
					if (debug_frames_section != null) sections.Add(debug_frames_section);
					sections.AddRange(data_sections);
					if (debug_lines_section != null) sections.Add(debug_lines_section);

					var object_file = Settings.IsTargetWindows
						? PeFormat.Build(sections, parser.Exports)
						: ElfFormat.Build(sections, parser.Exports);

					File.WriteAllBytes($"{Settings.OutputName}.{file.GetFilenameWithoutExtension()}{AssemblyPhase.ObjectFileExtension}", object_file);
				}
			}
		}
		catch (Exception e)
		{
			Console.Error.WriteLine(e.Message);
			succeeded = false;
		}

		if (succeeded) Environment.Exit(0);

		return new Status("Assembler failed");
	}
}