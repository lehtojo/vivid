package fi.quanfoxes.parser;

public class Type extends Context {
    private String name;

    public Type(Context context, String name) throws Exception {
        this.name = name;

        super.link(context);
        context.declare(this);
    }

    public String getName() {
        return name;
    }
}