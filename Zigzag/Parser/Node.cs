using System;
using System.Collections.Generic;
using System.Linq;

public class Node
{
	public Node? Parent { get; private set; }
	public IEnumerable<Node> Path => Parent != null ? new List<Node> { Parent }.Concat(Parent.Path) : new List<Node>(); 

	public Node? Previous { get; private set; }
	public Node? Next { get; private set; }

	public Node? First { get; protected set; }
	public Node? Last { get; protected set; }

	public bool Is(NodeType type)
	{
		return GetNodeType() == type;
	}

	public T To<T>() where T : Node
	{
		return (T)this ?? throw new ApplicationException($"Couldn't convert 'Node' to '{typeof(T).Name}'");
	}

	public new Type GetType()
	{
		return (this as IType)?.GetType() ?? throw new ApplicationException($"Couldn't get type from {Enum.GetName(typeof(NodeType), GetNodeType())}");
	}

	public Type? TryGetType()
	{
		return (this as IType ?? throw new InvalidOperationException("Tried to get type from a node which didn't represent a typed object")).GetType();
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

	/// <summary>
	/// Returns the number of children this node holds
	/// </summary>
	public int Count()
	{
		var iterator = First;
		var count = 0;

		while (iterator != null)
		{
			count++;
			iterator = iterator.Next;
		}

		return count;
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

	public bool Remove()
	{
		return Parent != null && Parent.Remove(this);
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

		if (First == child)
		{
			First = right;
		}
		
		if (Last == child)
		{
			Last = left;
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
	
	private static Node[] GetNodesUnderSharedParent(Node a, Node b)
	{
		var x = a.Path.Reverse().ToArray();
		var y = b.Path.Reverse().ToArray();

		var i = 0;
		var count = Math.Min(x.Length, y.Length);

		for (; i < count; i++)
		{
			if (x[i] != y[i]) break;
		}

		if (i == 0)
		{
			return Array.Empty<Node>();
		}
		
		if (i == count)
		{
			// This scenario means that one of the nodes is parent of the other
			/// TODO: Remove after this function is proven to work (Identical return)
			return Array.Empty<Node>();
		}

		return new [] { x[i], y[i] };
	}
	
	/// <summary>
	/// Returns whether this node is placed before the specified node
	/// </summary>
	/// <param name="other">The node used for comparison</param>
	/// <returns>True if this node is before the specified node, otherwise false</returns>
	public bool IsBefore(Node other)
	{
		var positions = GetNodesUnderSharedParent(other, this);

		// If the given node is before the given position node, the target node can be found be iterating backwards
		var iterator = (Node?)positions[1];
		var target = positions[0];

		if (target == iterator)
		{
			return false;
		}

		// Iterate backwards and try to find the target node
		while (iterator != null)
		{
			if (iterator == target)
			{
				return true;
			}

			iterator = iterator.Previous;
		}

		return false;
	}

	public virtual NodeType GetNodeType()
	{
		return NodeType.NORMAL;
	}

	public override bool Equals(object? obj)
	{
		return obj is Node node &&
			   GetNodeType() == node.GetNodeType() &&
			   EqualityComparer<Node?>.Default.Equals(Next, node.Next) &&
			   EqualityComparer<Node?>.Default.Equals(First, node.First);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Next, First);
	}
}
