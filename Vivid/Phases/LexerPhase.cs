using System;
using System.Linq;
using System.Collections.Generic;

public class LexerPhase : Phase
{
	public const string OUTPUT = "files";

	public override Status Execute(Bundle bundle)
	{
		var files = bundle.Get(FilePhase.OUTPUT, new List<SourceFile>());

		if (!files.Any())
		{
			return Status.Error("Nothing to tokenize");
		}

		for (var i = 0; i < files.Count; i++)
		{
			var index = i;

			Run(() =>
			{
				var file = files[index];

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

				return Status.OK;
			});
		}

		return Status.OK;
	}
}