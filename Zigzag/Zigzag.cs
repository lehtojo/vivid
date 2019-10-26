using System;

public class Zigzag
{
	public static void Main(string[] arguments)
	{
		var start = DateTime.Now;

		// Configure the flow of the compiler
		var chain = new Chain
		(
			typeof(ConfigurationPhase),
			typeof(FilePhase),
			typeof(LexerPhase),
			typeof(ParserPhase),
			typeof(ResolverPhase),
			typeof(AssemblerPhase)
		);

		// Pack the program arguments in the chain
		var bundle = new Bundle();
		bundle.Put("arguments", arguments);

		// Execute the chain
		chain.Execute(bundle);

		var end = DateTime.Now;

		if (bundle.Contains("time"))
		{
			Console.WriteLine($"Time: {(int)(end - start).TotalMilliseconds} ms");
		}
	}
}
