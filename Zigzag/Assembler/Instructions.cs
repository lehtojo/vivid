using System.Text;

public class Instructions
{
	private StringBuilder Builder = new StringBuilder();
	public Reference Reference { get; set; }

	public static Instructions GetReference(Reference reference)
	{
		return new Instructions().SetReference(reference);
	}

	public Instructions Comment(string comment)
	{
		Builder = Builder.Append("; ").Append(comment).Append("\n");
		return this;
	}

	public Instructions Label(string name)
	{
		Builder = Builder.Append(name).Append(":\n");
		return this;
	}

	public Instructions Append(string raw)
	{
		Builder = Builder.Append(raw).Append("\n");
		return this;
	}

	public Instructions Append(string format, params object[] args)
	{
		Builder = Builder.Append(string.Format(format, args)).Append("\n");
		return this;
	}

	public Instructions Append(Instruction instruction)
	{
		Builder = Builder.Append(instruction).Append("\n");
		return this;
	}

	public Instructions Append(params Instructions[] instructions)
	{
		foreach (Instructions i in instructions)
		{
			Builder = Builder.Append(i);
		}

		return this;
	}

	public void Break()
	{
		Builder = Builder.Append('\n');
	}

	public Instructions SetReference(Reference reference)
	{
		Reference = reference;
		return this;
	}

	public override string ToString()
	{
		return Builder.ToString();
	}
}