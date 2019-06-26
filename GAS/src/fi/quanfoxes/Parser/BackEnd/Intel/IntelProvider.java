package fi.quanfoxes.Parser.BackEnd.Intel;

import fi.quanfoxes.Lexer.NameToken;
import fi.quanfoxes.Lexer.NumberToken;
import fi.quanfoxes.Lexer.TokenType;
import fi.quanfoxes.Parser.Instruction;
import fi.quanfoxes.Parser.instructions.AddInstruction;
import fi.quanfoxes.Parser.instructions.CreateLocalVariableInstruction;

import java.io.*;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

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


    public void Parameeter(List<Instruction> instructions) throws Exception {
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
                if (addInstruction.getLeft().getType() == TokenType.NAME) {
                    NameToken source = (NameToken) addInstruction.getLeft();
                    Variable var = Variables.get(source.getName());
                    int offset = var.offset;
                    Right_immediate = true;
                    Source = "ebp";
                    Right_Math = "-";
                    Right_Number = String.valueOf(offset);
                    Right_byteSize = "dword";
                }
                if (addInstruction.getRigth().getType() == TokenType.NUMBER) {
                    NumberToken source = (NumberToken) addInstruction.getLeft();
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
                    String result = backend.Combine();
                    output.write(result);
                    output.newLine();
                }
                if (addInstruction.getRigth().getType() == TokenType.NAME) {
                    NameToken destination = (NameToken) addInstruction.getLeft();
                    Variable var = Variables.get(destination.getName());
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
    }


}
