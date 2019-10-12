package fi.quanfoxes;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.Callable;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Future;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.TimeoutException;

/**
 * Represents one phase in compilation
 */
public abstract class Phase {
    private ExecutorService executors;
    private boolean enabled;

    private List<Future<Status>> tasks = new ArrayList<>();

    /**
     * Represents a completed task
     * @param <T> Type of value the task produced
     */
    private class CompletedTask<T> implements Future<T> {
        private T value;

        /**
         * Represents a task that produced the given value
         * @param value Value that the task produced
         */
        public CompletedTask(T value) {
            this.value = value;
        }

        /**
         * Cancels the task
         */
        public boolean cancel(boolean b) {
            return false;
        }

        /**
         * Returns the value that the task produced
         */
        public T get() throws InterruptedException, ExecutionException {
            return value;
        }

        /**
         * Returns the value that the task produced
         */
        public T get(long time, TimeUnit unit) throws InterruptedException, ExecutionException, TimeoutException {
            return value;
        }

        /**
         * Returns whether the task was canceled
         */
        public boolean isCancelled() {
            return false;
        }

        /**
         * Returns whether the task has finished
         */
        public boolean isDone() {
            return true;
        }
    }

    /**
     * Sets the executors for this phase
     * 
     * @param executors Executors to use during this phase
     * @param enabled   Should this phase use multi-threading
     */
    public void setExecutors(ExecutorService executors, boolean enabled) {
        this.executors = executors;
        this.enabled = enabled;
    }

    /**
     * Executes the phase with the given data
     * 
     * @param bundle Data collection that the phase may need
     */
    public abstract Status execute(Bundle bundle);

    /**
     * Executes runnable on another thread if multithreading is enabled, otherwise
     * executes locally
     * 
     * @param runnable Runnable to run
     */
    public void async(Callable<Status> runnable) {
        if (enabled) {
            tasks.add(executors.submit(runnable));
        } else {
            Status status = null;

            try {
                status = runnable.call();
            } catch (Exception e) {
                status = Status.error(e.getMessage());
            }

            tasks.add(new CompletedTask<Status>(status));
        }
    }

    /**
     * Waits for all tasks to finish
     */
    public void sync() {
        int i = 0;

        while (i < tasks.size()) {
            Future<Status> task = tasks.get(i++);
            while (!task.isDone());
        }
    }

    /**
     * Returns whether all tasks succeeded during the phase
     * @return True if all tasks succeeded during the phase
     */
    public boolean succeeded() {
        return !tasks.stream().filter((t) -> {
            try {
                return !t.isDone() || t.get().isProblematic();
            }
            catch (Exception e) {
                return false;
            }
        }).findFirst().isPresent();
    }

    /**
     * Aborts the execution
     */
    public void abort() {
        System.exit(1);
    }
}