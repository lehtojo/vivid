public class Node
{
	public Node Parent { get; private set; }

	public Node Previous { get; private set; }
	public Node Next { get; private set; }

	public Node First { get; protected set; }
	public Node Last { get; protected set; }

	public void Insert(Node position, Node child)
	{
		if (position == First)
		{
			First = child;
		}

		Node left = position.Previous;

		if (left != null)
		{
			left.Next = child;
		}

		position.Previous = child;

		if (child.Parent != null)
		{
			child.Parent.Remove(child);
		}

		child.Parent = position.Parent;
		child.Previous = left;
		child.Next = position;
	}

	public void Add(Node child)
	{
		child.Parent = this;
		child.Previous = Last;
		child.Next = null;

		if (First == null)
		{
			First = child;
		}

		if (Last != null)
		{
			Last.Next = child;
		}

		Last = child;
	}

	public bool Remove(Node child)
	{
		if (child.Parent != this)
		{
			return false;
		}

		Node left = child.Previous;
		Node right = child.Next;

		if (left != null)
		{
			left.Next = right;
		}

		if (right != null)
		{
			right.Previous = left;
		}

		return true;
	}

	public void Replace(Node node)
	{
		Node iterator = First;

		while (iterator != null)
		{
			iterator.Parent = node;
			iterator = iterator.Next;
		}

		if (Previous == null)
		{
			if (Parent != null)
			{
				Parent.First = node;
			}
		}
		else
		{
			Previous.Next = node;
		}

		if (Next == null)
		{
			if (Parent != null)
			{
				Parent.Last = node;
			}
		}
		else
		{
			Next.Previous = node;
		}

		node.Previous = Previous;
		node.Next = Next;
	}

	public void Merge(Node node)
	{
		Node iterator = node.First;

		while (iterator != null)
		{
			Node Next = iterator.Next;
			Add(iterator);
			iterator = Next;
		}

		node.Destroy();
	}

	public void Destroy()
	{
		Parent = null;
		Previous = null;
		Next = null;
		First = null;
		Last = null;
	}

	public Node Disconnect()
	{
		Parent.Remove(this);
		return this;
	}

	public virtual NodeType GetNodeType()
	{
		return NodeType.NORMAL;
	}
}
