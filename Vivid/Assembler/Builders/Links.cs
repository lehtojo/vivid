using System;

public static class Links
{
	public static Result GetMemberFunctionCall(Unit unit, FunctionNode function, Node self_node, Type self_type)
	{
		// Static functions can't access any instance data
		if (function.Function.IsStatic)
		{
			return Calls.Build(unit, function);
		}

		// Retrieve the context where the function is defined
		var function_context = function.Function.Metadata!.GetTypeParent()!;
		var self = References.Get(unit, self_node);

		// If the function is not defined inside the type of the self pointer, it means it must have been defined in its supertypes, therefore casting is needed
		if (function_context != self_type)
		{
			self = Casts.Cast(unit, self, self_type, function_context);
		}

		return Calls.Build(unit, self, function);
	}

	private static Result GetLambdaCall(Unit unit, LambdaCallNode function, Node self_node, Type self_type)
	{
		// Retrieve the context where the function is defined
		var self = References.Get(unit, self_node);
		var lambda_type = self_type.To<LambdaType>();

		return Calls.Build(unit, self, CallingConvention.X64, lambda_type.ReturnType, function.Parameters, lambda_type.Parameters!);
	}

	public static Result Build(Unit unit, LinkNode node)
	{
		var self_type = node.Object.GetType();

		return node.Member switch
		{
			VariableNode member when member.Variable.Category == VariableCategory.GLOBAL => References.GetVariable(unit,
				member.Variable),

			VariableNode member => new GetObjectPointerInstruction(unit, member.Variable, References.Get(unit, node.Object),
					member.Variable.GetAlignment(self_type) ?? throw new ApplicationException("Member variable was not aligned"))
				.Execute(),

			FunctionNode function => GetMemberFunctionCall(unit, function, node.Object, self_type),

			LambdaCallNode lambda => GetLambdaCall(unit, lambda, node.Object, self_type),

			_ => throw new NotImplementedException("Unsupported member node")
		};
	}
}