
import fi.quanfoxes.DataTypeDatabase;
import fi.quanfoxes.Lexer.NameToken;
import fi.quanfoxes.Lexer.NumberToken;
import fi.quanfoxes.Parser.BackEnd.Intel.IntelProvider;
import fi.quanfoxes.Parser.instructions.AddInstruction;
import fi.quanfoxes.Parser.instructions.CreateLocalVariableInstruction;
import org.junit.jupiter.api.Test;

import java.io.FileWriter;
import java.io.IOException;
import java.io.StringWriter;
import java.io.Writer;
import java.util.Arrays;

import static org.junit.jupiter.api.Assertions.assertIterableEquals;
import static org.junit.jupiter.api.Assertions.assertSame;

public class IntelProviderTest {
    @Test
    public void addition_variableAndNumber () throws Exception {
        IntelProvider provider = new IntelProvider();
        StringWriter writer = new StringWriter();
        provider.setOutput(writer);

        CreateLocalVariableInstruction local = new CreateLocalVariableInstruction(DataTypeDatabase.get("num"), "a");
        AddInstruction addInstruction = new AddInstruction(new NameToken("a"), new NumberToken(6));

        provider.Parameeter(Arrays.asList(local, addInstruction));

        assertSame("mov eax, [bp - 2]\nadd eax, 6", writer.toString());
    }
}
