package fi.quanfoxes.phases;

import java.io.File;
import java.util.ArrayDeque;
import java.util.Arrays;
import java.util.Queue;
import java.util.Vector;

import fi.quanfoxes.BinaryType;
import fi.quanfoxes.Bundle;
import fi.quanfoxes.Phase;
import fi.quanfoxes.Status;

public class ConfigurationPhase extends Phase {

    public static final String EXTENSION = ".z";
    public static final String DEFAULT_OUTPUT = "z";

    private Vector<String> libraries = new Vector<>();
    private Vector<String> files = new Vector<>();

    private void collect(Bundle bundle, File folder, boolean recursive) {
        for (File item : folder.listFiles()) {
            if (item.isFile()) {
                if (item.getName().endsWith(EXTENSION)) {
                    files.add(item.getAbsolutePath());
                }
            }
            else if (recursive) {
                collect(bundle, item, true);
            }
        }
    }

    private Status configure(Bundle bundle, String option, Queue<String> parameters) {
        switch (option) {
            case "-help":
            case "--help": {
                String[][] options = 
                {
                    { "-help / --help",                         "Displays this information" },
                    { "-r <folder> / --recursive <folder>",     "Includes source files (.z) from the given folder and its subfolders"},
                    { "-d / --debug",                           "Generates the output binary with debug info" },
                    { "-o <filename> / --output <filename>",    "Sets the output filename (Default: z.asm, z.o, z, ...)" },
                    { "-l <library> / --library <library>",     "Includes a library to the compilation process" },
                    { "--asm",                                  "Exports the generated assembly to a file" },
                    { "--shared",                               "Sets the output type to shared library (.so)" },
                    { "--static",                               "Sets the output type to static library (.a)"}, 
                    { "-st / --single-thread",                  "Compiles on a single thread instead of multiple threads" },
                    { "-q / --quiet",                           "Suppresses the console output" }
                };

                StringBuilder builder = new StringBuilder();

                for (String[] row : options) {
                    String name = row[0];
                    String description = row[1];

                    builder.append(String.format("\t%-40s%s\n", name, description));
                }

                System.out.println
                (
                    "Usage: zz [options] <folders / files>" + "\n" +
                    "Options:" + "\n" +
                    builder.toString()
                );

                abort();
                return Status.OK;
            }

            case "-r":
            case "--recursive": {
                String folder = parameters.poll();

                if (folder == null || isOption(folder)) {
                    return Status.error("Missing or invalid value for option '%s'", option);
                }

                File handle = new File(folder);

                if (!handle.exists()) {
                    return Status.error("Couldn't find folder '%s'", folder);
                }

                collect(bundle, handle, true);
                return Status.OK;
            }

            case "-d":
            case "--debug": {
                bundle.putBool("debug", true);
                return Status.OK;
            }

            case "-o":
            case "--output": {
                String output = parameters.poll();

                if (output == null || isOption(output)) {
                    return Status.error("Missing or invalid value for option '%s'", option);
                }

                bundle.put("output", output);
                return Status.OK;
            }

            case "-l":
            case "--lib":
            case "--library": {
                String library = parameters.poll();

                if (library == null || isOption(library)) {
                    return Status.error("Missing or invalid value for option '%s'", option);
                }

                libraries.add(library);
                return Status.OK;
            }

            case "--asm": {
                bundle.putBool("assembly", true);
                return Status.OK;
            }

            case "--shared": {
                bundle.put("output_type", BinaryType.SHARED_LIBRARY);
                return Status.OK;
            }

            case "--static": {
                bundle.put("output_type", BinaryType.STATIC_LIBRARY);
                return Status.OK;
            }

            case "-st":
            case "--single-thread": {
                bundle.putBool("multithreaded", false);
                return Status.OK;
            }

            default: {
                return Status.error("Unknown option");
            }
        }
    }

    private boolean isOption(String element) {
        return element.charAt(0) == '-';
    }

    @Override
    public Status execute(Bundle bundle) {
        String[] arguments = bundle.get("arguments", null);

        if (arguments == null) {
            return Status.error("Couldn't configure settings");
        }

        Queue<String> parameters = new ArrayDeque<String>(Arrays.asList(arguments));

        while (!parameters.isEmpty()) {
            String element = parameters.poll();

            if (isOption(element)) {
                Status status = configure(bundle, element, parameters);

                if (status.isProblematic()) {
                    return status;
                }
            }
            else {
                File file = new File(element);

                if (!file.exists()) {
                    return Status.error("Invalid source file/folder '%s'", element);
                }

                if (file.isDirectory()) {
                    collect(bundle, file.getAbsoluteFile(), false);
                }
                else if (file.getName().endsWith(EXTENSION)) {
                    files.add(file.getAbsolutePath());
                }
                else {
                    return Status.error("Source file must have '%s' extension", EXTENSION);
                }
            }
        }

        bundle.put("input_files", files.toArray(new String[files.size()]));
        bundle.put("libraries", libraries.toArray(new String[libraries.size()]));

        if (!bundle.has("output")) {
            bundle.put("output", DEFAULT_OUTPUT);
        }

        return Status.OK;
    }
}