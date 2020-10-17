public class CapturedVariable : Variable
{
	public Variable Captured { get; private set; }

	public static CapturedVariable Create(Context context, Variable captured, bool declare = true)
	{
		return new CapturedVariable(context, captured, declare);
	}

	public CapturedVariable(Context context, Variable captured, bool declare = true)
	   : base(context, captured.Type, captured.Category, captured.Name, captured.Modifiers, declare)
	{
		Captured = captured;
	}
}