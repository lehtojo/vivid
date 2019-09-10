package fi.quanfoxes.assembler.builders;

import fi.quanfoxes.assembler.Instructions;
import fi.quanfoxes.assembler.Unit;
import fi.quanfoxes.parser.Label;
import fi.quanfoxes.parser.nodes.JumpNode;
import fi.quanfoxes.parser.nodes.LabelNode;

public class Labels {
    private static final String PREFIX = "_label_";

    public static Instructions build(Unit unit, LabelNode node) {
        Instructions instructions = new Instructions();

        Label label = node.getLabel();
        String fullname = unit.getPrefix() + PREFIX + label.getName();

        return instructions.label(fullname);
    }

    public static Instructions build(Unit unit, JumpNode node) {
        Instructions instructions = new Instructions();

        Label label = node.getLabel();
        String fullname = unit.getPrefix() + PREFIX + label.getName();

        return instructions.append("jmp %s", fullname);
    }
}