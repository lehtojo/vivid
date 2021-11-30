using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class NodeReferenceEqualityComparer : IEqualityComparer<Node>
{
	public bool Equals(Node? x, Node? y)
	{
		return ReferenceEquals(x, y);
	}

	public int GetHashCode(Node x)
	{
		return HashCode.Combine(x);
	}
}

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
	public Dictionary<Node, int> Indices { get; private set; } = new Dictionary<Node, int>(new NodeReferenceEqualityComparer());
	public Dictionary<JumpNode, int> Jumps { get; private set; } = new Dictionary<JumpNode, int>(new NodeReferenceEqualityComparer());
	public Dictionary<Label, int> Labels { get; private set; } = new Dictionary<Label, int>(new ReferenceEqualityComparer<Label>());
	public Dictionary<Label, List<JumpNode>> Paths { get; private set; } = new Dictionary<Label, List<JumpNode>>(new ReferenceEqualityComparer<Label>());
	public Dictionary<LoopNode, LoopDescriptor> Loops { get; private set; } = new Dictionary<LoopNode, LoopDescriptor>(new NodeReferenceEqualityComparer());
	public Label End { get; private set; }
	public int LabelIdentity { get; private set; } = 0;

	public string GetNextLabel()
	{
		return (LabelIdentity++).ToString();
	}

	public Flow(Node root)
	{
		End = new Label(GetNextLabel());
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
			var intermediate = new Label(GetNextLabel());

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
			LinearizeLogicalOperator(y, success, failure);
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
			var success = new Label(GetNextLabel());
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
			var success = new Label(GetNextLabel());
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
				var intermediate = new Label(GetNextLabel());
				var end = new Label(GetNextLabel());

				LinearizeCondition(statement, intermediate);

				// The body may be executed based on the condition. If it executes, it jumps to the end label
				Linearize(statement.Body);
				Add(new JumpNode(end));
				Add(new LabelNode(intermediate));

				foreach (var successor in statement.GetSuccessors())
				{
					if (successor is ElseIfNode x)
					{
						intermediate = new Label(GetNextLabel());

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
				var start = new Label(GetNextLabel());
				var end = new Label(GetNextLabel());

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
					var loop = node.FindParent(NodeType.LOOP) ?? throw new ApplicationException("Loop control node missing parent loop");

					var start = Loops[loop.To<LoopNode>()].Start;

					Add(new JumpNode(start));
					Add(node);
				}
				else if (instruction == Keywords.STOP)
				{
					var loop = node.FindParent(NodeType.LOOP) ?? throw new ApplicationException("Loop control node missing parent loop");

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

	/// <summary>
	/// Finds the positions which can be reached starting from the specified position while avoiding the specified obstacles
	/// NOTE: Provide a copy of the positions since this function edits the specified list
	/// </summary>
	public List<int>? GetExecutablePositions(int start, int[] obstacles, List<int> positions, SortedSet<int> denylist, int layer = 3)
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

			if (layer - 1 <= 0) return null;
			var result = GetExecutablePositions(destination, obstacles, positions, denylist, layer - 1);
			if (result == null) return null;
			executable.AddRange(result);

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

	public bool Between(Node from, Node to, Func<Node, bool> filter)
	{
		return Indices.Keys.Skip(Indices[from]).TakeWhile(i => i != to).Any(filter);
	}

	public IEnumerable<Node> FindBetween(Node from, Node to, Func<Node, bool> filter)
	{
		return Indices.Keys.Skip(Indices[from]).TakeWhile(i => i != to).Where(filter);
	}
}

public class StatementFlow
{
	public List<Node?> Nodes { get; private set; } = new List<Node?>();
	private Dictionary<Node, int> Indices { get; set; } = new Dictionary<Node, int>(new NodeReferenceEqualityComparer());
	public Dictionary<JumpNode, int> Jumps { get; private set; } = new Dictionary<JumpNode, int>(new NodeReferenceEqualityComparer());
	public Dictionary<Label, int> Labels { get; private set; } = new Dictionary<Label, int>(new ReferenceEqualityComparer<Label>());
	public Dictionary<Label, List<JumpNode>> Paths { get; private set; } = new Dictionary<Label, List<JumpNode>>(new ReferenceEqualityComparer<Label>());
	public Dictionary<LoopNode, LoopDescriptor> Loops { get; private set; } = new Dictionary<LoopNode, LoopDescriptor>(new NodeReferenceEqualityComparer());
	public Label End { get; private set; }
	public int LabelIdentity { get; private set; } = 0;

	public string GetNextLabel()
	{
		return (LabelIdentity++).ToString();
	}

	public StatementFlow(Node root)
	{
		End = new Label(GetNextLabel());
		Linearize(root);
		Add(new LabelNode(End));

		RegisterJumpsAndLabels();
	}

	private void RegisterJumpsAndLabels()
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

	public void Remove(Node node)
	{
		if (Indices.ContainsKey(node))
		{
			var index = Indices[node];
			Indices.Remove(node);
			Nodes[index] = null;
		}
	}

	public void Replace(Node what, Node with)
	{
		if (Indices.ContainsKey(what))
		{
			var index = Indices[what];
			Indices[with] = index;
			Indices.Remove(what);
			Nodes[index] = with;
		}
	}

	public int IndexOf(Node node)
	{
		for (var iterator = node; iterator != null; iterator = iterator.Parent)
		{
			if (Indices.TryGetValue(iterator, out var index)) return index;
		}

		throw new ApplicationException("Could not return the flow index of the specified node");
	}

	private void LinearizeLogicalOperator(OperatorNode operation, Label success, Label failure)
	{
		if (operation.Left is OperatorNode x && x.Operator.Type == OperatorType.LOGIC)
		{
			var intermediate = new Label(GetNextLabel());

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
			LinearizeLogicalOperator(y, success, failure);
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
			var success = new Label(GetNextLabel());
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
			var success = new Label(GetNextLabel());
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
				Add(node);
				break;
			}

			case NodeType.IF:
			{
				var statement = node.To<IfNode>();
				var intermediate = new Label(GetNextLabel());
				var end = new Label(GetNextLabel());

				LinearizeCondition(statement, intermediate);

				// The body may be executed based on the condition. If it executes, it jumps to the end label
				Linearize(statement.Body);
				Add(new JumpNode(end));
				Add(new LabelNode(intermediate));

				foreach (var successor in statement.GetSuccessors())
				{
					if (successor is ElseIfNode x)
					{
						intermediate = new Label(GetNextLabel());

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
				var start = new Label(GetNextLabel());
				var end = new Label(GetNextLabel());

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
					var loop = node.FindParent(NodeType.LOOP) ?? throw new ApplicationException("Loop control node missing parent loop");

					var start = Loops[loop.To<LoopNode>()].Start;

					Add(new JumpNode(start));
					Add(node);
				}
				else if (instruction == Keywords.STOP)
				{
					var loop = node.FindParent(NodeType.LOOP) ?? throw new ApplicationException("Loop control node missing parent loop");

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

			case NodeType.SCOPE:
			case NodeType.NORMAL:
			case NodeType.INLINE:
			{
				foreach (var iterator in node)
				{
					Linearize(iterator);
				}

				Add(node);
				break;
			}

			default:
			{
				Add(node);
				break;
			}
		}
	}

	/// <summary>
	/// Finds the positions which can be reached starting from the specified position while avoiding the specified obstacles
	/// NOTE: Provide a copy of the positions since this function edits the specified list
	/// </summary>
	public List<int>? GetExecutablePositions(int start, int[] obstacles, List<int> positions, SortedSet<int> denylist, int layer = 3)
	{
		var executable = new List<int>(positions.Count);

	Start:

		// Try to find the closest obstacle which is ahead of the current position
		var closest_obstacle = int.MaxValue;

		foreach (var obstacle in obstacles)
		{
			if (obstacle >= start && obstacle < closest_obstacle)
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
			if (jump.Value >= start)
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

			if (position >= start && position <= closest)
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

			if (layer - 1 <= 0) return null;
			var result = GetExecutablePositions(destination, obstacles, positions, denylist, layer - 1);
			if (result == null) return null;
			executable.AddRange(result);

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
}