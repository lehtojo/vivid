using System;
using System.Collections.Generic;
using System.Linq;

public enum AttributeType
{
    VARIABLE,
    CONSTANT,
    COMPLEX_MEMORY_ADDRESS
}

public class MetadataAttribute
{
    public AttributeType Type { get; private set; }

    public MetadataAttribute(AttributeType type)
    {
        Type = type;
    }

    public virtual bool Contradicts(MetadataAttribute other)
    {
        return false;
    }
}

public class Metadata
{
    public MetadataAttribute? PrimaryAttribute => Attributes.Count > 0 ? Attributes[0] : null;
    public IEnumerable<MetadataAttribute> SecondaryAttributes => Attributes.Skip(1);
    public IEnumerable<VariableAttribute> Dependencies => SecondaryAttributes.Where(a => a.Type == AttributeType.VARIABLE).Select(a => (VariableAttribute)a);

    public List<MetadataAttribute> Attributes { get; private set; } = new List<MetadataAttribute>();

    public IEnumerable<VariableAttribute> Variables => Attributes
        .Where(a => a.Type == AttributeType.VARIABLE)
        .Select(a => (VariableAttribute)a);
    
    public bool IsPrimarilyVariable => PrimaryAttribute?.Type == AttributeType.VARIABLE;
    public bool IsPrimarilyConstant => PrimaryAttribute?.Type == AttributeType.CONSTANT;
    public bool IsVariable => Attributes.Exists(a => a.Type == AttributeType.VARIABLE);
    public bool IsConstant => Attributes.Exists(a => a.Type == AttributeType.CONSTANT);
    public bool IsComplexMemoryAddress => Attributes.Exists(a => a.Type == AttributeType.COMPLEX_MEMORY_ADDRESS);
    public bool IsComplex => IsComplexMemoryAddress || (PrimaryAttribute is VariableAttribute attribute && !attribute.Variable.IsPredictable);
    public bool IsDependent => SecondaryAttributes.Any(a => a.Type == AttributeType.VARIABLE || a.Type == AttributeType.COMPLEX_MEMORY_ADDRESS);

    public VariableAttribute this[Variable variable]
    {
        get => Variables.Where(i => i.Variable == variable).Aggregate((i, j) => i.Version > j.Version ? i : j);
    }

    public void Attach(MetadataAttribute attribute)
    {
        Attributes.RemoveAll(a => a.Contradicts(attribute));
        Attributes.Add(attribute);
    }

    public void Attach(IEnumerable<MetadataAttribute> attributes)
    {
        foreach (var attribute in attributes)
        {
            Attach(attribute);
        }
    }
    
    public override bool Equals(object? other)
    {
        if (base.Equals(other))
        {
            return true;
        }

        if (other is (Variable a, int b))
        {
            return Contains(a, b);
        }
        else if (other is Variable variable)
        {
            return Contains(variable, -1);
        }
        else if (other != null)
        {
            return Contains(other);
        }
        else
        {
            return false;
        }
    }

    public bool Contains(Variable variable, int version)
    {
        return Attributes.Exists(a => a is VariableAttribute attribute && attribute.Equals(variable, version));
    }

    public bool Contains(object constant)
    {
        return Attributes.Exists(a => a is ConstantAttribute attribute && attribute.Constant == constant);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Attributes);
    }
}

public class VariableAttribute : MetadataAttribute
{
    public Variable Variable { get; private set; }
    public int Version { get; private set; }

    public VariableAttribute(Variable variable, int version) : base(AttributeType.VARIABLE)
    {
        Variable = variable;
        Version = version;
    }

    public VariableAttribute(Unit unit, Variable variable) : base(AttributeType.VARIABLE)
    {
        Variable = variable;
        Version = unit.GetCurrentVariableVersion(variable);
    }

    public bool Equals(Variable variable, int version)
    {
        return Variable == variable; //&& (version == -1 || Version == version);
    }

    public override bool Contradicts(MetadataAttribute other)
    {
        return other is VariableAttribute attribute && attribute.Variable == Variable;
    }
}

public class ConstantAttribute : MetadataAttribute
{
    public object Constant { get; private set; }

    public ConstantAttribute(object constant) : base(AttributeType.CONSTANT)
    {
        Constant = constant;    
    }

    public override bool Contradicts(MetadataAttribute other)
    {
        return other is ConstantAttribute;
    }
}

public class ComplexMemoryAddressAttribute : MetadataAttribute
{
    public ComplexMemoryAddressAttribute() : base(AttributeType.COMPLEX_MEMORY_ADDRESS) {}
}