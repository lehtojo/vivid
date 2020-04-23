using System.Collections.Generic;

public class Keywords
{
	public static readonly Keyword AS = new Keyword("as");
	public static readonly Keyword BASE = new Keyword("base");
	public static readonly Keyword CASE = new FlowKeyword("case");
	public static readonly Keyword CONTINUE = new FlowKeyword("continue");
	public static readonly Keyword DEINIT = new Keyword("deinit");
	public static readonly Keyword ELSE = new FlowKeyword("else");
	public static readonly Keyword EXTERNAL = new Keyword("external");
	public static readonly Keyword FUNC = new Keyword("func");
	public static readonly Keyword GOTO = new FlowKeyword("goto");
	public static readonly Keyword IF = new FlowKeyword("if");
	public static readonly Keyword IMPORT = new Keyword("import");
	public static readonly Keyword INIT = new Keyword("init");
	public static readonly Keyword LOCK = new FlowKeyword("lock");
	public static readonly Keyword LOOP = new Keyword("loop");
	public static readonly Keyword NEW = new Keyword("new");
	public static readonly Keyword PRIVATE = new AccessModifierKeyword("private", AccessModifier.PRIVATE);
	public static readonly Keyword PROTECTED = new AccessModifierKeyword("protected", AccessModifier.PROTECTED);
	public static readonly Keyword PUBLIC = new AccessModifierKeyword("public", AccessModifier.PUBLIC);
	public static readonly Keyword READONLY = new AccessModifierKeyword("readonly", AccessModifier.READONLY);
	public static readonly Keyword RETURN = new FlowKeyword("return");
	public static readonly Keyword STATIC = new AccessModifierKeyword("static", AccessModifier.STATIC);
	public static readonly Keyword STOP = new FlowKeyword("stop");
	public static readonly Keyword THIS = new Keyword("this");
	public static readonly Keyword TYPE = new Keyword("type");
	public static readonly Keyword VAR = new Keyword("var");
	public static readonly Keyword WHILE = new Keyword("while");

	private static Dictionary<string, Keyword> Values = new Dictionary<string, Keyword>();

	static Keywords()
	{
		Values.Add(AS.Identifier, AS);
		Values.Add(BASE.Identifier, BASE);
		Values.Add(CASE.Identifier, CASE);
		Values.Add(CONTINUE.Identifier, CONTINUE);
		Values.Add(DEINIT.Identifier, DEINIT);
		Values.Add(ELSE.Identifier, ELSE);
		Values.Add(EXTERNAL.Identifier, EXTERNAL);
		Values.Add(FUNC.Identifier, FUNC);
		Values.Add(GOTO.Identifier, GOTO);
		Values.Add(IF.Identifier, IF);
		Values.Add(IMPORT.Identifier, IMPORT);
		Values.Add(INIT.Identifier, INIT);
		Values.Add(LOCK.Identifier, LOCK);
		Values.Add(LOOP.Identifier, LOOP);
		Values.Add(NEW.Identifier, NEW);
		Values.Add(PRIVATE.Identifier, PRIVATE);
		Values.Add(PROTECTED.Identifier, PROTECTED);
		Values.Add(PUBLIC.Identifier, PUBLIC);
		Values.Add(READONLY.Identifier, READONLY);
		Values.Add(RETURN.Identifier, RETURN);
		Values.Add(STATIC.Identifier, STATIC);
		Values.Add(STOP.Identifier, STOP);
		Values.Add(THIS.Identifier, THIS);
		Values.Add(TYPE.Identifier, TYPE);
		Values.Add(VAR.Identifier, VAR);
		Values.Add(WHILE.Identifier, WHILE);
	}

	public static bool Exists(string name)
	{
		return Values.ContainsKey(name);
	}

	public static Keyword Get(string text)
	{
		return Values[text];
	}
}
