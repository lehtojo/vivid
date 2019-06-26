package fi.quanfoxes.Parser.BackEnd;

import fi.quanfoxes.Lexer.NumberToken;
import fi.quanfoxes.Lexer.Token;
import fi.quanfoxes.Lexer.TokenType;
import fi.quanfoxes.Parser.Instruction;
import fi.quanfoxes.Parser.instructions.Mul;

import java.util.List;

public class IntelProvider {
    public List<Variable> variables;
    public Intell_Backend backend = new Intell_Backend();
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

    public int eax = 0;
    public int ebx = 0;
    public int ecx = 0;
    public int edx = 0;
    public int esp = 0;
    public int ebp = esp;
    public List<Integer> Stack;
    public List<Integer> Heap;

    public void getNextInstructon(Instruction instruction)
    {
        if (instruction instanceof Mul)
        {
            Mul mul = (Mul) instruction;
            if ()
        }
    }

    public int getFreeRegister()
    {
        if (eax == 0)
        {
            return 0;
        }
        else if (ebx == 0)
        {
            return 1;
        }
        else if (ecx == 0)
        {
            return 2;
        }
        else if (edx == 0)
        {
            return 3;
        }
        else
        {
            return 4;
        }
    }


}
