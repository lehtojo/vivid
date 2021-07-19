using System.Collections.Generic;

public static class Keywords
{
	public static readonly Keyword AS = new("as");
	public static readonly Keyword COMPILES = new("compiles");
	public static readonly Keyword CONSTANT = new ModifierKeyword("constant", Modifier.CONSTANT);
	public static readonly Keyword CONTINUE = new("continue", KeywordType.FLOW);
	public static readonly Keyword DEINIT = new("deinit");
	public static readonly Keyword ELSE = new("else", KeywordType.FLOW);
	public static readonly Keyword EXPORT = new ModifierKeyword("export", Modifier.EXPORTED);
	public static readonly Keyword HAS = new("has", KeywordType.FLOW);
	public static readonly Keyword IF = new("if", KeywordType.FLOW);
	public static readonly Keyword IN = new("in");
	public static readonly Keyword INLINE = new ModifierKeyword("inline", Modifier.INLINE);
	public static readonly Keyword IS = new("is", KeywordType.FLOW);
	public static readonly Keyword IS_NOT = new("is not", KeywordType.FLOW);
	public static readonly Keyword INIT = new("init");
	public static readonly Keyword IMPORT = new ModifierKeyword("import", Modifier.IMPORTED);
	public static readonly Keyword LOOP = new("loop");
	public static readonly Keyword NAMESPACE = new("namespace", KeywordType.NORMAL);
	public static readonly Keyword NOT = new("not", KeywordType.FLOW);
	public static readonly Keyword OUTLINE = new ModifierKeyword("outline", Modifier.OUTLINE);
	public static readonly Keyword OVERRIDE = new("override");
	public static readonly Keyword PLAIN = new ModifierKeyword("plain", Modifier.PLAIN);
	public static readonly Keyword PRIVATE = new ModifierKeyword("private", Modifier.PRIVATE);
	public static readonly Keyword PROTECTED = new ModifierKeyword("protected", Modifier.PROTECTED);
	public static readonly Keyword PUBLIC = new ModifierKeyword("public", Modifier.PUBLIC);
	public static readonly Keyword READONLY = new ModifierKeyword("readonly", Modifier.READONLY);
	public static readonly Keyword RETURN = new("return", KeywordType.FLOW);
	public static readonly Keyword STATIC = new ModifierKeyword("static", Modifier.STATIC);
	public static readonly Keyword STOP = new("stop", KeywordType.FLOW);
	public static readonly Keyword VIRTUAL = new("virtual", KeywordType.NORMAL);
	public static readonly Keyword WHEN = new("when", KeywordType.FLOW);

	public static Dictionary<string, Keyword> Values { get; } = new Dictionary<string, Keyword>();

	static Keywords()
	{
		Values.Add(AS.Identifier, AS);
		Values.Add(COMPILES.Identifier, COMPILES);
		Values.Add(CONSTANT.Identifier, CONSTANT);
		Values.Add(CONTINUE.Identifier, CONTINUE);
		Values.Add(ELSE.Identifier, ELSE);
		Values.Add(EXPORT.Identifier, EXPORT);
		Values.Add(HAS.Identifier, HAS);
		Values.Add(IF.Identifier, IF);
		Values.Add(IN.Identifier, IN);
		Values.Add(INLINE.Identifier, INLINE);
		Values.Add(IS.Identifier, IS);
		Values.Add(IMPORT.Identifier, IMPORT);
		Values.Add(LOOP.Identifier, LOOP);
		Values.Add(NAMESPACE.Identifier, NAMESPACE);
		Values.Add(NOT.Identifier, NOT);
		Values.Add(OUTLINE.Identifier, OUTLINE);
		Values.Add(OVERRIDE.Identifier, OVERRIDE);
		Values.Add(PLAIN.Identifier, PLAIN);
		Values.Add(PRIVATE.Identifier, PRIVATE);
		Values.Add(PROTECTED.Identifier, PROTECTED);
		Values.Add(PUBLIC.Identifier, PUBLIC);
		Values.Add(READONLY.Identifier, READONLY);
		Values.Add(RETURN.Identifier, RETURN);
		Values.Add(STATIC.Identifier, STATIC);
		Values.Add(STOP.Identifier, STOP);
		Values.Add(VIRTUAL.Identifier, VIRTUAL);
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
