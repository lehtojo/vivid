using System.IO;

public class FilePhase : Phase
{
	public override Status Execute(Bundle bundle)
	{
		string[] files = bundle.Get("input_files", new string[] { });

		if (files.Length == 0)
		{
			return Status.Error("Please enter input files");
		}

		string[] contents = new string[files.Length];

		for (int i = 0; i < files.Length; i++)
		{
			int index = i;

			Async(() =>
			{
				string file = files[index];

				try
				{
					string content = File.ReadAllText(file);
					contents[index] = content.Replace("\r\n", "\n");
				}
				catch
				{
					return Status.Error("Couldn't load file '{0}'", file);
				}

				return Status.OK;
			});
		}

		bundle.Put("input_file_contents", contents);

		return Status.OK;
	}
}