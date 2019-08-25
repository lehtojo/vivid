package fi.quanfoxes.assembler;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Optional;

import fi.quanfoxes.assembler.References.ReferenceType;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Variable;
import fi.quanfoxes.parser.nodes.ConstructionNode;
import fi.quanfoxes.parser.nodes.FunctionNode;
import fi.quanfoxes.parser.nodes.IfNode;
import fi.quanfoxes.parser.nodes.LinkNode;
import fi.quanfoxes.parser.nodes.OperatorNode;
import fi.quanfoxes.parser.nodes.ReturnNode;
import fi.quanfoxes.parser.nodes.WhileNode;

public class Unit {
    public Register eax;
    public Register ebx;
    public Register ecx;
    public Register edx;
    public Register esi;
    public Register edi;
    public Register ebp;
    public Register esp;
    
    public List<Register> registers = new ArrayList<>();

    private String prefix;
    private int counter = 1;

    public Unit(String prefix) {
        this.prefix = prefix;
        this.eax = new Register("eax", 4);
        this.ebx = new Register("ebx", 4);
        this.ecx = new Register("ecx", 4);
        this.edx = new Register("edx", 4);
        this.esi = new Register("esi", 4);
        this.edi = new Register("edi", 4);
        this.ebp = new Register("ebp", 4);
        this.esp = new Register("esp", 4);

        registers.addAll(Arrays.asList(eax, ebx, ecx, edx, esi, edi));
    }

    /**
     * Clones an unit
     * @param unit Unit to clone
     */
    private Unit(Unit unit) {
        this.prefix = unit.prefix;
        this.counter = unit.counter;
        this.eax = unit.eax.clone();
        this.ebx = unit.ebx.clone();
        this.ecx = unit.ecx.clone();
        this.edx = unit.edx.clone();
        this.esi = unit.esi.clone();
        this.edi = unit.edi.clone();
        this.ebp = unit.ebp.clone();
        this.esp = unit.esp.clone();

        registers.addAll(Arrays.asList(eax, ebx, ecx, edx, esi, edi));
    }

    /**
     * @return True if any register doesn't hold a value, otherwise false
     */
    public boolean isAnyRegisterAvailable() {
        return registers.stream().filter(Register::isAvailable).findFirst().isPresent();
    }

    /**
     * @return True if any register doesn't hold a value or a held value isn't critical, otherwise false
     */
    public boolean isAnyRegisterUncritical() {
        return registers.stream().filter(r -> !r.isCritical()).findFirst().isPresent();
    }

    public Register getNextRegister() {
        Optional<Register> register = registers.stream().filter(Register::isAvailable).findFirst();

        if (register.isPresent()) {
            return register.get();
        }

        register = registers.stream().filter(r -> !r.isCritical()).findFirst();

        if (register.isPresent()) {
            return register.get();
        }

        return registers.get(0);
    }

    public boolean isObjectPointerLoaded(Register register) {
        return register.isReserved() && register.getValue().getType() == ValueType.OBJECT_POINTER;
    }

    public Register isObjectPointerLoaded() {
        if (esi.isReserved() && esi.getValue().getType() == ValueType.OBJECT_POINTER) {
            return esi;
        }

        if (edi.isReserved() && edi.getValue().getType() == ValueType.OBJECT_POINTER) {
            return edi;
        }
        
        return null;
    }

    public List<Register> getRegisters() {
        return registers;
    }

    public void reset(Variable variable) {
        for (Register register : registers) {
            if (register.contains(variable)) {
                register.reset();
                break;
            }
        }
    }

    public void reset() {
        for (Register register : registers) {
            register.reset();
        }
    }

    public Register contains(Variable variable) {
        for (Register register : registers) {
            if (register.contains(variable)) {
                return register;
            }
        }

        return null;
    }

    public Instructions assemble(Node node) {
        if (node instanceof OperatorNode) {
            OperatorNode operator = (OperatorNode)node;

            switch (operator.getOperator().getType()) {
                case CLASSIC:
                    return Classic.build(this, (OperatorNode)node);
                case ACTION:
                    return Assign.build(this, (OperatorNode)node);
                case INDEPENDENT:
                    return Link.build(this, (LinkNode)node, ReferenceType.READ);
                default:
                    return null;
            }
        }
        else if (node instanceof FunctionNode) {
            return Call.build(this, (FunctionNode)node);
        }
        else if (node instanceof ConstructionNode) {
            return Construction.build(this, (ConstructionNode)node);
        }
        else if (node instanceof IfNode) {
            return Conditionals.start(this, (IfNode)node);
        }
        else if (node instanceof WhileNode) {
            return Loop.build(this, (WhileNode)node);
        }
        else if (node instanceof ReturnNode) {
            return Return.build(this, (ReturnNode)node);
        }
        else {
            Instructions bundle = new Instructions();
            Node iterator = node.first();

            while (iterator != null) {
                Instructions instructions = assemble(iterator);

                if (instructions != null) {
                    bundle.append(instructions);
                }

                step();

                iterator = iterator.next();
            }

            return bundle;
        }
    }

	public void step() {
        for (Register register : registers) {
            if (register.isReserved()) {
                Value value = register.getValue();
                value.setCritical(false);
            }
        }
	}

	public String getLabel() {
		return String.format("%s_L%d", prefix, counter++);
    }
    
    public Unit clone() {
        return new Unit(this);
    }
}