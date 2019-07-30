type Base 
{
    private num a

    init
    {
        a = 1
    }
}

type Derived : Base
{
    private num b

    init
    {
        b = 2
    }

    func getSum
    {
        return a + b
    }
}

func run
{
    Base b = new Derived()
    num sum = b->Derived.getSum()
}