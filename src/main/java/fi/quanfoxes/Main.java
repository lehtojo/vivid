package fi.quanfoxes;

import fi.quanfoxes.phases.ConfigurationPhase;
import fi.quanfoxes.phases.AssemblerPhase;
import fi.quanfoxes.phases.FilePhase;
import fi.quanfoxes.phases.LexerPhase;
import fi.quanfoxes.phases.ParserPhase;
import fi.quanfoxes.phases.ResolverPhase;

import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.ThreadFactory;

public class Main {

    public static void main(String[] arguments) throws InterruptedException {
        
        long a = System.nanoTime();

        // Create thread pool for multi-threading
        Runtime runtime = Runtime.getRuntime();
        ThreadFactory factory = Executors.privilegedThreadFactory();
        ExecutorService executors = Executors.newFixedThreadPool(runtime.availableProcessors(), factory);
        
        // Configure the flow of the compiler
        Chain chain = new Chain
        (
            executors, 

            ConfigurationPhase.class, 
            FilePhase.class,                                 
            LexerPhase.class, 
            ParserPhase.class, 
            ResolverPhase.class, 
            AssemblerPhase.class
        );
        
        // Pack the program arguments in the chain
        Bundle bundle = new Bundle();
        bundle.put("arguments", arguments);

        // Execute the chain
        chain.execute(bundle);
        executors.shutdown();

        long b = System.nanoTime();
        System.out.println((b - a) / 1000000000.0f);

        System.exit(0);
    }
}
