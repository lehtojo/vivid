using System;

public class Zigzag
{
	public static void Main(string[] arguments)
	{
		DateTime start = DateTime.Now;

		// Configure the flow of the compiler
		Chain chain = new Chain
		(
			typeof(ConfigurationPhase),
			typeof(FilePhase),                                
            typeof(LexerPhase),
			typeof(ParserPhase),
			typeof(ResolverPhase),
			typeof(AssemblerPhase)
		);
        
        // Pack the program arguments in the chain
        Bundle bundle = new Bundle();
		bundle.Put("arguments", arguments);

        // Execute the chain
        chain.Execute(bundle);

		DateTime end = DateTime.Now;

		Console.WriteLine((end - start).TotalMilliseconds);
	}
}
