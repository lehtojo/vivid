using System;
using System.Collections.Generic;
using System.Globalization;

public enum HandleType
{
   MEMORY,
   CONSTANT,
   REGISTER,
   MEDIA_REGISTER,
   CALCULATION,
   NONE
}

public class Handle
{
   public HandleType Type { get; protected set; }
   public bool IsSizeVisible { get; set; } = false;

   public Format Format { get; set; } = Assembler.Format;
   public Size Size => Size.FromFormat(Format);
   public bool IsUnsigned => Format.IsUnsigned();

   public Handle()
   {
      Type = HandleType.NONE;
   }

   public Handle(HandleType type)
   {
      Type = type;
   }

   public bool Is(HandleType type)
   {
      return Type == type;
   }

   /// <summary>
   /// Returns all results which the handle requires to be in registers
   /// </summary>
   public virtual Result[] GetRegisterDependentResults()
   {
      return Array.Empty<Result>();
   }

   public T To<T>() where T : Handle
   {
      return (T)this;
   }

   public virtual void Use(int position) { }
   public virtual Handle Finalize()
   {
      return (Handle)this.MemberwiseClone();
   }

   public override string ToString()
   {
      throw new NotImplementedException("Missing text conversion from handle");
   }
}

public class ConstantDataSectionHandle : DataSectionHandle
{
   public object Value { get; private set; }

   public ConstantDataSectionHandle(ConstantHandle handle) : base(handle.ToString())
   {
      Value = handle.Value;
   }

   public override Handle Finalize()
   {
      return (Handle)MemberwiseClone();
   }

   public override bool Equals(object? obj)
   {
      return obj is ConstantDataSectionHandle handle &&
             base.Equals(obj) &&
             EqualityComparer<object>.Default.Equals(Value, handle.Value);
   }

   public override int GetHashCode()
   {
      return HashCode.Combine(base.GetHashCode(), Value);
   }
}

public class DataSectionHandle : Handle
{
   public string Identifier { get; set; }
   public bool Address { get; set; } = false;

   public DataSectionHandle(string identifier, bool address = false) : base(HandleType.MEMORY)
   {
      Identifier = identifier;
      Address = address;
   }

   public override string ToString()
   {
      if (Address)
      {
         return Identifier;
      }

      if (Assembler.IsTargetX64)
      {
         return IsSizeVisible ? $"{Size} [rel {Identifier}]" : $"[rel {Identifier}]";
      }

      return IsSizeVisible ? $"{Size} [{Identifier}]" : $"[{Identifier}]";
   }

   public override Handle Finalize()
   {
      return (Handle)MemberwiseClone();
   }

   public override bool Equals(object? obj)
   {
      return obj is DataSectionHandle handle &&
            Type == handle.Type &&
            Identifier == handle.Identifier;
   }

   public override int GetHashCode()
   {
      return HashCode.Combine(Type, Identifier);
   }
}

public class ConstantHandle : Handle
{
   public object Value { get; private set; }
   public int Bits => GetBits();

   private int GetBits()
   {
      if (Value is double)
      {
         return Assembler.Size.Bits;
      }

      var x = (long)Value;

      if (x < 0)
      {
         if (x < int.MaxValue)
      	{
         	return 64;
      	}
			else if (x < short.MaxValue)
			{
         	return 32;
      	}
      	else if (x < byte.MaxValue)
      	{
         	return 16;
      	}
      }
      else
      {
         if (x > int.MaxValue)
      	{
         	return 64;
      	}
			else if (x > short.MaxValue)
			{
         	return 32;
      	}
      	else if (x > byte.MaxValue)
      	{
         	return 16;
      	}
      }

      return 8;
   }

   public ConstantHandle(object value) : base(HandleType.CONSTANT)
   {
      Value = value;
   }

   public void Convert(Format format)
   {
      Value = format switch
      {
         Format.DECIMAL => System.Convert.ToDouble(Value, CultureInfo.InvariantCulture),
         Format.INT8 => System.Convert.ToSByte(Value, CultureInfo.InvariantCulture),
         Format.INT16 => System.Convert.ToInt16(Value, CultureInfo.InvariantCulture),
         Format.INT32 => System.Convert.ToInt32(Value, CultureInfo.InvariantCulture),
         Format.INT64 => System.Convert.ToInt64(Value, CultureInfo.InvariantCulture),
         Format.UINT8 => System.Convert.ToByte(Value, CultureInfo.InvariantCulture),
         Format.UINT16 => System.Convert.ToUInt16(Value, CultureInfo.InvariantCulture),
         Format.UINT32 => System.Convert.ToUInt32(Value, CultureInfo.InvariantCulture),
         Format.UINT64 => System.Convert.ToUInt64(Value, CultureInfo.InvariantCulture),
         _ => throw new ApplicationException("Unsupported format encountered while converting a handle"),
      };
   }

   public override string ToString()
   {
      var result = Value?.ToString()?.Replace(',', '.') ?? throw new NullReferenceException("Constant value was missing");

      if (Format.IsDecimal() && !result.Contains('.'))
      {
         return result + ".0";
      }

      return result;
   }

   public override bool Equals(object? obj)
   {
      return obj is ConstantHandle handle &&
            EqualityComparer<object>.Default.Equals(Value, handle.Value);
   }

   public override Handle Finalize()
   {
      return (Handle)this.MemberwiseClone();
   }

   public override int GetHashCode()
   {
      return HashCode.Combine(Value);
   }
}

public class StackVariableHandle : StackMemoryHandle
{
   public Variable Variable { get; private set; }

   public StackVariableHandle(Unit unit, Variable variable) : base(unit, variable.LocalAlignment ?? 0)
   {
      Variable = variable;

      if (!Variable.IsPredictable)
      {
         throw new ArgumentException("Tried to create stack variable handle for a variable which isn't stored in the stack");
      }
   }

   public override string ToString()
   {
      if (Variable.LocalAlignment == null)
      {
         return $"[{Variable.Name}]";
      }

      Offset = (int)Variable.LocalAlignment;

      return base.ToString();
   }

   public override Handle Finalize()
   {
      return (Handle)MemberwiseClone();
   }

   public override bool Equals(object? obj)
   {
      return obj is StackVariableHandle handle &&
            base.Equals(obj) &&
            EqualityComparer<Variable>.Default.Equals(Variable, handle.Variable);
   }

   public override int GetHashCode()
   {
      return HashCode.Combine(base.GetHashCode(), Variable);
   }
}

public class MemoryHandle : Handle
{
   public Unit Unit { get; private set; }
   public Result Start { get; private set; }
   public int Offset { get; set; }

   private int AbsoluteOffset => GetAbsoluteOffset();

   public MemoryHandle(Unit unit, Result start, int offset) : base(HandleType.MEMORY)
   {
      Unit = unit;
      Start = start;
      Offset = offset;
   }

   public virtual int GetAbsoluteOffset()
   {
      return Offset;
   }

   public override void Use(int position)
   {
      Start.Use(position);
   }

   public override string ToString()
   {
      var offset = string.Empty;

      if (AbsoluteOffset > 0)
      {
         offset = $"+{AbsoluteOffset}";
      }
      else if (AbsoluteOffset < 0)
      {
         offset = AbsoluteOffset.ToString(CultureInfo.InvariantCulture);
      }

      if (Start.IsStandardRegister || Start.IsConstant)
      {
         var address = $"[{Start.Value}{offset}]";

         if (IsSizeVisible)
         {
            return $"{Size} {address}";
         }
         else
         {
            return $"{address}";
         }
      }

      throw new ApplicationException("Start of the memory handle was no longer in register");
   }

   public override Result[] GetRegisterDependentResults()
   {
      return new Result[] { Start };
   }

   public override Handle Finalize()
   {
      if (Start.IsStandardRegister || Start.IsConstant)
      {
         return new MemoryHandle(Unit, new Result(Start.Value, Start.Format), Offset);
      }

      throw new ApplicationException("Start of the memory handle was in invalid format for freeze operation");
   }

   public override bool Equals(object? obj)
   {
      return obj is MemoryHandle handle &&
            EqualityComparer<Result>.Default.Equals(Start, handle.Start) &&
            Offset == handle.Offset;
   }

   public override int GetHashCode()
   {
      return HashCode.Combine(Start, Offset);
   }
}

public class StackMemoryHandle : MemoryHandle
{
   public bool IsAbsolute { get; private set; }

   public StackMemoryHandle(Unit unit, int offset, bool absolute = true) : base
   (
      unit,
      new Result
      (
         new RegisterHandle(unit.GetStackPointer()),
         Assembler.Format
      ),
      offset

   )
   { IsAbsolute = absolute; }

   public override int GetAbsoluteOffset()
   {
      return (IsAbsolute ? Unit.StackOffset : 0) + Offset;
   }

   public override Handle Finalize()
   {
      if (Start.Value.To<RegisterHandle>().Register == Unit.GetStackPointer())
      {
         return new StackMemoryHandle(Unit, Offset, IsAbsolute);
      }

      throw new ApplicationException("Stack memory handle's register was invalid");
   }

   public override bool Equals(object? obj)
   {
      return obj is StackMemoryHandle handle &&
               Offset == handle.Offset &&
                IsAbsolute == handle.IsAbsolute;
   }

   public override int GetHashCode()
   {
      HashCode hash = new HashCode();
      hash.Add(base.GetHashCode());
      hash.Add(IsAbsolute);
      return hash.ToHashCode();
   }
}

public class TemporaryMemoryHandle : StackMemoryHandle
{
   public Guid Identifier { get; private set; }

   public TemporaryMemoryHandle(Unit unit) : base(unit, 0)
   {
      Identifier = Guid.NewGuid();
   }

   public override Handle Finalize()
   {
      return (Handle)this.MemberwiseClone();
   }

   public override bool Equals(object? obj)
   {
      return obj is TemporaryMemoryHandle handle &&
            base.Equals(obj) &&
            EqualityComparer<Guid>.Default.Equals(Identifier, handle.Identifier);
   }

   public override int GetHashCode()
   {
      return HashCode.Combine(base.GetHashCode(), Identifier);
   }
}

public class ComplexMemoryHandle : Handle
{
   public Result Start { get; private set; }
   public Result Offset { get; private set; }
   public int Stride { get; private set; }

   public ComplexMemoryHandle(Result start, Result offset, int stride) : base(HandleType.MEMORY)
   {
      Start = start;
      Offset = offset;
      Stride = stride;
   }

   public override void Use(int position)
   {
      Start.Use(position);
      Offset.Use(position);
   }

   public override string ToString()
   {
      var offset = string.Empty;

      if (Offset.IsStandardRegister)
      {
         offset = "+" + Offset.ToString() + (Stride == 1 ? string.Empty : $"*{Stride}");
      }
      else if (Offset.Value is ConstantHandle constant)
      {
         var index = (Int64)constant.Value;
         var value = index * Stride;

         if (value > 0)
         {
            offset = $"+{value}";
         }
         else if (value < 0)
         {
            offset = value.ToString(CultureInfo.InvariantCulture);
         }
      }
      else
      {
         throw new ApplicationException("Complex memory address's offset wasn't a constant or in a register");
      }

      if (Start.Value.Type == HandleType.REGISTER ||
         Start.Value.Type == HandleType.CONSTANT)
      {
         var address = $"[{Start.Value}{offset}]";

         if (IsSizeVisible)
         {
            return $"{Size} {address}";
         }
         else
         {
            return $"{address}";
         }
      }

      throw new ApplicationException("Base of the memory handle was no longer in register");
   }

   public override Result[] GetRegisterDependentResults()
   {
      if (!Offset.IsConstant)
      {
         return new Result[] { Start, Offset };
      }

      return new Result[] { Start };
   }

   public override Handle Finalize()
   {
      if ((Start.Value.Type == HandleType.REGISTER || Start.Value.Type == HandleType.CONSTANT) &&
         (Offset.Value.Type == HandleType.REGISTER || Offset.Value.Type == HandleType.CONSTANT))
      {
         return new ComplexMemoryHandle
         (
            new Result(Start.Value, Start.Format),
            new Result(Offset.Value, Offset.Format),
            Stride

         );
      }

      throw new ApplicationException("Parameters of a complex memory handle were in invalid format for freeze operation");
   }

   public override bool Equals(object? obj)
   {
      return obj is ComplexMemoryHandle handle &&
            EqualityComparer<Result>.Default.Equals(Start, handle.Start) &&
            EqualityComparer<Result>.Default.Equals(Offset, handle.Offset) &&
            Stride == handle.Stride;
   }

   public override int GetHashCode()
   {
      return HashCode.Combine(Start, Offset, Stride);
   }
}

public class CalculationHandle : Handle
{
   public Result Multiplicand { get; private set; }
   public int Multiplier { get; private set; }
   public Result? Addition { get; private set; }
   public int Constant { get; private set; }

   public static CalculationHandle CreateAddition(Result left, Result right)
   {
      return new CalculationHandle(left, 1, right, 0);
   }

   public CalculationHandle(Result multiplicand, int multiplier, Result? addition, int constant) : base(HandleType.CALCULATION)
   {
      Multiplicand = multiplicand;
      Multiplier = multiplier;
      Addition = addition;
      Constant = constant;
   }

   public override void Use(int position)
   {
      Multiplicand.Use(position);
      Addition?.Use(position);
   }

   private void Validate()
   {
      if ((Multiplicand.Value.Type != HandleType.REGISTER && Multiplicand.Value.Type != HandleType.CONSTANT) ||
          (Addition != null && (Addition.Value.Type != HandleType.REGISTER && Addition.Value.Type != HandleType.CONSTANT)) ||
            Multiplier <= 0 || Constant < 0)
      {
         throw new ApplicationException("Detected an invalid calculation handle");
      }
   }

   public override string ToString()
   {
      Validate();

      var result = Multiplicand.ToString();

      if (Multiplier > 1)
      {
         result += "*" + Multiplier.ToString(CultureInfo.InvariantCulture);
      }

      if (Addition != null)
      {
         result += "+" + Addition.ToString();
      }

      if (Constant != 0)
      {
         result += "+" + Constant;
      }

      return '[' + result + ']';
   }

   public override Result[] GetRegisterDependentResults()
   {
      var result = new List<Result>();

      if (Multiplicand.Value.Type != HandleType.CONSTANT)
      {
         result.Add(Multiplicand);
      }

      if (Addition != null && Addition.Value.Type != HandleType.CONSTANT)
      {
         result.Add(Addition);
      }

      return result.ToArray();
   }

   public override Handle Finalize()
   {
      Validate();

      return new CalculationHandle
      (
         new Result(Multiplicand.Value, Assembler.Format),
         Multiplier,
         Addition == null ? null : new Result(Addition.Value, Assembler.Format),
         Constant
      );
   }

   public override bool Equals(object? obj)
   {
      return obj is CalculationHandle handle &&
             EqualityComparer<Result>.Default.Equals(Multiplicand, handle.Multiplicand) &&
             Multiplier == handle.Multiplier &&
             EqualityComparer<Result?>.Default.Equals(Addition, handle.Addition) &&
             Constant == handle.Constant;
   }

   public override int GetHashCode()
   {
      HashCode hash = new HashCode();
      hash.Add(Type);
      hash.Add(Multiplicand);
      hash.Add(Multiplier);
      hash.Add(Addition);
      hash.Add(Constant);
      return hash.ToHashCode();
   }
}

public class RegisterHandle : Handle
{
   public Register Register { get; private set; }

   public RegisterHandle(Register register) : base(register.IsMediaRegister ? HandleType.MEDIA_REGISTER : HandleType.REGISTER)
   {
      Register = register;
   }

   public override string ToString()
   {
      if (Size == Size.NONE)
      {
         return Register[Assembler.Size];
      }

      return Register[Size];
   }

   public override Handle Finalize()
   {
      return (Handle)this.MemberwiseClone();
   }

   public override bool Equals(object? obj)
   {
      return obj is RegisterHandle handle && Register == handle.Register;
   }

   public override int GetHashCode()
   {
      return HashCode.Combine(Register);
   }
}