using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class VariableLoad
{
	public Variable Variable { get; private set; }
	public Result Result { get; private set; }

	public VariableLoad(Variable variable, Result reference)
	{
		Variable = variable;
		Result = reference;
	}
}

public class VariableUsageDescriptor
{
	public Variable Variable { get; }
	public Result? Reference { get; set; }
	public int Usages { get; }

	public VariableUsageDescriptor(Variable variable, int usages)
	{
		Variable = variable;
		Usages = usages;
	}
}

public sealed class Scope : IDisposable
{
	public const int CIRCULAR_SCOPE_THRESHOLD = 1000000;

	/// <summary>
	/// Returns whether the specified variable is a non-local variable
	/// </summary>
	private static bool IsNonLocalVariable(Variable variable, Context[] local_contexts)
	{
		return !local_contexts.Any(local_context => variable.Context.IsInside(local_context));
	}

	/// <summary>
	/// Analyzes how many times each variable in the specified node tree is used and sorts the result as well
	/// </summary>
	private static Dictionary<Variable, int> GetNonLocalVariableUsageCount(Unit unit, Node root, params Context[] local_contexts)
	{
		var variables = new Dictionary<Variable, int>();

		foreach (var iterator in root)
		{
			if (iterator.Is(NodeType.VARIABLE) && IsNonLocalVariable(iterator.To<VariableNode>().Variable, local_contexts))
			{
				var node = iterator.To<VariableNode>();

				if (node.Variable.IsPredictable)
				{
					variables[node.Variable] = variables.GetValueOrDefault(node.Variable, 0) + 1;
				}
				else if (node.Variable.IsConstant || node.Variable.Category == VariableCategory.GLOBAL)
				{
					continue;
				}
				else if (!node.Parent?.Is(NodeType.LINK) ?? false)
				{
					if (unit.Self == null) throw new ApplicationException("Missing self pointer");
					variables[unit.Self] = variables.GetValueOrDefault(unit.Self, 0) + 1;
				}
			}
			else
			{
				foreach (var usage in GetNonLocalVariableUsageCount(unit, iterator, local_contexts))
				{
					variables[usage.Key] = variables.GetValueOrDefault(usage.Key, 0) + usage.Value;
				}
			}
		}

		return variables;
	}

	/// <summary>
	/// Returns information about variable usage in the specified loop
	/// </summary>
	private static List<VariableUsageDescriptor> GetAllVariableUsages(Unit unit, Node[] roots, Context[] contexts)
	{
		if (roots.Length != contexts.Length) throw new ArgumentException("Each root must have a corresponding context");

		var result = new Dictionary<Variable, int>();

		for (var i = 0; i < roots.Length; i++)
		{
			// Get all non-local variables in the loop and their number of usages
			foreach (var j in GetNonLocalVariableUsageCount(unit, roots[i], contexts[i]))
			{
				result[j.Key] = result.GetValueOrDefault(j.Key, 0) + j.Value;
			}
		}

		// Take only those variables which are initialized
		var descriptors = result.Where(i => unit.IsInitialized(i.Key)).Select(i => new VariableUsageDescriptor(i.Key, i.Value)).ToList();

		// Sort the variables based on their number of usages (most used variable first)
		descriptors.Sort((a, b) => -a.Usages.CompareTo(b.Usages));

		return descriptors;
	}

	/// <summary>
	/// Tries to move most used loop variables into registers
	/// </summary>
	public static void Cache(Unit unit, LoopNode node)
	{
		var variables = GetAllVariableUsages(unit, new[] { node }, new[] { node.Body.Context });

		// If the loop contains at least one function, the variables should be cached into non-volatile registers
		// NOTE: Otherwise there would be a lot of register moves trying to save the cached variables
		var non_volatile_mode = node.Find(NodeType.FUNCTION, NodeType.CALL) != null;

		unit.Append(new CacheVariablesInstruction(unit, new[] { node }, variables, non_volatile_mode));
	}

	/// <summary>
	/// Tries to move most used loop variables into registers
	/// </summary>
	public static void Cache(Unit unit, Node[] roots, Context[] contexts, Context current)
	{
		var variables = GetAllVariableUsages(unit, roots, contexts);

		// Ensure the variables are declared in the current context or in one of its parents
		variables = variables.Where(i => current.IsInside(i.Variable.Context)).ToList();

		// If the loop contains at least one function, the variables should be cached into non-volatile registers
		// NOTE: Otherwise there would be a lot of register moves trying to save the cached variables
		var non_volatile_mode = roots.Any(i => i.Find(NodeType.FUNCTION, NodeType.CALL) != null);

		unit.Append(new CacheVariablesInstruction(unit, roots, variables, non_volatile_mode));
	}

	/// <summary>
	/// Returns all variables in the given node tree that are not declared in the given local context
	/// </summary>
	public static IEnumerable<Variable> GetAllNonLocalVariables(Node[] roots, params Context[] local_contexts)
	{
		return roots.SelectMany(i => i.FindAll(NodeType.VARIABLE)
				 .Select(i => i.To<VariableNode>().Variable)
				 .Where(i => i.IsPredictable && IsNonLocalVariable(i, local_contexts)))
				 .Distinct();
	}

	/// <summary>
	/// Returns all variables that the scope must take care of
	/// </summary>
	public static List<Variable> GetAllActiveVariables(Unit unit, Node[] roots)
	{
		var actives = new List<Variable>();

		foreach (var variable in unit.Scope!.Variables.Keys)
		{
			// 1. If the variable is used inside any of the roots, it must be included
			// 2. If the variable is used after any of the roots, it must be included
			if (variable.References.Any(i => roots.Any(j => i.IsUnder(j))) || roots.Any(i => Analysis.IsUsedLater(variable, i)))
			{
				actives.Add(variable);
			}
		}

		return actives;
	}

	/// <summary>
	/// Returns all variables that the scope must take care of
	/// </summary>
	public static List<Variable> GetAllActiveVariables(Unit unit, Node root)
	{
		return GetAllActiveVariables(unit, new[] { root });
	}

	private static Context[] GetTopLocalContexts(Node root)
	{
		return root.FindTop(i => i.Is(NodeType.INLINE)).Where(i => i.To<InlineNode>().IsContext).Cast<ContextInlineNode>().Select(i => i.Context).ToArray();
	}

	/// <summary>
	/// Loads constants which might be edited inside the specified root
	/// </summary>
	public static void LoadConstants(Unit unit, Node root, params Context[] contexts)
	{
		var local_contexts = GetTopLocalContexts(root).Concat(contexts).ToArray();

		// Find all variables inside the root node which are edited
		var edited_variables = GetAllNonLocalVariables(new Node[] { root }, local_contexts).Where(i => i.IsEditedInside(root));

		// All edited variables that are constants must be moved to registers or into memory
		foreach (var variable in edited_variables)
		{
			unit.Append(new SetModifiableInstruction(unit, variable));
		}
	}

	/// <summary>
	/// Loads constants which might be edited inside the specified root
	/// </summary>
	public static void LoadConstants(Unit unit, IfNode root)
	{
		var edited_variables = new List<Variable>();
		var iterator = (Node?)root;
		var local_contexts = GetTopLocalContexts(root);

		while (iterator != null)
		{
			// Find all variables inside the root node which are edited
			edited_variables.AddRange(GetAllNonLocalVariables(new Node[] { iterator }, local_contexts).Where(i => i.IsEditedInside(iterator)));

			if (iterator.Is(NodeType.ELSE_IF))
			{
				iterator = root.To<IfNode>().Successor;

				// Retrieve the context of the successor
				if (iterator != null)
				{
					local_contexts = GetTopLocalContexts(iterator);
				}
			}
			else
			{
				break;
			}
		}

		// Remove duplicate variables
		edited_variables = edited_variables.Distinct().ToList();

		// All edited variables that are constants must be moved to registers or into memory
		foreach (var variable in edited_variables)
		{
			unit.Append(new SetModifiableInstruction(unit, variable));
		}
	}

	public Node Root { get; }

	private List<VariableLoad> Loads { get; set; } = new List<VariableLoad>();
	private HashSet<Variable> Initializers { get; set; } = new HashSet<Variable>();
	private HashSet<Variable> Finalizers { get; set; } = new HashSet<Variable>();

	public List<Variable> Actives { get; } = new List<Variable>();
	public Dictionary<Variable, Result> Variables { get; } = new Dictionary<Variable, Result>();
	public Dictionary<Variable, Result> Transferers { get; } = new Dictionary<Variable, Result>();

	public Unit? Unit { get; private set; }
	public Instruction? End { get; private set; }
	public Scope? Outer { get; private set; } = null;

	/// <summary>
	/// Creates a scope with variables that are returned to their original locations once the scope is exited
	/// </summary>
	public Scope(Unit unit, Node root, IEnumerable<Variable>? active_variables = null)
	{
		Root = root;
		Actives = active_variables?.ToList() ?? new List<Variable>();
		Enter(unit);
	}

	/// <summary>
	/// Sets or creates the handle for the specified variable
	/// </summary>
	private Result SetOrCreateTransitionHandle(Variable variable, Handle handle, Format format)
	{
		if (!variable.IsPredictable)
		{
			throw new InvalidOperationException("Tried to create transition handle for an unpredictable variable");
		}

		handle = handle.Finalize();

		if (Transferers.TryGetValue(variable, out Result? transferrer))
		{
			transferrer.Value = handle;
			transferrer.Format = format;
		}
		else
		{
			transferrer = new Result(handle, format);
			Transferers.Add(variable, transferrer);
		}

		// Update the current handle to the variable
		Variables[variable] = transferrer;

		// If the transferrer is a register, the transferrer value must be attached there
		if (transferrer.Value.Is(HandleInstanceType.REGISTER))
		{
			transferrer.Value.To<RegisterHandle>().Register.Handle = transferrer;
		}

		return transferrer;
	}

	/// <summary>
	/// Ensure that the current scope is not circular by iterating parent scopes for limited number of times
	/// </summary>
	[Conditional("DEBUG")]
	public void CaptureCircularScopes()
	{
		var iterator = this;
		var limit = CIRCULAR_SCOPE_THRESHOLD;

		while (iterator != null)
		{
			iterator = iterator.Outer;

			if (iterator == this || limit-- == 0)
			{
				throw new ApplicationException("Captured a circular scope");
			}
		}
	}

	/// <summary>
	/// Assigns a register or a stack address for the specified parameter depending on the situation
	/// </summary>
	private void ReceiveParameter(List<Register> standard_parameter_registers, List<Register> decimal_parameter_registers, Variable parameter)
	{
		if (parameter.Type!.IsPack)
		{
			var representives = Common.GetPackRepresentives(parameter);
			foreach (var representive in representives) { ReceiveParameter(standard_parameter_registers, decimal_parameter_registers, representive); }
			return;
		}

		var register = parameter.Type!.Format.IsDecimal() ? decimal_parameter_registers.Pop() : standard_parameter_registers.Pop();

		if (register != null)
		{
			register.Handle = SetOrCreateTransitionHandle(parameter, new RegisterHandle(register), parameter.GetRegisterFormat());
		}
		else
		{
			SetOrCreateTransitionHandle(parameter, References.CreateVariableHandle(Unit!, parameter), parameter.GetRegisterFormat());
		}
	}

	/// <summary>
	/// Switches the current scope to this scope
	/// </summary>
	public void Enter(Unit unit)
	{
		Unit = unit;

		// Reset variable data
		Reset();

		// Save the outer scope so that this scope can be exited later
		if (unit.Scope != this) { Outer = unit.Scope; }

		// Detect if there are new variables to load
		if (Loads.Count != Actives.Count)
		{
			var instruction = new RequireVariablesInstruction(unit, Actives);
			instruction.Description = "Requires variables to enter a scope";
			Unit.Append(instruction);
			Loads = instruction.Variables.Zip(instruction.Dependencies!).Select(i => new VariableLoad(i.First, i.Second)).ToList();
		}

		if (Unit.Mode == UnitMode.BUILD)
		{
			foreach (var load in Loads)
			{
				var result = load.Result;

				// 1. Load all memory handles into registers which do not use the stack
				var is_complex_memory_address = result.IsMemoryAddress && !result.Value.Is(HandleInstanceType.STACK_MEMORY, HandleInstanceType.STACK_VARIABLE, HandleInstanceType.TEMPORARY_MEMORY);

				// 2. Load all expressions into registers
				var is_expression = result.Value.Instance == HandleInstanceType.EXPRESSION || result.Value.Instance == HandleInstanceType.INLINE;

				if (is_complex_memory_address || is_expression)
				{
					Memory.MoveToRegister(Unit, result, Assembler.Size, result.Format.IsDecimal(), Trace.GetDirectives(Unit, result));
				}
			}
		}

		// Switch the current unit scope to be this scope
		Unit.Scope = this;

		// Connect to the outer scope if it exists
		if (Outer != null)
		{
			for (var i = 0; i < Actives.Count; i++)
			{
				var variable = Actives[i];
				var external_handle = Loads[i].Result;

				SetOrCreateTransitionHandle(variable, external_handle.Value, external_handle.Format);
			}

			if (!Initializers.Any())
			{
				var instruction = new RequireVariablesInstruction(unit, Actives);
				instruction.Description = "Initializes outer scope variables";
				Unit.Append(instruction);
				Actives.ForEach(i => Initializers.Add(i));
			}

			// Get all the register which hold any active variable
			var denylist = Variables.Values.Where(i => i.Value.Is(HandleInstanceType.REGISTER)).Select(i => i.Value.To<RegisterHandle>().Register);
			var registers = Unit.NonReservedRegisters.Where(i => !denylist.Contains(i));

			// All register which do not hold active variables must be reset since they would disturb the execution of the scope
			foreach (var register in registers)
			{
				register.Reset();
			}
		}
		else if (!Assembler.IsDebuggingEnabled)
		{
			// Move all parameters to their expected registers since this is the first scope
			var decimal_parameter_registers = unit.MediaRegisters.Take(Calls.GetMaxMediaRegisterParameters()).ToList();
			var standard_parameter_registers = Calls.GetStandardParameterRegisters().Select(name => unit.Registers.Find(r => r[Size.QWORD] == name)!).ToList();

			if ((unit.Function.IsMember && !unit.Function.IsStatic) || unit.Function.IsLambdaImplementation)
			{
				ReceiveParameter(standard_parameter_registers, decimal_parameter_registers, unit.Self ?? throw new ApplicationException("Missing self pointer"));
			}

			foreach (var parameter in unit.Function.Parameters)
			{
				ReceiveParameter(standard_parameter_registers, decimal_parameter_registers, parameter);
			}
		}

		CaptureCircularScopes();
	}

	/// <summary>
	/// Returns the current handle of the specified variable, if one is present
	/// </summary>
	public Result? GetVariableValue(Variable variable, bool recursive = true)
	{
		// Only predictable variables are allowed to be cached
		if (!variable.IsPredictable) return null;

		// First check if the variable handle list already exists
		if (Variables.TryGetValue(variable, out Result? handle))
		{
			// When debugging is enabled, all variables should be stored in stack, which is the default location if this function returns null
			/// NOTE: Disposable handles assigned to local variables are an exception to this rule, the values inside them must be extracted to invidual local variables
			return (Assembler.IsDebuggingEnabled && handle.Value.Instance != HandleInstanceType.DISPOSABLE_PACK) ? null : handle;
		}
		else if (recursive)
		{
			handle = Outer?.GetVariableValue(variable);
			if (handle != null) { Variables.Add(variable, handle); }

			return handle;
		}

		return null;
	}

	/// <summary>
	/// Switches back to the outer scope
	/// </summary>
	public void Exit()
	{
		if (Unit == null) throw new ApplicationException("Unit was not assigned to a scope or the scope was never entered");

		if (!Finalizers.Any())
		{
			var instruction = new RequireVariablesInstruction(Unit, Actives.Where(i => Analysis.IsUsedLater(i, Root)).ToList());
			instruction.Description = "Keeps outer scope variables active across the scope";
			Unit.Append(instruction);
			Actives.ForEach(i => Finalizers.Add(i));
		}

		if (End == null)
		{
			End = new Instruction(Unit, InstructionType.NORMAL) { Description = "Marks the end of a scope" };
			End.IsAbstract = true;
			End.Execute();
		}

		// Exit to the outer scope
		Unit.Scope = Outer ?? this;

		// Reset all registers
		Unit.Registers.ForEach(i => i.Reset());

		// Attach all the variables before entering back to their registers
		foreach (var load in Loads)
		{
			if (!load.Result.IsAnyRegister) continue;

			load.Result.Value.To<RegisterHandle>().Register.Handle = load.Result;
		}
	}

	/// <summary>
	/// Clears all data regarding variables
	/// </summary>
	public void Reset()
	{
		Variables.Clear();
	}

	/// <summary>
	/// Exits this scope after the using-statement
	/// </summary>
	public void Dispose()
	{
		Exit();
	}
}