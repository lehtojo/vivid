using System;
using System.Collections.Generic;

public class Parse
{
	public Context Context { get; private set; }
	public Node Node { get; private set; }

	public Parse(Context context, Node node)
	{
		Context = context;
		Node = node;
	}
}

public class ParserPhase : Phase
{
<<<<<<< HEAD
	private List<Exception> Errors { get; } = new List<Exception>();
=======
	private readonly List<Exception> Errors = new List<Exception>();
>>>>>>> ec8e325... Improved code quality and implemented basic support for operator overloading

	private void ParseMembers(Node root)
	{
		var node = root.First;

		while (node != null)
		{
			if (node.GetNodeType() == NodeType.TYPE_NODE)
			{
				var type = (TypeNode)node;

				Run(() =>
				{
					try
					{
						type.Parse();
					}
					catch (Exception e)
					{
						Errors.Add(e);
					}

					return Status.OK;
				});

				ParseMembers(type);
			}

			node = node.Next;
		}
	}
	public override Status Execute(Bundle bundle)
	{
		if (!bundle.Contains("input_file_tokens"))
		{
			return Status.Error("Nothing to parse");
		}

		var files = bundle.Get<List<Token>[]>("input_file_tokens");
		var parses = new Parse[files.Length];

		// Form the 'hull' of the code
		for (var i = 0; i < files.Length; i++)
		{
			var index = i;

			Run(() =>
			{
				var tokens = files[index];

				var node = new Node();
				var context = Parser.Initialize();

				try
				{
					Parser.Parse(node, context, tokens);
				}
				catch (Exception e)
				{
					return Status.Error(e.Message);
				}

				parses[index] = new Parse(context, node);

				return Status.OK;
			});
		}

		Sync();

		if (Failed)
		{
			return Status.Error(GetTaskErrors());
		}

		// Parse types, subtypes and their members
		for (int i = 0; i < files.Length; i++)
		{
			int index = i;

			Run(() =>
			{
				ParseMembers(parses[index].Node);
				return Status.OK;
			});
		}

		Sync();

		if (Failed)
		{
			return Status.Error(GetTaskErrors());
		}

		// Merge all parsed files
		var context = new Context();
		var root = new Node();

		foreach (var parse in parses)
		{
			context.Merge(parse.Context);
			root.Merge(parse.Node);
		}

		var function = context.GetFunction("run");

		if (function == null)
		{
			return Status.Error("Couldn't find function 'run'");
		}

		function.Overloads[0].Implement(new List<Type>());

		bundle.Put("parse", new Parse(context, root));

		return Status.OK;
	}
}