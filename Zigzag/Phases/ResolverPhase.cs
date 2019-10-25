using System.Collections.Generic;
using System;

public class ResolverPhase : Phase
{
	private static void Complain(List<Exception> errors)
	{
		foreach (Exception error in errors)
		{
			Console.Error.WriteLine($"Error: {error.Message}\n");
		}
	}

	public override Status Execute(Bundle bundle)
	{
		Parse parse = bundle.Get<Parse>("parse", null);

		if (parse == null)
		{
			return Status.Error("Nothing to resolve");
		}

		List<Exception> errors = new List<Exception>();

		Context context = parse.Context;
		Node node = parse.Node;

		// Try to resolve any problems in the node tree
		Resolver.Resolve(context, node, errors);

		if (errors.Count > 0)
		{
			int previous = errors.Count;
			int count;

			while (true)
			{
				errors.Clear();

				// Try to resolve any problems in the node tree
				Resolver.Resolve(context, node, errors);

				count = errors.Count;

				// Try again only if the amount of errors has decreased
				if (count >= previous)
				{
					break;
				}

				previous = count;
			}
		}

		if (errors.Count > 0)
		{
			Complain(errors);
			return Status.Error("Compilation error");
		}

		Processor.Process(node);
		Aligner.Align(context);

		return Status.OK;
	}
}