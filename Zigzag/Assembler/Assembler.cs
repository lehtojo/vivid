using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

public static class Lists
{
	public static IEnumerable<T> Each<T>(this IEnumerable<T> items, Action<T> action)
	{
		foreach (T item in items)
		{
			action(item);
		}

		return items;
	}

	public static IEnumerable<Task<T>> Wait<T>(this IEnumerable<Task<T>> items)
	{
		foreach (var item in items)
		{
			item.Wait();
		}

		return items;
	}
}

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

	private struct Fragement
	{
		public string Content { get; set; }
		public bool Data { get; set; }
	}

	public static string Build(Node root, Context context)
	{
		var text = new Builder(SECTION_TEXT);
		var data = Assembler.Data(root);

		var iterator = root.First;

		var fragments = new List<Task<Fragement>>();

		while (iterator != null)
		{
			if (iterator.GetNodeType() == NodeType.TYPE_NODE)
			{
				var type = iterator as TypeNode;
				fragments.Add(Task.Run(() => new Fragement { Content = Assembler.Build(type), Data = false }));
			}
			else if (iterator.GetNodeType() == NodeType.FUNCTION_NODE)
			{
				var function = iterator as FunctionNode;
				fragments.Add(Task.Run(() => new Fragement { Content = Functions.Build(function), Data = false }));
			}
			else if (iterator.GetNodeType() == NodeType.VARIABLE_NODE)
			{
				var variable = iterator as VariableNode;
				fragments.Add(Task.Run(() => new Fragement { Content = Assembler.Build(variable), Data = true }));
			}

			iterator = iterator.Next;
		}

		fragments.Wait();
		fragments.Where(t => !t.Result.Data).Each(t => text.Append(t.Result.Content));
		fragments.Where(t => t.Result.Data).Each(t => data.Append(t.Result.Content));

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
		var text = new StringBuilder();
		var iterator = node.First;

		var fragments = new List<Task<string>>();

		while (iterator != null)
		{
			if (iterator.GetNodeType() == NodeType.TYPE_NODE)
			{
				var type = iterator as TypeNode;
				fragments.Add(Task.Run(() => Assembler.Build(type)));
			}
			else if (iterator.GetNodeType() == NodeType.FUNCTION_NODE)
			{
				var function = iterator as FunctionNode;
				fragments.Add(Task.Run(() => Functions.Build(function)));
			}

			iterator = iterator.Next;
		}

		fragments.Wait().Each(t => text.Append(t.Result));

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