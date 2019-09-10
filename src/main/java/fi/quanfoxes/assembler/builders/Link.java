package fi.quanfoxes.assembler.builders;

import fi.quanfoxes.assembler.*;
import fi.quanfoxes.assembler.builders.References.ReferenceType;
import fi.quanfoxes.assembler.references.*;
import fi.quanfoxes.parser.Variable;
import fi.quanfoxes.parser.nodes.FunctionNode;
import fi.quanfoxes.parser.nodes.LinkNode;
import fi.quanfoxes.parser.nodes.VariableNode;

public class Link {
    public static Instructions build(Unit unit, LinkNode node, ReferenceType type) {
        Instructions instructions = new Instructions();
        
        if (node.getRight() instanceof FunctionNode) {
            Instructions left = References.read(unit, node.getLeft());
            instructions.append(left);

            Instructions call = Call.build(unit, left.getReference(), (FunctionNode)node.getRight());
            instructions.append(call).setReference(call.getReference());
        }
        else if (node.getRight() instanceof VariableNode) {
            Variable variable = ((VariableNode)node.getRight()).getVariable(); 
            
            if (type != ReferenceType.DIRECT) {
                Register register = unit.contains(variable);

                if (register != null) {
                    return instructions.setReference(register.getValue());
                }
            }

            Instructions left = References.register(unit, node.getLeft());
            instructions.append(left);
            
            Reference reference = new MemoryReference(left.getReference().getRegister(), variable.getAlignment(), variable.getType().getSize());

            if (type == ReferenceType.VALUE || type == ReferenceType.REGISTER) {
                Instructions move = Memory.toRegister(unit, reference);
                instructions.append(move);

                Size size = Size.get(variable.getType().getSize());

                return instructions.setReference(Value.getOperation(move.getReference().getRegister(), size));
            }
            else {
                return instructions.setReference(reference);
            }
        }
  
        return instructions;
    }
}