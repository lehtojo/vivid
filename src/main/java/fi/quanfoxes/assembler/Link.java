package fi.quanfoxes.assembler;

import fi.quanfoxes.assembler.References.ReferenceType;
import fi.quanfoxes.parser.Variable;
import fi.quanfoxes.parser.nodes.FunctionNode;
import fi.quanfoxes.parser.nodes.LinkNode;
import fi.quanfoxes.parser.nodes.VariableNode;

public class Link {
    public static Instructions build(Unit unit, LinkNode node, ReferenceType type) {
        Instructions instructions = new Instructions();

        Instructions left = References.read(unit, node.getLeft());
        instructions.append(left);

        if (node.getRight() instanceof FunctionNode) {
            Instructions call = Call.build(unit, left.getReference(), (FunctionNode)node.getRight());
            instructions.append(call).setReference(call.getReference());
        }
        else if (node.getRight() instanceof VariableNode) {
            Reference reference = left.getReference();

            if (!left.getReference().isRegister()) {
                Instructions move = Memory.move(unit, reference, Reference.from(type == ReferenceType.WRITE ? unit.edi : unit.esi));
                instructions.append(move);

                reference = move.getReference();
            }

            Variable variable = ((VariableNode)node.getRight()).getVariable();
            instructions.setReference(new MemoryReference(reference.getRegister(), variable.getAlignment(), variable.getType().getSize()));
        }
  
        return instructions;
    }
}