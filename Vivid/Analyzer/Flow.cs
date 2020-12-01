using System.Collections.Generic;
using System;
using System.Linq;

public class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
{
	public bool Equals(T? x, T? y)
	{
		return object.ReferenceEquals(x, y);
	}

	public int GetHashCode(T x)
	{
		return HashCode.Combine(x);
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

public class Flow
{
	public Dictionary<Node, int> Indices { get; private set; } = new Dictionary<Node, int>(new ReferenceEqualityComparer<Node>());
	public Dictionary<JumpNode, int> Jumps { get; private set; } = new Dictionary<JumpNode, int>(new ReferenceEqualityComparer<JumpNode>());
	public Dictionary<Label, int> Labels { get; private set; } = new Dictionary<Label, int>(new ReferenceEqualityComparer<Label>());
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
				jump.Label.Jumps.Add(jump);
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
		statement.GetConditionInitialization().ForEach(i => Linearize(i));

		if (statement.Condition is OperatorNode x && x.Operator.Type == OperatorType.LOGIC)
		{
			var success = new Label();
			LinearizeLogicalOperator(x, success, failure);
			Add(new LabelNode(success));
		}
		else
		{
			Linearize(statement.Condition);
			Add(new JumpNode(failure, true));
		}
	}

	private void LinearizeCondition(LoopNode statement, Label failure)
	{
		statement.GetConditionInitialization().ForEach(i => Linearize(i));

		if (statement.Condition is OperatorNode x && x.Operator.Type == OperatorType.LOGIC)
		{
			var success = new Label();
			LinearizeLogicalOperator(x, success, failure);
			Add(new LabelNode(success));
		}
		else
		{
			Linearize(statement.Condition);
		}
	}

	private void Linearize(Node node)
	{
		switch (node.GetNodeType())
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
						node.Reverse().ForEach(i => Linearize(i));
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

	public bool IsReachableWithoutExecuting(Node node, Node from, Node[] obstacles)
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

		var indices = obstacles.Select(i => Indices.GetValueOrDefault(i, -1)).ToArray();

		if (indices.Any(i => i == -1))
		{
			throw new ApplicationException("Flow analysis was passed a node which was not part of the flow");
		}

		return Approach(j, i, indices);
	}

	/// <summary>
	/// Returns whether the specified node is executed before the specified position (from)
	/// NOTE: This does not check whether the node is always executed before the position rather whether is it executed before the first time
	/// </summary>
	public bool IsExecutedBefore(Node node, Node position)
	{
		var i = Indices.GetValueOrDefault(node, -1);
		var j = Indices.GetValueOrDefault(position, -1);

		if (i == -1 || j == -1)
		{
			throw new ApplicationException("Flow analysis was passed a node which was not part of the flow");
		}

		if (i > j)
		{
			var t = i;
			i = j;
			j = t;
		}

		// Find all skip routes and check whether any of the can reach labels between the node and the position
		var labels = Labels.Where(l => l.Value > i && l.Value < j).ToArray();

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
		var i = Indices.GetValueOrDefault(node, -1);
		var j = Indices.GetValueOrDefault(position, -1);

		if (i == -1 || j == -1)
		{
			throw new ApplicationException("Flow analysis was passed a node which was not part of the flow");
		}

		if (i > j)
		{
			var t = i;
			i = j;
			j = t;
		}

		// Find all skip routes and check whether any of the can reach labels between the node and the position
		var labels = Labels.Where(l => l.Value > i && l.Value < j).ToArray();

		// If there is even one jump that goes to one the labels above, the node is not always executed
		return !labels.Any(i => i.Key.Jumps.Any());
	}

	public bool Between(Node from, Node to, Func<Node, bool> filter)
	{
		var i = Indices.GetValueOrDefault(from, -1);

		if (i == -1)
		{
			throw new ApplicationException("Flow analysis was passed a node which was not part of the flow");
		}

		return Indices.Keys.Skip(i).TakeWhile(j => j != to).Any(filter);
	}

	public IEnumerable<Node> FindBetween(Node from, Node to, Func<Node, bool> filter)
	{
		var i = Indices.GetValueOrDefault(from, -1);

		if (i == -1)
		{
			throw new ApplicationException("Flow analysis was passed a node which was not part of the flow");
		}

		return Indices.Keys.Skip(i).TakeWhile(j => j != to).Where(filter);
	}

	/// <summary>
	/// Returns whether node a is before b
	/// </summary>
	public bool IsBefore(Node a, Node b)
	{
		var i = Indices.GetValueOrDefault(a, -1);
		var j = Indices.GetValueOrDefault(b, -1);

		if (i == -1 || j == -1)
		{
			throw new ApplicationException("Flow analysis was passed a node which was not part of the flow");
		}

		return i < j;
	}

	/// <summary>
	/// Returns whether node a is after b
	/// </summary>
	public bool IsAfter(Node a, Node b)
	{
		var i = Indices.GetValueOrDefault(a, -1);
		var j = Indices.GetValueOrDefault(b, -1);

		if (i == -1 || j == -1)
		{
			throw new ApplicationException("Flow analysis was passed a node which was not part of the flow");
		}

		return i > j;
	}

	public bool IsBetween(Node node, Node from, Node to)
	{
		var x = Indices.GetValueOrDefault(node, -1);

		var a = Indices.GetValueOrDefault(from, -1);
		var b = Indices.GetValueOrDefault(to, -1);

		if (x == -1 || a == -1 || b == -1)
		{
			throw new ApplicationException("Flow analysis was passed a node which was not part of the flow");
		}

		return x > a && x < b;
	}
}