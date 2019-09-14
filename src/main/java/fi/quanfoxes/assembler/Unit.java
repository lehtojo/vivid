package fi.quanfoxes.assembler;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Map;
import java.util.Optional;

import fi.quanfoxes.assembler.builders.*;
import fi.quanfoxes.assembler.builders.References.ReferenceType;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Variable;
import fi.quanfoxes.parser.nodes.ConstructionNode;
import fi.quanfoxes.parser.nodes.FunctionNode;
import fi.quanfoxes.parser.nodes.IfNode;
import fi.quanfoxes.parser.nodes.JumpNode;
import fi.quanfoxes.parser.nodes.LabelNode;
import fi.quanfoxes.parser.nodes.LinkNode;
import fi.quanfoxes.parser.nodes.OperatorNode;
import fi.quanfoxes.parser.nodes.ReturnNode;
import fi.quanfoxes.parser.nodes.LoopNode;

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
        this.eax = new Register(Map.of(Size.DWORD, "eax", Size.WORD, "ax", Size.BYTE, "al"));
        this.ebx = new Register(Map.of(Size.DWORD, "ebx", Size.WORD, "bx", Size.BYTE, "bl"));
        this.ecx = new Register(Map.of(Size.DWORD, "ecx", Size.WORD, "cx", Size.BYTE, "cl"));
        this.edx = new Register(Map.of(Size.DWORD, "edx", Size.WORD, "dx", Size.BYTE, "dl"));
        this.esi = new Register(Map.of(Size.DWORD, "esi"));
        this.edi = new Register(Map.of(Size.DWORD, "edi"));
        this.ebp = new Register(Map.of(Size.DWORD, "ebp"));
        this.esp = new Register(Map.of(Size.DWORD, "esp"));

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
        return register.isReserved() && register.getValue().getValueType() == ValueType.OBJECT_POINTER;
    }

    public Register getObjectPointer() {
        for (Register register : registers) {
            if (register.isReserved() && register.getValue().getValueType() == ValueType.OBJECT_POINTER) {
                return register;
            }
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

    /**
     * Returns a possible cache reference to variable
     * @param variable Variable to look for
     * @return Success: Cache reference to the variable, Failure: null
     */
    public Reference cached(Variable variable) {
        for (Register register : registers) {
            if (register.contains(variable)) {
                return register.getValue();
            }
        }

        return null;
    }

    public Register contains(Variable variable) {
        for (Register register : registers) {
            if (register.contains(variable)) {
                return register;
            }
        }

        return null;
    }

    /**
     * Turns node tree structure into assembly
     * @param node Program represented in node tree form
     * @return Assembly representation of the node tree
     */
    public Instructions assemble(Node node) {
        switch (node.getNodeType()) {
            
            case OPERATOR_NODE: {
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

            case FUNCTION_NODE: {
                return Call.build(this, (FunctionNode)node);
            }

            case CONSTRUCTION_NODE: {
                return Construction.build(this, (ConstructionNode)node);
            }

            case IF_NODE: {
                return Conditionals.start(this, (IfNode)node);
            }

            case LOOP_NODE: {
                return Loop.build(this, (LoopNode)node);
            }

            case RETURN_NODE: {
                return Return.build(this, (ReturnNode)node);
            }

            case JUMP_NODE: {
                return Labels.build(this, (JumpNode)node);
            }

            case LABEL_NODE: {
                return Labels.build(this, (LabelNode)node);
            }

            default: {
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
    }

	public void step() {
        for (Register register : registers) {
            if (register.isReserved()) {
                Value value = register.getValue();
                value.setCritical(false);
            }
        }
    }
    
    public String getPrefix() {
        return prefix;
    }

	public String getLabel() {
		return String.format("%s_L%d", prefix, counter++);
    }
    
    public Unit clone() {
        return new Unit(this);
    }
}