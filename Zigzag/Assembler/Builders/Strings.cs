public class Strings
{
	public static string Build(StringNode @string, string label)
	{
		@string.Identifier = label;
		return $"{label} db '{@string.Text}', 0";
	}
}