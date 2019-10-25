public class Uint : Number 
{
    private const int BYTES = 4;

    public Uint() : base(NumberType.UINT32, 32, "uint") {}

    public override int GetSize() 
	{
        return BYTES;
    }
}
