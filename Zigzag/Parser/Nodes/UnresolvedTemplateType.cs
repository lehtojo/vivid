public class UnresolvedTemplateType : Node, IResolvable
{
    private TemplateType TemplateType { get; }
    private Status Status { get; set; } = Status.Error("Could not solve template type arguments");
    
    public UnresolvedTemplateType(TemplateType template_type, Node parameters)
    {
        TemplateType = template_type;
        
        var parameter = parameters.First;

        while (parameter != null)
        {
            var next = parameter.Next;
            Add(parameter);
            parameter = next;
        }
    }
    
    public Node? Resolve(Context context)
    {
        // Try to solve all the parameters
        var parameter = First;

        while (parameter != null)
        {
            Resolver.Resolve(context, parameter);
            parameter = parameter.Next;
        }
        
        // Try to retrieve the types of the parameters
        var types = Resolver.GetTypes(this);

        // Parameter types must be known
        if (types == null)
        {
            return null;
        }
        
        Status = Status.OK;
        
        return new TypeNode(TemplateType[types.ToArray()]);
    }

    public Status GetStatus()
    {
        return Status;
    }
}