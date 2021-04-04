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
		if (!condition.Remove())
		{
			throw new ApplicationException("Could not remove the condition of a conditional statement during flow analysis");
		}

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
		if (!condition.Remove())
		{
			throw new ApplicationException("Could not remove the condition of a conditional statement during flow analysis");
		}

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
	/// NOTE: This does not check whether the node is always executed before the position rather whether is it executed before the first time
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