package fi.quanfoxes.phases;

import java.io.IOException;
import java.lang.ProcessBuilder.Redirect;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.nio.file.StandardOpenOption;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.stream.Collectors;

import fi.quanfoxes.Bundle;
import fi.quanfoxes.Phase;
import fi.quanfoxes.Status;
import fi.quanfoxes.assembler.Assembler;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.phases.ParserPhase.Parse;

public class AssemblerPhase extends Phase {
    private static final String COMPILER = "yasm";
    private static final String LINKER = "ld";

    private static final String COMPILER_DEBUG_ARGUMENT = "-g dwarf2";
    private static final String COMPILER_PLATFORM = "-f elf32";

    private static final String LINKER_PLATFORM = "-m elf_i386";
    private static final String LINKER_STANDARD_LIBRARY = "libz.o";

    private static final String ERROR = "Internal assembler failed";

    private Status run(List<String> arguments) {
        String combined = arguments.stream().collect(Collectors.joining(" "));

        ProcessBuilder builder = new ProcessBuilder(combined.split(" "));
        builder.inheritIO();
        builder.directory(Paths.get("").toAbsolutePath().toFile());
        builder.redirectInput(Redirect.INHERIT);
        builder.redirectOutput(Redirect.INHERIT);
        builder.redirectError(Redirect.INHERIT);

        try {
            Process process = builder.start();
            return process.waitFor() == 0 ? Status.OK : Status.error(ERROR);

        } catch (Exception e) {
            return Status.error(ERROR);
        }
    }

    private Status compile(Bundle bundle, String input, String output) {
        boolean debug = bundle.getBool("debug", false);
        boolean delete = !bundle.getBool("assembly", false);
        
        List<String> arguments = new ArrayList<>(Arrays.asList(COMPILER, COMPILER_PLATFORM));

        if (debug) {
            arguments.add(COMPILER_DEBUG_ARGUMENT);
        }

        arguments.add(String.format("-o %s", output));
        arguments.add(input);

        Status status = run(arguments);

        if (delete) {
            try {
                Files.delete(Paths.get(input));
            } catch (IOException e) {
                System.err.println("Warning: Couldn't remove generated assembly file");
            }
        }

        return status;
    }

    private Status link(Bundle bundle, String input, String output) {
        List<String> arguments = new ArrayList<>(Arrays.asList(LINKER, LINKER_PLATFORM));

        arguments.add(String.format("-o %s", output));
        arguments.add(input);
        arguments.add(LINKER_STANDARD_LIBRARY);

        return run(arguments);
    }

    @Override
    public Status execute(Bundle bundle) {
        Parse parse = bundle.get("parse", null);

        if (parse == null) {
            return Status.error("Nothing to assemble");
        }

        String output = bundle.getString("output", ConfigurationPhase.DEFAULT_OUTPUT);
        String source = output + ".asm";
        String object = output + ".o";

        Node node = parse.getNode();
        Context context = parse.getContext();

        String assembly = Assembler.build(node, context);

        try {
            Files.writeString(Paths.get(source), assembly, StandardOpenOption.CREATE);
        }
        catch (Exception e) {
            return Status.error("Couldn't move generated assembly into a file");
        }

        if (compile(bundle, source, object).isProblematic()) {
            return Status.error(ERROR);
        }
        
        if (link(bundle, object, output).isProblematic()) {
            return Status.error(ERROR);
        }

        return Status.OK;
    }
}