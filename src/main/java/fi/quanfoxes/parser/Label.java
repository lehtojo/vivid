package fi.quanfoxes.parser;

public class Label {
    private Context context;
    private String name;

    public Label(Context context, String name) throws Exception {
        this.context = context;
        this.name = name;

        context.declare(this);
    }

    public String getName() {
        return name;
    }

    public Context getContext() {
        return context;
    }
}