using System;

public static class Links
{
	public static Result GetMemberFunctionCall(Unit unit, FunctionNode function, Node self_node, Type self_type)
	{
		// Static functions can not access any instance data
		if (function.Function.IsStatic)
		{
			return Calls.Build(unit, function);
		}

		// Retrieve the context where the function is defined
		var function_context = function.Function.Metadata!.FindTypeParent()!;
		var self = References.Get(unit, self_node);

		// If the function is not defined inside the type of the self pointer, it means it must have been defined in its supertypes, therefore casting is needed
		if (function_context != self_type)
		{
			self = Casts.Cast(unit, self, self_type, function_context);
		}

		return Calls.Build(unit, self, function);
	}

	public static Result Build(Unit unit, LinkNode node, AccessMode mode)
	{
		var self_type = node.Left.GetType();

		if (node.Right.Is(NodeType.VARIABLE))
		{
			var member = node.Right.To<VariableNode>().Variable;

			// Link nodes can also access static variables for example
			if (member.IsGlobal)
			{
				return References.GetVariable(unit, member, mode);
			}

			var left = References.Get(unit, node.Left);
			var alignment = member.GetAlignment(self_type) ?? throw new ApplicationException("Member variable was not aligned");

			return new GetObjectPointerInstruction(unit, member, left, alignment, mode).Execute();
		}

		if (!node.Right.Is(NodeType.FUNCTION)) throw new NotImplementedException("Unsupported member node");

		return GetMemberFunctionCall(unit, node.Right.To<FunctionNode>(), node.Left, self_type);
	}
}