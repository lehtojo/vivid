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

	public static Dictionary<string, Keyword> All { get; } = new Dictionary<string, Keyword>();

	public static void Initialize()
	{
		All.Add(AS.Identifier, AS);
		All.Add(COMPILES.Identifier, COMPILES);
		All.Add(CONSTANT.Identifier, CONSTANT);
		All.Add(CONTINUE.Identifier, CONTINUE);
		All.Add(ELSE.Identifier, ELSE);
		All.Add(EXPORT.Identifier, EXPORT);
		All.Add(GLOBAL.Identifier, GLOBAL);
		All.Add(HAS.Identifier, HAS);
		All.Add(HAS_NOT.Identifier, HAS_NOT);
		All.Add(IF.Identifier, IF);
		All.Add(IN.Identifier, IN);
		All.Add(INLINE.Identifier, INLINE);
		All.Add(IS.Identifier, IS);
		All.Add(IMPORT.Identifier, IMPORT);
		All.Add(LOOP.Identifier, LOOP);
		All.Add(NAMESPACE.Identifier, NAMESPACE);
		All.Add(NOT.Identifier, NOT);
		All.Add(OUTLINE.Identifier, OUTLINE);
		All.Add(OVERRIDE.Identifier, OVERRIDE);
		All.Add(PACK.Identifier, PACK);
		All.Add(PLAIN.Identifier, PLAIN);
		All.Add(PRIVATE.Identifier, PRIVATE);
		All.Add(PROTECTED.Identifier, PROTECTED);
		All.Add(PUBLIC.Identifier, PUBLIC);
		All.Add(READABLE.Identifier, READABLE);
		All.Add(RETURN.Identifier, RETURN);
		All.Add(SHARED.Identifier, SHARED);
		All.Add(STOP.Identifier, STOP);
		// Do not reserve using-keyword
		All.Add(VIRTUAL.Identifier, VIRTUAL);
		All.Add(WHEN.Identifier, WHEN);
	}

	public static bool Exists(string name)
	{
		return All.ContainsKey(name);
	}

	public static Keyword Get(string text)
	{
		return All[text];
	}
}
