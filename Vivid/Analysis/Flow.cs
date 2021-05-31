using System;
using System.Collections.Generic;
using System.Linq;

public class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
{
	public bool Equals(T? x, T? y)
	{
		return ReferenceEquals(x, y);
	}

	public int GetHashCode(T x)
	{
		return HashCode.Combine(x);
	}
}

public class HashlessReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
{
	public bool Equals(T? x, T? y)
	{
		return ReferenceEquals(x, y);
	}

	public int GetHashCode(T x)
	{
		return 0;
	}
}

public class LoopDescriptor
{
	public Label Start { get; }
	public Label End { get; }

	public LoopDescriptor(Label start, Label end)
	{
		Start = start;
		End = end;
	}
}

public class Index : IComparable<Index>
{
	private int[] Values { get; set; }

	/// <summary>
	/// Returns whether the most significant index equals the maximum integer value
	/// </summary>
	public bool IsMax => Values[0] == int.MaxValue;

	/// <summary>
	/// Create an index which has one dimension whose value is the specified value
	/// </summary>
	public Index(int value = 0)
	{
		Values = new[] { value };
	}

	/// <summary>
	/// Create an index which has the specified dimensions
	/// </summary>
	public Index(int[] values)
	{
		Values = values;
	}

	/// <summary>
	/// Create an index which is greater than the specified index a and less than the specified index b.
	/// NOTE: Index a must be less than index b.
	/// </summary>
	public Index(Index? a, Index? b)
	{
		if (a == null && b == null)
		{
			Values = new[] { 0 };
			return;
		}

		if (a == null)
		{
			// Create a copy of the values
			Values = new int[b!.Values.Length];
			b!.Values.CopyTo(Values, 0);
			
			// Decrement the last dimension
			Values[Values.Length - 1]--;
			return;
		}

		if (b == null)
		{
			// Create a copy of the values
			Values = new int[a!.Values.Length];
			a!.Values.CopyTo(Values, 0);
			
			// Increment the last dimension
			Values[Values.Length - 1]++;
			return;
		}

#if _DEBUG
		if (a <= b) throw new ApplicationException("Specified indices were invalid");
#endif

		var i = a.Values;
		var j = b.Values;

		var x = i.Length;
		var y = j.Length;

		// If the indices have the same amount of dimensions, a new dimension must be created
		// Example:
		// 0.0.0.0 <- a
		// 0.0.0.0.0 <- this
		// 0.0.0.1 <- b
		if (x == y)
		{
			Values = new int[x + 1]; /// NOTE: The last value will be zero
			Array.Copy(i, Values, x);
		}
		else if (x > y)
		{
			// Example:
			// 0.0.0.0.0 <- a
			// 0.0.0.0.1 <- this
			// 0.0.0.1 <- b
			Values = new int[x];
			Values[x - 1] = i[x - 1] + 1;
			Array.Copy(i, Values, x - 1);
		}
		else
		{
			// Example:
			// 0.0.0.0 <- a
			// 0.0.0.0.-1 <- this
			// 0.0.0.0.0 <- b
			Values = new int[y];
			Values[y - 1] = j[y - 1] - 1;
			Array.Copy(j, Values, y - 1);
		}
	}

	/// <summary>
	/// Returns an index which is one increment larger in the least significant dimension
	/// </summary>
	public Index Next()
	{
		// Create a copy of the values
		var values = new int[Values.Length];
		Values.CopyTo(values, 0);

		// Increment the last dimension
		values[values.Length - 1]++;

		return new Index(values);
	}

	/// <summary>
	/// NOTE: Shared dimensions are marked with brackets.
	/// If shared dimensions are not equal, the index that has larger value in the same dimension as the other index, is larger than the other.
	/// The first dimension is the most significant and the last the least significant.
	/// Examples: [1] < [2], [1.1] < [1.2], [1.-2] < [1.-1], [1.2.1] > [1.1.2], [1.2.1] > [1.1.2].2
	/// If the shared dimensions are equal, the index who has more dimensions is always larger than the other.
	/// Examples: [0.0.0].0 > [0.0.0], [3.2.1.0] < [3.2.1.0].-1 
	/// </summary>
	public static int CompareTo(Index? x, Index? y)
	{
		if (x == null || y == null) return (x != null ? 1 : 0).CompareTo(y != null ? 1 : 0);

		var a = x.Values.Length;
		var b = y.Values.Length;
		var c = Math.Min(a, b);

		for (var i = 0; i < c; i++)
		{
			var d = x.Values[i] - y.Values[i];
			if (d == 0) continue;

			return Math.Sign(d);
		}

		return Math.Sign(a - b);
	}

	public static bool operator <(Index? a, Index? b) => CompareTo(a, b) < 0;
	public static bool operator >(Index? a, Index? b) => CompareTo(a, b) > 0;
	public static bool operator <=(Index? a, Index? b) => CompareTo(a, b) <= 0;
	public static bool operator >=(Index? a, Index? b) => CompareTo(a, b) >= 0;

	public override string ToString()
	{
		return string.Join('.', Values);
	}

	public static bool Equals(Index? a, Index? b)
	{
		if (ReferenceEquals(a, b)) return true;
		if (a == null || b == null || a.Values.Length != b.Values.Length) return false;
		
		for (var i = 0; i < a.Values.Length; i++)
		{
			if (a.Values[i] != b.Values[i]) return false;
		}

		return true;
	}

	public override bool Equals(object? other)
	{
		if (other is not Index index) return false;

		if (ReferenceEquals(this, other)) return true;
		if (Values.Length != index.Values.Length) return false;
		
		for (var i = 0; i < Values.Length; i++)
		{
			if (Values[i] != index.Values[i]) return false;
		}

		return true;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Values);
	}

	public int CompareTo(Index? other)
	{
		return CompareTo(this, other);
	}
}

public struct ModifiableFlowElement
{
	public Node Node { get; set; }
	public Index Index { get; set; }
	public int Capacity { get; set; }

	public ModifiableFlowElement(Node node, Index index, int capacity)
	{
		Node = node;
		Index = index;
		Capacity = capacity;
	}
}

public class ModifiableFlow
{
	public List<ModifiableFlowElement> Nodes { get; private set; } = new List<ModifiableFlowElement>();
	public Dictionary<Node, Index> Indices { get; private set; } = new Dictionary<Node, Index>(new ReferenceEqualityComparer<Node>());
	public Dictionary<JumpNode, Index> Jumps { get; private set; } = new Dictionary<JumpNode, Index>(new ReferenceEqualityComparer<JumpNode>());
	public Dictionary<Label, Index> Labels { get; private set; } = new Dictionary<Label, Index>(new ReferenceEqualityComparer<Label>());
	public Dictionary<Label, List<JumpNode>> Paths { get; private set; } = new Dictionary<Label, List<JumpNode>>(new ReferenceEqualityComparer<Label>());
	public Dictionary<LoopNode, LoopDescriptor> Loops { get; private set; } = new Dictionary<LoopNode, LoopDescriptor>(new ReferenceEqualityComparer<LoopNode>());
	public Label End { get; private set; }

	public void Replace(Node before, Node after)
	{
		var position = GetNodeIndex(before);
		var descriptor = Nodes[position];

		var destination = position - descriptor.Capacity;
		var nodes = Nodes.GetRange(destination, descriptor.Capacity + 1);

		Nodes.RemoveRange(destination, nodes.Count);

		foreach (var iterator in nodes)
		{
			var node = iterator.Node;
			Indices.Remove(node);

			if (node.Is(NodeType.JUMP))
			{
				var jump = (JumpNode)node;
				var label = jump.Label;

				Jumps.Remove(jump);

				if (Paths.ContainsKey(label)) Paths[label].Remove(jump);
				continue;
			}
			
			if (node.Is(NodeType.LABEL))
			{
				Labels.Remove(node.To<LabelNode>().Label);
				Paths.Remove(node.To<LabelNode>().Label);
				continue;
			}
			
			if (node.Is(NodeType.LOOP)) { Loops.Remove((LoopNode)node); }
		}

		if (after.First == null)
		{
			descriptor.Node = after;
			descriptor.Capacity = 0;
			Nodes.Insert(destination, descriptor);
			return;
		}

		var flow = new ModifiableFlow(after, End);
		var start = destination - 1 >= 0 ? Nodes[destination - 1].Index : null;
		var end = destination < Nodes.Count ? Nodes[destination].Index : null;

		for (var i = flow.Nodes.Count - 1; i >= 0; i--)
		{
			var element = flow.Nodes[i];
			var node = element.Node;
			end = new Index(start, end);
			element.Index = end;

			Nodes.Insert(destination, element);
			Indices.Add(node, end);

			if (node.Is(NodeType.JUMP)) { Jumps.Add((JumpNode)node, end); }
			else if (node.Is(NodeType.LABEL)) { Labels.Add(node.To<LabelNode>().Label, end); }
		}

		foreach (var path in flow.Paths)
		{
			var label = path.Key;
			var jumps = path.Value;

			if (Paths.ContainsKey(label))
			{
				Paths[label].AddRange(jumps);
				continue;
			}

			Paths[label] = jumps;
		}

		flow.Loops.ForEach(i => Loops.Add(i.Key, i.Value));
	}

	public int GetNodeIndex(Node node)
	{
		return GetNodeIndex(Indices[node]);
	}

	public int GetNodeIndex(Index index)
	{
		var start = 0;
		var end = Indices.Count;

		while (start != end)
		{
			var middle = (start + end) / 2;
			var element = Nodes[middle].Index;
			
			if (index.Equals(element)) return middle;
			if (index > element) { start = middle + 1; }
			else { end = middle; }
		}

		return -1;
	}

	public ModifiableFlow(Node root)
	{
		End = new Label();
		Linearize(root);
		Add(new LabelNode(End), 0);

		Register();
	}

	private ModifiableFlow(Node root, Label end)
	{
		End = end;
		Linearize(root);
		Register();
	}

	private void Register()
	{
		foreach (var iterator in Indices)
		{
			if (iterator.Key is JumpNode jump)
			{
				Jumps.Add(jump, iterator.Value);

				if (!Paths.TryGetValue(jump.Label, out List<JumpNode>? jumps))
				{
					jumps = new List<JumpNode>();
					Paths[jump.Label] = jumps;
				}

				jumps.Add(jump);
			}
			else if (iterator.Key is LabelNode node)
			{
				Labels.Add(node.Label, iterator.Value);
			}
		}
	}

	private void Add(Node node, int capacity)
	{
		var index = new Index(Indices.Count);
		Indices.Add(node, new Index(Indices.Count));
		Nodes.Add(new ModifiableFlowElement(node, index, capacity));
	}

	private void LinearizeLogicalOperator(OperatorNode operation, Label success, Label failure)
	{
		var count = Nodes.Count;

		if (operation.Left.Is(OperatorType.LOGIC))
		{
			var intermediate = new Label();

			if (operation.Operator == Operators.AND)
			{
				// Operator: AND
				LinearizeLogicalOperator((OperatorNode)operation.Left, intermediate, failure);
			}
			else
			{
				// Operator: OR
				LinearizeLogicalOperator((OperatorNode)operation.Left, success, intermediate);
			}

			Add(new LabelNode(intermediate), 0);
		}
		else if (operation.Operator == Operators.AND)
		{
			// Operator: AND
			Linearize(operation.Left, false);
			Add(new JumpNode(failure, true), 0);
		}
		else
		{
			// Operator: OR
			Linearize(operation.Left, false);
			Add(new JumpNode(success, true), 0);
		}

		Add(operation.Left, Nodes.Count - count);
		count = Nodes.Count;

		if (operation.Right.Is(OperatorType.LOGIC))
		{
			var intermediate = new Label();

			if (operation.Operator == Operators.AND)
			{
				// Operator: AND
				LinearizeLogicalOperator((OperatorNode)operation.Right, intermediate, failure);
			}
			else
			{
				// Operator: OR
				LinearizeLogicalOperator((OperatorNode)operation.Right, success, intermediate);
			}

			Add(new LabelNode(intermediate), 0);
		}
		else if (operation.Operator == Operators.AND)
		{
			// Operator: AND
			Linearize(operation.Right, false);
			Add(new JumpNode(failure, true), 0);
		}
		else
		{
			// Operator: OR
			Linearize(operation.Right, false);
			Add(new JumpNode(failure, true), 0);
		}

		Add(operation.Right, Nodes.Count - count);
	}

	private void LinearizeCondition(IfNode statement, Label failure)
	{
		var condition = statement.Condition;
		var parent = condition.Parent!;

		// Remove the condition for a while
		if (!condition.Remove()) throw new ApplicationException("Could not remove the condition of a conditional statement during flow analysis");

		// Linearize all the nodes under the condition step except the actual condition
		statement.GetConditionStep().ForEach(i => Linearize(i));

		// Add the condition back
		parent.Add(condition);
		
		var count = Nodes.Count;

		if (condition.Is(OperatorType.LOGIC))
		{
			var success = new Label();

			LinearizeLogicalOperator(condition.To<OperatorNode>(), success, failure);

			Add(new LabelNode(success), 0);
			Add(condition, Nodes.Count - count);
		}
		else
		{
			Linearize(condition, false);
			Add(new JumpNode(failure, true), 0);
			Add(condition, Nodes.Count - count);
		}
	}

	private void LinearizeCondition(LoopNode statement, Label failure)
	{
		var condition = statement.Condition;
		var parent = condition.Parent!;

		// Remove the condition for a while
		if (!condition.Remove()) throw new ApplicationException("Could not remove the condition of a conditional statement during flow analysis");

		// Linearize all the nodes under the condition step except the actual condition
		statement.GetConditionStep().ForEach(i => Linearize(i));

		// Add the condition back
		parent.Add(condition);

		if (condition.Is(OperatorType.LOGIC))
		{
			var success = new Label();
			var count = Nodes.Count;

			LinearizeLogicalOperator(condition.To<OperatorNode>(), success, failure);
			
			Add(new LabelNode(success), 0);
			Add(condition, Nodes.Count - count);
		}
		else
		{
			Linearize(condition);
		}
	}

	private void Linearize(Node node, bool add = true)
	{
		var count = Nodes.Count;

		switch (node.Instance)
		{
			case NodeType.OPERATOR:
			{
				var operation = node.To<OperatorNode>();
				if (operation.Operator == Operators.AND || operation.Operator == Operators.OR) { throw new ApplicationException("Flow analysis encountered wild logical operator"); }

				node.ForEach(i => Linearize(i));

				if (add) Add(node, Nodes.Count - count);
				break;
			}

			case NodeType.IF:
			{
				if (!add) throw new NotSupportedException("Delaying addition of conditional statements in modifiable flow is not supported");

				var statement = node.To<IfNode>();
				var intermediate = new Label();
				var end = new Label();

				LinearizeCondition(statement, intermediate);

				// The body may be executed based on the condition. If it executes, it jumps to the end label
				Linearize(statement.Body);
				Add(new JumpNode(end), 0);
				Add(new LabelNode(intermediate), 0);
				Add(node, Nodes.Count - count);

				foreach (var successor in statement.GetSuccessors())
				{
					count = Nodes.Count;

					if (successor.Is(NodeType.ELSE_IF))
					{
						intermediate = new Label();

						LinearizeCondition((ElseIfNode)successor, intermediate);

						// The body may be executed based on the condition. If it executes, it jumps to the end label
						Linearize(successor.To<ElseIfNode>().Body);
						Add(new JumpNode(end), 0);

						Add(new LabelNode(intermediate), 0);
					}
					else if (successor.Is(NodeType.ELSE))
					{
						// The body always executes and jumps to the end label
						Linearize(successor.To<ElseNode>().Body);
					}

					Add(successor, Nodes.Count - count);
				}

				// NOTE: If all the conditional branches were removed, the end label would still remain. This should not break anything since the label would just be unused.
				Add(new LabelNode(end), 0);
				break;
			}

			case NodeType.LOOP:
			{
				if (!add) throw new NotSupportedException("Delaying addition of conditional statements in modifiable flow is not supported");
				
				var statement = node.To<LoopNode>();
				var start = new Label();
				var end = new Label();

				Loops.Add(statement, new LoopDescriptor(start, end));

				if (statement.IsForeverLoop)
				{
					Add(new LabelNode(start), 0);
					Linearize(statement.Body);
					Add(new JumpNode(start), 0);
					Add(new LabelNode(end), 0);
					Add(statement, Nodes.Count - count);
					break;
				}

				Linearize(statement.Initialization);

				Add(new LabelNode(start), 0);
				LinearizeCondition(statement, end);
				Add(new JumpNode(end, true), 0);

				Linearize(statement.Body);
				Linearize(statement.Action);

				Add(new JumpNode(start), 0);
				Add(new LabelNode(end), 0);
				Add(statement, Nodes.Count - count);

				break;
			}

			case NodeType.LOOP_CONTROL:
			{
				var instruction = node.To<LoopControlNode>().Instruction;

				if (instruction == Keywords.CONTINUE)
				{
					var loop = node.FindParent(i => i.Is(NodeType.LOOP)) ?? throw new ApplicationException("Loop control node missing parent loop");
					var start = Loops[loop.To<LoopNode>()].Start;

					Add(new JumpNode(start), 0);
					if (add) Add(node, Nodes.Count - count);
				}
				else if (instruction == Keywords.STOP)
				{
					var loop = node.FindParent(i => i.Is(NodeType.LOOP)) ?? throw new ApplicationException("Loop control node missing parent loop");
					var end = Loops[loop.To<LoopNode>()].End;

					Add(new JumpNode(end), 0);
					if (add) Add(node, Nodes.Count - count);
				}
				else
				{
					throw new ApplicationException("Invalid loop control node");
				}

				break;
			}

			case NodeType.RETURN:
			{
				node.ForEach(i => Linearize(i));
				Add(new JumpNode(End), 0);
				if (add) Add(node, Nodes.Count - count);
				break;
			}

			case NodeType.ELSE:
			case NodeType.ELSE_IF: { break; }

			default:
			{
				foreach (var iterator in node)
				{
					Linearize(iterator);
				}

				if (add) Add(node, Nodes.Count - count);
				break;
			}
		}
	}

	/// <summary>
	/// Finds the positions which can be reached starting from the specified position while avoiding the specified obstacles
	/// NOTE: Provide a copy of the positions since this function edits the specified list
	/// </summary>
	public List<Index> GetExecutablePositions(Index start, Index[] obstacles, List<Index> positions, SortedSet<Index> denylist)
	{
		var executable = new List<Index>(positions.Count);

	Start:

		// Try to find the closest obstacle which is ahead of the current position
		var closest_obstacle = new Index(int.MaxValue);

		foreach (var obstacle in obstacles)
		{
			if (obstacle > start && obstacle < closest_obstacle)
			{
				closest_obstacle = obstacle;
				break;
			}
		}

		// Try to find the closest jump which is ahead of the current position
		var closest_jump = new Index(int.MaxValue);
		var closest_jump_node = (JumpNode?)null;

		foreach (var jump in Jumps)
		{
			if (jump.Value > start)
			{
				closest_jump = jump.Value;
				closest_jump_node = jump.Key;
				break;
			}
		}

		// Determine whether an obstacle or a jump is closer
		var closest = (Index?)null;
		if (closest_obstacle <= closest_jump) { closest = closest_obstacle; }
		else { closest = closest_jump; }

		// Register all positions which fall between the closest obstacle or jump and the current position
		for (var i = positions.Count - 1; i >= 0; i--)
		{
			var position = positions[i];

			if (position >= start && position < closest)
			{
				executable.Add(position);
				positions.RemoveAt(i);
			}
		}

		// 1. Return if there are no positions to be reached
		// 2. If the closest has the value of the maximum integer, it means there is no jump or obstacle ahead
		// 3. Return from the call if an obstacle is hit
		// 4. The closest value must represent a jump so ensure it is not visited before
		if (!positions.Any() || closest.IsMax || Equals(closest, closest_obstacle) || denylist.Contains(closest))
		{
			return executable;
		}

		// Do not visit this jump again
		denylist.Add(closest_jump);

		if (closest_jump_node!.IsConditional)
		{
			// Visit the jump destination and try to reach the positions there
			var destination = Labels[closest_jump_node.Label];
			executable.AddRange(GetExecutablePositions(destination, obstacles, positions, denylist));

			// Do not continue if all positions have been reached already
			if (!positions.Any())
			{
				return executable;
			}

			// Fall through the conditional jump and return the reached positions if the end of nodes has been reached
			var i = GetNodeIndex(closest) + 1;
			if (i >= Nodes.Count) return executable;

			start = Nodes[i].Index;
		}
		else
		{
			// Since the jump is not conditional go to its label
			start = Labels[closest_jump_node.Label];
		}

		goto Start;
	}
}

public class Flow
{
	public List<Node> Nodes { get; private set; } = new List<Node>();
	public Dictionary<Node, int> Indices { get; private set; } = new Dictionary<Node, int>(new ReferenceEqualityComparer<Node>());
	public Dictionary<JumpNode, int> Jumps { get; private set; } = new Dictionary<JumpNode, int>(new ReferenceEqualityComparer<JumpNode>());
	public Dictionary<Label, int> Labels { get; private set; } = new Dictionary<Label, int>(new ReferenceEqualityComparer<Label>());
	public Dictionary<Label, List<JumpNode>> Paths { get; private set; } = new Dictionary<Label, List<JumpNode>>(new ReferenceEqualityComparer<Label>());
	public Dictionary<LoopNode, LoopDescriptor> Loops { get; private set; } = new Dictionary<LoopNode, LoopDescriptor>(new ReferenceEqualityComparer<LoopNode>());
	public Label End { get; private set; }

	public Flow(Node root)
	{
		End = new Label();
		Linearize(root);
		Add(new LabelNode(End));

		Register();
	}

	private void Register()
	{
		foreach (var iterator in Indices)
		{
			if (iterator.Key is JumpNode jump)
			{
				Jumps.Add(jump, iterator.Value);

				if (!Paths.TryGetValue(jump.Label, out List<JumpNode>? jumps))
				{
					jumps = new List<JumpNode>();
					Paths[jump.Label] = jumps;
				}

				jumps.Add(jump);
			}
			else if (iterator.Key is LabelNode node)
			{
				Labels.Add(node.Label, iterator.Value);
			}
		}
	}

	private void Add(Node node)
	{
		Indices.Add(node, Indices.Count);
		Nodes.Add(node);
	}

	private void LinearizeLogicalOperator(OperatorNode operation, Label success, Label failure)
	{
		if (operation.Left is OperatorNode x && x.Operator.Type == OperatorType.LOGIC)
		{
			var intermediate = new Label();

			if (operation.Operator == Operators.AND)
			{
				// Operator: AND
				LinearizeLogicalOperator(x, intermediate, failure);
			}
			else
			{
				// Operator: OR
				LinearizeLogicalOperator(x, success, intermediate);
			}

			Add(new LabelNode(intermediate));
		}
		else if (operation.Operator == Operators.AND)
		{
			// Operator: AND
			Linearize(operation.Left);
			Add(new JumpNode(failure, true));
		}
		else
		{
			// Operator: OR
			Linearize(operation.Left);
			Add(new JumpNode(success, true));
		}

		if (operation.Right is OperatorNode y && y.Operator.Type == OperatorType.LOGIC)
		{
			var intermediate = new Label();

			if (operation.Operator == Operators.AND)
			{
				// Operator: AND
				LinearizeLogicalOperator(y, intermediate, failure);
			}
			else
			{
				// Operator: OR
				LinearizeLogicalOperator(y, success, intermediate);
			}

			Add(new LabelNode(intermediate));
		}
		else if (operation.Operator == Operators.AND)
		{
			// Operator: AND
			Linearize(operation.Right);
			Add(new JumpNode(failure, true));
		}
		else
		{
			// Operator: OR
			Linearize(operation.Right);
			Add(new JumpNode(failure, true));
		}
	}

	private void LinearizeCondition(IfNode statement, Label failure)
	{
		var condition = statement.Condition;
		var parent = condition.Parent!;

		// Remove the condition for a while
		if (!condition.Remove()) throw new ApplicationException("Could not remove the condition of a conditional statement during flow analysis");

		// Linearize all the nodes under the condition step except the actual condition
		statement.GetConditionStep().ForEach(i => Linearize(i));

		// Add the condition back
		parent.Add(condition);

		if (condition.Is(OperatorType.LOGIC))
		{
			var success = new Label();
			LinearizeLogicalOperator(condition.To<OperatorNode>(), success, failure);
			Add(new LabelNode(success));
		}
		else
		{
			Linearize(condition);
			Add(new JumpNode(failure, true));
		}
	}

	private void LinearizeCondition(LoopNode statement, Label failure)
	{
		var condition = statement.Condition;
		var parent = condition.Parent!;

		// Remove the condition for a while
		if (!condition.Remove()) throw new ApplicationException("Could not remove the condition of a conditional statement during flow analysis");

		// Linearize all the nodes under the condition step except the actual condition
		statement.GetConditionStep().ForEach(i => Linearize(i));

		// Add the condition back
		parent.Add(condition);

		if (condition.Is(OperatorType.LOGIC))
		{
			var success = new Label();
			LinearizeLogicalOperator(condition.To<OperatorNode>(), success, failure);
			Add(new LabelNode(success));
		}
		else
		{
			Linearize(condition);
		}
	}

	private void Linearize(Node node)
	{
		switch (node.Instance)
		{
			case NodeType.OPERATOR:
			{
				var operation = node.To<OperatorNode>();

				if (operation.Operator == Operators.AND || operation.Operator == Operators.OR)
				{
					throw new ApplicationException("Flow analysis encountered wild logical operator");
				}

				if (operation.Operator.Type == OperatorType.ACTION)
				{
					// Action operators are processed the other way around
					// node.Reverse().ForEach(i => Linearize(i));
					node.ForEach(i => Linearize(i));
					Add(node);
					break;
				}

				node.ForEach(i => Linearize(i));
				Add(node);
				break;
			}

			case NodeType.IF:
			{
				var statement = node.To<IfNode>();
				var intermediate = new Label();
				var end = new Label();

				LinearizeCondition(statement, intermediate);

				// The body may be executed based on the condition. If it executes, it jumps to the end label
				Linearize(statement.Body);
				Add(new JumpNode(end));
				Add(new LabelNode(intermediate));

				foreach (var successor in statement.GetSuccessors())
				{
					if (successor is ElseIfNode x)
					{
						intermediate = new Label();

						LinearizeCondition(x, intermediate);

						// The body may be executed based on the condition. If it executes, it jumps to the end label
						Linearize(x.Body);
						Add(new JumpNode(end));

						Add(new LabelNode(intermediate));
					}
					else if (successor is ElseNode y)
					{
						// The body always executes and jumps to the end label
						Linearize(y.Body);
					}
				}

				Add(new LabelNode(end));

				break;
			}

			case NodeType.LOOP:
			{
				var statement = node.To<LoopNode>();
				var start = new Label();
				var end = new Label();

				Loops.Add(statement, new LoopDescriptor(start, end));

				if (statement.IsForeverLoop)
				{
					Add(new LabelNode(start));
					Linearize(statement.Body);
					Add(new JumpNode(start));
					Add(new LabelNode(end));
					break;
				}

				Linearize(statement.Initialization);

				Add(new LabelNode(start));

				LinearizeCondition(statement, end);

				Add(new JumpNode(end, true));

				Linearize(statement.Body);
				Linearize(statement.Action);

				Add(new JumpNode(start));
				Add(new LabelNode(end));

				break;
			}

			case NodeType.LOOP_CONTROL:
			{
				var instruction = node.To<LoopControlNode>().Instruction;

				if (instruction == Keywords.CONTINUE)
				{
					var loop = node.FindParent(i => i.Is(NodeType.LOOP)) ?? throw new ApplicationException("Loop control node missing parent loop");

					var start = Loops[loop.To<LoopNode>()].Start;

					Add(new JumpNode(start));
					Add(node);
				}
				else if (instruction == Keywords.STOP)
				{
					var loop = node.FindParent(i => i.Is(NodeType.LOOP)) ?? throw new ApplicationException("Loop control node missing parent loop");

					var end = Loops[loop.To<LoopNode>()].End;

					Add(new JumpNode(end));
					Add(node);
				}
				else
				{
					throw new ApplicationException("Invalid loop control node");
				}

				break;
			}

			case NodeType.RETURN:
			{
				node.ForEach(i => Linearize(i));
				Add(new JumpNode(End));
				break;
			}

			case NodeType.ELSE:
			case NodeType.ELSE_IF:
			{
				break;
			}

			default:
			{
				foreach (var iterator in node)
				{
					Linearize(iterator);
				}

				Add(node);
				break;
			}
		}
	}

	private bool TryDirectApproach(int from, int to)
	{
		// If the current position is the same as the destination, return true
		if (from == to)
		{
			return true;
		}

		// If the current position has already passed the destination, direct approach won't work
		if (from > to)
		{
			return false;
		}

		foreach (var iterator in Jumps)
		{
			var position = iterator.Value;
			var jump = iterator.Key;

			// Check if the jump blocks direct approach
			if (position > from && position < to)
			{
				var l = Labels[jump.Label];

				// If the jump goes out of the direct approach zone, it means the from node can not be reached with direct approach
				if ((l < from || l > to) && !jump.IsConditional)
				{
					return false;
				}

				// Since the label of the jump is inside the approach zone, use it to skip checking
				if (l > from && TryDirectApproach(l, to))
				{
					return true;
				}
			}
		}

		return true;
	}

	private bool TryDirectApproach(int from, int to, int[] obstacles)
	{
		// If the current position is the same as the destination, return true
		if (from == to)
		{
			return true;
		}

		// If the current position has already passed the destination, direct approach won't work
		if (from > to)
		{
			return false;
		}

		foreach (var iterator in Jumps)
		{
			var position = iterator.Value;
			var jump = iterator.Key;

			// Check if the jump blocks direct approach
			if (position > from && position < to)
			{
				// If any of the obstacles is before the jump but still in the approach zone, it means the jump is not reachable
				if (obstacles.Any(i => i > from && i < position))
				{
					return false;
				}

				var l = Labels[jump.Label];

				// If the jump goes out of the direct approach zone, it means the from node can not be reached with direct approach
				if ((l < from || l > to) && !jump.IsConditional)
				{
					return false;
				}

				// Since the label of the jump is inside the approach zone, use it to skip checking
				if (l > from && TryDirectApproach(l, to, obstacles))
				{
					return true;
				}
			}
		}

		// If any of the obstacles is inside the approach zone and no jump inside the approach zone allowed skipping the obstacles, it means the destination is not reachable
		return !obstacles.Any(i => i > from && i < to);
	}

	private bool Approach(int position, int destination)
	{
		return TryIndirectApproach(new Dictionary<int, Label>(), position, destination);
	}

	private bool Approach(int position, int destination, int[] obstacles)
	{
		return TryIndirectApproach(new Dictionary<int, Label>(), position, destination, obstacles);
	}

	private bool TryIndirectApproach(Dictionary<int, Label> labels, int position, int destination, int[] obstacles)
	{
		if (TryDirectApproach(position, destination, obstacles))
		{
			return true;
		}

		// Take all jumps that are after the current position but don't intercept with the obstacles
		foreach (var jump in Jumps.SkipWhile(i => i.Value <= position).Where(i => !obstacles.Any(j => j >= position && j <= i.Value)))
		{
			var label = jump.Key.Label;
			var key = Labels[label];

			if (labels.ContainsKey(key))
			{
				// If the jump is conditional, other jumps below this jump may execute
				if (jump.Key.IsConditional)
				{
					continue;
				}
			}
			else
			{
				labels.Add(key, label);

				// Use the jump and start from the label it jumps to and try to reach the destination
				if (TryIndirectApproach(labels, key, destination, obstacles))
				{
					return true;
				}
			}

			// If the jump is not conditional, the visit stops here, since jumps below have already been taken into account by the recursive visit call above
			if (!jump.Key.IsConditional)
			{
				break;
			}
		}

		return false;
	}

	private bool TryIndirectApproach(Dictionary<int, Label> labels, int position, int destination)
	{
		if (TryDirectApproach(position, destination))
		{
			return true;
		}

		foreach (var jump in Jumps.SkipWhile(a => a.Value < position))
		{
			var label = jump.Key.Label;
			var key = Labels[label];

			if (labels.ContainsKey(key))
			{
				// If the jump is conditional, other jumps below this jump may execute
				if (jump.Key.IsConditional)
				{
					continue;
				}
			}
			else
			{
				labels.Add(key, label);

				if (TryIndirectApproach(labels, Labels[jump.Key.Label], destination))
				{
					return true;
				}
			}

			// If the jump is not conditional, the visit stops here, since jumps below have already been taken into account by the recursive visit call above
			if (!jump.Key.IsConditional)
			{
				break;
			}
		}

		return false;
	}

	/// <summary>
	/// Returns whether the specified node can execute in any way starting from the specified position (from)
	/// </summary>
	public bool IsReachable(Node node, Node from)
	{
		if (node == from)
		{
			return true;
		}

		var i = Indices.GetValueOrDefault(node, -1);
		var j = Indices.GetValueOrDefault(from, -1);

		if (i == -1 || j == -1)
		{
			throw new ApplicationException("Flow analysis was passed a node which was not part of the flow");
		}

		return Approach(j, i);
	}

	/// <summary>
	/// Returns whether the node can be executed at least twice
	/// </summary>
	public bool IsRepeated(Node node)
	{
		var i = Indices.GetValueOrDefault(node, -1);

		if (i == -1)
		{
			throw new ApplicationException("Flow analysis was passed a node which was not part of the flow");
		}

		return Approach(i + 1, i);
	}

	/// <summary>
	/// Finds the positions which can be reached starting from the specified position while avoiding the specified obstacles
	/// NOTE: Provide a copy of the positions since this function edits the specified list
	/// </summary>
	public List<int> GetExecutablePositions(int start, int[] obstacles, List<int> positions, SortedSet<int> denylist)
	{
		var executable = new List<int>(positions.Count);

	Start:

		// Try to find the closest obstacle which is ahead of the current position
		var closest_obstacle = int.MaxValue;

		foreach (var obstacle in obstacles)
		{
			if (obstacle > start && obstacle < closest_obstacle)
			{
				closest_obstacle = obstacle;
				break;
			}
		}

		// Try to find the closest jump which is ahead of the current position
		var closest_jump = int.MaxValue;
		var closest_jump_node = (JumpNode?)null;

		foreach (var jump in Jumps)
		{
			if (jump.Value > start)
			{
				closest_jump = jump.Value;
				closest_jump_node = jump.Key;
				break;
			}
		}

		// Determine whether an obstacle or a jump is closer
		var closest = Math.Min(closest_obstacle, closest_jump);

		// Register all positions which fall between the closest obstacle or jump and the current position
		for (var i = positions.Count - 1; i >= 0; i--)
		{
			var position = positions[i];

			if (position >= start && position < closest)
			{
				executable.Add(position);
				positions.RemoveAt(i);
			}
		}

		// 1. Return if there are no positions to be reached
		// 2. If the closest has the value of the maximum integer, it means there is no jump or obstacle ahead
		// 3. Return from the call if an obstacle is hit
		// 4. The closest value must represent a jump so ensure it is not visited before
		if (!positions.Any() || closest == int.MaxValue || closest == closest_obstacle || denylist.Contains(closest))
		{
			return executable;
		}

		// Do not visit this jump again
		denylist.Add(closest_jump);

		if (closest_jump_node!.IsConditional)
		{
			// Visit the jump destination and try to reach the positions there
			var destination = Labels[closest_jump_node.Label];
			executable.AddRange(GetExecutablePositions(destination, obstacles, positions, denylist));

			// Do not continue if all positions have been reached already
			if (!positions.Any())
			{
				return executable;
			}

			// Fall through the conditional jump
			start = closest + 1;
		}
		else
		{
			// Since the jump is not conditional go to its label
			start = Labels[closest_jump_node.Label];
		}

		goto Start;
	}

	public bool IsReachableWithoutExecuting(Node node, Node from, IEnumerable<Node> obstacles)
	{
		if (node == from)
		{
			return true;
		}

		var i = Indices[node];
		var j = Indices[from];

		var indices = obstacles.Select(i => Indices[i]).ToArray();

		return Approach(j, i, indices);
	}

	public bool IsReachableWithoutExecuting(Node node, Node from, int[] obstacles)
	{
		if (node == from)
		{
			return true;
		}

		var i = Indices[node];
		var j = Indices[from];

		return Approach(j, i, obstacles);
	}

	/// <summary>
	/// Returns whether the specified node is executed before the specified position (from)
	/// NOTE: This does not check whether the node is always executed before the position rather whether is it executed at least once before reaching the specified position
	/// </summary>
	public bool IsExecutedBefore(Node node, Node position)
	{
		var i = Indices[node];
		var j = Indices[position];

		if (i > j)
		{
			var t = i;
			i = j;
			j = t;
		}

		// Collect all jumps that are before the specified node and jump over it
		var skips = Jumps.Where(x => x.Value < i && Labels[x.Key.Label] > i).ToArray();

		// Check if any of the skips are reachable in the future
		return !skips.Any(x => TryIndirectApproach(new Dictionary<int, Label>(), Labels[x.Key.Label], j));
	}

	/// <summary>
	/// Returns whether the specified node is always executed before the specified position (from)
	/// </summary>
	public bool IsAlwaysExecutedBefore(Node node, Node position)
	{
		var i = Indices[node];
		var j = Indices[position];

		if (i > j)
		{
			var t = i;
			i = j;
			j = t;
		}

		// Find all skip routes and check whether any of the can reach labels between the node and the position
		var labels = Labels.Where(l => l.Value > i && l.Value < j).ToArray();

		// If there is even one jump that goes to one the labels above, the node is not always executed
		return !labels.SelectMany(i => Paths.GetValueOrDefault(i.Key, new List<JumpNode>())).Any(a => { var x = Indices[a]; return x < i || x > j; });
	}

	public bool Between(Node from, Node to, Func<Node, bool> filter)
	{
		return Indices.Keys.Skip(Indices[from]).TakeWhile(i => i != to).Any(filter);
	}

	public IEnumerable<Node> FindBetween(Node from, Node to, Func<Node, bool> filter)
	{
		return Indices.Keys.Skip(Indices[from]).TakeWhile(i => i != to).Where(filter);
	}

	/// <summary>
	/// Returns whether node a is before b
	/// </summary>
	public bool IsBefore(Node a, Node b)
	{
		return Indices[a] < Indices[b];
	}

	/// <summary>
	/// Returns whether node a is after b
	/// </summary>
	public bool IsAfter(Node a, Node b)
	{
		return Indices[a] > Indices[b];
	}

	public bool IsBetween(Node node, Node from, Node to)
	{
		var x = Indices[node];

		return x > Indices[from] && x < Indices[to];
	}

	/// <summary>
	/// Returns all the nodes which are located between the specified range
	/// </summary>
	public List<Node> GetNodesBetween(Node from, Node to)
	{
		var a = Indices[from];
		var b = Indices[to];

		return Nodes.GetRange(a, b - a);
	}
}