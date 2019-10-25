public class Label
{
	public Context Context { get; private set; }
	public string Name { get; private set; }

	public Label(Context context, string name)
	{
		Context = context;
		Name = name;

		context.Declare(this);
	}
}