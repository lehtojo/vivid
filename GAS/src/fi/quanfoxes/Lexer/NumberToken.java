package fi.quanfoxes.Lexer;

public class NumberToken extends Token {
    private Long number;
    private NumberType numberType;
    private int bits;

    public NumberToken(Lexer.TokenArea area) {
        super(area.text, TokenType.NUMBER);
        number = Long.parseLong(area.text);
        bits = (int)(Math.log(Long.highestOneBit(number)) / Math.log(2)) + 1;

        if (bits <= 8) {
            numberType = NumberType.BYTE;
        }
        else if (bits <= 16) {
            numberType = NumberType.SHORT;
        }
        else if (bits <= 32) {
            numberType = NumberType.INT;
        }
        else if (bits <= 64) {
            numberType = NumberType.LONG;
        }
    }

    public<T extends Number> T getNumber () {
        return (T)number;
    }

    public NumberType getNumberType () {
        return numberType;
    }

    public int getBitCount () {
        return bits;
    }
}
