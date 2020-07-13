using System;

public static class Links
{
	private static Result GetMemberFunctionCall(Unit unit, FunctionNode function, Result self, Type self_type)
	{
		// Retrieve the context where the function is defined
		var function_context = function.Function.Metadata!.GetTypeParent()!;

		// If the function is not defined inside the type of the self pointer, it means it must have been defined in its supertypes, therefore casting is needed
		if (function_context != self_type)
		{
			self = Casts.Cast(self, self_type, function_context);
		}

		return Calls.Build(unit, self, function);
	}

	public static Result Build(Unit unit, LinkNode node)
	{
		var self = References.Get(unit, node.Object);
		var self_type = node.Object.GetType();

		return node.Member switch
		{
			VariableNode member when member.Variable.Category == VariableCategory.GLOBAL => References.GetVariable(unit,
				member.Variable, AccessMode.READ),
			
			VariableNode member => new GetObjectPointerInstruction(unit, member.Variable, self,
					member.Variable.GetAlignment(self_type) ?? throw new ApplicationException("Member variable wasn't aligned"))
				.Execute(),
			
			FunctionNode function => GetMemberFunctionCall(unit, function, self, self_type),
			
			_ => throw new NotImplementedException("Unsupported member node")
		};
	}
}