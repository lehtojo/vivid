using System;
using System.Collections.Generic;

public class Node
{
	public Node? Parent { get; private set; }

	public Node? Previous { get; private set; }
	public Node? Next { get; private set; }

	public Node? First { get; protected set; }
	public Node? Last { get; protected set; }

	public bool Is(NodeType type)
	{
		return GetNodeType() == type;
	}

	public List<T> Select<T>(Func<Node, T> selector)
	{
		var result = new List<T>();
		var iterator = First;

		while (iterator != null)
		{
			result.Add(selector(iterator));

			var subresult = iterator.Select(selector);
			result.AddRange(subresult);

			iterator = iterator.Next;
		}

		return result;
	}

	public Node? FindParent(Predicate<Node> filter)
	{
		if (Parent == null)
		{
			return null;
		}

		return filter(Parent) ? Parent : Parent.FindParent(filter);
	}

	public Node? Find(Predicate<Node> filter)
	{
		var iterator = First;

		while (iterator != null)
		{
			if (filter(iterator))
			{
				return iterator;
			}

			var node = iterator.Find(filter);

			if (node != null)
			{
				return node;
			}

			iterator = iterator.Next;
		}

		return null;
	}

	public List<Node> FindAll(Predicate<Node> filter)
	{
		var nodes = new List<Node>();
		var iterator = First;

		while (iterator != null)
		{
			if (filter(iterator))
			{
				nodes.Add(iterator);
			}

			var result = iterator.FindAll(filter);
			nodes.AddRange(result);

			iterator = iterator.Next;
		}

		return nodes;
	}

	public void Insert(Node position, Node child)
	{
		if (position == First)
		{
			First = child;
		}

		var left = position.Previous;

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

		var left = child.Previous;
		var right = child.Next;

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
		// No need to replace if the replacement is this node
		if (node == this)
		{
			return;
		}

		var iterator = First;

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

		node.Parent = Parent;
		node.Previous = Previous;
		node.Next = Next;
	}

	public void Merge(Node node)
	{
		var iterator = node.First;

		while (iterator != null)
		{
			var Next = iterator.Next;
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
		Parent?.Remove(this);
		return this;
	}

	public virtual NodeType GetNodeType()
	{
		return NodeType.NORMAL;
	}
}
