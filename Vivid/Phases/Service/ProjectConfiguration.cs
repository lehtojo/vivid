using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.IO;
using System;

public class ProjectBuildConfiguration
{
	public string Name { get; set; } = string.Empty;
	public string Output { get; set; } = ConfigurationPhase.DEFAULT_OUTPUT;
	public bool Default { get; set; } = false;
	public bool Debug { get; set; } = false;
	public string[] Include { get; set; } = Array.Empty<string>();
	public string[] Exclude { get; set; } = Array.Empty<string>();

	public List<string> FindSourceFiles(string folder)
	{
		// Normalize the include and exclude paths
		var normalized_include = Include.Select(i => ServiceUtility.Normalize(i, folder)).Distinct().ToArray();
		var normalized_exclude = Exclude.Select(i => ServiceUtility.Normalize(i, folder)).Distinct().ToArray();

		// Separate included folders and source files
		var included_files = normalized_include.Where(i => !File.GetAttributes(i).HasFlag(FileAttributes.Directory)).ToList();
		var included_folders = normalized_include.Where(i => File.GetAttributes(i).HasFlag(FileAttributes.Directory)).ToArray();

		// Separate excluded folders and source files
		var excluded_files = normalized_exclude.Where(i => !File.GetAttributes(i).HasFlag(FileAttributes.Directory)).ToList();
		var excluded_folders = normalized_exclude.Where(i => File.GetAttributes(i).HasFlag(FileAttributes.Directory)).ToArray();

		// Add all source files from the included folders
		foreach (var included_folder in included_folders)
		{
			var files = Directory.GetFiles(included_folder);
			included_files.AddRange(files.Where(i => i.EndsWith(ConfigurationPhase.VIVID_EXTENSION)));
		}

		// Remove duplicated source files
		included_files = included_files.Distinct().ToList();

		// Remove all excluded files from the included source files
		included_files = included_files.Where(i => !excluded_files.Contains(i)).ToList();

		// Remove all included source files that are inside any of the excluded folders
		included_files = included_files.Where(i => excluded_folders.All(folder => !i.StartsWith(folder))).ToList();

		return included_files.Select(i => ServiceUtility.Normalize(i)).ToList();
	}
}

public class ProjectConfiguration
{
	public const string Extension = ".project";

	public ProjectBuildConfiguration[] Configurations { get; set; } = Array.Empty<ProjectBuildConfiguration>();

	public static ProjectConfiguration? Load(string path)
	{
		try
		{
			var options = new JsonSerializerOptions()
			{
				PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance
			};

			return JsonSerializer.Deserialize<ProjectConfiguration>(File.ReadAllText(path), options);
		}
		catch
		{
			return null;
		}
	}

	public ProjectBuildConfiguration? DefaultConfiguration => Configurations.FirstOrDefault(i => i != null && i.Default, Configurations.FirstOrDefault());
}