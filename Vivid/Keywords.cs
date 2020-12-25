using System.Collections.Generic;

public static class Keywords
{
	public static readonly Keyword AS = new Keyword("as");
	public static readonly Keyword COMPILES = new Keyword("compiles");
	public static readonly Keyword CONTINUE = new FlowKeyword("continue");
	public static readonly Keyword DEINIT = new Keyword("deinit");
	public static readonly Keyword ELSE = new FlowKeyword("else");
	public static readonly Keyword EXPORT = new AccessModifierKeyword("export", AccessModifier.GLOBAL);
	public static readonly Keyword GOTO = new FlowKeyword("goto");
	public static readonly Keyword HAS = new FlowKeyword("has");
	public static readonly Keyword IF = new FlowKeyword("if");
	public static readonly Keyword INLINE = new AccessModifierKeyword("inline", AccessModifier.INLINE);
	public static readonly Keyword IS = new FlowKeyword("is");
	public static readonly Keyword INIT = new Keyword("init");
	public static readonly Keyword IMPORT = new AccessModifierKeyword("import", AccessModifier.EXTERNAL);
	public static readonly Keyword LOOP = new Keyword("loop");
	public static readonly Keyword OUTLINE = new AccessModifierKeyword("outline", AccessModifier.OUTLINE);
	public static readonly Keyword PRIVATE = new AccessModifierKeyword("private", AccessModifier.PRIVATE);
	public static readonly Keyword PROTECTED = new AccessModifierKeyword("protected", AccessModifier.PROTECTED);
	public static readonly Keyword PUBLIC = new AccessModifierKeyword("public", AccessModifier.PUBLIC);
	public static readonly Keyword READONLY = new AccessModifierKeyword("readonly", AccessModifier.READONLY);
	public static readonly Keyword RETURN = new FlowKeyword("return");
	public static readonly Keyword STATIC = new AccessModifierKeyword("static", AccessModifier.STATIC);
	public static readonly Keyword STOP = new FlowKeyword("stop");
	public static readonly Keyword WHEN = new FlowKeyword("when");

	private static Dictionary<string, Keyword> Values { get; } = new Dictionary<string, Keyword>();

	static Keywords()
	{
		Values.Add(AS.Identifier, AS);
		Values.Add(COMPILES.Identifier, COMPILES);
		Values.Add(CONTINUE.Identifier, CONTINUE);
		Values.Add(ELSE.Identifier, ELSE);
		Values.Add(EXPORT.Identifier, EXPORT);
		Values.Add(GOTO.Identifier, GOTO);
		Values.Add(HAS.Identifier, HAS);
		Values.Add(IF.Identifier, IF);
		Values.Add(INLINE.Identifier, INLINE);
		Values.Add(IS.Identifier, IS);
		Values.Add(IMPORT.Identifier, IMPORT);
		Values.Add(LOOP.Identifier, LOOP);
		Values.Add(OUTLINE.Identifier, OUTLINE);
		Values.Add(PRIVATE.Identifier, PRIVATE);
		Values.Add(PROTECTED.Identifier, PROTECTED);
		Values.Add(PUBLIC.Identifier, PUBLIC);
		Values.Add(READONLY.Identifier, READONLY);
		Values.Add(RETURN.Identifier, RETURN);
		Values.Add(STATIC.Identifier, STATIC);
		Values.Add(STOP.Identifier, STOP);
		Values.Add(WHEN.Identifier, WHEN);
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
