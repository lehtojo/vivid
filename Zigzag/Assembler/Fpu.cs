using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

public class FpuRegister
{
	public static readonly FpuRegister First = new FpuRegister(0);

	public int Index { get; private set; }

	public FpuRegister(int index)
	{
		Index = index;
	}

	public override string ToString()
	{
		return $"st{Index}";
	}
}

public class FpuStackElement
{
	public Fpu Fpu { get; private set; }

	public object Metadata { get; set; }

	public bool Integer { get; private set; }
	public bool Critical { get; set; } = true;

	public FpuRegister Register => Fpu.GetRegister(this);

	public FpuStackElement(Fpu fpu, bool integer)
	{
		Fpu = fpu;
		Integer = integer;
	}
}

public class Fpu
{
	private const string POP_INTEGER = "fistp";
	private const string POP_FLOAT = "fstp";

	private const string LOAD_INTEGER = "fild";
	private const string LOAD_FLOAT = "fld";

	private const string EXCHANGE = "fxch";

	private const string ADD = "fadd";
	private const string SUBTRACT = "fsub";
	private const string MULTIPLY = "fmul";
	private const string DIVIDE = "fdiv";

	public List<FpuStackElement> Elements { get; private set; } = new List<FpuStackElement>();

	/// <summary>
	/// Tries to find variable from the current stack
	/// </summary>
	/// <param name="variable">Variable to search for</param>
	/// <returns>Success: FPU stack element which contains the variable, Failure: null</returns>
	public FpuStackElement Find(Variable variable)
	{
		return Elements.Find(e => e.Metadata == variable);
	}

	/// <summary>
	/// Saves the state of the fpu into stack
	/// </summary>
	/// <param name="unit">Unit used for operating</param>
	/// <returns>Instructions for saving the state of the fpu</returns>
	public Instructions Save(Unit unit)
	{
		// Example:
		// ; Saving the current state of the FPU
		// sub esp, 8 ; Allocate local memory for the current state
		// fstp dword [esp+4] ; Pop and store a float from the FPU
		// fistp dword [esp] ; Pop and store a integer from the FPU

		var elements = Elements.Where(e => e.Critical).ToList();

		// Saving is not always necessary
		if (elements.Count == 0)
		{
			return new Instructions();
		}

		var instructions = new Instructions();
		instructions.Comment("Saving the current state of the FPU");

		var stack = unit.Stack;
		stack.Reserve(instructions, elements.Count * unit.Bytes);

		for (var i = 0; i < elements.Count; i++)
		{
			var element = elements[i];

			if (element.Register.Index != 0)
			{
				Exchange(instructions, element.Register, FpuRegister.First);
			}

			var alignment = (elements.Count - i - 1) * unit.Bytes;
			var destination = new MemoryReference(unit.ESP, alignment, unit.Bytes);
			var instruction = element.Integer ? POP_INTEGER : POP_FLOAT;

			instructions.Append($"{instruction} {destination}");
		}

		return instructions;
	}

	/// <summary>
	/// Restores the state of the fpu from stack.
	/// This function assumes that the previous state is saved at the current stack pointer address.
	/// </summary>
	/// <param name="unit">Unit used for operating</param>
	/// <returns>Instructions for restoring the state of the fpu</returns>
	public Instructions Restore(Unit unit)
	{
		// Example:
		// pop eax
		// fild eax
		// pop eax
		// fld eax

		var elements = Elements.Where(e => e.Critical).ToList();

		// Restoring is not always necessary
		if (elements.Count == 0)
		{
			return new Instructions();
		}

		// # error TODO: Ihan buginen tallennus

		var instructions = new Instructions();
		instructions.Comment("Restoring the state of the FPU");

		var register = unit.GetNextRegister();
		var stack = unit.Stack;

		// The temporary register must be empty
		instructions.Append(Memory.Clear(unit, register, false));

		for (var i = elements.Count - 1; i >= 0; i--)
		{
			var element = elements[i];
			stack.Pop(instructions, new RegisterReference(register));

			var instruction = element.Integer ? LOAD_INTEGER : LOAD_FLOAT;
			instructions.Append($"{instruction} {register}");
		}

		return instructions;
	}

	/// <summary>
	/// Loads an integer or float into FPU stack
	/// </summary>
	/// <param name="instructions">Instructions where the load instructions are appended</param>
	/// <param name="reference">Reference to the integer/float to load into FPU stack</param>
	/// <param name="integer">Whether the number is an integer or float</param>
	public FpuStackReference Push(Instructions instructions, Reference reference, bool integer)
	{
		var instruction = integer ? LOAD_INTEGER : LOAD_FLOAT;
		instructions.Append(new Instruction(instruction, reference));

		var element = new FpuStackElement(this, integer);
		Elements.Insert(0, element);

		return new FpuStackReference(element);
	}

	public StackReference ToStack(Unit unit, Instructions instructions, bool integer)
	{
		var stack = unit.Stack;
		var destination = stack.Reserve(instructions);

		var instruction = integer ? POP_INTEGER : POP_FLOAT;
		instructions.Append(new Instruction(instruction, destination));

		Elements.RemoveAt(0);

		return destination;
	}

	/// <summary>
	/// Finds in which register the given element is stored in the FPU
	/// </summary>
	/// <param name="element">Element to search for</param>
	/// <returns>FPU register index which contains the element</returns>
	public FpuRegister GetRegister(FpuStackElement element)
	{
		return new FpuRegister(Elements.IndexOf(element));
	}

	/// <summary>
	/// Performs the operation between the two elements, setting the left element equal to the result
	/// </summary>
	/// <param name="left">Left element</param>
	/// <param name="right">Right element</param>
	public FpuStackReference Perform(Instructions instructions, Operator operation, FpuStackReference left, FpuStackReference right)
	{
		if (left.Element.Register.Index != 0 && right.Element.Register.Index != 0)
		{
			var further = left.Element.Register.Index >= right.Element.Register.Index ? left : right;
			Exchange(instructions, further.Element.Register, FpuRegister.First);
		}

		string instruction;

		if (operation == Operators.ADD)
		{
			instruction = ADD;
		}
		else if (operation == Operators.SUBTRACT)
		{
			instruction = SUBTRACT;
		}
		else if (operation == Operators.MULTIPLY)
		{
			instruction = MULTIPLY;
		}
		else if (operation == Operators.DIVIDE)
		{
			instruction = DIVIDE;
		}
		else
		{
			throw new NotImplementedException($"ERROR: FPU operation not implemented yet: {operation.Identifier}");
		}

		instructions.Append(new Instruction(instruction, left, right, Size.DWORD));
		left.Element.Metadata = null;

		return left;
	}

	/// <summary>
	/// Exchanges the contents of the two FPU registers
	/// </summary>
	/// <param name="instructions">Instructions in which to append the exchange instructions</param>
	public void Exchange(Instructions instructions, FpuRegister a, FpuRegister b)
	{
		instructions.Append($"{EXCHANGE} {a}, {b}");

		var temporary = Elements[a.Index];
		Elements[a.Index] = Elements[b.Index];
		Elements[b.Index] = temporary;
	}

	/// <summary>
	/// Exports the source element from FPU to the destination
	/// </summary>
	/// <param name="instructions">Instructions in which to append export instructions</param>
	/// <param name="source">Source FPU element</param>
	/// <param name="destination">Destination address</param>
	public void Export(Unit unit, Instructions instructions, FpuStackReference source, Reference destination)
	{
		// The export value must be located at ST0 register
		var register = source.Element.Register;

		if (register.Index != 0)
		{
			Exchange(instructions, register, FpuRegister.First);
		}

		// Moving FPU stack element to a register requires stack memory as a intermediate
		if (destination.IsRegister())
		{
			// Pop the result from FPU to the top of the stack
			var reference = ToStack(unit, instructions, source.Element.Integer);

			// Pop the result from stack to the destination register
			Memory.Move(unit, instructions, reference, destination);
		}
		else
		{
			var instruction = source.Element.Integer ? POP_INTEGER : POP_FLOAT;
			instructions.Append(new Instruction(instruction, destination));
		}
	}
}