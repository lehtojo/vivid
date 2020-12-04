using System;
using System.Collections.Generic;

public class LexerPhase : Phase
{
	public override Status Execute(Bundle bundle)
	{
		var contents = bundle.Get("input_file_contents", Array.Empty<string>());

		if (contents.Length == 0)
		{
			return Status.Error("Nothing to tokenize");
		}

		var tokens = new List<Token>[contents.Length];

		for (var i = 0; i < contents.Length; i++)
		{
			var index = i;

			Run(() =>
			{
				var content = contents[index];

				try
				{
					tokens[index] = Lexer.GetTokens(content);
				}
				catch (Exception e)
				{
					return Status.Error(e.Message);
				}

				return Status.OK;
			});
		}

		bundle.Put("input_file_tokens", tokens);

		return Status.OK;
	}
}