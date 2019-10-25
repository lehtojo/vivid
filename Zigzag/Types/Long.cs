public class Long : Number 
{
    private const int BYTES = 8;

    public Long() : base(NumberType.INT64, 64, "long") {}

    public override int GetSize() 
	{
        return BYTES;
    }
}
