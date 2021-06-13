using System;
using System.Collections.Generic;
using System.Linq;

public static class Analysis
{
	public const int VARIABLE_ACCESS_COST = 1;
	public const int STANDARD_OPERATOR_COST = 10;

	public const int ADDITION_COST = STANDARD_OPERATOR_COST;
	public const int SUBTRACTION_COST = STANDARD_OPERATOR_COST;

	public const int POWER_OF_TWO_MULTIPLICATION_COST = STANDARD_OPERATOR_COST;
	public const int MULTIPLICATION_COST = 3 * STANDARD_OPERATOR_COST;

	public const int POWER_OF_TWO_DIVISION_COST = STANDARD_OPERATOR_COST;
	public const int DIVISION_COST = 100 * STANDARD_OPERATOR_COST;

	public const int MEMORY_ACCESS_COST = 50 * STANDARD_OPERATOR_COST;
	public const int CONDITIONAL_JUMP_COST = 20 * STANDARD_OPERATOR_COST;

	public static bool IsInstructionAnalysisEnabled { get; set; } = false;
	public static bool IsUnwrapAnalysisEnabled { get; set; } = false;
	public static bool IsMathematicalAnalysisEnabled { get; set; } = false;
	public static bool IsRepetitionAnalysisEnabled { get; set; } = false;
	public static bool IsFunctionInliningEnabled { get; set; } = false;
	public static bool IsGarbageCollectorEnabled { get; set; } = false;

	#region Components

	/// <summary>
	/// Creates a node tree representing the specified components
	/// </summary>
	public static Node Recreate(List<Component> components)
	{
		var result = Recreate(components.First());

		for (var i = 1; i < components.Count; i++)
		{
			var component = components[i];

			if (component is NumberComponent number_component)
			{
				if (Numbers.IsZero(number_component.Value))
				{
					continue;
				}

				var is_negative = number_component.Value is long a && a < 0L || number_component.Value is double b && b < 0.0;
				var number = new NumberNode(number_component.Value is long ? Assembler.Format : Format.DECIMAL, number_component.Value);

				result = is_negative
					? new OperatorNode(Operators.SUBTRACT).SetOperands(result, number.Negate())
					: new OperatorNode(Operators.ADD).SetOperands(result, number);
			}

			if (component is VariableComponent variable_component)
			{
				// When the coefficient is exactly zero (double), the variable can be ignored, meaning the inaccuracy of the comparison is expected
				if (Numbers.IsZero(variable_component.Coefficient))
				{
					continue;
				}

				var node = CreateVariableWithOrder(variable_component.Variable, variable_component.Order);
				bool is_coefficient_negative;

				// When the coefficient is exactly one (double), the coefficient can be ignored, meaning the inaccuracy of the comparison is expected
				if (variable_component.Coefficient is double b)
				{
					is_coefficient_negative = b < 0.0;

					if (Math.Abs(b) != 1.0)
					{
						node = new OperatorNode(Operators.MULTIPLY).SetOperands(
							node,
							new NumberNode(Format.DECIMAL, Math.Abs(b))
						);
					}
				}
				else
				{
					is_coefficient_negative = (long)variable_component.Coefficient < 0;

					if (Math.Abs((long)variable_component.Coefficient) != 1L)
					{
						node = new OperatorNode(Operators.MULTIPLY).SetOperands(
							node,
							new NumberNode(Assembler.Format, Math.Abs((long)variable_component.Coefficient))
						);
					}
				}

				result = new OperatorNode(is_coefficient_negative ? Operators.SUBTRACT : Operators.ADD).SetOperands(result, node);
			}

			if (component is ComplexComponent complex_component)
			{
				result = new OperatorNode(complex_component.IsNegative ? Operators.SUBTRACT : Operators.ADD).SetOperands(
					result,
					complex_component.Node.Clone()
				);
			}

			if (component is VariableProductComponent product)
			{
				var is_negative = product.Coefficient is long a && a < 0L || product.Coefficient is double b && b < 0.0;

				if (is_negative)
				{
					product.Negate();
				}

				var other = Recreate(product);

				result = is_negative
					? new OperatorNode(Operators.SUBTRACT).SetOperands(result, other)
					: new OperatorNode(Operators.ADD).SetOperands(result, other);
			}
		}

		return result;
	}

	/// <summary>
	/// Builds a node tree representing a variable with an order
	/// </summary>
	public static Node CreateVariableWithOrder(Variable variable, int order)
	{
		if (order == 0)
		{
			return new NumberNode(Assembler.Format, 1L);
		}

		var result = (Node)new VariableNode(variable);

		for (var i = 1; i < Math.Abs(order); i++)
		{
			result = new OperatorNode(Operators.MULTIPLY).SetOperands(result, new VariableNode(variable));
		}

		if (order < 0)
		{
			result = new OperatorNode(Operators.DIVIDE).SetOperands(new NumberNode(Assembler.Format, 1L), result);
		}

		return result;
	}
	
	/// <summary>
	/// If the coefficient is a decimal, decimal format is returned, otherwise the default integer format is returned
	/// </summary>
	public static Format GetCoefficientFormat(object coefficient)
	{
		return coefficient is double ? Format.DECIMAL : Assembler.Format;
	}

	/// <summary>
	/// Creates a node tree representing the specified component
	/// </summary>
	public static Node Recreate(Component component)
	{
		if (component is NumberComponent number_component)
		{
			return new NumberNode(GetCoefficientFormat(number_component.Value), number_component.Value);
		}

		if (component is VariableComponent variable_component)
		{
			if (Numbers.IsZero(variable_component.Coefficient))
			{
				return new NumberNode(GetCoefficientFormat(variable_component.Coefficient), variable_component.Coefficient);
			}

			var result = CreateVariableWithOrder(variable_component.Variable, variable_component.Order);

			return !Numbers.IsOne(variable_component.Coefficient)
				? new OperatorNode(Operators.MULTIPLY).SetOperands(result, new NumberNode(GetCoefficientFormat(variable_component.Coefficient), variable_component.Coefficient))
				: result;
		}

		if (component is ComplexComponent complex_component)
		{
			return complex_component.IsNegative ? new NegateNode(complex_component.Node) : complex_component.Node;
		}

		if (component is VariableProductComponent product)
		{
			var result = CreateVariableWithOrder(product.Variables.First().Variable, product.Variables.First().Order);

			for (var i = 1; i < product.Variables.Count; i++)
			{
				var variable = product.Variables[i];

				result = new OperatorNode(Operators.MULTIPLY).SetOperands(result, CreateVariableWithOrder(variable.Variable, variable.Order));
			}

			return !Numbers.Equals(product.Coefficient, 1L)
				? new OperatorNode(Operators.MULTIPLY).SetOperands(result, new NumberNode(GetCoefficientFormat(product.Coefficient), product.Coefficient))
				: result;
		}

		throw new NotImplementedException("Unsupported component encountered while recreating");
	}

	/// <summary>
	/// Negates the all the specified components using their internal negation method
	/// </summary>
	public static List<Component> Negate(List<Component> components)
	{
		components.ForEach(c => c.Negate());
		return components;
	}

	/// <summary>
	/// Returns a component list which describes the specified expression
	/// </summary>
	public static List<Component> CollectComponents(Node expression)
	{
		var result = new List<Component>();

		if (expression.Is(NodeType.NUMBER))
		{
			result.Add(new NumberComponent(expression.To<NumberNode>().Value));
		}
		else if (expression.Is(NodeType.VARIABLE))
		{
			result.Add(new VariableComponent(expression.To<VariableNode>().Variable));
		}
		else if (expression.Is(NodeType.OPERATOR))
		{
			result.AddRange(CollectComponents(expression.To<OperatorNode>()));
		}
		else if (expression.Is(NodeType.CONTENT))
		{
			if (!Equals(expression.First, null))
			{
				result.AddRange(CollectComponents(expression.First));
			}
		}
		else if (expression.Is(NodeType.NEGATE))
		{
			result.AddRange(Negate(CollectComponents(expression.First!)));
		}
		else
		{
			result.Add(new ComplexComponent(expression));
		}

		return result;
	}

	/// <summary>
	/// Returns a component list which describes the specified operator node
	/// </summary>
	public static List<Component> CollectComponents(OperatorNode node)
	{
		var left_components = CollectComponents(node.Left);
		var right_components = CollectComponents(node.Right);

		if (Equals(node.Operator, Operators.ADD))
		{
			return SimplifyAddition(left_components, right_components);
		}

		if (Equals(node.Operator, Operators.SUBTRACT))
		{
			return SimplifySubtraction(left_components, right_components);
		}

		if (Equals(node.Operator, Operators.MULTIPLY))
		{
			return SimplifyMultiplication(left_components, right_components);
		}

		if (Equals(node.Operator, Operators.DIVIDE))
		{
			return SimplifyDivision(left_components, right_components);
		}

		return new List<Component>
		{
			new ComplexComponent(new OperatorNode(node.Operator).SetOperands(Recreate(left_components), Recreate(right_components)))
		};
	}

	/// <summary>
	/// Tries to simplify the specified components
	/// </summary>
	public static List<Component> Simplify(List<Component> components)
	{
		if (components.Count <= 1)
		{
			return components;
		}

		for (var i = 0; i < components.Count; i++)
		{
			var current = components[i];

			// Start iterating from the next component
			for (var j = i + 1; j < components.Count;)
			{
				var result = current + components[j];

				// Move to the next component if the two components could not be added together
				if (result == null)
				{
					j++;
					continue;
				}

				// Remove the components added together
				components.RemoveAt(j);
				components.RemoveAt(i);

				// Apply the changes
				components.Insert(i, result);
				current = result;
			}
		}

		return components;
	}

	/// <summary>
	/// Simplifies the addition between the specified operands
	/// </summary>
	public static List<Component> SimplifyAddition(List<Component> left_components, List<Component> right_components)
	{
		return Simplify(left_components.Concat(right_components).ToList());
	}

	/// <summary>
	/// Simplifies the subtraction between the specified operands
	/// </summary>
	public static List<Component> SimplifySubtraction(List<Component> left_components, List<Component> right_components)
	{
		Negate(right_components);

		return SimplifyAddition(left_components, right_components);
	}

	/// <summary>
	/// Simplifies the multiplication between the specified operands
	/// </summary>
	public static List<Component> SimplifyMultiplication(List<Component> left_components, List<Component> right_components)
	{
		var components = new List<Component>();

		foreach (var left_component in left_components)
		{
			foreach (var right_component in right_components)
			{
				var result = left_component * right_component ?? new ComplexComponent(
					new OperatorNode(Operators.MULTIPLY).SetOperands(Recreate(left_component), Recreate(right_component))
				);

				components.Add(result);
			}
		}

		return Simplify(components);
	}

	/// <summary>
	/// Simplifies the division between the specified operands
	/// </summary>
	public static List<Component> SimplifyDivision(List<Component> left_components, List<Component> right_components)
	{
		if (left_components.Count == 1 && right_components.Count == 1)
		{
			var result = left_components.First() / right_components.First();

			if (result != null)
			{
				return new List<Component> { result };
			}
		}

		return new List<Component>
		{
			new ComplexComponent(new OperatorNode(Operators.DIVIDE).SetOperands(Recreate(left_components), Recreate(right_components)))
		};
	}

	/// <summary>
	/// Tries to simplify the specified node
	/// </summary>
	public static Node GetSimplifiedValue(Node value)
	{
		var components = CollectComponents(value);
		var simplified = Recreate(components);

		return simplified;
	}

	/// <summary>
	/// Finds comparisons and tries to simplify them.
	/// Returns whether any modifications were done.
	/// </summary>
	public static bool OptimizeComparisons(Node root)
	{
		var comparisons = root.FindAll(i => i.Is(OperatorType.COMPARISON));
		var precomputed = false;

		foreach (var comparison in comparisons)
		{
			var left = CollectComponents(comparison.First!);
			var right = CollectComponents(comparison.Last!);

			var i = 0;
			var j = 0;

			while (left.Any() && right.Any() && i < left.Count)
			{
				if (j >= right.Count)
				{
					i++;
					j = 0;
					continue;
				}

				var x = left[i];
				var y = right[j];

				if (x is ComplexComponent)
				{
					i++;
					j = 0;
					continue;
				}
				else if (y is ComplexComponent)
				{
					j++;
					continue;
				}

				var s = x - y;

				if (s != null)
				{
					left.RemoveAt(i);
					right.RemoveAt(j);

					left.Insert(i, s);

					j = 0;
				}
				else
				{
					j++;
				}
			}

			if (!left.Any())
			{
				left.Add(new NumberComponent(0L));
			}

			if (!right.Any())
			{
				right.Add(new NumberComponent(0L));
			}

			left = Simplify(left);
			right = Simplify(right);

			comparison.First!.Replace(Recreate(left));
			comparison.Last!.Replace(Recreate(right));

			var evaluation = Evaluator.TryEvaluateOperator(comparison.To<OperatorNode>());

			if (evaluation != null)
			{
				comparison.Replace(new NumberNode(Parser.Format, (bool)evaluation ? 1L : 0L, comparison.Position));
				precomputed = true;
			}
		}

		var precomputations = root.FindAll(i => i.Is(OperatorType.LOGIC));
		precomputations.Reverse();

		foreach (var condition in precomputations)
		{
			var parent = condition.Parent;

			if (condition.Left.Is(NodeType.NUMBER))
			{
				var passes = !Numbers.IsZero(condition.Left.To<NumberNode>().Value);
				if (condition.Is(Operators.AND)) condition.Replace(passes ? condition.Right : condition.Left);
				else condition.Replace(passes ? condition.Left : condition.Right);
				precomputed = true;
				continue;
			}

			if (condition.Right.Is(NodeType.NUMBER))
			{
				var passes = !Numbers.IsZero(condition.Right.To<NumberNode>().Value);
				if (condition.Is(Operators.AND)) condition.Replace(passes ? condition.Left : condition.Right);
				else condition.Replace(passes ? condition.Left : condition.Right);
				precomputed = true;
			}
		}

		return precomputed;
	}
	
	/// <summary>
	/// Tries to optimize all expressions in the specified node tree
	/// </summary>
	public static void OptimizeAllExpressions(Node root)
	{
		// Find all top level operators
		var expressions = root.FindTop(i => i.Is(OperatorType.CLASSIC) || i.Is(NodeType.NEGATE));

		foreach (var expression in expressions)
		{
			// Replace the expression with a simplified version
			expression.Replace(GetSimplifiedValue(expression));
		}
	}

	#endregion

	#region Nodes

	/// <summary>
	/// Returns whether the specified node is primitive that is whether it contains only operators, numbers, parameter- or local variables
	/// </summary>
	/// <returns>True if the definition is primitive, otherwise false</returns>
	public static bool IsPrimitive(Node node)
	{
		return node.Find(n => !(n.Is(NodeType.NUMBER) || n.Is(NodeType.OPERATOR) || n.Is(NodeType.VARIABLE) && n.To<VariableNode>().Variable.IsPredictable)) == null;
	}

	/// <summary>
	/// Finds the branch which contains the specified node
	/// </summary>
	private static Node? GetBranch(Node node)
	{
		return node.FindParent(NodeType.LOOP, NodeType.IF, NodeType.ELSE_IF, NodeType.ELSE);
	}

	/// <summary>
	/// If the specified node represents a conditional branch, this function appends the other branches to the specified denylist
	/// </summary>
	private static void DenyOtherBranches(List<Node> denylist, Node node)
	{
		if (node.Is(NodeType.IF))
		{
			denylist.AddRange(node.To<IfNode>().GetBranches().Where(i => i != node));
		}
		else if (node.Is(NodeType.ELSE_IF))
		{
			denylist.AddRange(node.To<ElseIfNode>().GetRoot().GetBranches().Where(i => i != node));
		}
		else if (node.Is(NodeType.ELSE))
		{
			denylist.AddRange(node.To<ElseNode>().GetRoot().GetBranches().Where(i => i != node));
		}
	}

	/// <summary>
	/// Returns whether the specified perspective is inside the condition of the specified branch
	/// </summary>
	private static bool IsInsideBranchCondition(Node perspective, Node branch)
	{
		if (branch.Is(NodeType.IF))
		{
			return perspective == branch.To<IfNode>().GetConditionStep() || perspective.IsUnder(branch.To<IfNode>().GetConditionStep());
		}
		else if (branch.Is(NodeType.ELSE_IF))
		{
			return perspective == branch.To<ElseIfNode>().GetConditionStep() || perspective.IsUnder(branch.To<ElseIfNode>().GetConditionStep());
		}
		else if (branch.Is(NodeType.LOOP))
		{
			return perspective == branch.To<LoopNode>().GetConditionStep() || perspective.IsUnder(branch.To<LoopNode>().GetConditionStep()); 
		}

		return false;
	}

	/// <summary>
	/// Returns nodes whose contents should be taken into account if execution were to start from the specified perspective
	/// </summary>
	public static List<Node> GetDenylist(Node perspective)
	{
		var denylist = new List<Node>();
		var branch = perspective;

		while ((branch = GetBranch(branch)) != null)
		{
			// If the perspective is inside the condition of the branch, it can still enter the other branches
			if (IsInsideBranchCondition(perspective, branch)) continue;

			DenyOtherBranches(denylist, branch);
		}

		return denylist;
	}

	/// <summary>
	/// Returns whether the specified variable will be used in the future starting from the specified node perspective
	/// NOTE: Usually the perspective node is a branch but it is not counted as one.
	/// This behavior is required for determining active variables when there is an if-statement followed by an else-if-statement and both of the conditions use same variables.
	/// </summary>
	public static bool IsUsedLater(Variable variable, Node perspective, bool self = false)
	{
		// Get a denylist which describes which sections of the node tree have not been executed in the past or will not be executed in the future
		var denylist = GetDenylist(perspective);

		// If the it is allowed to count the perspective as a branch as well, append the other branches to the denylist
		if (self) DenyOtherBranches(denylist, perspective);

		// If any of the references is placed after the specified perspective, the variable is needed
		if (variable.References.Any(i => !denylist.Any(j => i.IsUnder(j)) && i.IsAfter(perspective))) return true;

		return perspective.FindParent(NodeType.LOOP) != null;
	}

	/// <summary>
	/// Returns all operator nodes which are first encounter when descending from the specified node
	/// </summary>
	private static List<OperatorNode> FindTopLevelOperators(Node node)
	{
		if (node.Is(NodeType.OPERATOR) && node.To<OperatorNode>().Operator.Type == OperatorType.CLASSIC)
		{
			return new List<OperatorNode> { (OperatorNode)node };
		}

		var operators = new List<OperatorNode>();
		var child = node.First;

		while (child != null)
		{
			if (child.Is(NodeType.OPERATOR))
			{
				var operation = child.To<OperatorNode>();

				if (operation.Operator.Type != OperatorType.CLASSIC)
				{
					operators.AddRange(FindTopLevelOperators(operation.Left));
					operators.AddRange(FindTopLevelOperators(operation.Right));
				}
				else
				{
					operators.Add(operation);
				}
			}
			else
			{
				operators.AddRange(FindTopLevelOperators(child));
			}

			child = child.Next;
		}

		return operators;
	}

	/// <summary>
	/// Approximates the complexity of the specified node tree to execute
	/// </summary>
	public static long GetCost(Node node)
	{
		var result = 0L;
		var iterator = node.First;

		while (iterator != null)
		{
			if (iterator.Is(NodeType.OPERATOR))
			{
				var operation = iterator.To<OperatorNode>().Operator;

				if (operation == Operators.ADD)
				{
					result += ADDITION_COST;
				}
				else if (operation == Operators.SUBTRACT)
				{
					result += SUBTRACTION_COST;
				}
				else if (operation == Operators.MULTIPLY)
				{
					var multipliers = iterator.Where(i => i.Is(NodeType.NUMBER)).Cast<NumberNode>();

					// Take into account that power of two multiplications are significantly faster
					if (multipliers.Any(i => !i.Type.IsDecimal() && Common.IsPowerOfTwo((long)i.Value)))
					{
						result += POWER_OF_TWO_MULTIPLICATION_COST;
					}
					else
					{
						result += MULTIPLICATION_COST;
					}
				}
				else if (operation == Operators.DIVIDE)
				{
					var divisor = iterator.Right.Is(NodeType.NUMBER) ? iterator.Right.To<NumberNode>() : null;

					// Take into account that power of two division are significantly faster
					if (divisor != null && !divisor.Type.IsDecimal() && Common.IsPowerOfTwo((long)divisor.Value))
					{
						result += POWER_OF_TWO_DIVISION_COST;
					}
					else
					{
						result += DIVISION_COST;
					}
				}
				else
				{
					result += STANDARD_OPERATOR_COST;
				}
			}
			else if (iterator.Is(NodeType.LINK, NodeType.OFFSET))
			{
				result += MEMORY_ACCESS_COST;
			}
			else if (iterator.Is(NodeType.IF, NodeType.ELSE_IF, NodeType.ELSE))
			{
				result += CONDITIONAL_JUMP_COST;
			}
			else if (iterator.Is(NodeType.LOOP))
			{
				result += CONDITIONAL_JUMP_COST;
				result += GetCost(iterator) * UnwrapmentAnalysis.MAXIMUM_LOOP_UNWRAP_STEPS;

				iterator = iterator.Next;
				continue;
			}
			else if (iterator.Is(NodeType.VARIABLE))
			{
				result += VARIABLE_ACCESS_COST;
				iterator = iterator.Next;
				continue;
			}
			else if (iterator.Is(NodeType.NEGATE))
			{
				result += STANDARD_OPERATOR_COST;
				iterator = iterator.Next;
				continue;
			}

			result += GetCost(iterator);

			iterator = iterator.Next;
		}

		return result;
	}

	#endregion

	/// <summary>
	/// Iterates through the constructors and destructors of the specified type and adds default calls to them
	/// </summary>
	private static void AddDefaultConstructorCalls(Type type, SortedSet<Type> denylist)
	{
		// 1. Do not process the specified type if it is already processed
		// 2. Primitive types do not have constructors or destructors, so skip them
		if (denylist.Contains(type) || !type.Constructors.Overloads.Any()) return;

		// Do not process the specified type again later
		denylist.Add(type);

		var all = type.Constructors.Overloads.Concat(type.Destructors.Overloads).SelectMany(i => i.Implementations).Where(i => i.Node != null);

		foreach (var iterator in all)
		{
			var supertypes = new List<Type>(type.Supertypes);
			supertypes.Reverse();

			foreach (var supertype in supertypes)
			{
				// If the supertype is not processed yet, process it now
				AddDefaultConstructorCalls(supertype, denylist);

				// Get all the constructor or destructor overloads of the current supertype
				var overloads = (iterator.IsConstructor ? supertype.Constructors : supertype.Destructors).Overloads;

				// Check if there is already a function call using any of the overloads above, if so, no need to generate another call
				var calls = iterator.Node!.FindAll(NodeType.FUNCTION).Cast<FunctionNode>();
				
				if (calls.Any(i => overloads.Contains(i.Function.Metadata) && ReconstructionAnalysis.IsUsingLocalSelfPointer(i))) continue;

				// Get the implementation which requires no arguments
				var implementation = (iterator.IsConstructor ? supertype.Constructors : supertype.Destructors).GetImplementation();

				// 1. If such implementation can not be found, no automatic call for the current supertype can be generated
				// 2. If the implementation is empty, there is now use calling it
				if (implementation == null || implementation.IsEmpty) continue;

				// Next try to get the self pointer, this should not fail
				var self = Common.GetSelfPointer(iterator, null);

				if (self == null) continue;

				// Add the default call
				if (iterator.IsConstructor)
				{
					iterator.Node!.Insert(iterator.Node.First, new LinkNode(self, new FunctionNode(implementation)));
				}
				else
				{
					iterator.Node!.Add(new LinkNode(self, new FunctionNode(implementation)));
				}
			}
		}
	}

	/// <summary>
	/// Adds logic for allocating an instance of the specified type, registering virtual functions and initializing member variables to the constructors of the specified type
	/// </summary>
	private static void CompleteConstructors(Type type)
	{
		if (type.Configuration == null) return;

		var expressions = new List<OperatorNode>(type.Initialization);

		foreach (var constructor in type.Constructors.Overloads.SelectMany(i => i.Implementations).Where(i => i.Node != null))
		{
			var self = Common.GetSelfPointer(constructor, null);

			foreach (var iterator in expressions)
			{
				var expression = iterator.Clone().To<OperatorNode>();
				var members = expression.FindAll(i => i.Is(NodeType.VARIABLE) && i.To<VariableNode>().Variable.IsMember && !i.Parent!.Is(NodeType.LINK));

				foreach (var member in members)
				{
					member.Replace(new LinkNode(self.Clone(), member.Clone()));
				}

				var position = constructor.Node!.First;

				if (position == null)
				{
					constructor.Node!.Add(expression);
				}
				else
				{
					constructor.Node!.Insert(position, expression);
				}
			}
		}
	}

	/// <summary>
	/// Adds logic for deallocating an instance of the specified type
	/// </summary>
	private static void CompleteDestructors(Type type)
	{
		if (!IsGarbageCollectorEnabled) return;
		
		foreach (var destructor in type.Destructors.Overloads.SelectMany(i => i.Implementations))
		{
			var root = destructor.Node ?? throw new ApplicationException("Destructor was not implemented");
			var self = destructor.GetSelfPointer() ?? throw new ApplicationException("Missing self pointer");

			foreach (var member in type.Variables.Values)
			{
				// If the member is not destructible, it is not unlinkable, so skip it
				if (member.IsStatic || !member.Type!.IsUserDefined) continue;

				// Unlink the member variable
				var implementation = Parser.UnlinkFunction!.Get(member.Type!) ?? throw new ApplicationException("Missing unlink function overload");;
				root.Add(new FunctionNode(implementation).SetArguments(new Node { new LinkNode(new VariableNode(self), new VariableNode(member)) }));
			}
		}
	}

	/// <summary>
	/// Evaluates the values of size nodes
	/// </summary>
	private static void CompleteInspections(Node root)
	{
		var inspections = root.FindAll(NodeType.INSPECTION).Cast<InspectionNode>();

		foreach (var inspection in inspections)
		{
			var type = inspection.Object.GetType();

			if (inspection.Type == InspectionType.NAME)
			{
				inspection.Replace(new StringNode(type.ToString(), inspection.Position));
			}
			else if (inspection.Type == InspectionType.SIZE)
			{
				inspection.Replace(new NumberNode(Parser.Format, (long)type.AllocationSize));
			}
		}
	}

	/// <summary>
	/// Adds logic for allocating the instance, registering virtual functions and initializing member variables to the constructors of the specified type
	/// </summary>
	public static void Complete(Context context)
	{
		var denylist = new SortedSet<Type>();

		foreach (var type in Common.GetAllTypes(context))
		{
			AddDefaultConstructorCalls(type, denylist);
			CompleteConstructors(type);
			CompleteDestructors(type);
		}

		foreach (var implementation in Common.GetAllFunctionImplementations(context))
		{
			Complete(implementation);
			CompleteInspections(implementation.Node!);
		}
	}

	/// <summary>
	/// Tries to find nodes which access information which they should not.
	/// Historically context leaks have happened while inlining functions for example.
	/// This function is intended for debugging purposes.
	/// </summary>
	public static void CaptureContextLeaks(Context context, Node root)
	{
		var variables = root.FindAll(NodeType.VARIABLE).Cast<VariableNode>().Where(i => !i.Variable.IsConstant && i.Variable.IsPredictable && !i.Variable.Context.IsInside(context));

		// Try to find variables whose parent context is not defined inside the implementation, if even one is found it means something has leaked
		if (variables.Any()) { throw new ApplicationException("Found a context leak"); }

		var declarations = root.FindAll(NodeType.DECLARE).Cast<DeclareNode>().Where(i => !i.Variable.IsConstant && i.Variable.IsPredictable && !i.Variable.Context.IsInside(context));

		// Try to find declaration nodes whose variable is not defined inside the implementation, if even one is found it means something has leaked
		if (declarations.Any()) { throw new ApplicationException("Found a context leak"); }

		var subcontexts = root.FindAll(i => i is IScope && !i.Is(NodeType.TYPE)).Cast<IScope>().Where(i => !i.GetContext().IsInside(context));

		// Try to find declaration nodes whose variable is not defined inside the implementation, if even one is found it means something has leaked
		if (subcontexts.Any()) { throw new ApplicationException("Found a context leak"); }
	}

	/// <summary>
	/// Analyzes the specified context
	/// </summary>
	public static void Analyze(Bundle bundle, Context context)
	{
		var implementations = Common.GetAllFunctionImplementations(context).Where(i => !i.Metadata.IsImported).OrderByDescending(i => i.References.Count).ToList();
		var verbose = Assembler.IsVerboseOutputEnabled;
		var time = bundle.Get(ConfigurationPhase.OUTPUT_TIME, false);

		if (time || verbose) Console.WriteLine("1. Pass");

		// Optimize all function implementations
		for (var i = 0; i < implementations.Count; i++)
		{
			var implementation = implementations[i];

			var start = DateTime.UtcNow;
			
			// Reconstruct necessary nodes in the function implementation
			ReconstructionAnalysis.Reconstruct(implementation, implementation.Node!);

			// Do a safety check
			CaptureContextLeaks(implementation, implementation.Node!);

			// Now optimize the function implementation
			implementation.Node = GeneralAnalysis.Optimize(implementation, implementation.Node!);

			// Finish optimizing the function implementation
			ReconstructionAnalysis.Finish(implementation.Node!);

			if (implementation is LambdaImplementation lambda)
			{
				lambda.Seal();
			}

			var interval = (DateTime.UtcNow - start).TotalMilliseconds;

			if (time)
			{
				Console.WriteLine($"[{i + 1}/{implementations.Count}] {implementation.GetHeader()} [{interval} ms]");
			}
			else if (verbose)
			{
				Console.WriteLine($"[{i + 1}/{implementations.Count}] {implementation.GetHeader()}");
			}
		}

		if (!IsGarbageCollectorEnabled) return;
		if (time || verbose) Console.WriteLine("2. Pass");

		// Optimize all function implementations
		for (var i = 0; i < implementations.Count; i++)
		{
			var implementation = implementations[i];

			var start = DateTime.UtcNow;

			// Analyze variable usages
			Analyzer.ResetVariableUsages(implementation.Node!, implementation);
			Analyzer.AnalyzeVariableUsages(implementation.Node!, implementation);

			// Adds garbage collecting
			GarbageCollector.Generate(implementation);
			
			// Reconstruct necessary nodes in the function implementation
			ReconstructionAnalysis.Reconstruct(implementation, implementation.Node!);

			// Do a safety check
			CaptureContextLeaks(implementation, implementation.Node!);

			// Now optimize the function implementation
			implementation.Node = GeneralAnalysis.Optimize(implementation, implementation.Node!);

			// Finish optimizing the function implementation
			ReconstructionAnalysis.Finish(implementation.Node!);

			var interval = (DateTime.UtcNow - start).TotalMilliseconds;

			if (time)
			{
				Console.WriteLine($"[{i + 1}/{implementations.Count}] {implementation.GetHeader()} [{interval} ms]");
			}
			else if (verbose)
			{
				Console.WriteLine($"[{i + 1}/{implementations.Count}] {implementation.GetHeader()}");
			}
		}
	}
}