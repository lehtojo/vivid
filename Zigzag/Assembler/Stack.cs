using System;
using System.Collections.Generic;
using System.Text;

public class StackElement
{
	public Stack Stack { get; private set; }
	public object Metadata { get; set; }

	public int Alignment => Stack.GetAlignment(this);

	public StackElement(Stack stack)
	{
		Stack = stack;
	}
}

public class Stack
{
	public List<StackElement> Elements { get; private set; } = new List<StackElement>();

	public StackReference Push(Instructions instructions, Reference reference)
	{
		instructions.Append(new Instruction("push", reference));

		var element = new StackElement(this);
		element.Metadata = reference.Metadata;

		Elements.Insert(0, element);

		return new StackReference(element);
	}

	public void Pop(Instructions instructions, Reference register)
	{
		instructions.Append(new Instruction("pop", register));

		var element = Elements[0];
		register.Metadata = element.Metadata;

		Elements.RemoveAt(0);
	}

	public void Restore(Instructions instructions)
	{
		var bytes = Elements.Count * 4;

		if (bytes > 0)
		{
			Shrink(instructions, bytes);
		}

		Elements.Clear();
	}

	public StackReference Reserve(Instructions instructions, int bytes = 4)
	{
		if (bytes % 4 != 0)
		{
			throw new ArgumentException("Stack memory must be reserved using multiples of 4 bytes");
		}

		instructions.Append($"sub esp, {bytes}");

		for (var i = 0; i < bytes / 4; i++)
		{
			var element = new StackElement(this);
			Elements.Insert(0, element);
		}

		return new StackReference(Elements[0]);
	}

	public void Shrink(Instructions instructions, int bytes)
	{
		if (bytes % 4 != 0)
		{
			throw new ArgumentException("Stack must be shrinked by multiple of 4 bytes");
		}

		instructions.Append($"add esp, {bytes}");

		for (var i = 0; i < bytes / 4; i++)
		{
			Elements.RemoveAt(0);
		}
	}

	public StackReference Find(Variable variable)
	{
		var element = Elements.Find(e => e.Metadata == variable);
		return element != null ? new StackReference(element) : null;
	}

	public int GetAlignment(StackElement element)
	{
		return Elements.IndexOf(element) * 4;
	}
}