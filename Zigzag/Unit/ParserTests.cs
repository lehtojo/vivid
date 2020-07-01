using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Zigzag.Unit
{
	[TestFixture]
	class ParserTests
	{
		private class Configuration
		{
			private List<Token>? Tokens { get; set; }

			public Context Context { get; set; }
			public Node? Actual { get; set; }

			public Configuration(string text)
			{
				Context = Parser.Initialize();
				Tokens = Lexer.GetTokens(text);
			}

			public Configuration()
			{
				Context = Parser.Initialize();
			}

			public Configuration Local(string name, string type)
			{
				return Local(name, Context.GetType(type) ?? throw new ApplicationException($"Type '{type}' not found"));
			}

			public Configuration Local(string name, Type type)
			{
				return Declare(name, type, VariableCategory.LOCAL);
			}

			public Configuration Parameter(string name, string type)
			{
				return Parameter(name, Context.GetType(type) ?? throw new ApplicationException($"Type '{name}' not found"));
			}

			public Configuration Parameter(string name, Type type)
			{
				return Declare(name, type, VariableCategory.PARAMETER);
			}

			public Configuration Declare(string name, Type type, VariableCategory category)
			{
				Context.Declare(type, category, name);
				return this;
			}	

			public Configuration Type(string name)
			{
            _ = new Type(Context, name, AccessModifier.PUBLIC);
				return this;
			}

			public Type GetType(string name)
			{
				return Context.GetType(name) ?? throw new ApplicationException($"Type '{name}' not found");
			}

			public VariableNode this[string name]
			{
				get => new VariableNode(Context.GetVariable(name) ?? throw new ApplicationException($"Variable '{name}' not found"));
			}

			public NumberNode this[long constant]
			{
				get => new NumberNode(Format.INT64, constant);
			}

			public NumberNode this[int constant]
			{
				get => new NumberNode(Format.INT32, constant);
			}

			public NumberNode this[double constant]
			{
				get => new NumberNode(Format.DECIMAL, constant);
			}

			public void CompareTo(params Node[] nodes)
			{
				if (Actual == null)
				{
					Actual = Parser.Parse(Context, Tokens ?? throw new ApplicationException("Code was not given to the configuration"));
				}

				var expected = new Node();

				foreach (var node in nodes)
				{
					expected.Add(node);
				}

				Assert.AreEqual(expected, Actual);
			}
		}

		[TestCase]
		public void Parser_PrimitiveTypes()
		{
			new Configuration(string.Empty)
				.Local("a", "i8")
				.Local("b", "i16")
				.Local("c", "i32")
				.Local("d", "i64")
				.Local("e", "u8")
				.Local("f", "u16")
				.Local("g", "u32")
				.Local("h", "u64")
				.Local("i", "bool")
				.Local("j", "byte")
				.Local("k", "tiny")
				.Local("l", "small")
				.Local("m", "normal")
				.Local("n", "large")
				.Local("o", "decimal")
				.Local("p", "link");

			Assert.Pass();
		}

		[TestCase]
		public void Parser_SimpleMath()
		{
			var config = new Configuration("a = b / 7 + c * d + 3.14159")
				.Local("a", "num")
				.Local("b", "num")
				.Local("c", "num")
				.Local("d", "num");

			var expected = new OperatorNode
			(
				/* a = b / 7 + c * d + 3.14159 */
				Operators.ASSIGN,
				config["a"],
				new OperatorNode
				(
					/* b / 7 + c * d + 3.14159 */
					Operators.ADD,
					new OperatorNode
					(
						/* b / 7 + c * d */
						Operators.ADD,
						new OperatorNode
						(
							/* b / 7 */
							Operators.DIVIDE,
							config["b"],
							config[(long)7]
						),
						new OperatorNode
						(
							/* c * d */
							Operators.MULTIPLY,
							config["c"],
							config["d"]
						)
					),
					config[3.14159]
				)
			);

			config.CompareTo(expected);
		}

		[TestCase]
		public void Parser_ArrayAllocation()
		{
			var actual_config = new Configuration("cars = Vehicle[100]").Type("Vehicle");
			
			var expected_config = new Configuration().Type("Vehicle").Local("cars", "Vehicle");
			var bytes = expected_config.GetType("Vehicle").ReferenceSize * 100;
			var expected_tree = new OperatorNode
			(
				Operators.ASSIGN,
				expected_config["cars"],
				new ArrayAllocationNode
				(
					expected_config.GetType("Vehicle"),
					expected_config[bytes]
				)
			);

			actual_config.CompareTo(expected_tree);
		}

		[TestCase]
		public void Parser_VariableWithTypeDeclaration()
		{
			var actual_config = new Configuration("car: Vehicle").Type("Vehicle");
			
			var expected_config = new Configuration().Type("Vehicle").Local("car", "Vehicle");
			var expected_tree = expected_config["car"];

			actual_config.CompareTo(expected_tree);
		}
	}
}
