public class ErrorNode : Node, IResolvable
{
	public Status Error { get; }

	public ErrorNode(Status error)
	{
		Instance = NodeType.ERROR;
		Error = error;
	}

	public Node? Resolve(Context context) => null;
	public Status GetStatus() => Error;

	public override string ToString() => "Error " + Error.Message;
}