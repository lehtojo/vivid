package fi.quanfoxes.phases;

import java.nio.file.Files;
import java.nio.file.Paths;

import fi.quanfoxes.Bundle;
import fi.quanfoxes.Phase;
import fi.quanfoxes.Status;

public class FilePhase extends Phase {

    @Override
    public Status execute(Bundle bundle) {
        final String[] files = bundle.get("input_files", new String[] {});

        if (files.length == 0) {
            return Status.error("Please enter input files");
        }

        final String[] contents = new String[files.length];

        for (int i = 0; i < files.length; i++) {
            final int index = i;

            async(() -> {
                String file = files[index];

                try {
                    String content = Files.readString(Paths.get(file));
                    contents[index] = content;
                }
                catch (Exception e) {
                    return Status.error("Couldn't load file '%s'", file);
                }          

                return Status.OK;
            });
        }

        bundle.put("input_file_contents", contents);

        return Status.OK;
    }
}