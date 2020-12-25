using System.Collections.Generic;
using System.Linq;
using System;

public class VariableLoad
{
	public Variable Variable { get; private set; }
	public Result Reference { get; private set; }

	public VariableLoad(Variable variable, Result reference)
	{
		Variable = variable;
		Reference = reference;
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
	/// <summary>
	/// Returns whether the given variable is a non-local variable
	/// </summary>
	private static bool IsNonLocalVariable(Variable variable, Context[] local_contexts)
	{
		return !local_contexts.Any(local_context => variable.Context.IsInside(local_context));
	}

	/// <summary>
	/// Analyzes how many times each variable in the given node tree is used and sorts the result as well
	/// </summary>
	private static Dictionary<Variable, int> GetNonLocalVariableUsageCount(Unit unit, Node root, params Context[] local_contexts)
	{
		var variables = new Dictionary<Variable, int>();

		foreach (var iterator in root)
		{
			if (iterator is VariableNode node && IsNonLocalVariable(node.Variable, local_contexts))
			{
				if (node.Variable.IsPredictable)
				{
					variables[node.Variable] = variables.GetValueOrDefault(node.Variable, 0) + 1;
				}
				else if (!node.Parent?.Is(NodeType.LINK) ?? false)
				{
					if (unit.Self == null)
					{
						throw new ApplicationException("Detected an use of the this pointer but it was missing");
					}

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
	/// Returns info about variable usage in the given loop
	/// </summary>
	private static List<VariableUsageDescriptor> GetAllVariableUsages(Unit unit, Node[] roots, Context[] contexts)
	{
		if (roots.Length != contexts.Length)
		{
			throw new ArgumentException("Each root must have a corresponding context");
		}

		var result = new Dictionary<Variable, int>();

		for (var i = 0; i < roots.Length; i++)
		{
			// Get all non-local variables in the loop and their number of usages
			foreach (var j in GetNonLocalVariableUsageCount(unit, roots[i], contexts[i]))
			{
				result[j.Key] = result.GetValueOrDefault(j.Key, 0) + j.Value;
			}
		}
		
		var descriptors = result.Select(i => new VariableUsageDescriptor(i.Key, i.Value)).ToList();

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
		// (Otherwise there would be a lot of register moves trying to save the cached variables)
		var non_volatile_mode = node.Find(n => n.Is(NodeType.FUNCTION, NodeType.CALL)) != null;

		unit.Append(new CacheVariablesInstruction(unit, new[] { node }, variables, non_volatile_mode));
	}

	/// <summary>
	/// Tries to move most used loop variables into registers
	/// </summary>
	public static void Cache(Unit unit, Node[] roots, Context[] contexts)
	{
		var variables = GetAllVariableUsages(unit, roots, contexts);

		// If the loop contains at least one function, the variables should be cached into non-volatile registers
		// (Otherwise there would be a lot of register moves trying to save the cached variables)
		var non_volatile_mode = roots.Any(i => i.Find(j => j.Is(NodeType.FUNCTION, NodeType.CALL)) != null);

		unit.Append(new CacheVariablesInstruction(unit, roots, variables, non_volatile_mode));
	}

	/// <summary>
	/// Returns all variables in the given node tree that are not declared in the given local context
	/// </summary>
	public static IEnumerable<Variable> GetAllNonLocalVariables(Node[] roots, params Context[] local_contexts)
	{
		return roots.SelectMany(r => r.FindAll(n => n.Is(NodeType.VARIABLE))
				 .Select(n => n.To<VariableNode>().Variable)
				 .Where(v => v.IsPredictable && IsNonLocalVariable(v, local_contexts)))
				 .Distinct();
	}

	/// <summary>
	/// Retrieves all variables that are visible to the specified context and are inside the current function
	/// </summary>
	private static List<Variable> GetAllContextVariables(Context? current)
	{
		var variables = new List<Variable>();

		while (current != null && !current.IsType)
		{
			variables.AddRange(current.Variables.Values);
			current = current.Parent;
		}

		return variables;
	}

	/// <summary>
	/// Returns all variables in the given context that are used before the current instruction position
	/// </summary>
	private static IEnumerable<Variable> GetAllActiveContextVariables(Unit unit, Context context, Node[] roots)
	{
		return GetAllContextVariables(context).Where(v => unit.Instructions.Exists(i =>
		   (i is GetVariableInstruction x && x.Variable == v) ||
		   (i is RequireVariablesInstruction r && r.Variables.Contains(v))

		)).Where(i => roots.Any(r => Analysis.IsUsedLater(i, r)));
		
		// Legacy: .Where(v => v.References.Any(reference => roots.Any(root => !reference.IsBefore(root))));
	}

	/// </summary>
	/// Returns all variables that the scope must take care of
	/// </summary>
	public static IEnumerable<Variable> GetAllActiveVariablesForScope(Unit unit, Node[] roots, Context current_context, params Context[] scope_contexts)
	{
		return GetAllActiveContextVariables(unit, current_context, roots)
			  .Concat(GetAllNonLocalVariables(roots, scope_contexts))
			  .Concat(unit.Scope!.Actives)
			  .Distinct();
	}

	/// </summary>
	/// Returns all variables that the scope must take care of
	/// </summary>
	public static IEnumerable<Variable> GetAllActiveVariablesForScope(Unit unit, Node root, Context current_context, params Context[] scope_contexts)
	{
		return GetAllActiveContextVariables(unit, current_context, new[] { root })
			  .Concat(GetAllNonLocalVariables(new[] { root }, scope_contexts))
			  .Concat(unit.Scope!.Actives)
			  .Distinct();
	}

	public static void PrepareConditionallyChangingConstants(Unit unit, Node root, params Context[] local_contexts)
	{
		// Find all variables inside the root node which are edited
		var edited_variables = GetAllNonLocalVariables(new Node[] { root }, local_contexts).Where(v => v.IsEditedInside(root));

		// All edited variables that are constants must be moved to registers or into memory
		foreach (var variable in edited_variables)
		{
			unit.Append(new SetModifiableInstruction(unit, variable)
			{
				Description = $"Loads the variable '{variable.Name}' into a register or memory if it's a constant"
			});
		}
	}

	public static void PrepareConditionallyChangingConstants(Unit unit, IfNode root)
	{
		var edited_variables = new List<Variable>();
		var iterator = (Node?)root;
		var context = root.Context;

		while (iterator != null)
		{
			// Find all variables inside the root node which are edited
			edited_variables.AddRange(GetAllNonLocalVariables(new Node[] { iterator }, context).Where(v => v.IsEditedInside(iterator)));

			if (iterator.Is(NodeType.ELSE_IF))
			{
				iterator = root.To<IfNode>().Successor;

				// Retrieve the context of the successor
				if (iterator != null)
				{
					if (iterator.Is(NodeType.ELSE_IF))
					{
						context = iterator.To<ElseIfNode>().Context;
					}
					else
					{
						context = iterator.To<ElseNode>().Context;
					}
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

	private List<VariableLoad> Loads { get; set; } = new List<VariableLoad>();
	private HashSet<Variable> Initializers { get; set; } = new HashSet<Variable>();
	private HashSet<Variable> Finalizers { get; set; } = new HashSet<Variable>();

	public List<Variable> Actives { get; } = new List<Variable>();
	public Dictionary<Variable, Result> Variables { get; } = new Dictionary<Variable, Result>();
	public Dictionary<Variable, Result> Transferers { get; } = new Dictionary<Variable, Result>();

	public Dictionary<object, List<Result>> Constants { get; } = new Dictionary<object, List<Result>>();

	public Unit? Unit { get; private set; }
	public Scope? Outer { get; private set; } = null;

	public int StackOffset { get; set; } = 0;


	public bool AppendFinalizers { get; set; } = true;

	/// <summary>
	/// Creates a scope with variables that are returned to their original locations once the scope is exited
	/// </summary>
	/// <param name="active_variables">Variables that must not be released</param>
	public Scope(Unit unit, IEnumerable<Variable>? active_variables = null)
	{
		Actives = active_variables?.ToList() ?? new List<Variable>();
		Enter(unit);
	}

	/// <summary>
	/// Returns whether the variable is used later looking from the current instruction position
	/// </summary>
	public bool IsUsedLater(Variable variable)
	{
		// Try to get the most recently used handle of the variable
		var current = GetCurrentVariableHandle(variable);

		// If there's no handle of the variable, it means that the outer scope doesn't have it either
		if (current == null)
		{
			return false;
		}

		// Check if the handle is used later or if the outer scope uses the variable later
		return current.Lifetime.End > Unit!.Position || (Outer?.IsUsedLater(variable) ?? false);
	}

	/// <summary>
	/// Sets or creates the handle for the specified variable
	/// </summary>
	private Result SetOrCreateTransitionHandle(Variable variable, Handle handle)
	{
		if (!variable.IsPredictable)
		{
			throw new InvalidOperationException("Tried to create transition handle for an unpredictable variable");
		}

		handle = handle.Finalize();

		if (Transferers.TryGetValue(variable, out Result? transferer))
		{
			transferer.Value = handle;
		}
		else
		{
			var format = variable.Type! == Types.DECIMAL ? Format.DECIMAL : Assembler.Format;

			transferer = new Result(handle, format);
			transferer.Metadata.Attach(new VariableAttribute(variable));

			Transferers.Add(variable, transferer);
		}

		// Update the current handle to the variable
		Variables[variable] = transferer;

		// If the transferer is a register, the transferer value must be attached there
		if (transferer.Value is RegisterHandle value)
		{
			value.Register.Handle = transferer;
		}

		return transferer;
	}

	/// <summary>
	/// Switches the current scope to this scope
	/// </summary>
	public void Enter(Unit unit)
	{
		Unit = unit;
		StackOffset = Unit.StackOffset;

		// Remove old variable data
		Reset();

		// Save the outer scope so that this scope can be exited later
		if (unit.Scope != this)
		{
			Outer = unit.Scope;
		}

		// Detect if there are new variables to load
		if (Loads.Count != Actives.Count)
		{
			// Add all the missing variable loads
			foreach (var variable in Actives)
			{
				// Skip variables which are already loaded
				if (Loads.Exists(l => l.Variable == variable))
				{
					continue;
				}

				var handle = References.GetVariable(Unit, variable, AccessMode.READ);
				var instruction = handle.Instruction!;

				instruction.Description = $"Transfers the current handle of variable '{variable.Name}' to the upcoming scope";

				Loads.Add(new VariableLoad(variable, handle));
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
				var external_handle = Loads[i].Reference;

				if (external_handle != null)
				{
					SetOrCreateTransitionHandle(variable, external_handle.Value);
				}

				if (!Initializers.Contains(variable))
				{
					// The current variable is an active one so it must stay protected during the whole scope
					References.GetVariable(unit, variable, AccessMode.READ).Instruction!.Description = $"Registers variable '{variable.Name}' as active";

					Initializers.Add(variable);
				}
			}

			// Get all the register which hold any active variable
			var denylist = Variables.Values.Where(i => i.Value.Is(HandleType.REGISTER)).Select(i => i.Value.To<RegisterHandle>().Register);
			var registers = Unit.NonReservedRegisters.Where(r => !denylist.Contains(r));

			// All register which don't hold active variables must be reset since they would disturb the execution of the scope
			foreach (var register in registers)
			{
				register.Reset();
			}
		}
		else if (unit.Function.Convention == CallingConvention.X64)
		{
			// Move all parameters to their expected registers since this is the first scope
			var decimal_parameter_registers = unit.MediaRegisters.Take(Calls.GetMaxMediaRegisterParameters()).ToList();
			var standard_parameter_registers = Calls.GetStandardParameterRegisters().Select(name => unit.Registers.Find(r => r[Size.QWORD] == name)!).ToList();

			var register = (Register?)null;

			if (unit.Function.IsMember || unit.Function.IsLambda)
			{
				var self = unit.Self ?? throw new ApplicationException("Missing self pointer");

				register = standard_parameter_registers.Pop();

				if (register != null)
				{
					register.Handle = SetOrCreateTransitionHandle(self, new RegisterHandle(register));
				}
				else
				{
					throw new ApplicationException("Self pointer should not be in stack (x64 calling convention)");
				}
			}

			foreach (var parameter in unit.Function.Parameters)
			{
				register = parameter.Type!.Format.IsDecimal() ? decimal_parameter_registers.Pop() : standard_parameter_registers.Pop();

				if (register != null)
				{
					register.Handle = SetOrCreateTransitionHandle(parameter, new RegisterHandle(register));
				}
				else
				{
					SetOrCreateTransitionHandle(parameter, References.CreateVariableHandle(Unit, parameter));
				}
			}
		}
	}

	public Result? GetCurrentVariableHandle(Variable variable)
	{
		// When debugging is enabled, all variables should be stored in stack, which is the default location if this function returns null
		if (Assembler.IsDebuggingEnabled)
		{
			return null;
		}

		// Only predictable variables are allowed to be cached
		if (!variable.IsPredictable)
		{
			return null;
		}

		// First check if the variable handle list already exists
		if (Variables.TryGetValue(variable, out Result? handle))
		{
			return handle;
		}
		else
		{
			var source = Outer?.GetCurrentVariableHandle(variable);

			if (source != null)
			{
				Variables.Add(variable, source);
			}

			return source;
		}
	}

	/// <summary>
	/// Switches back to the outer scope
	/// </summary>
	public void Exit()
	{
		if (Unit == null)
		{
			throw new ApplicationException("Unit was not assigned to a scope or the scope was never entered");
		}

		if (AppendFinalizers)
		{
			foreach (var variable in Actives)
			{
				if (!Finalizers.Contains(variable))
				{
					// The current variable is an active one so it must stay protected during the whole scope lifetime
					References.GetVariable(Unit, variable, AccessMode.READ).Instruction!.Description = "Keep variable active";

					Finalizers.Add(variable);
				}
			}
		}

		// Exit to the outer scope
		Unit.Scope = Outer ?? this;
	}

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