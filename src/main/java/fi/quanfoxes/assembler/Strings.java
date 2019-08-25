package fi.quanfoxes.assembler;

import fi.quanfoxes.parser.nodes.StringNode;

public class Strings {
    public static String build(StringNode string, String label) {
        string.setIdentifier(label);
        return String.format("%s db '%s', 0", label, string.getText());
    }
}