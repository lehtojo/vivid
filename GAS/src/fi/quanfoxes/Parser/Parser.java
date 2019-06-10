package fi.quanfoxes.Parser;

import fi.quanfoxes.Lexer.Token;

import java.util.List;

public class Parser {
    public int Reserved_REGISTER;
    public List<String> OUT;
    public List<Token> tokens;

    public void Add_To_Parser(List<Token> token)
    {
        tokens = token;
    }

    public void Translator()
    {

    }
}
