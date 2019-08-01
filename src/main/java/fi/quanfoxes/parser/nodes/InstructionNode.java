package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.Keyword;
import fi.quanfoxes.parser.Node;

public class InstructionNode extends Node {
    private Keyword instruction;

    public InstructionNode(Keyword instruction) {
        this.instruction = instruction;
    }

    public Keyword getInstruction() {
        return instruction;
    }
}