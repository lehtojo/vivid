using System.Collections.Generic;

public class Option
{
	public Pattern Pattern { get; private set; }
	private List<int> Optionals { get; set; }

	public List<int> Missing => new List<int>(Optionals);

	public Option(Pattern pattern, List<int> optionals)
	{
		Pattern = pattern;
		Optionals = optionals;
	}
}

public class Patterns
{
	public Dictionary<int, Patterns> Branches { get; private set; } = new Dictionary<int, Patterns>();
	public List<Option> Options { get; private set; } = new List<Option>();

	public bool HasOptions => Options.Count > 0;
	public bool HasBranches => Branches.Count > 0;

	private Patterns ForceGetBranch(int branch)
	{
		if (Branches.TryGetValue(branch, out Patterns? patterns))
		{
			return patterns;
		}

		patterns = new Patterns();
		Branches.Add(branch, patterns);

		return patterns;
	}

	private void Grow(Pattern pattern, List<int> path, List<int> missing, int position)
	{
		if (position >= path.Count)
		{
			Options.Add(new Option(pattern, missing));
			return;
		}

		var mask = path[position];

		if (Flag.Has(mask, TokenType.OPTIONAL))
		{
			var variation = new List<int>(missing)
			{
				position
			};

			Grow(pattern, path, variation, position + 1);
		}

		for (int i = 0; i < TokenType.COUNT; i++)
		{
			var type = 1 << i;

			if (Flag.Has(mask, type))
			{
				var branch = ForceGetBranch(type);
				branch.Grow(pattern, path, missing, position + 1);
			}
		}
	}

	public Patterns? Navigate(int type)
	{
		return Branches.GetValueOrDefault(type, null);
	}

	public static readonly Patterns Root = new Patterns();

	private static void Add(Pattern pattern)
	{
		Root.Grow(pattern, pattern.GetPath(), new List<int>(), 0);
	}

	static Patterns()
	{
		Add(new AssignPattern());
		Add(new CastPattern());
		Add(new ConstructorPattern());
		Add(new ElseIfPattern());
		Add(new ElsePattern());
		Add(new ExtensionFunction());
		Add(new FunctionPattern());
		Add(new IfPattern());
		Add(new ImportPattern());
		Add(new InheritancePattern());
		Add(new LambdaPattern());
		Add(new PostIncrementAndDecrementPattern());
		Add(new PreIncrementAndDecrementPattern());
		Add(new LinkPattern());
		Add(new ListPattern());
		Add(new CommandPattern());
		Add(new LoopPattern());
		Add(new SpecificModificationPattern());
		Add(new NotPattern());
		Add(new OffsetPattern());
		Add(new OperatorPattern());
		Add(new ReturnPattern());
		Add(new ModifierSectionPattern());
		Add(new SectionModificationPattern());
		Add(new ShortFunctionPattern());
		Add(new SingletonPattern());
		Add(new TemplateFunctionCallPattern());
		Add(new TemplateFunctionPattern());
		Add(new TemplateTypePattern());
		Add(new TypePattern());
		Add(new UnarySignPattern());
		Add(new VariableDeclarationPattern());
	}
}
