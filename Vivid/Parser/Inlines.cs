using System.Collections.Generic;
using System.Linq;
using System;

public static class Inlines
{
	/// <summary>
	/// Inlines all implementations defined in the specified context
	/// </summary>
	/// <param name="context">Context to go through</param>
	public static void Build(Context context)
	{
		foreach (var implementation in context.GetImplementedFunctions())
		{
			var batch = implementation.Node!.FindAll(n => n is FunctionNode f && !f.Function.Metadata.IsConstructor &&
				!f.Function.Metadata.IsImported && !Flag.Has(f.Function.Metadata.Modifiers, AccessModifier.OUTLINE));

			while (true)
			{
				// Inline all references contained in the batch
				foreach (var function in batch)
				{
					Inline(function.To<FunctionNode>().Function, function.To<FunctionNode>());
				}

				// Since inlining can create new function references, they must be searched
				batch = implementation.Node!.FindAll(n => n is FunctionNode f && !f.Function.Metadata.IsImported && !Flag.Has(f.Function.Metadata.Modifiers, AccessModifier.OUTLINE));

				// Stop if there are no function references
				if (!batch.Any())
				{
					break;
				}
			}

			// The implementation is inlined completely only if it's not exported
			//implementation.IsInlined = !implementation.Metadata.IsExported; 
		}

		foreach (var type in context.Types.Values)
		{
			Build(type);
		}
	}

	private static void UnwrapSubcontexts(Context context, Node root)
	{
		foreach (var iterator in root)
		{
			if (iterator is IContext subcontext && !iterator.Is(NodeType.TYPE))
			{
				// The subcontext must be replaced with a new context so that there are no references to the original function
				var replacement_context = new Context();
				replacement_context.Link(context);

				foreach (var local in subcontext.GetContext().Variables.Values)
				{
					// Skip the variable if it's the self pointer
					if (local.IsSelfPointer)
					{
						continue;
					}

					// Create a new variable which represents the original variable and redirect all the usages of the original variable
					var replacement_variable = replacement_context.DeclareHidden(local.Type!);
					var usages = root.FindAll(i => i.Is(NodeType.VARIABLE) && i.To<VariableNode>().Variable == local).Select(i => i.To<VariableNode>());

					usages.ForEach(i => i.Variable = replacement_variable);
				}

				// Update the context of the node
				subcontext.SetContext(replacement_context);

				// Now unwrap all subcontexts under the current subcontext
				UnwrapSubcontexts(replacement_context, (Node)subcontext);
			}
			else
			{
				// Now unwrap all subcontexts under the current iterator 
				UnwrapSubcontexts(context, iterator);
			}
		}
	}

	private static Node GetInlineBody(Context context, FunctionImplementation implementation, FunctionNode reference, out Node destination)
	{
		var body = implementation.Node!.Clone();

		foreach (var (parameter, value) in implementation.Parameters.Zip((IEnumerable<Node>)reference))
		{
			var initialization = new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(parameter),
				value
			);

			if (body.First == null)
			{
				body.Add(initialization);
			}
			else
			{
				body.Insert(body.First!, initialization);
			}
		}

		foreach (var local in implementation.Variables.Values)
		{
			if (local.IsSelfPointer && !implementation.IsConstructor)
			{
				continue;
			}

			var replacement = context.DeclareHidden(local.Type!);
			var usages = body.FindAll(i => i.Is(local)).Select(i => i.To<VariableNode>());

			usages.ForEach(i => i.Variable = replacement);
		}

		UnwrapSubcontexts(context, body);

		if (WrapMemberAccess(context, reference, body))
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
	/// Returns a node representing a position where the inlined body can be inserted
	/// </summary>
	/// <param name="reference">The call reference which should be inlined</param>
	public static Node GetInlineInsertPosition(Node reference)
	{
		var iterator = reference.Parent!;
		var position = reference;

		while (!(iterator is IContext || iterator.Is(NodeType.NORMAL)))
		{
			position = iterator;
			iterator = iterator.Parent!;
		}

		return position;
	}

	/// <summary>
	/// Replaces the function call with the body of the specified function using the parameter values of the call
	/// </summary>
	public static void Inline(FunctionImplementation implementation, FunctionNode reference)
	{
		var context = reference.GetParentContext();
		var body = GetInlineBody(context, implementation, reference, out Node destination);
		var position = GetInlineInsertPosition(reference);

		if (implementation.ReturnType != Types.UNIT)
		{
			// Declare a variable which contains the result of the inlined function
			var result = context.DeclareHidden(implementation.ReturnType!);

			// Find all return statements
			var return_statements = body.FindAll(i => i.Is(NodeType.RETURN)).Select(i => i.To<ReturnNode>());

			// Request a label representing the end of the function only if needed
			Label? end = null;

			if (return_statements.Any())
			{
				end = implementation.GetLabel();
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

			// Insert a node to the destination position and replace it with the inlined body
			//var temporary = new Node();
			//position.Insert(temporary);

			// Replace the function call with the result of the inlined function
			//destination.Replace(new VariableNode(result));

			var temporary = new InlineNode();
			body.ForEach(i => temporary.Add(i));
			temporary.Add(new VariableNode(result));

			destination.Replace(temporary);

			//temporary.ReplaceWithChildren(body);

			return;
		}
		else
		{
			// Find all return statements
			var return_statements = body.FindAll(i => i.Is(NodeType.RETURN)).Select(i => i.To<ReturnNode>());

			if (return_statements.Any())
			{
				var end = implementation.GetLabel();

				// Replace each return statement with a jump node which goes to the end of the inlined body
				return_statements.ForEach(i => i.Replace(new JumpNode(end)));

				body.Add(new LabelNode(end));
			}

			// Replace the function call with the body of the inlined function
			var temporary = new InlineNode();
			body.ForEach(i => temporary.Add(i));

			destination.Replace(temporary);
			//destination.ReplaceWithChildren(body);
		}
	}

	private static void WrapCurrentSelfPointer(Context context, FunctionNode reference, Node body)
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

	private static bool WrapMemberAccess(Context context, FunctionNode reference, Node body)
	{
		var self_pointer = GetSelfPointer(reference);

		if (self_pointer == null)
		{
			WrapCurrentSelfPointer(context, reference, body);
			return false;
		}

		var usages = body.FindAll(i => i.Is(NodeType.VARIABLE) &&
			i.To<VariableNode>().Variable.IsMember &&
			i.Parent!.Is(NodeType.LINK) &&
			i.Parent!.First == i
		).Select(i => i.To<VariableNode>());

		usages.ForEach(i =>
		{
			i.Replace(new LinkNode(
				self_pointer.Clone(),
				new VariableNode(i.Variable)
			));
		});

		var self_pointers = body.FindAll(i => i.Is(NodeType.VARIABLE) &&
			i.To<VariableNode>().Variable.IsSelfPointer
		);

		self_pointers.ForEach(i => i.Replace(self_pointer.Clone()));

		return true;
	}

	private static Node? GetSelfPointer(FunctionNode reference)
	{
		if (reference.Parent is LinkNode link && link.Right == reference)
		{
			return link.Left;
		}

		return null;
	}
}