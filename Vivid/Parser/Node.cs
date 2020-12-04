using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public sealed class NodeEnumerator : IEnumerator, IEnumerator<Node>
{
	private Node? Position { get; set; }
	private Node? Next { get; set; }

	public NodeEnumerator(Node root)
	{
		Position = null;
		Next = root.First;
	}

	public bool MoveNext()
	{
		if (Next == null)
		{
			return false;
		}

		Position = Next;
		Next = Position.Next;
		return true;
	}

	public void Reset()
	{
		if (Position == null)
		{
			return;
		}

		while (Position.Previous != null)
		{
			Position = Position.Previous;
		}

		Next = Position;
		Position = null;
	}

	public void Dispose()
	{
		Position = null;
		Next = null;
	}

	public Node Current
	{
		get => Position ?? throw new InvalidOperationException("Node enumerator out of bounds");
	}

	object IEnumerator.Current
	{
		get
		{
			return Position!;
		}
	}
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1710")]
public class Node : IEnumerable, IEnumerable<Node>
{
	public Position? Position { get; set; }

	public Node? Parent { get; set; }
	public IEnumerable<Node> Path => new List<Node> { this }.Concat(Parent != null ? Parent.Path : new List<Node>());

	public Node? Previous { get; private set; }
	public Node? Next { get; private set; }

	public Node? First { get; protected set; }
	public Node? Last { get; protected set; }

	public bool IsEmpty => First == null;

	public bool Is(NodeType type)
	{
		return GetNodeType() == type;
	}

	public bool Is(params NodeType[] types)
	{
		var actual = GetNodeType();
		return types.Any(type => actual == type);
	}

	public T To<T>() where T : Node
	{
		return (T)this ?? throw new ApplicationException($"Could not convert 'Node' to '{typeof(T).Name}'");
	}

	public new Type GetType()
	{
		return (this as IType)?.GetType() ?? throw new ApplicationException($"Could not get type from {Enum.GetName(typeof(NodeType), GetNodeType())}");
	}

	public Type? TryGetType()
	{
		return (this as IType ?? throw new InvalidOperationException("Tried to get type from a node which did not represent a typed object")).GetType();
	}

	public Node? FindParent(Predicate<Node> filter)
	{
		if (Parent == null)
		{
			return null;
		}

		return filter(Parent) ? Parent : Parent.FindParent(filter);
	}

	public List<Node> FindParents(Predicate<Node> filter)
	{
		var result = new List<Node>();

		if (Parent == null)
		{
			return result;
		}

		if (filter(Parent))
		{
			result.Add(Parent);
		}

		result.AddRange(Parent.FindParents(filter));

		return result;
	}

	public IContext FindContext()
	{
		return (IContext)FindParent(p => p is IContext)!;
	}

	public Context GetParentContext()
	{
		return FindContext().GetContext();
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

	public List<Node> FindTop(Predicate<Node> filter)
	{
		var nodes = new List<Node>();

		foreach (var iterator in this)
		{
			if (filter(iterator))
			{
				nodes.Add(iterator);
			}
			else
			{
				nodes.AddRange(iterator.FindTop(filter));
			}
		}

		return nodes;
	}

	public List<Node> FindChildren(Predicate<Node> filter)
	{
		var nodes = new List<Node>();
		var iterator = First;

		while (iterator != null)
		{
			if (filter(iterator))
			{
				nodes.Add(iterator);
			}

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

	public void Insert(Node node)
	{
		if (Parent == null)
		{
			throw new ApplicationException("Tried to insert node but the operating node did not have parent");
		}

		Parent.Insert(this, node);
	}

	public void InsertChildren(Node children)
	{
		var iterator = children.First;

		while (iterator != null)
		{
			var next = iterator.Next;
			Insert(iterator);
			iterator = next;
		}
	}

	public void Insert(Node? position, Node child)
	{
		if (position == null)
		{
			Add(child);
			return;
		}

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

	public bool ReplaceWithChildren(Node children)
	{
		if (Parent == null)
		{
			throw new ApplicationException("Tried to replace a node with children nodes but the target node did not have a parent node");
		}

		var iterator = children.First;

		while (iterator != null)
		{
			var next = iterator.Next;
			Parent.Insert(this, iterator);
			iterator = next;
		}

		return Remove();
	}

	public void Merge(Node node)
	{
		foreach (var iterator in node)
		{
			Add(iterator);
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

	public static Node? GetSharedParent(Node a, Node b)
	{
		var x = a.Path.Reverse().ToArray();
		var y = b.Path.Reverse().ToArray();

		var i = 0;
		var count = Math.Min(x.Length, y.Length);

		for (; i < count; i++)
		{
			if (x[i] != y[i])
			{
				break;
			}
		}

		// Second condition: This scenario means that one of the nodes is parent of the other
		if (i == 0 || i == count)
		{
			return null;
		}

		return x[i - 1];
	}

	public static Node[]? GetNodesUnderSharedParent(Node a, Node b)
	{
		var x = a.Path.Reverse().ToArray();
		var y = b.Path.Reverse().ToArray();

		var i = 0;
		var count = Math.Min(x.Length, y.Length);

		for (; i < count; i++)
		{
			if (x[i] != y[i])
			{
				break;
			}
		}

		if (i == 0)
		{
			return Array.Empty<Node>();
		}

		if (i == count)
		{
			// This scenario means that one of the nodes is parent of the other
			return null;
		}

		return new[] { x[i], y[i] };
	}

	/// <summary>
	/// Returns whether this node is placed before the specified node
	/// </summary>
	/// <param name="other">The node used for comparison</param>
	/// <returns>True if this node is before the specified node, otherwise false</returns>
	public bool IsBefore(Node other)
	{
		var positions = GetNodesUnderSharedParent(other, this);

		if (positions == null)
		{
			return false;
		}

		if (positions.Length == 0)
		{
			throw new ApplicationException("Tried to resolve whether a node is before another but they did not have a shared parent");
		}

		// If this node is after the specified position node (other), the position node can be found by iterating backwards
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
				return false;
			}

			iterator = iterator.Previous;
		}

		return true;
	}

	/// <summary>
	/// Returns whether this node is placed after the specified node
	/// </summary>
	/// <param name="other">The node used for comparison</param>
	/// <returns>True if this node is after the specified node, otherwise false</returns>
	public bool IsAfter(Node other)
	{
		var positions = GetNodesUnderSharedParent(other, this);

		if (positions == null)
		{
			return false;
		}

		if (positions.Length == 0)
		{
			throw new ApplicationException("Tried to resolve whether a node is before another but they did not have a shared parent");
		}

		// If this node is after the specified position node (other), the position node can be found by iterating backwards
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

	/// <summary>
	/// Returns whether this node is under the specified node
	/// </summary>
	public bool IsUnder(Node node)
	{
		var iterator = Parent;

		while (iterator != node && iterator != null)
		{
			iterator = iterator.Parent;
		}

		return iterator == node;
	}

	/// <summary>
	/// Returns whether this node is between the two specified nodes in the node tree
	/// </summary>
	public bool IsBetween(Node start, Node end)
	{
		return IsBefore(end) && IsAfter(start);
	}

	public Node? GetLeftWhile(Predicate<Node> filter)
	{
		if (First == null)
		{
			return null;
		}

		var iterator = First;

		while (iterator.First != null && filter(iterator))
		{
			iterator = iterator.First;
		}

		return iterator;
	}

	public Node? GetRightWhile(Predicate<Node> filter)
	{
		if (Last == null)
		{
			return null;
		}

		var iterator = Last;

		while (iterator.Last != null && filter(iterator))
		{
			iterator = iterator.Last;
		}

		return iterator;
	}

	public Node? GetBottomLeft()
	{
		if (First == null)
		{
			return null;
		}

		var iterator = First;

		while (iterator.First != null)
		{
			iterator = iterator.First;
		}

		return iterator;
	}

	public Node? GetBottomRight()
	{
		if (Last == null)
		{
			return null;
		}

		var iterator = Last;

		while (iterator.Last != null)
		{
			iterator = iterator.Last;
		}

		return iterator;
	}

	public Node Clone()
	{
		var result = (Node)MemberwiseClone();
		result.First = null;
		result.Last = null;

		var iterator = First;

		while (iterator != null)
		{
			result.Add(iterator.Clone());
			iterator = iterator.Next;
		}

		return result;
	}

	public virtual NodeType GetNodeType()
	{
		return NodeType.NORMAL;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return (IEnumerator)GetEnumerator();
	}

	IEnumerator<Node> IEnumerable<Node>.GetEnumerator()
	{
		return (IEnumerator<Node>)GetEnumerator();
	}

	public NodeEnumerator GetEnumerator()
	{
		return new NodeEnumerator(this);
	}

	public override bool Equals(object? other)
	{
		if (other is Node node && GetNodeType() == node.GetNodeType())
		{
			var a = Count();
			var b = node.Count();

			if (a != b)
			{
				return false;
			}

			var i = First;
			var j = node.First;

			while (i != null)
			{
				if (!i.Equals(j))
				{
					return false;
				}

				i = i.Next;
				j = j.Next;
			}

			return true;
		}

		return false;
	}

	public override int GetHashCode()
	{
		var hash = new HashCode();
		hash.Add(GetNodeType());

		foreach (var iterator in this)
		{
			hash.Add(iterator);
		}

		return hash.ToHashCode();
	}
}
