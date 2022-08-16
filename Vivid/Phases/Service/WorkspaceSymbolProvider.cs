using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System;

public enum SymbolKind
{
	File = 0,
	Module = 1,
	Namespace = 2,
	Package = 3,
	Class = 4,
	Method = 5,
	Property = 6,
	Field = 7,
	Constructor = 8,
	Enum = 9,
	Interface = 10,
	Function = 11,
	Variable = 12,
	Constant = 13,
	String = 14,
	Number = 15,
	Boolean = 16,
	Array = 17,
	Object = 18,
	Key = 19,
	Null = 20,
	EnumMember = 21,
	Struct = 22,
	Event = 23,
	Operator = 24,
	TypeParameter = 25
}

public class WorkspaceSymbolInformation
{
	public string Name { get; set; }
	public SymbolKind Kind { get; set; }
	public string Container { get; set; }
	public DocumentPosition Position { get; set; }

	public WorkspaceSymbolInformation(string name, SymbolKind kind, string container, DocumentPosition position)
	{
		Name = name;
		Kind = kind;
		Container = container;
		Position = position;
	}

	public override bool Equals(object? other)
	{
		return other is WorkspaceSymbolInformation symbol && Name == symbol.Name && Kind == symbol.Kind && Container == symbol.Container && Position.Equals(symbol.Position);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Name, Kind, Container, Position);
	}
}

public static class WorkspaceSymbolProvider
{
	/// <summary>
	/// Returns the appropriate symbol kind for the specified variable
	/// </summary>
	private static SymbolKind GetVariableSymbolKind(Variable variable)
	{
		if (variable.IsConstant) return SymbolKind.Constant;
		if (variable.IsMember) return SymbolKind.Property;
		return SymbolKind.Variable;
	}

	public static void Provide(Project project, IServiceResponse response, DocumentRequest request)
	{
		var result = new List<FileDivider>();

		foreach (var iterator in project.Documents)
		{
			var file = iterator.Key;
			var parse = iterator.Value;
			if (parse.Context == null) continue;

			var symbols = new List<WorkspaceSymbolInformation>();

			var functions = parse.Blueprints.Keys.ToList();
			var types = Common.GetAllTypes(parse.Context);
			var variables = Common.GetAllVariablesOutsideFunctions(parse.Context);

			symbols.AddRange(functions.Where(i => i.Start != null).Select(i => new WorkspaceSymbolInformation(i.ToString(), SymbolKind.Function, string.Empty, new DocumentPosition(i.Start!.FriendlyLine, i.Start!.FriendlyCharacter))));
			symbols.AddRange(types.Where(i => i.Position != null).Select(i => new WorkspaceSymbolInformation(i.ToString(), i.IsStatic ? SymbolKind.Namespace : SymbolKind.Class, string.Empty, new DocumentPosition(i.Position!.FriendlyLine, i.Position!.FriendlyCharacter))));
			symbols.AddRange(variables.Where(i => i.Position != null).Select(i => new WorkspaceSymbolInformation(i.ToString(), GetVariableSymbolKind(i), string.Empty, new DocumentPosition(i.Position!.FriendlyLine, i.Position!.FriendlyCharacter))));

			symbols = symbols.Where(i => i.Name.Contains(request.Query ?? string.Empty)).ToList();

			var divider = new FileDivider(ServiceUtility.ToUri(file.Fullname), JsonSerializer.Serialize(symbols));
			result.Add(divider);
		}

		response.SendResponse(string.Empty, DocumentResponseStatus.OK, result.Distinct().ToArray());
	}
}