using System;

public static class RegisterFlag
{
	public const int NONE = 0;
	public const int VOLATILE = 1;
	public const int RESERVED = 2;
	public const int RETURN = 4;
	public const int STACK_POINTER = 8;
	public const int NUMERATOR = 16;
	public const int REMAINDER = 32;
	public const int MEDIA = 64;
	public const int DECIMAL_RETURN = 128;
	public const int SHIFT = 256;
	public const int BASE_POINTER = 512;
	public const int ZERO = 1024;
	public const int RETURN_ADDRESS = 2048;
}

public class Register
{
	public byte Identifier { get; set; } = 0;
	public byte Name { get; set; } = 0;
	public string[] Partitions { get; private set; }
	public Result? Value { get; set; } = null;
	public int Flags { get; set; }
	public Size Width { get; private set; }
	public bool IsLocked { get; set; } = false;

	public string this[Size size] => Partitions[(int)Math.Log2(Width.Bytes) - (int)Math.Log2(size.Bytes)];

	public bool IsVolatile => Flag.Has(Flags, RegisterFlag.VOLATILE);
	public bool IsReserved => Flag.Has(Flags, RegisterFlag.RESERVED);
	public bool IsMediaRegister => Flag.Has(Flags, RegisterFlag.MEDIA);

	public Register(Size width, string[] partitions, params int[] flags)
	{
		Width = width;
		Partitions = partitions;
		Flags = Flag.Combine(flags);
	}

	public Register(byte identifier, Size width, string[] partitions, params int[] flags)
	{
		Identifier = identifier;
		Name = (byte)(identifier & 7);
		Width = width;
		Partitions = partitions;
		Flags = Flag.Combine(flags);
	}

	public bool IsHandleCopy()
	{
		return Value != null && !(Value.Value.Is(HandleInstanceType.REGISTER) && Value.Value.To<RegisterHandle>().Register == this);
	}

	public bool IsAvailable()
	{
		return !IsLocked && (Value == null || !Value.IsActive() || IsHandleCopy());
	}

	public bool IsDeactivating()
	{
		return !IsLocked && Value != null && Value.IsDeactivating();
	}

	public bool IsReleasable(Unit unit)
	{
		return !IsLocked && (Value == null || Value.IsReleasable(unit));
	}

	public void Lock()
	{
		IsLocked = true;
	}

	public void Unlock()
	{
		IsLocked = false;
	}

	public void Reset()
	{
		Value = null;
	}

	public override string ToString()
	{
		return Partitions[Partitions.Length - 1 - (int)Math.Log2(Settings.Bytes)];
	}
}