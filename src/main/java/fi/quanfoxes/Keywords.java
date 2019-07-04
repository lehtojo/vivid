package fi.quanfoxes;

import java.util.HashMap;

public class Keywords {
    public static Keyword BASE = new Keyword("base");
    public static Keyword BREAK = new FlowKeyword("break");
    public static Keyword CASE = new FlowKeyword("case");
    public static Keyword CONTINUE = new FlowKeyword("continue");
    public static Keyword ELSE = new FlowKeyword("else");
    public static Keyword EXTERNAL = new Keyword("external");
    public static Keyword FUNC = new Keyword("func");
    public static Keyword GOTO = new FlowKeyword("goto");
    public static Keyword IF = new FlowKeyword("if");
    public static Keyword LOCK = new FlowKeyword("lock");
    public static Keyword LOOP = new FlowKeyword("loop");
    public static Keyword NEW = new Keyword("new");
    public static Keyword PRIVATE = new AccessModifierKeyword("private", AccessModifier.PRIVATE);
    public static Keyword PROTECTED = new AccessModifierKeyword("protected", AccessModifier.PROTECTED);
    public static Keyword PUBLIC = new AccessModifierKeyword("public", AccessModifier.PUBLIC);
    public static Keyword READONLY = new AccessModifierKeyword("readonly", AccessModifier.READONLY);
    public static Keyword RETURN = new FlowKeyword("return");
    public static Keyword STATIC = new AccessModifierKeyword("static", AccessModifier.STATIC);
    public static Keyword THIS = new Keyword("this");
    public static Keyword TYPE = new Keyword("type");

    private static HashMap<String, Keyword> keywords = new HashMap<>();

    static  {
        keywords.put(BASE.getIdentifier(), BASE);
        keywords.put(BREAK.getIdentifier(), BREAK);
        keywords.put(CASE.getIdentifier(), CASE);
        keywords.put(CONTINUE.getIdentifier(), CONTINUE);
        keywords.put(ELSE.getIdentifier(), ELSE);
        keywords.put(EXTERNAL.getIdentifier(), EXTERNAL);
        keywords.put(FUNC.getIdentifier(), FUNC);
        keywords.put(GOTO.getIdentifier(), GOTO);
        keywords.put(IF.getIdentifier(), IF);
        keywords.put(LOCK.getIdentifier(), LOCK);
        keywords.put(LOOP.getIdentifier(), LOOP);
        keywords.put(NEW.getIdentifier(), NEW);
        keywords.put(PRIVATE.getIdentifier(), PRIVATE);
        keywords.put(PROTECTED.getIdentifier(), PROTECTED);
        keywords.put(PUBLIC.getIdentifier(), PUBLIC);
        keywords.put(READONLY.getIdentifier(), READONLY);
        keywords.put(RETURN.getIdentifier(), RETURN);
        keywords.put(STATIC.getIdentifier(), STATIC);
        keywords.put(THIS.getIdentifier(), THIS);
        keywords.put(TYPE.getIdentifier(), TYPE);
    }

    public static boolean exists (String name) {
        return keywords.containsKey(name);
    }

    public static Keyword get(String text) {
        return keywords.get(text);
    }
}
