using System.Collections.Generic;

public class CreatePackInstruction : Instruction
{
	public List<Result> Values { get; }
	public new Type Type { get; }
	public DisposablePackHandle Value { get; }

	public CreatePackInstruction(Unit unit, Type type, List<Result> values) : base(unit, InstructionType.CREATE_PACK)
	{
		Values = values;
		Type = type;

		var dependencies = new List<Result>();
		dependencies.Add(Result);
		dependencies.AddRange(values);

		Dependencies = dependencies;
		Value = new DisposablePackHandle(unit, type);

		OnBuild();
	}

	private int RegisterMemberValues(DisposablePackHandle pack, Type type, int position)
	{
		foreach (var iterator in type.Variables)
		{
			var member = iterator.Value;

			if (member.Type!.IsPack)
			{
				position = RegisterMemberValues(pack.Members[member].Value.To<DisposablePackHandle>(), member.Type!, position);
				continue;
			}

			pack.Members[member] = Values[position];
			position++;
		}

		return position;
	}

	public override void OnBuild()
	{
		RegisterMemberValues(Value, Type, 0);

		Result.Value = Value;
		Result.Format = Settings.Format;
	}
}