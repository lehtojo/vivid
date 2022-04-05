using System.Collections.Generic;
using System.Linq;

public struct FunctionSignatureProviderLoadInformation
{
	public string? Filter { get; set; }
	public Function? CursorFunction { get; set; }
}

public class FunctionParameter
{
	public string Name { get; set; }
	public string Documentation { get; set; }

	public FunctionParameter(string name, string documentation)
	{
		Name = name;
		Documentation = documentation;
	}
}

public class FunctionSignature
{
	public string Identifier { get; set; }
	public string Documentation { get; set; }
	public FunctionParameter[] Parameters { get; set; }

	public FunctionSignature(string identifier, string documentation, FunctionParameter[] parameters)
	{
		Identifier = identifier;
		Documentation = documentation;
		Parameters = parameters;
	}
}

public static class FunctionSignatureProvider
{
	/// <summary>
	/// Finds the function token, which contains the cursor specified in the request.
	/// If the function fails to find this token, it will return null.
	/// Otherwise, the function name will be returned.
	/// </summary>
	public static string? RegisterFunctionToken(DocumentRequest request, List<Token> tokens)
	{
		var type = request.Type;
		var uri = request.Uri;

		// The request is to send the current function signature, so try to find the parenthesis where the cursor is inside
		var parenthesis = CursorInformationProvider.FindCursorParenthesis(tokens, request.Absolute);

		// If the cursor is not inside parenthesis, nothing can be done
		if (parenthesis == null) return null;

		Lexer.Join(tokens);

		// Require the previous token to be an identifier token
		if (parenthesis.Index - 1 < 0 || !parenthesis.Container[parenthesis.Index - 1].Is(TokenType.IDENTIFIER)) return null;

		var identifier = parenthesis.Container[parenthesis.Index - 1];

		// Register the function name as the cursor position
		identifier.Position.IsCursor = true;

		// Now the filter must be the value of the identifier token
		return identifier.To<IdentifierToken>().Value;
	}

	/// <summary>
	/// Loads the specified files while taking into account the cursor specified in the request.
	/// Returns the function, which contains the cursor specified in the request.
	/// </summary>
	public static FunctionSignatureProviderLoadInformation UpdateAndMark(Project project, DocumentRequest request)
	{
		var document = request.Document;
		var file = project.GetSourceFile(request.Uri);
		var parse = project.GetParse(file);

		var changed = project.Update(request.Uri, document);

		var filter = RegisterFunctionToken(request, changed);
		if (filter == null) return new FunctionSignatureProviderLoadInformation();

		// Find the function that contains the cursor
		var cursor_function = CursorInformationProvider.FindCursorFunction(parse);

		return new FunctionSignatureProviderLoadInformation()
		{
			Filter = filter,
			CursorFunction = cursor_function,
		};
	}

	/// <summary>
	/// Creates function signatures from the specified function overloads.
	/// </summary>
	private static FunctionSignature[] GetFunctionSignatures(IEnumerable<Function> overloads)
	{
		return overloads.Select(i => new FunctionSignature(i.ToString(), string.Empty, i.Parameters.Select(i => new FunctionParameter(i.Name, string.Empty)).ToArray())).ToArray();
	}

	/// <summary>
	/// Provides function signature information for the specified request.
	/// </summary>
	public static void Provide(Project project, IServiceResponse response, DocumentRequest request)
	{
		var filename = ServiceUtility.ToPath(request.Uri);
		var information = UpdateAndMark(project, request);
		var filter = information.Filter;
		var cursor_function = information.CursorFunction;

		// If no function contains the cursor, send an error
		if (filter == null || cursor_function == null)
		{
			response.SendStatusCode(request.Uri, DocumentResponseStatus.ERROR);
			return;
		}

		// Finally, build the document
		var parse = ProjectBuilder.Build(project.Documents, filename, false, cursor_function);

		// Find the cursor
		var cursor = CursorInformationProvider.FindCursorNode(cursor_function);

		// If the cursor is not found, send an error
		if (cursor == null)
		{
			response.SendStatusCode(request.Uri, DocumentResponseStatus.ERROR);
			return;
		}

		FunctionList? overloads;

		if (cursor.Parent != null && cursor.Parent.Is(NodeType.LINK))
		{
			var primary = cursor.Parent.Left.TryGetType();

			if (primary != null)
			{
				// Try to get function overloads with the name equal to the filter
				overloads = primary.GetFunction(filter);

				if (overloads != null)
				{
					var signatures = GetFunctionSignatures(overloads.Overloads);

					response.SendResponse(request.Uri, DocumentResponseStatus.OK, signatures);
					return;
				}

				// If no function could be found using the filter, return the global function candidates
			}
		}

		var environment = cursor.GetParentContext();

		// Try to get function overloads with the name equal to the filter using the environment context
		overloads = environment.GetFunction(filter);

		if (overloads != null)
		{
			var signatures = GetFunctionSignatures(overloads.Overloads);
			response.SendResponse(request.Uri, DocumentResponseStatus.OK, signatures);
			return;
		}

		response.SendStatusCode(request.Uri, DocumentResponseStatus.ERROR);
	}
}