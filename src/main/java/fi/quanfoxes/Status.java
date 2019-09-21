package fi.quanfoxes;

public class Status {
    public static final Status OK = new Status("OK", false);

    private String description;
    private boolean problematic;

    public Status(String description, boolean problematic) {
        this.description = description;
        this.problematic = problematic;
    }

    public String getDescription() {
        return description;
    }

    public boolean isProblematic() {
        return problematic;
    }

    public static Status error(String format, Object... args) {
        return new Status(String.format(format, args), true);
    }

    public static Status error(String description) {
        return new Status(description, true);
    }
}