using System;
using System.Linq;
using System.Globalization;

public static class Program
{
	public static void Main(string[] arguments)
	{
		CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
		CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

		Settings.Initialize();

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

		Settings.Arguments = arguments.ToList();

		// Execute the chain
		Environment.Exit(chain.Execute() ? 0 : 1);
	}
}