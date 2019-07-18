package fi.quanfoxes.parser.BackEnd.Intel;

import java.io.*;
import java.util.List;

public class IntelProvider {
    public List<Variable> variables;
    public Intell_Backend backend = new Intell_Backend();
    public String opcode = " ";
    public String Repeater = " ";

    public boolean Left_immediate = false;
    public String Destination = " ";
    public String Left_Math = " ";
    public String Left_Number = " ";

    public boolean Right_immediate = false;
    public String Source = " ";
    public String Right_byteSize = " ";
    public String Right_Math = " ";
    public String Right_Number = " ";

    public int eax = 0;
    public int ebx = 0;
    public int ecx = 0;
    public int edx = 0;
    public int esp = 0;
    public int ebp = esp;

    public int regInUse = 0;

    public List<Integer> Stack;
    public List<Integer> Heap;

    private Writer writer;

    public void setOutput(Writer writer) {
        this.writer = writer;
    }


    /*public void Parameeter(List<Instruction> instructions) throws Exception {
        Map<String, Variable> Variables = new HashMap<>();
        BufferedWriter output = new BufferedWriter(writer, 1024);

        for (int i = 0; i < instructions.size(); i++) {
            Instruction present = instructions.get(i);

            if (present instanceof CreateLocalVariableInstruction) {
                CreateLocalVariableInstruction local = (CreateLocalVariableInstruction)present;
                if (Variables.containsKey(local.getName()))
                {
                    throw new Exception("Local variable already exists");
                }
                Variable variable = new Variable();
                variable.name = local.getName();
                variable.offset = 0;
                variable.value = 0;
                Variables.put(local.getName(), variable);
            }

            if (present instanceof AddInstruction) {
                AddInstruction addInstruction = (AddInstruction) present;
                if (addInstruction.getLeft().getType() == TokenType.NUMBER) {
                    NumberToken source = (NumberToken) addInstruction.getLeft();
                    Source = source.getNumber().toString();
                }
                if (addInstruction.getLeft().getType() == TokenType.IDENTIFIER) {
                    IdentifierToken source = (IdentifierToken) addInstruction.getLeft();
                    Variable var = Variables.get(source.getIdentifier());
                    int offset = var.offset;
                    Right_immediate = true;
                    Source = "ebp";
                    Right_Math = "-";
                    Right_Number = String.valueOf(offset);
                    Right_byteSize = "dword";
                }
                if (addInstruction.getRight().getType() == TokenType.NUMBER) {
                    NumberToken source = (NumberToken) addInstruction.getRight();
                    Destination = source.getNumber().toString();
                    opcode = "mov";
                    Source = Destination;
                    if (regInUse == 0)
                    {
                        Destination = "eax";
                    }
                    if (regInUse == 1)
                    {
                        Destination = "ebx";
                    }

                    Intell_Backend backend = new Intell_Backend();
                    backend.Destination = Destination;
                    backend.Source = Source;
                    backend.Left_immediate = Left_immediate;
                    backend.Left_Math = Left_Math;
                    backend.Left_Number = Left_Number;
                    backend.opcode = opcode;
                    backend.Repeater = Repeater;
                    backend.Right_byteSize = Right_byteSize;
                    backend.Right_immediate = Right_immediate;
                    backend.Right_Math = Right_Math;
                    backend.Right_Number = Right_Number;

                    String result = backend.Combine();
                    output.write(result);
                    output.newLine();
                }
                if (addInstruction.getRight().getType() == TokenType.IDENTIFIER) {
                    IdentifierToken destination = (IdentifierToken) addInstruction.getRight();
                    Variable var = Variables.get(destination.getIdentifier());
                    int offset = var.offset;
                    Left_immediate = true;
                    Destination = "ebp";
                    Left_Math = "-";
                    Left_Number = String.valueOf(offset);
                }
                opcode = "add";

            }
        }

        output.flush();
        output.close();
    }*/


}
