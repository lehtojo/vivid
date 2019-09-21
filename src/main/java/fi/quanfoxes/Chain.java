package fi.quanfoxes;

import java.util.concurrent.ExecutorService;

public class Chain {
    private ExecutorService executors;
    private Class<? extends Phase>[] phases;

    @SafeVarargs
    public Chain(ExecutorService executors, Class<? extends Phase>... phases) {
        this.executors = executors;
        this.phases = phases;
    }

    public void execute(Bundle bundle) {
        for (var template : phases) {

            boolean multithreaded = bundle.getBool("multithreaded", true);

            try {
                Phase phase = template.getConstructor().newInstance();
                phase.setExecutors(executors, multithreaded);
                phase.execute(bundle);
                phase.sync();

                if (!phase.succeeded()) {
                    break;
                }
            }    
            catch (Exception e) {
                System.err.println("Internal error: " + e.getMessage());
                return;
            }
        }
    }
}