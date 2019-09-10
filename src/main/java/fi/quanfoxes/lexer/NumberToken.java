package fi.quanfoxes.lexer;

import java.util.Objects;

public class NumberToken extends Token {
    private Long number;
    private NumberType numberType;
    private int bits;

    public NumberToken(String text) {
        super(TokenType.NUMBER);
        number = Long.parseLong(text);
        numberType = NumberType.INT32;
        bits = 32;
    }

    private void calculateBitCount() {
        bits = (int)(Math.log(Long.highestOneBit(number)) / Math.log(2)) + 1;

        if (bits <= 8) {
            numberType = NumberType.UINT8;
        }
        else if (bits <= 16) {
            numberType = NumberType.UINT16;
        }
        else if (bits <= 32) {
            numberType = NumberType.UINT32;
        }
        else if (bits <= 64) {
            numberType = NumberType.UINT64;
        }
    }

    public NumberToken(byte number) {
        super(TokenType.NUMBER);
        this.number = (long)number;
        calculateBitCount();
    }

    public NumberToken(short number) {
        super(TokenType.NUMBER);
        this.number = (long)number;
        calculateBitCount();
    }

    public NumberToken(int number) {
        super(TokenType.NUMBER);
        this.number = (long)number;
        calculateBitCount();
    }

    public NumberToken(long number) {
        super(TokenType.NUMBER);
        this.number = number;
        calculateBitCount();
    }

    public Number getNumber () {
        return number;
    }

    public NumberType getNumberType () {
        return numberType;
    }

    public int getBitCount () {
        return bits;
    }

    @Override
    public String getText() {
        switch (numberType) {
            case UINT8:
                return String.valueOf(number.byteValue());
            case INT16:
                return String.valueOf(number.shortValue());
            case INT32:
                return String.valueOf(number.intValue());
            case INT64:
                return String.valueOf(number.longValue());
            default:
                return "";
        }
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (!(o instanceof NumberToken)) return false;
        if (!super.equals(o)) return false;
        NumberToken that = (NumberToken) o;
        return bits == that.bits &&
                Objects.equals(number, that.number) &&
                numberType == that.numberType;
    }

    @Override
    public int hashCode() {
        return Objects.hash(super.hashCode(), number, numberType, bits);
    }
}
