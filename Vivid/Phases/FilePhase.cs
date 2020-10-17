using System.IO;
using System;

public class FilePhase : Phase
{
	private const string COMPLEX_LINE_ENDING = "\r\n";
	private const string LINE_ENDING = "\n";

	public override Status Execute(Bundle bundle)
	{
		string[] files = bundle.Get("input_files", Array.Empty<string>());

		if (files.Length == 0)
		{
			return Status.Error("Please enter input files");
		}

		string[] contents = new string[files.Length];

		for (int i = 0; i < files.Length; i++)
		{
			int index = i;

			Run(() =>
			{
				string file = files[index];

				try
				{
					string content = File.ReadAllText(file);
					contents[index] = content.Replace(COMPLEX_LINE_ENDING, LINE_ENDING).Replace('\t', ' ');
				}
				catch
				{
					return Status.Error("Could not load file '{0}'", file);
				}

				return Status.OK;
			});
		}

		bundle.Put("input_file_contents", contents);

		return Status.OK;
	}
}