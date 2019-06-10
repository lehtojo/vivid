package fi.quanfoxes.Parser.BackEnd;

import fi.quanfoxes.Lexer.NumberToken;
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
                    int value = 0;
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
                                    if (tokens.get(i+4).getText().equals("+"))
                                    {
                                        if (tokens.get(i+5).getType() == TokenType.NUMBER)
                                        {
                                            NumberToken Left = (NumberToken) tokens.get(i+3);
                                            NumberToken Right = (NumberToken) tokens.get(i+5);
                                            value = Left.getNumber().intValue() + Right.getNumber().intValue();
                                        }
                                    }
                                    if (tokens.get(i+4).getText().equals("-"))
                                    {
                                        if (tokens.get(i+5).getType() == TokenType.NUMBER)
                                        {
                                            NumberToken Left = (NumberToken) tokens.get(i+3);
                                            NumberToken Right = (NumberToken) tokens.get(i+5);
                                            value = Left.getNumber().intValue() - Right.getNumber().intValue();
                                        }
                                    }
                                    if (tokens.get(i+4).getText().equals("/"))
                                    {
                                        if (tokens.get(i+5).getType() == TokenType.NUMBER)
                                        {
                                            NumberToken Left = (NumberToken) tokens.get(i+3);
                                            NumberToken Right = (NumberToken) tokens.get(i+5);
                                            value = Left.getNumber().intValue() / Right.getNumber().intValue();
                                        }
                                    }
                                    if (tokens.get(i+4).getText().equals("*"))
                                    {
                                        if (tokens.get(i+5).getType() == TokenType.NUMBER)
                                        {
                                            NumberToken Left = (NumberToken) tokens.get(i+3);
                                            NumberToken Right = (NumberToken) tokens.get(i+5);
                                            value = Left.getNumber().intValue() * Right.getNumber().intValue();
                                        }
                                    }
                                }
                            }
                            if (tokens.get(i+3).getType() == TokenType.VARIABLE)
                            {
                                boolean exist2 = false;
                                int value2;
                                Right_immediate = true;
                                int x = 0;
                                for (int j = 0; j < variables.size(); j++)
                                {
                                    if (tokens.get(i+3).getText().equals(variables.get(j)))
                                    {
                                        value2 = variables.get(j).value;
                                        exist2 = true;
                                        Source = "ebp";
                                        x = variables.get(j).X;
                                    }
                                    if (exist2)
                                    {
                                        Right_Math = "-";
                                        Right_Number = String.valueOf(Math.abs(x));
                                    }
                                    else
                                    {
                                        String error = String.format("  G::Error %d doesn´t seem to exist´s", variables.get(j).name);
                                        System.out.println(error);
                                    }
                                }
                            }
                        }
                    }
                    if (exists)
                    {
                        variables.get(i).value = value;
                    }
                    else
                    {
                        Variable v = new Variable();
                        if (variables.size() == 0)
                        {
                            v.X = 0;
                        }
                        else
                        {
                            v.X = variables.get(variables.size() - 1).X + 1;
                        }
                        variables.add(v);
                    }
                }
            }
        }
    }

}
