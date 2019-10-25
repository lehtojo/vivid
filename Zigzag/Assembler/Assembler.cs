using System.Text;
public class Assembler
{
	private const string SECTION_TEXT = "section .text" + "\n" +
										"extern _ExitProcess@4" + "\n" +
										"" + "\n" +
										"global main" + "\n" +
										"main:" + "\n" +
										"call function_run" + "\n" +
										"" + "\n" +
										"push 0" + "\n" +
										"call _ExitProcess@4" + "\n" +
										"add esp, 4" + "\n" +
										"ret" + "\n" +
										"" + "\n" +
										"extern function_allocate" + "\n" +
										"extern function_integer_power" + "\n\n";

	private const string SECTION_DATA = "section .data";

	public static string Build(Node root, Context context)
	{
		Builder text = new Builder(SECTION_TEXT);
		Builder data = Assembler.Data(root);

		Node iterator = root.First;

		while (iterator != null)
		{
			if (iterator.GetNodeType() == NodeType.TYPE_NODE)
			{
				text.Append(Assembler.Build((TypeNode)iterator));
			}
			else if (iterator.GetNodeType() == NodeType.FUNCTION_NODE)
			{
				text.Append(Functions.Build((FunctionNode)iterator));
			}
			else if (iterator.GetNodeType() == NodeType.VARIABLE_NODE)
			{
				data.Append(Assembler.Build((VariableNode)iterator));
			}

			iterator = iterator.Next;
		}

		return text + "\n" + data + "\n";
	}

	private static Builder Data(Node root)
	{
		Builder bss = new Builder(SECTION_DATA);
		Assembler.Data(root, bss, 1);
		return bss;
	}

	private static int Data(Node root, Builder builder, int i)
	{
		Node iterator = root.First;

		while (iterator != null)
		{
			if (iterator.GetNodeType() == NodeType.STRING_NODE)
			{
				string label = "S" + (i++);
				builder.Append(Strings.Build((StringNode)iterator, label));
			}
			else
			{
				i = Assembler.Data(iterator, builder, i);
			}

			iterator = iterator.Next;
		}

		return i;
	}

	private static string Build(TypeNode node)
	{
		StringBuilder text = new StringBuilder();
		Node iterator = node.First;

		while (iterator != null)
		{
			if (iterator.GetNodeType() == NodeType.TYPE_NODE)
			{
				text = text.Append(Assembler.Build((TypeNode)iterator));
			}
			else if (iterator.GetNodeType() == NodeType.FUNCTION_NODE)
			{
				text = text.Append(Functions.Build((FunctionNode)iterator));
			}

			iterator = iterator.Next;
		}

		return text.ToString();
	}

	private const string DATA = "{0} {1} 0";

	private static string Build(VariableNode node)
	{
		Variable variable = node.Variable;
		string operand = Size.Get(variable.Type.Size).Data;

		return string.Format(DATA, variable.Name, operand);
	}
}