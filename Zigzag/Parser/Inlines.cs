using System.Collections.Generic;
using System.Linq;
using System;

/*public class InlineContext : Context
{
	public InlineNode Node { get; private set; }

	public InlineContext(InlineNode node)
	{
		Node = node;
	}

	public override Variable? GetSelfPointer()
	{
		return Node.HasSelf ? Node.Self!.Left.To<VariableNode>().Variable : Parent?.GetSelfPointer();
	}
}*/

public static class Inlines
{
	/*private static Variable Wrap(Context context, Variable variable, List<VariableNode> locals)
	{
		var replacement = context.DeclareHidden(variable.Type!);
		var usages = locals.Where(n => n.Variable == variable).ToArray();

		usages.ForEach(u => u.Variable = replacement);
		usages.ForEach(u => locals.Remove(u));

		return replacement;
	}

	private static List<IContext> FindTopLevelContexts(Node root)
	{
		var result = new List<IContext>();
		
		foreach (var iterator in root)
		{
			if (iterator is IContext context && !iterator.Is(NodeType.TYPE_NODE))
			{
				result.Add(context);
			}
			else
			{
				result.AddRange(FindTopLevelContexts(iterator));
			}
		}

		return result;
	}

	private static void Wrap(Context root, ImplementationNode node, List<VariableNode> variables)
	{
		var context = new Context();
		context.Link(root);

		var locals = variables.FindAll(n => n.Variable.Context == node.Context);

		foreach (var local in locals.GroupBy(l => l.Variable).ToArray())
		{
			var replacement = context.DeclareHidden(local.Key.Type!);

			foreach (var usage in local)
			{
				usage.Variable = replacement;
			}
		}

		locals.ForEach(l => variables.Remove(l));
		node.SetContext(context);

		Wrap(context, (Node)node, variables);
	}

	private static void Wrap(Context root_context, Node root, List<VariableNode> variables)
	{
		var contexts = FindTopLevelContexts(root);

		foreach (var iterator in contexts)
		{
			var context = new Context();
			context.Link(root_context);

			var locals = variables.FindAll(n => n.Variable.Context == iterator.GetContext());

			foreach (var local in locals.GroupBy(l => l.Variable).ToArray())
			{
				var replacement = context.DeclareHidden(local.Key.Type!);

				foreach (var usage in local)
				{
		  usage.Variable = replacement;
				}
			}

			locals.ForEach(l => variables.Remove(l));
			iterator.SetContext(context);
		}

		foreach (var iterator in contexts)
		{
			Wrap(iterator.GetContext(), (Node)iterator, variables);
		}
	}

	private static void Inline(FunctionImplementation implementation, Node reference)
	{
		var environment = reference.FindContext().GetContext();

		var inline_node = new InlineNode(implementation);

		var root = new InlineContext(inline_node);
		root.Link(environment);

		// Pair parameters with corresponding nodes
		var parameters = implementation.Parameters.Zip(reference.ToList()).ToList();
		var body = implementation.Node!.Clone();

		// Find all local variables inside the body
		var variables = body.FindAll(n => n.Is(NodeType.VARIABLE_NODE) && n.To<VariableNode>().Variable.IsPredictable).Cast<VariableNode>().ToList();
		
		bool has_self = false;

		// Initialize the parameters with corresponding values
		var initialization = new Node();

		// The whole constructor node should be replaced if it exists
		if (reference.Parent is ConstructionNode construction)
		{
			reference = construction;
		}
		else if (reference.Parent is LinkNode link)
		{
			reference = link;

			var self = implementation.GetSelfPointer() ?? throw new ApplicationException("Missing self pointer encountered while inlining");
			var destination = Wrap(root, self, variables);

			has_self = true;

			initialization.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(destination),
				link.Left
			));
		}
		else if (implementation.IsMember)
		{
			var self = implementation.GetSelfPointer() ?? throw new ApplicationException("Missing self pointer encountered while inlining");
			var replacement = root.GetSelfPointer() ?? throw new ApplicationException("Missing self pointer encountered while inlining");

			var usages = variables.Where(n => n.Variable == self).ToArray();

			usages.ForEach(u => u.Variable = replacement);
			usages.ForEach(u => variables.Remove(u));
		}
	
		foreach (var (parameter, value) in parameters)
		{
			var replacement = Wrap(root, parameter, variables);

			initialization.Add(
				new OperatorNode(Operators.ASSIGN).SetOperands(
					new VariableNode(replacement),
					value
				)
			);
		}

		inline_node.Add(initialization);
		inline_node.Add(body);
		inline_node.Context = root;
		inline_node.HasSelf = has_self;
		
		Wrap(root, body.To<ImplementationNode>(), variables);
		
		var return_statements = body.FindAll(n => n.Is(NodeType.RETURN_NODE)).Select(n => n.To<ReturnNode>());

		// Handle return values by storing the result of the function to a variable and replacing the function reference with the result variable
		if (implementation.ReturnType != null)
		{
			// Declare a variable which will store the result of the inlined function
			var result = root.DeclareHidden(implementation.ReturnType!);

			initialization.Add(
				new OperatorNode(Operators.ASSIGN).SetOperands(
					new VariableNode(result),
					new NumberNode(Parser.Size.ToFormat(), 0L)
				)
			);

			// Replace all the return statements with an assign operator which stores the value to the result variable
			foreach (var return_statement in return_statements)
			{
				var assign = new OperatorNode(Operators.ASSIGN).SetOperands(
					new VariableNode(result),
					return_statement.Value
				);
				
				var jump = new JumpNode(inline_node.End);

				// Replace the return statement with the assign statement and add a jump to exit the inline node
				return_statement.Replace(jump);
				jump.Insert(assign);
			}

			inline_node.Result = result;
		}
		else
		{
			// Replace all the return statements with a jump to exit the inline node
			foreach (var return_statement in return_statements)
			{
				return_statement.Replace(new JumpNode(inline_node.End));
			}	
		}

		reference.Replace(inline_node);
	}*/

	public static void Build(Context context)
	{
		foreach (var implementation in context.GetImplementedFunctions())
		{
			var batch = implementation.Node!.FindAll(n => n is FunctionNode f && !f.Function.Metadata.IsConstructor && 
				!f.Function.Metadata.IsImported && !Flag.Has(f.Function.Metadata.Modifiers, AccessModifier.OUTLINE));

			while (true)
			{
				foreach (var function in batch)
				{
					Inline(function.To<FunctionNode>().Function, function.To<FunctionNode>());
				}
				
				batch = implementation.Node!.FindAll(n => n is FunctionNode f && !f.Function.Metadata.IsConstructor && 
					!f.Function.Metadata.IsImported && !Flag.Has(f.Function.Metadata.Modifiers, AccessModifier.OUTLINE));
				
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

	/*private static void Transfer(FunctionImplementation implementation)
	{
		var reference = (FunctionNode)implementation.References[0];
		var node = (ImplementationNode?)reference.FindParent(n => n.GetNodeType() == NodeType.IMPLEMENTATION_NODE);

		if (node == null)
		{
			throw new ApplicationException("Could not find the implementation node of a function");
		}

		var context = node.Implementation;

		// Change inline function parameter into local variables
		implementation.Parameters.ForEach(p => p.Category = VariableCategory.LOCAL);

		// Transfer all inline function properties
		context.Merge(implementation);

		foreach (var parameter in implementation.Parameters)
		{
			var name = $"inline.{implementation.Metadata!.GetFullname().Replace('_', '.')}.{parameter.Name}";
			parameter.Name = name;

			context.Declare(parameter);
		}
		
		var inline = new InlineNode(implementation, reference, implementation.Node!);
		reference.Replace(inline);
	}*/

	private static void UnwrapSubcontexts(Context context, Node root)
	{
		var subcontext = (IContext?)root.Find(i => i is IContext);

		while (subcontext != null)
		{
			var replacement_context = new Context();
			replacement_context.Link(context);

			foreach (var local in subcontext.GetContext().Variables.Values)
			{
				if (local.IsSelfPointer)
				{
					continue;
				}

				var replacement_variable = replacement_context.DeclareHidden(local.Type!);
				var usages = root.FindAll(i => i.Is(NodeType.VARIABLE_NODE) && i.To<VariableNode>().Variable == local).Select(i => i.To<VariableNode>());
			
				usages.ForEach(i => i.Variable = replacement_variable);	
			}

			subcontext.SetContext(replacement_context);

			UnwrapSubcontexts(replacement_context, (Node)subcontext);

			subcontext = (IContext?)root.Find(i => i is IContext);
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
			if (local.IsSelfPointer)
			{
				continue;
			}

			var replacement = context.DeclareHidden(local.Type!);
	
			var usages = body.FindAll(i => i.Is(NodeType.VARIABLE_NODE) && i.To<VariableNode>().Variable == local && !i.To<VariableNode>().Variable.IsSelfPointer).Select(i => i.To<VariableNode>());
			
			usages.ForEach(i => i.Variable = replacement);
		}

		UnwrapSubcontexts(context, body);

		if (WrapMemberAccess(reference, body))
		{
			destination = reference.FindParent(i => i.Is(NodeType.LINK_NODE)) ?? throw new ApplicationException("Could not find the self pointer of the inlined function from its reference");
		}
		else
		{
			destination = reference;
		}

		return body;
	}	

	private static Node GetInlineInsertPosition(Node reference)
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

	public static void Inline(FunctionImplementation implementation, FunctionNode reference)
	{
		//var position = GetInlineInsertPosition(reference);
		var context = reference.FindContext().GetContext();
		var body = GetInlineBody(context, implementation, reference, out Node destination);
		var position = GetInlineInsertPosition(reference);

		if (implementation.ReturnType != null)
		{
			var result = context.DeclareHidden(implementation.ReturnType);
			
			var return_statements = body.FindAll(i => i.Is(NodeType.RETURN_NODE)).Select(i => i.To<ReturnNode>());
			//var inline_node = new InlineNode(implementation, result);
			
			// Replace all the return statements with an assign operator which stores the value to the result variable
			foreach (var return_statement in return_statements)
			{
				var assign = new OperatorNode(Operators.ASSIGN).SetOperands(
					new VariableNode(result),
					return_statement.Value
				);
				
				var jump = new JumpNode(implementation.GetLabel());

				// Replace the return statement with the assign statement and add a jump to exit the inline node
				return_statement.Replace(jump);
				jump.Insert(assign);
			}

			//body.ForEach(i => inline_node.Body.Add(i));
			
			//destination.Replace(inline_node);

		  	destination.Replace(new VariableNode(result));
			
			// Insert a node to the destination position and replace it with the inlined body
			var temporary = new Node();
			position.Insert(temporary);

			temporary.ReplaceWithChildren(body);
		}
		else
		{
			// Since the function doesn't return anything, it can't be used in the middle of an calculation. This means the reference can be replaced with the inlined body
			//var inline_node = new InlineNode(implementation, null);
			//body.ForEach(i => inline_node.Body.Add(i));
			
			//destination.Replace(inline_node);

			destination.ReplaceWithChildren(body);
		}
	}

	private static bool WrapMemberAccess(FunctionNode reference, Node body)
	{
		var self_pointer = GetSelfPointer(reference);

		if (self_pointer == null)
		{
			return false;
		}

		var usages = body.FindAll(i => i.Is(NodeType.VARIABLE_NODE) && 
			i.To<VariableNode>().Variable.IsMember && 
			i.Parent!.Is(NodeType.LINK_NODE) && 
			i.Parent!.First == i
		).Select(i => i.To<VariableNode>());

		usages.ForEach(i => {
			i.Replace(new LinkNode(
				self_pointer.Clone(),
				new VariableNode(i.Variable)
			));
		});

		var self_pointers = body.FindAll(i => i.Is(NodeType.VARIABLE_NODE) && 
			i.To<VariableNode>().Variable.IsSelfPointer
		);

		self_pointers.ForEach(i => i.Replace(self_pointer.Clone()));

		return true;
	}

	private static Node? GetSelfPointer(FunctionNode reference)
	{
		if (reference.Parent!.Is(NodeType.LINK_NODE))
		{
			return reference.Parent!.To<LinkNode>().Left;
		}

		return null;
	}
}
















