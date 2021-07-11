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
	public string[] Partitions { get; private set; }
	public Size Width { get; private set; }
	public Format Format => IsMediaRegister ? Format.DECIMAL : Assembler.Format;

	private Result? _Value { get; set; } = null;
	public Result? Handle
	{
		get => _Value;
		set { _Value = value; }
	}

	public string this[Size size] => Partitions[(int)Math.Log2(Width.Bytes) - (int)Math.Log2(size.Bytes)];

	public int Flags { get; set; }

	public bool IsLocked { get; set; } = false;
	public bool IsVolatile => Flag.Has(Flags, RegisterFlag.VOLATILE);
	public bool IsReserved => Flag.Has(Flags, RegisterFlag.RESERVED);
	public bool IsMediaRegister => Flag.Has(Flags, RegisterFlag.MEDIA);

	public Register(Size width, string[] partitions, params int[] flags)
	{
		Width = width;
		Partitions = partitions;
		Flags = Flag.Combine(flags);
	}

	public string GetDescription()
	{
		return _Value == null ? "Empty" : "Occupied";
	}

	public bool IsHandleCopy()
	{
		return Handle != null && !(Handle.Value.Is(HandleInstanceType.REGISTER) && Handle.Value.To<RegisterHandle>().Register == this);
	}

	public bool IsAvailable(int position)
	{
		return !IsLocked && (Handle == null || !Handle.IsValid(position) || IsHandleCopy());
	}

	public bool IsReleasable(Unit unit)
	{
		return !IsLocked && (Handle == null || Handle.IsReleasable(unit));
	}

	public void Reset(bool full = false)
	{
		_Value = null;
	}

	public override string ToString()
	{
		return Partitions[Partitions.Length - 1 - (int)Math.Log2(Assembler.Size.Bytes)];
	}
}