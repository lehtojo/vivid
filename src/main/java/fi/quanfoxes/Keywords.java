package fi.quanfoxes;

import java.util.HashMap;
import java.util.Map;

public class Keywords {
    public static Keyword BASE = new Keyword("base");
    public static Keyword CASE = new FlowKeyword("case");
    public static Keyword CONTINUE = new FlowKeyword("continue");
    public static Keyword DEINIT = new Keyword("deinit");
    public static Keyword ELSE = new FlowKeyword("else");
    public static Keyword EXTERNAL = new Keyword("external");
    public static Keyword FUNC = new Keyword("func");
    public static Keyword GOTO = new FlowKeyword("goto");
    public static Keyword IF = new FlowKeyword("if");
    public static Keyword IMPORT = new Keyword("import");
    public static Keyword INIT = new Keyword("init");
    public static Keyword LOCK = new FlowKeyword("lock");
    public static Keyword LOOP = new Keyword("loop");
    public static Keyword NEW = new Keyword("new");
    public static Keyword PRIVATE = new AccessModifierKeyword("private", AccessModifier.PRIVATE);
    public static Keyword PROTECTED = new AccessModifierKeyword("protected", AccessModifier.PROTECTED);
    public static Keyword PUBLIC = new AccessModifierKeyword("public", AccessModifier.PUBLIC);
    public static Keyword READONLY = new AccessModifierKeyword("readonly", AccessModifier.READONLY);
    public static Keyword RETURN = new FlowKeyword("return");
    public static Keyword STATIC = new AccessModifierKeyword("static", AccessModifier.STATIC);
    public static Keyword STOP = new FlowKeyword("stop");
    public static Keyword THIS = new Keyword("this");
    public static Keyword TYPE = new Keyword("type");
    public static Keyword VAR = new Keyword("var");
    public static Keyword WHILE = new Keyword("while");

    private static Map<String, Keyword> keywords = new HashMap<>();

    static  {
        keywords.put(BASE.getIdentifier(), BASE);
        keywords.put(CASE.getIdentifier(), CASE);
        keywords.put(CONTINUE.getIdentifier(), CONTINUE);
        keywords.put(DEINIT.getIdentifier(), DEINIT);
        keywords.put(ELSE.getIdentifier(), ELSE);
        keywords.put(EXTERNAL.getIdentifier(), EXTERNAL);
        keywords.put(FUNC.getIdentifier(), FUNC);
        keywords.put(GOTO.getIdentifier(), GOTO);
        keywords.put(IF.getIdentifier(), IF);
        keywords.put(IMPORT.getIdentifier(), IMPORT);
        keywords.put(INIT.getIdentifier(), INIT);
        keywords.put(LOCK.getIdentifier(), LOCK);
        keywords.put(LOOP.getIdentifier(), LOOP);
        keywords.put(NEW.getIdentifier(), NEW);
        keywords.put(PRIVATE.getIdentifier(), PRIVATE);
        keywords.put(PROTECTED.getIdentifier(), PROTECTED);
        keywords.put(PUBLIC.getIdentifier(), PUBLIC);
        keywords.put(READONLY.getIdentifier(), READONLY);
        keywords.put(RETURN.getIdentifier(), RETURN);
        keywords.put(STATIC.getIdentifier(), STATIC);
        keywords.put(STOP.getIdentifier(), STOP);
        keywords.put(THIS.getIdentifier(), THIS);
        keywords.put(TYPE.getIdentifier(), TYPE);
        keywords.put(VAR.getIdentifier(), VAR);
        keywords.put(WHILE.getIdentifier(), WHILE);
    }

    public static boolean exists(String name) {
        return keywords.containsKey(name);
    }

    public static Keyword get(String text) {
        return keywords.get(text);
    }
}
