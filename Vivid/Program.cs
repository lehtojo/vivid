using System;
using System.Globalization;

public static class Program
{
	public static void Main(string[] arguments)
	{
		CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
		CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

		// Configure the flow of the compiler
		var chain = new Chain
		(
			typeof(ConfigurationPhase),
			typeof(ServicePhase),
			typeof(FilePhase),
			typeof(AssemblerPhase),
			typeof(LexerPhase),
			typeof(ParserPhase),
			typeof(ResolverPhase),
			typeof(AssemblyPhase)
		);
		
		// Pack the program arguments in the chain
		var bundle = new Bundle();
		bundle.Put(ConfigurationPhase.ARGUMENTS, arguments);

		// Execute the chain
		Environment.Exit(chain.Execute(bundle) ? 0 : 1);
	}
}