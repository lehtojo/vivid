using System;
using System.Collections.Generic;

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

	public static Result BuildCall(Unit unit, CallNode node)
	{
		unit.AddDebugPosition(node.Position);

		var function_pointer = (Result?)null;
		var self_type = (Type?)null;

		// If the self argument is "empty", do not pass it
		if (node.Self.Instance == NodeType.NORMAL)
		{
			function_pointer = References.Get(unit, node.Pointer, AccessMode.READ);

			return Calls.Build(unit, function_pointer, node.Descriptor.ReturnType, node.Parameters, node.Descriptor.Parameters!);
		}
	
		var call = (CallNode)node;
		var self = References.Get(unit, call.Self);

		if (call.Descriptor.Self != null)
		{
			self = Casts.Cast(unit, self, call.Self.GetType(), call.Descriptor.Self);
		}

		function_pointer = References.Get(unit, call.Pointer);
		self_type = call.Descriptor.Self ?? call.Self.GetType();

		return Calls.Build(unit, self, self_type, function_pointer, call.Descriptor.ReturnType, call.Parameters, call.Descriptor.Parameters!);
	}

	public static Result Build(Unit unit, Node node)
	{
		switch (node.Instance)
		{
			case NodeType.CALL:
			{
				return BuildCall(unit, (CallNode)node);
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

			case NodeType.JUMP:
			{
				return Jumps.Build(unit, (JumpNode)node);
			}

			case NodeType.OPERATOR:
			{
				return Arithmetic.Build(unit, (OperatorNode)node);
			}

			case NodeType.ACCESSOR:
			{
				return Accessors.Build(unit, (AccessorNode)node, AccessMode.READ);
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
				return new LabelInstruction(unit, node.To<LabelNode>().Label).Add();
			}

			case NodeType.COMMAND:
			{
				return Loops.BuildCommand(unit, (CommandNode)node);
			}

			case NodeType.ELSE_IF:
			case NodeType.ELSE:
			{
				// Skip else-statements since they are already built
				return new Result();
			}

			case NodeType.PACK:
			{
				var values = new List<Result>();
				foreach (var iterator in node) { values.Add(References.Get(unit, iterator)); }
				return new CreatePackInstruction(unit, node.GetType(), values).Add();
			}

			case NodeType.STACK_ADDRESS:
			{
				return new AllocateStackInstruction(unit, node.To<StackAddressNode>()).Add();
			}

			case NodeType.DISABLED: return new Result();

			case NodeType.UNDEFINED:
			{
				return new AllocateRegisterInstruction(unit, node.To<UndefinedNode>().Format).Add();
			}

			case NodeType.OBJECT_LINK:
			{
				return Objects.Build(unit, (ObjectLinkNode)node);
			}

			case NodeType.OBJECT_UNLINK:
			{
				return Objects.Build(unit, (ObjectUnlinkNode)node);
			}

			default: return BuildChilds(unit, node);
		}
	}
}