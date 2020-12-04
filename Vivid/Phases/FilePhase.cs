using System.IO;
using System;

public class FilePhase : Phase
{
	private const string COMPLEX_LINE_ENDING = "\r\n";
	private const string LINE_ENDING = "\n";

	public override Status Execute(Bundle bundle)
	{
		var files = bundle.Get("input_files", Array.Empty<string>());

		if (files.Length == 0)
		{
			return Status.Error("Please enter input files");
		}

		var contents = new string[files.Length];

		for (int i = 0; i < files.Length; i++)
		{
			var index = i;

			Run(() =>
			{
				var file = files[index];

				try
				{
					var content = File.ReadAllText(file);
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