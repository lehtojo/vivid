using System.Collections.Generic;
using System;

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
	private List<Exception> Errors = new List<Exception>();

	private void ParseMembers(Node root)
	{
		Node node = root.First;

		while (node != null)
		{
			if (node.GetNodeType() == NodeType.TYPE_NODE)
			{
				TypeNode type = (TypeNode)node;

				Async(() =>
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

	public void ParseFunctions(Node parent)
	{
		Node node = parent.First;

		while (node != null)
		{
			if (node.GetNodeType() == NodeType.TYPE_NODE)
			{
				TypeNode type = (TypeNode)node;
				ParseFunctions(type);
			}
			else if (node.GetNodeType() == NodeType.FUNCTION_NODE)
			{
				FunctionNode function = (FunctionNode)node;

				Async(() =>
				{
					try
					{
						function.Parse();
					}
					catch (Exception e)
					{
						Errors.Add(e);
					}

					return Status.OK;
				});
			}

			node = node.Next;
		}
	}

	public override Status Execute(Bundle bundle)
	{
		List<Token>[] files = bundle.Get<List<Token>[]>("input_file_tokens", null);

		if (files == null)
		{
			return Status.Error("Nothing to parse");
		}

		Parse[] parses = new Parse[files.Length];

		// Form the 'hull' of the code
		for (int i = 0; i < files.Length; i++)
		{
			int index = i;

			Async(() =>
			{
				List<Token> tokens = files[index];

				Node node = new Node();
				Context context = Parser.Initialize();

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

		// Parse types, subtypes and their members
		for (int i = 0; i < files.Length; i++)
		{
			int index = i;

			Async(() =>
			{
				ParseMembers(parses[index].Node);
				return Status.OK;
			});
		}

		Sync();

		// Parse types, subtypes and their members
		for (int i = 0; i < files.Length; i++)
		{
			int index = i;

			Async(() =>
			{
				ParseFunctions(parses[index].Node);
				return Status.OK;
			});
		}

		Sync();

		// Merge all parsed files
		Context context = new Context();
		Node root = new Node();

		foreach (Parse parse in parses)
		{
			context.Merge(parse.Context);
			root.Merge(parse.Node);
		}

		bundle.Put("parse", new Parse(context, root));

		return Status.OK;
	}
}