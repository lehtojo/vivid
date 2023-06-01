using System;
using System.Linq;

public class LexerPhase : Phase
{
	public override Status Execute()
	{
		var files = Settings.SourceFiles;
		if (!files.Any()) return new Status("Nothing to tokenize");

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
				return new Status(e.Position, e.Description);
			}
			catch (Exception e)
			{
				return new Status(e.Message);
			}
		}

		return Status.OK;
	}
}