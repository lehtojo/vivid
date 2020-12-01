public static class Builders
{
	public static Result Build(Unit unit, Node node)
	{
		switch (node.GetNodeType())
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

				return Calls.Build(unit, self, function_pointer, CallingConvention.X64, call.Descriptor.ReturnType, call.Parameters, call.Descriptor.Parameters!);
			}

			case NodeType.FUNCTION:
			{
				return Calls.Build(unit, (FunctionNode)node);
			}

			case NodeType.INCREMENT:
			{
				return Arithmetic.Build(unit, (IncrementNode)node);
			}

			case NodeType.DECLARE:
			{
				var declaration = (DeclareNode)node;
				var result = new DeclareInstruction(unit, declaration.Variable).Execute();

				return new SetVariableInstruction(unit, declaration.Variable, result).Execute();
			}

			case NodeType.DECREMENT:
			{
				return Arithmetic.Build(unit, (DecrementNode)node);
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
				return Lambdas.Build(unit, (LambdaNode)node);
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

			default:
			{
				Result? reference = null;

				foreach (var iterator in node)
				{
					reference = References.Get(unit, iterator);
				}

				return reference ?? new Result();
			}
		}
	}
}