package fi.quanfoxes.Parser.BackEnd;

import fi.quanfoxes.Lexer.Token;
import fi.quanfoxes.Lexer.TokenType;

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

    public void Translator(List<Token> tokens)
    {
        for (int i = 0; i < tokens.size(); i++)
        {
            if (tokens.get(i).getType() == TokenType.TYPE)
            {
                if (tokens.get(i).getText().equals("num"))
                {
                    // do something m8
                }
                if (tokens.get(i+1).getType() == TokenType.VARIABLE)
                {
                    boolean exists = false;
                    int value;
                    for (int j = 0; j < variables.size(); j++)
                    {
                        if (tokens.get(i+1).getText().equals(variables.get(j)))
                        {
                            exists = true;
                            value = variables.get(j).value;
                        }
                    }
                    if (tokens.get(i+2).getType() == TokenType.OPERATOR)
                    {
                        if (tokens.get(i+2).getText().equals("="))
                        {
                            opcode = "mov";
                            if (tokens.get(i+3).getType() == TokenType.NUMBER)
                            {
                                if (tokens.get(i+4).getType() == TokenType.OPERATOR)
                                {
                                    if (tokens.get(i+4).getText().equals("+") || tokens.get(i+4).getText().equals("-") || tokens.get(i+4).getText().equals("/") || tokens.get(i+4).getText().equals("*"))
                                    {

                                    }
                                }
                            }
                            if (tokens.get(i+3).getType() == TokenType.VARIABLE)
                            {

                            }
                        }
                        if (tokens.get(i+2).getText().equals("++"))
                        {
                            opcode = "add";
                        }
                        if (tokens.get(i+3).getType() == TokenType.NUMBER)
                        {

                        }
                        if (tokens.get(i+3).getType() == TokenType.VARIABLE)
                        {

                        }
                    }
                    if (exists)
                    {

                    }
                }
            }
        }
    }

}
