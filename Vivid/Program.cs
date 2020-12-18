using System;

public static class Program
{
	public static void Main(string[] arguments)
	{
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
		Environment.Exit(chain.Execute(bundle) ? 0 : 1);
	}
}
