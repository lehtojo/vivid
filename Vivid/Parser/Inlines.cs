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

		foreach (var local in function.Variables.Values)
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
		if (!Primitives.IsPrimitive(function.ReturnType, Primitives.UNIT))
		{
			var root = instance.Parent != null && instance.Parent.Is(NodeType.LINK) ? instance.Parent : instance;
			var container = Common.CreateInlineContainer(function.ReturnType!, root);
			var context = container.Node.IsContext ? container.Node.To<ContextInlineNode>().Context : instance.GetParentContext();
			var body = GetInlineBody(environment, context, function, instance, out Node destination);

			// Find all return statements
			var return_statements = body.FindAll(NodeType.RETURN).Select(i => i.To<ReturnNode>());

			// Request a label representing the end of the function only if needed
			Label? end = null;

			if (return_statements.Any())
			{
				end = environment.CreateLabel();
				body.Add(new DeclareNode(container.Result) { Registerize = false });
				body.Add(new JumpNode(end));
				body.Add(new LabelNode(end));
			}

			// Replace all the return statements with an assign operator which stores the value to the result variable
			foreach (var return_statement in return_statements)
			{
				// Assign the return value of the function to the variable which represents the result of the function
				var assign = new OperatorNode(Operators.ASSIGN).SetOperands(
					new VariableNode(container.Result),
					return_statement.Value!
				);

				// Create a node which exists the inlined function since there can be more inlined code after the result is assigned
				var jump = new JumpNode(end!);

				// Replace the return statement with the assign statement and add a jump to exit the inline node
				return_statement.Replace(jump);
				jump.Insert(assign);
			}

			// Transfer the contents to the container node and replace the destination with it
			body.ForEach(i => container.Node.Add(i));
			container.Destination.Replace(container.Node);

			// The container node must return the result, so add it
			container.Node.Add(new VariableNode(container.Result, instance.Position));
			ReconstructionAnalysis.Reconstruct(environment, container.Node);
		}
		else
		{
			// Find all return statements
			var body = GetInlineBody(environment, instance.GetParentContext(), function, instance, out Node destination);
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

				body.Add(new LabelNode(end));
			}

			var container = new InlineNode(instance.Position);
			body.ForEach(i => container.Add(i));
			destination.Replace(container);

			ReconstructionAnalysis.Reconstruct(environment, container);

			if (!ReconstructionAnalysis.IsValueUsed(destination))
			{
				container.ReplaceWithChildren(container);
			}
		}
	}
}