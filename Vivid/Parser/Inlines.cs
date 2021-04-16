using System.Linq;
using System;

public static class Inlines
{
	/// <summary>
	/// Finds function calls under the specified root and tries to inline them
	/// </summary>
	public static void Build(Node root)
	{
		/// NOTE: If a scope node does not have a parent, it must be the root scope
		var context = root.Is(NodeType.SCOPE) 
			? root.To<ScopeNode>().Context
			: root.FindParent(i => i.Is(NodeType.SCOPE) && i.Parent == null)!.To<ScopeNode>().Context;

		while (true)
		{
			var call = (FunctionNode?)root.Find(i => i.Is(NodeType.FUNCTION) && i.To<FunctionNode>().Function.IsInlineable());

			if (call == null)
			{
				break;
			}

			Inline(call.Function, call);
			Analysis.CaptureContextLeaks(context, root);
		}
	}

	/// <summary>
	/// Finds all the labels under the specified root and localizes them by declaring new labels to the specified context
	/// </summary>
	public static void LocalizeLabels(Context context, Node root)
	{
		// Find all the labels and the jumps under the specified root
		var labels = root.FindAll(i => i.Is(NodeType.LABEL)).Cast<LabelNode>();
		var jumps = root.FindAll(i => i.Is(NodeType.JUMP)).Cast<JumpNode>().ToList();

		// Go through all the labels
		foreach (var label in labels)
		{
			// Create a replacement for the label
			var replacement = context.CreateLabel();

			// Find all the jumps which use the current label and update them to use the replacement
			for (var i = jumps.Count - 1; i >= 0; i--)
			{
				var jump = jumps[i];

				if (jump.Label != label.Label)
				{
					continue;
				}

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
					if (local.IsSelfPointer)
					{
						continue;
					}

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

		var inline_self_pointer_usages = body.FindAll(i => i.Is(NodeType.VARIABLE))
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
	private static Node GetInlineBody(Context context, FunctionImplementation implementation, FunctionNode reference, out Node destination)
	{
		var body = implementation.Node!.Clone();

		// Load all the function call arguments into temporary variables
		foreach (var (parameter, value) in implementation.Parameters.Zip(reference).Reverse())
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

		LocalizeLabels(context, body);

		foreach (var local in implementation.Variables.Values)
		{
			if (local.IsSelfPointer)
			{
				continue;
			}

			var replacement = context.DeclareHidden(local.Type!);

			var usages = body.FindAll(i => i.Is(local));
			usages.Cast<VariableNode>().ForEach(i => i.Variable = replacement);

			usages = body.FindAll(i => i.Is(NodeType.DECLARE) && i.To<DeclareNode>().Variable == local);
			usages.Cast<DeclareNode>().ForEach(i => i.Variable = replacement);
		}

		LocalizeSubcontexts(context, body, body);

		if (LocalizeMemberAccess(context, reference, body))
		{
			destination = reference.FindParent(i => i.Is(NodeType.LINK)) ?? throw new ApplicationException("Could not find the self pointer of the inlined function from its reference");
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
	public static void Inline(FunctionImplementation implementation, FunctionNode reference)
	{
		var environment = reference.GetParentContext();
		var inline = new ContextInlineNode(new Context(environment), reference.Position);
		var body = GetInlineBody(inline.Context, implementation, reference, out Node destination);

		if (!Primitives.IsPrimitive(implementation.ReturnType, Primitives.UNIT))
		{
			// Declare a variable which contains the result of the inlined function
			var result = inline.Context.DeclareHidden(implementation.ReturnType!);

			// Find all return statements
			var return_statements = body.FindAll(i => i.Is(NodeType.RETURN)).Select(i => i.To<ReturnNode>());

			// Request a label representing the end of the function only if needed
			Label? end = null;

			if (return_statements.Any())
			{
				end = implementation.CreateLabel();
				body.Add(new LabelNode(end));
			}

			// Replace all the return statements with an assign operator which stores the value to the result variable
			foreach (var return_statement in return_statements)
			{
				// Assign the return value of the function to the variable which represents the result of the function
				var assign = new OperatorNode(Operators.ASSIGN).SetOperands(
					new VariableNode(result),
					return_statement.Value!
				);

				// Create a node which exists the inlined function since there can be more inlined code after the result is assigned
				var jump = new JumpNode(end!);

				// Replace the return statement with the assign statement and add a jump to exit the inline node
				return_statement.Replace(jump);
				jump.Insert(assign);
			}

			body.ForEach(i => inline.Add(i));
			inline.Add(new VariableNode(result));

			destination.Replace(inline);

			ReconstructionAnalysis.Reconstruct(inline);
			return;
		}
		else
		{
			// Find all return statements
			var return_statements = body.FindAll(i => i.Is(NodeType.RETURN)).Cast<ReturnNode>().ToArray();

			if (return_statements.Any())
			{
				var end = implementation.CreateLabel();

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

				body.Add(new LabelNode(end));
			}

			// Replace the function call with the body of the inlined function
			body.ForEach(i => inline.Add(i));

			destination.Replace(inline);

			ReconstructionAnalysis.Reconstruct(inline);
		}
	}
}