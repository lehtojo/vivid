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
	public static readonly Keyword GLOBAL = new("global", KeywordType.NORMAL);
	public static readonly Keyword HAS = new("has", KeywordType.NORMAL);
	public static readonly Keyword HAS_NOT = new("has not", KeywordType.NORMAL);
	public static readonly Keyword IF = new("if", KeywordType.FLOW);
	public static readonly Keyword IN = new("in");
	public static readonly Keyword INLINE = new ModifierKeyword("inline", Modifier.INLINE);
	public static readonly Keyword IS = new("is", KeywordType.NORMAL);
	public static readonly Keyword IS_NOT = new("is not", KeywordType.NORMAL);
	public static readonly Keyword INIT = new("init");
	public static readonly Keyword IMPORT = new ModifierKeyword("import", Modifier.IMPORTED);
	public static readonly Keyword LOOP = new("loop");
	public static readonly Keyword NAMESPACE = new("namespace", KeywordType.NORMAL);
	public static readonly Keyword NOT = new("not", KeywordType.NORMAL);
	public static readonly Keyword OUTLINE = new ModifierKeyword("outline", Modifier.OUTLINE);
	public static readonly Keyword OVERRIDE = new("override");
	public static readonly Keyword PACK = new ModifierKeyword("pack", Modifier.PACK);
	public static readonly Keyword PLAIN = new ModifierKeyword("plain", Modifier.PLAIN);
	public static readonly Keyword PRIVATE = new ModifierKeyword("private", Modifier.PRIVATE);
	public static readonly Keyword PROTECTED = new ModifierKeyword("protected", Modifier.PROTECTED);
	public static readonly Keyword PUBLIC = new ModifierKeyword("public", Modifier.PUBLIC);
	public static readonly Keyword READABLE = new ModifierKeyword("readable", Modifier.READABLE);
	public static readonly Keyword RETURN = new("return", KeywordType.FLOW);
	public static readonly Keyword SHARED = new ModifierKeyword("shared", Modifier.STATIC);
	public static readonly Keyword STOP = new("stop", KeywordType.FLOW);
	public static readonly Keyword USING = new("using", KeywordType.NORMAL);
	public static readonly Keyword VIRTUAL = new("open", KeywordType.NORMAL);
	public static readonly Keyword WHEN = new("when", KeywordType.FLOW);

	public static Dictionary<string, Keyword> Definitions { get; } = new Dictionary<string, Keyword>();

	public static void Initialize()
	{
		Definitions.Add(AS.Identifier, AS);
		Definitions.Add(COMPILES.Identifier, COMPILES);
		Definitions.Add(CONSTANT.Identifier, CONSTANT);
		Definitions.Add(CONTINUE.Identifier, CONTINUE);
		Definitions.Add(ELSE.Identifier, ELSE);
		Definitions.Add(EXPORT.Identifier, EXPORT);
		Definitions.Add(GLOBAL.Identifier, GLOBAL);
		Definitions.Add(HAS.Identifier, HAS);
		Definitions.Add(HAS_NOT.Identifier, HAS_NOT);
		Definitions.Add(IF.Identifier, IF);
		Definitions.Add(IN.Identifier, IN);
		Definitions.Add(INLINE.Identifier, INLINE);
		Definitions.Add(IS.Identifier, IS);
		Definitions.Add(IMPORT.Identifier, IMPORT);
		Definitions.Add(LOOP.Identifier, LOOP);
		Definitions.Add(NAMESPACE.Identifier, NAMESPACE);
		Definitions.Add(NOT.Identifier, NOT);
		Definitions.Add(OUTLINE.Identifier, OUTLINE);
		Definitions.Add(OVERRIDE.Identifier, OVERRIDE);
		Definitions.Add(PACK.Identifier, PACK);
		Definitions.Add(PLAIN.Identifier, PLAIN);
		Definitions.Add(PRIVATE.Identifier, PRIVATE);
		Definitions.Add(PROTECTED.Identifier, PROTECTED);
		Definitions.Add(PUBLIC.Identifier, PUBLIC);
		Definitions.Add(READABLE.Identifier, READABLE);
		Definitions.Add(RETURN.Identifier, RETURN);
		Definitions.Add(SHARED.Identifier, SHARED);
		Definitions.Add(STOP.Identifier, STOP);
		// Do not reserve using-keyword
		Definitions.Add(VIRTUAL.Identifier, VIRTUAL);
		Definitions.Add(WHEN.Identifier, WHEN);
	}

	public static bool Exists(string name)
	{
		return Definitions.ContainsKey(name);
	}

	public static Keyword Get(string text)
	{
		return Definitions[text];
	}
}
