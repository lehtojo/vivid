using System.Text;

public class Builder
{
	private StringBuilder Buffer = new StringBuilder();

	public Builder(string text = "")
	{
		Append(text);
	}

	public Builder Comment(string comment)
	{
		Buffer = Buffer.Append("; ").Append(comment).Append("\n");
		return this;
	}

	public Builder Comment(string format, params object[] args)
	{
		Buffer = Buffer.Append("; ").Append(string.Format(format, args)).Append("\n");
		return this;
	}

	public Builder Append(string text)
	{
		Buffer = Buffer.Append(text).Append("\n");
		return this;
	}

	public Builder Append(string format, params object[] args)
	{
		Buffer = Buffer.Append(string.Format(format, args)).Append("\n");
		return this;
	}

	public override string ToString()
	{
		return Buffer.ToString() + "\n";
	}
}