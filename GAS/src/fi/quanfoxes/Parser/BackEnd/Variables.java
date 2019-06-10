package fi.quanfoxes.Parser.BackEnd;

public class Variable {
    public String name;
    public int value;
    public int Y; //the stack frame that the variable is in.
    public int X; //the offset in the stack frame.
}
