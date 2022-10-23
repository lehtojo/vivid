using System.Linq;

public static class Inliner
{
	/// <summary>
	/// Finds all the labels under the specified root and localizes them by declaring new labels to the specified context
	/// </summary>
	public static void LocalizeLabels(FunctionImplementation implementation, Node root)
	{
		// Find all the labels and the jumps under the specified root
		var labels = root.FindAll(NodeType.LABEL).Cast<LabelNode>();
		var jumps = root.FindAll(NodeType.JUMP).Cast<JumpNode>().ToList();

		// Go through all the labels
		foreach (var label in labels)
		{
			// Create a replacement for the label
			var replacement = implementation.CreateLabel();

			// Find all the jumps which use the current label and update them to use the replacement
			for (var i = jumps.Count - 1; i >= 0; i--)
			{
				var jump = jumps[i];

				if (jump.Label != label.Label) continue;

				jump.Label = replacement;
				jumps.RemoveAt(i);
			}
			
			label.Label = replacement;
		}
	}
}