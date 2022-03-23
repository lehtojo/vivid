using System.Linq;
using System;

public static class Inlines
{
	/// <summary>
	/// Finds function calls under the specified root and tries to inline them
	/// </summary>
	public static void Build(FunctionImplementation implementation, Node root)
	{
		while (true)
		{
			var call = (FunctionNode?)root.Find(i => i.Is(NodeType.FUNCTION) && i.To<FunctionNode>().Function.IsInlineable());
			if (call == null) break;

			Inline(implementation, call.Function, call);
			Analysis.CaptureContextLeaks(implementation, root);
		}
	}

	/// <summary>
	/// Finds all the labels under the specified root and localizes them by declaring new labels to the specified context
	/// </summary>
	public static void LocalizeLabels(FunctionImplementation implementation, Node root)
	{
		// Find all the labels and the jumps under the specified root
		var labels = root.FindAll(NodeType.LABEL).Cast<LabelNode>();
		var jumps = root.FindAll(NodeType.JUMP).Cast<JumpNode>().ToList();

		// Go through all the labels
		foreach (var label in labels)
		{
			// Create a replacement for the label
			var replacement = implementation.CreateLabel();

			// Find all the jumps which use the current label and update them to use the replacement
			for (var i = jumps.Count - 1; i >= 0; i--)
			{
				var jump = jumps[i];

				if (jump.Label != label.Label) continue;

				jump.Label = replacement;
				jumps.RemoveAt(i);
			}
			
			label.Label = replacement;
		}
	}
	
	/// <summary>
	/// Finds subcontexts under the specified root and localizes them by declaring new subcontexts to the specified context
	/// </summary>
	private static void LocalizeSubcontexts(Context context, Node start, Node root)
	{
		foreach (var iterator in start)
		{
			if (iterator is IScope subcontext && !iterator.Is(NodeType.TYPE))
			{
				// The subcontext must be replaced with a new context so that there are no references to the original function
				var replacement_context = new Context(context);

				foreach (var local in subcontext.GetContext().Variables.Values)
				{
					// Skip the variable if it is the self pointer
					if (local.IsSelfPointer) continue;

					// Create a new variable which represents the original variable and redirect all the usages of the original variable
					var replacement_variable = replacement_context.DeclareHidden(local.Type!);

					var usages = root.FindAll(i => i.Is(local));
					usages.Cast<VariableNode>().ForEach(i => i.Variable = replacement_variable);

					usages = root.FindAll(i => i.Is(NodeType.DECLARE) && i.To<DeclareNode>().Variable == local);
					usages.Cast<DeclareNode>().ForEach(i => i.Variable = replacement_variable);
				}

				// Update the context of the node
				subcontext.SetContext(replacement_context);

				// Now unwrap all subcontexts under the current subcontext
				LocalizeSubcontexts(replacement_context, (Node)subcontext, root);
			}
			else
			{
				// Now unwrap all subcontexts under the current iterator 
				LocalizeSubcontexts(context, iterator, root);
			}
		}
	}

	/// <summary>
	/// Localize the current self pointer by inspecting the specified function call
	/// </summary>
	private static void LocalizeCurrentSelfPointer(Context context, FunctionNode reference, Node body)
	{
		var self_pointer = context.GetSelfPointer();
		var inline_self_pointer = reference.Function.GetSelfPointer();

		if (self_pointer == null || inline_self_pointer == null)
		{
			return;
		}

		var inline_self_pointer_usages = body.FindAll(NodeType.VARIABLE)
			.Where(i => i.To<VariableNode>().Variable == inline_self_pointer)
			.Cast<VariableNode>();

		inline_self_pointer_usages.ForEach(i => i.Variable = self_pointer);
	}

	/// <summary>
	/// Localize the self pointer by inspecting the specified function call
	/// </summary>
	private static bool LocalizeMemberAccess(Context context, FunctionNode reference, Node body)
	{
		// Try to get the self pointer from the function call
		var self_pointer_value = GetSelfPointer(reference);

		// If there is no self pointer value, try to localize the current self pointer, if one is present
		if (self_pointer_value == null)
		{
			LocalizeCurrentSelfPointer(context, reference, body);
			return false;
		}
		
		var self_pointer_variable = reference.Function.GetSelfPointer() ?? throw new ApplicationException("Missing self pointer");
		var self_pointer_replacement = context.DeclareHidden(self_pointer_variable.Type!);

		// Load the self pointer once
		var initialization = new OperatorNode(Operators.ASSIGN).SetOperands(
			new VariableNode(self_pointer_replacement),
			self_pointer_value
		);

		body.Insert(body.First, initialization);

		// Find all member variables which are missing the self pointer at the start
		var usages = body.FindAll(i => i.Is(NodeType.VARIABLE) &&
			i.To<VariableNode>().Variable.IsMember &&
			i.Parent!.Is(NodeType.LINK) &&
			i.Parent!.First == i
		).Select(i => i.To<VariableNode>());

		usages.ForEach(i => i.Replace(new LinkNode(new VariableNode(self_pointer_replacement), new VariableNode(i.Variable), i.Position)));

		// Find all references of the original self pointer in the body and replace them
		var self_pointers = body.FindAll(i => i.Is(NodeType.VARIABLE) && i.To<VariableNode>().Variable == self_pointer_variable);

		self_pointers.ForEach(i => i.Replace(new VariableNode(self_pointer_replacement)));

		return true;
	}

	/// <summary>
	/// Returns the part which represents the self pointer from the specified function call
	/// </summary>
	private static Node? GetSelfPointer(FunctionNode reference)
	{
		if (reference.Parent is LinkNode link && link.Right == reference)
		{
			return link.Left;
		}

		return null;
	}

	/// <summary>
	/// Returns a node tree which represents the body of the specified function implementation.
	/// The returned node tree does not have any connections to the function implementation.
	/// </summary>
	private static Node GetInlineBody(FunctionImplementation environment, Context context, FunctionImplementation function, FunctionNode reference, out Node destination)
	{
		var body = function.Node!.Clone();

		// Load all the function call arguments into temporary variables
		foreach (var (parameter, value) in function.Parameters.Zip(reference).Reverse())
		{
			// Determines whether the value should be casted to match the parameter type
			var is_cast_required = value.GetType() != parameter.Type!;

			// Load the value into a temporary variable
			var initialization = new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(parameter),
				is_cast_required ? new CastNode(value, new TypeNode(parameter.Type!), value.Position) : value
			);

			body.Insert(body.First, initialization);
		}

		LocalizeLabels(environment, body);

		// Localize variables and parameters in the top function context, subcontexts are handled later
		foreach (var local in function.Variables.Values.Concat(function.Parameters))
		{
			if (local.IsSelfPointer) continue;

			var replacement = context.DeclareHidden(local.Type!);

			var usages = body.FindAll(i => i.Is(local));
			usages.Cast<VariableNode>().ForEach(i => i.Variable = replacement);

			usages = body.FindAll(i => i.Is(NodeType.DECLARE) && i.To<DeclareNode>().Variable == local);
			usages.Cast<DeclareNode>().ForEach(i => i.Variable = replacement);
		}

		LocalizeSubcontexts(context, body, body);

		if (LocalizeMemberAccess(context, reference, body))
		{
			destination = reference.FindParent(NodeType.LINK) ?? throw new ApplicationException("Could not find the self pointer of the inlined function from its reference");
		}
		else
		{
			destination = reference;
		}

		return body;
	}

	/// <summary>
	/// Replaces the function call with the body of the specified function using the parameter values of the call
	/// </summary>
	public static void Inline(FunctionImplementation environment, FunctionImplementation function, FunctionNode instance)
	{
		// Get the root node of the function call
		var call_root = instance.Parent!.Is(NodeType.LINK) ? instance.Parent! : instance;

		// Get the node before which to insert the body of the called function
		var insertion_position = ReconstructionAnalysis.GetExpressionExtractPosition(call_root);

		var context = call_root.GetParentContext();

		if (!Primitives.IsPrimitive(function.ReturnType, Primitives.UNIT))
		{
			// Prepare the function body for inlining
			var body = GetInlineBody(environment, context, function, instance, out Node destination);

			// Determine the variable, which will store the result of the function call.
			// If the function call is assigned to a variable, use that variable.
			// Otherwise, create a new temporary variable.
			var return_value = (Variable?)null;
			var is_assigned_to_local = call_root.Parent!.Is(Operators.ASSIGN) && call_root.Previous!.Instance == NodeType.VARIABLE;

			if (is_assigned_to_local)
			{
				return_value = call_root.Previous!.To<VariableNode>().Variable;
			}
			else
			{
				return_value = context.DeclareHidden(function.ReturnType);
			}

			// Find all return statements
			var return_statements = body.FindAll(NodeType.RETURN).Select(i => i.To<ReturnNode>());

			// Request a label representing the end of the function only if needed
			Label? end = null;

			// Replace all the return statements with an assign operator which stores the value to the result variable
			foreach (var return_statement in return_statements)
			{
				// Assign the return value of the function to the variable which represents the result of the function
				var assignment = new OperatorNode(Operators.ASSIGN).SetOperands(new VariableNode(return_value), return_statement.Value!);

				// If the return statement is the last statement in the function, no need to create a jump
				if (return_statement.Next == null && return_statement.Parent!.Parent == null)
				{
					// Just save the return value
					return_statement.Replace(assignment);
				}
				else
				{
					if (end == null)
					{
						// Declare the return variable at the start of the inlined function
						end = environment.CreateLabel();
						body.Insert(body.First, new DeclareNode(return_value) { Registerize = false });
						body.Add(new JumpNode(end)); // Add this jump because it will trigger label merging
						body.Add(new LabelNode(end));
					}

					// Create a node which exists the inlined function since there can be more inlined code after the result is assigned
					var jump = new JumpNode(end!);

					// Replace the return statement with the assign statement and add a jump to exit the inline node
					return_statement.Replace(jump);
					jump.Insert(assignment);
				}
			}

			// Add the return value to the end of the body just in case
			body.Add(new VariableNode(return_value));

			// Insert the body of the called function
			insertion_position.InsertChildren(body);

			// 1. If the function call was assigned to a local variable,
			// the return statements were replaced with assignments to the local variable.
			// Therefore, the assignment created by the user is no longer needed after the inlined body.
			// 2. If the function call was not assigned to a local variable (complex destination or complex usage),
			// the return statements were replaced with assignments to a temporary variable.
			// The function call created by the user after the inlined body must be replaced with the temporary variable.
			if (is_assigned_to_local)
			{
				call_root.Parent!.Remove();
			}
			else
			{
				call_root.Replace(new VariableNode(return_value));
			}
		}
		else
		{
			// Find all return statements
			var body = GetInlineBody(environment, context, function, instance, out Node destination);
			var return_statements = body.FindAll(NodeType.RETURN).Cast<ReturnNode>().ToArray();

			if (return_statements.Any())
			{
				var end = function.CreateLabel();

				// Replace each return statement with a jump node which goes to the end of the inlined body
				foreach (var return_statement in return_statements)
				{
					var jump = new JumpNode(end);
					return_statement.Replace(jump);

					if (return_statement.Value != null)
					{
						jump.Insert(return_statement.Value);
					}
				}

				body.Add(new JumpNode(end)); // Add this jump because it will trigger label merging
				body.Add(new LabelNode(end));
			}

			// Insert the body of the called function
			insertion_position.InsertChildren(body);

			// If a value is expected to return even though the function does not return a value, replace the function call with an undefined value
			if (ReconstructionAnalysis.IsValueUsed(call_root))
			{
				call_root.Replace(new UndefinedNode(function.ReturnType!, Assembler.Format));
			}
			else
			{
				call_root.Remove();
			}
		}
	}
}