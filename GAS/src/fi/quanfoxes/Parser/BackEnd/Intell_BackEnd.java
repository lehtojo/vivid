package fi.quanfoxes.Parser;

import fi.quanfoxes.Lexer.Token;

import java.util.List;

public class OpCode {
    public String opcode = " ";
    public String Repeater = " ";

    public boolean Left_immediate = false;
    public String Destination = " ";
    public String Left_Math = " ";
    public String Left_Number = " ";

    public boolean Right_immediate = false;
    public String Source = " ";
    public String Right_byteSize = " ";
    public String Right_Math = " ";
    public String Right_Number = " ";



    public String Combine()
    {
        String result = Repeater + opcode;
        if (Left_immediate)
        {
            result += "[" + Destination + Left_Math + Left_Number + "]";
        }
        else
        {
            result += Destination + ", ";
        }
        if (Right_immediate)
        {
            result += Right_byteSize + " ";
            result += "[" + Source + Right_Math + Right_Number + "]";
        }
        else
        {
            result += Source;
        }
        return result;
    }
}
