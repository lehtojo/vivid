package fi.quanfoxes.phases;

import java.util.ArrayList;
import java.util.List;

import fi.quanfoxes.Bundle;
import fi.quanfoxes.Phase;
import fi.quanfoxes.Status;
import fi.quanfoxes.lexer.Lexer;
import fi.quanfoxes.lexer.Token;

public class LexerPhase extends Phase {

    @Override
    public Status execute(Bundle bundle) {
        final String[] contents = bundle.get("input_file_contents", new String[] {});

        if (contents.length == 0) {
            return Status.error("Nothing to tokenize");
        }

        @SuppressWarnings("unchecked")
        final List<Token>[] tokens = new ArrayList[contents.length];

        for (int i = 0; i < contents.length; i++) {
            final int index = i;

            async(() -> {
                String content = contents[index];

                try {
                    tokens[index] = Lexer.getTokens(content);
                }
                catch (Exception e) {
                    return Status.error(e.getMessage());
                }

                return Status.OK;
            });
        }

        bundle.put("input_file_tokens", tokens);

        return Status.OK;
    }
}