using System;

public static class Builders
{
	public static Result BuildChilds(Unit unit, Node node)
	{
		var result = (Result?)null;

		foreach (var iterator in node)
		{
			result = References.Get(unit, iterator);
		}

		return result ?? new Result();
	}

	public static Result Build(Unit unit, Node node)
	{
		switch (node.Instance)
		{
			case NodeType.CALL:
			{
				var call = (CallNode)node;
				var self = References.Get(unit, call.Self);

				if (call.Descriptor.Self != null)
				{
					self = Casts.Cast(unit, self, call.Self.GetType(), call.Descriptor.Self);
				}

				var function_pointer = References.Get(unit, call.Pointer);
				var self_type = call.Descriptor.Self ?? call.Self.GetType();

				return Calls.Build(unit, self, self_type, function_pointer, call.Descriptor.ReturnType, call.Parameters, call.Descriptor.Parameters!);
			}

			case NodeType.FUNCTION:
			{
				return Calls.Build(unit, (FunctionNode)node);
			}

			case NodeType.INCREMENT:
			{
				throw new ApplicationException("Increment operations should not be passed to the back end");
			}

			case NodeType.DECREMENT:
			{
				throw new ApplicationException("Decrement operations should not be passed to the back end");
			}

			case NodeType.DECLARE:
			{
				var declaration = (DeclareNode)node;

				// Do not declare the variable twice
				if (unit.IsInitialized(declaration.Variable)) return new Result();

				var result = new DeclareInstruction(unit, declaration.Variable, declaration.ToRegister).Execute();
				return new SetVariableInstruction(unit, declaration.Variable, result).Execute();
			}

			case NodeType.JUMP:
			{
				return Jumps.Build(unit, (JumpNode)node);
			}

			case NodeType.OPERATOR:
			{
				return Arithmetic.Build(unit, (OperatorNode)node);
			}

			case NodeType.OFFSET:
			{
				return Arrays.BuildOffset(unit, (OffsetNode)node, AccessMode.READ);
			}

			case NodeType.LAMBDA:
			{
				throw new ApplicationException("Lambda nodes should not be passed to the back end");
			}

			case NodeType.LINK:
			{
				return Links.Build(unit, (LinkNode)node, AccessMode.READ);
			}

			case NodeType.IF:
			{
				return Conditionals.Start(unit, (IfNode)node);
			}

			case NodeType.LOOP:
			{
				return Loops.Build(unit, (LoopNode)node);
			}

			case NodeType.RETURN:
			{
				return Returns.Build(unit, (ReturnNode)node);
			}

			case NodeType.CAST:
			{
				return Casts.Build(unit, (CastNode)node, AccessMode.READ);
			}

			case NodeType.NOT:
			{
				return Arithmetic.BuildNot(unit, (NotNode)node);
			}

			case NodeType.NEGATE:
			{
				return Arithmetic.BuildNegate(unit, (NegateNode)node);
			}

			case NodeType.LABEL:
			{
				return new LabelInstruction(unit, node.To<LabelNode>().Label).Execute();
			}

			case NodeType.LOOP_CONTROL:
			{
				return Loops.BuildControlInstruction(unit, (LoopControlNode)node);
			}

			case NodeType.ELSE_IF:
			case NodeType.ELSE:
			{
				// Skip else-statements since they are already built
				return new Result();
			}

			case NodeType.STACK_ADDRESS:
			{
				return new AllocateStackInstruction(unit, node.To<StackAddressNode>()).Execute();
			}

			case NodeType.DISABLED: return new Result();

			default: return BuildChilds(unit, node);
		}
	}
}