using System;
using System.Linq;
using System.Collections.Generic;

public class LexerPhase : Phase
{
	public override Status Execute()
	{
		var files = Settings.SourceFiles;
		if (!files.Any()) return Status.Error("Nothing to tokenize");

		foreach (var file in files)
		{
			try
			{
				file.Tokens.AddRange(Lexer.GetTokens(file.Content));
				Lexer.RegisterFile(file.Tokens, file);
			}
			catch (LexerException e)
			{
				e.Position.File = file;
				return Status.Error(e.Position, e.Description);
			}
			catch (Exception e)
			{
				return Status.Error(e.Message);
			}
		}

		return Status.OK;
	}
}