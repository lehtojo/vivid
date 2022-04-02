using System.Text.Json;
using System.Linq;

public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
	public static SnakeCaseNamingPolicy Instance { get; } = new SnakeCaseNamingPolicy();

	public override string ConvertName(string name)
	{
		return string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLowerInvariant();
	}
}