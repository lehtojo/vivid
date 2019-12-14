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

public static class Assembler
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

	private struct Fragment
	{
		public string Content { get; set; }
		public bool Data { get; set; }
	}

	public static string Build(Context context)
	{
		var text = new Builder(SECTION_TEXT);
		var data = Assembler.Data(context);

		var fragments = new List<Task<Fragment>>();

		foreach (var type in context.Types.Values)
		{
			text.Append(Build(type));
		}

		foreach (var function in context.Functions.Values)
		{
			foreach (var overload in function.Overloads)
			{
				text.Append(Functions.Build(overload));
			}
		}

		//fragments.Wait();
		//fragments.Where(t => !t.Result.Data).Each(t => text.Append(t.Result.Content));
		//fragments.Where(t => t.Result.Data).Each(t => data.Append(t.Result.Content));*/

		return text + "\n" + data + "\n";
	}

	private static Builder Data(Context context)
	{
		var builder = new Builder(SECTION_DATA);
		var i = 0;

		foreach (var variable in context.Variables.Values)
		{
			builder.Append(Build(variable));
		}

		foreach (var function in context.Functions.Values)
		{
			foreach (var overload in function.Overloads)
			{
				foreach (var implementation in overload.Implementations)
				{
					if (implementation != null && implementation.Node != null)
					{
						i = Assembler.Data(implementation.Node, builder, i);
					}
				}
			}
		}

		return builder;
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

	private static string Build(Type type)
	{
		var text = new StringBuilder();
		var fragments = new List<Task<string>>();

		foreach (var subtype in type.Types.Values)
		{
			text.Append(Build(subtype));
		}

		foreach (var overload in type.Constructors.Overloads)
		{
			var constructor = overload as Constructor;

			if (!constructor.IsDefault)
			{
				text.Append(Functions.Build(constructor));
			}
		}

		foreach (var destructor in type.Destructors.Overloads)
		{
			text.Append(Functions.Build(destructor));
		}

		foreach (var function in type.Functions.Values)
		{
			foreach (var overload in function.Overloads)
			{
				text.Append(Functions.Build(overload));
			}
		}

		fragments.Wait().Each(t => text.Append(t.Result));

		return text.ToString();
	}

	private const string DATA = "{0} {1} 0";

	private static string Build(Variable variable)
	{
		variable.Category = VariableCategory.GLOBAL;
		variable.Type = Types.NORMAL;
		var operand = Size.Get(variable.Type.Size).Data;
		return string.Format(DATA, variable.Name, operand);
	}
}