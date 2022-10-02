public class EnterScopeInstruction : Instruction
{
	public string Id { get; set; }

	public EnterScopeInstruction(Unit unit, string id) : base(unit, InstructionType.ENTER_SCOPE)
	{
		this.Id = id;
	}

	public override void OnBuild()
	{
		if (!Unit.States.ContainsKey(Id)) return;

		// Load the state
		var state = Unit.States[Id];

		foreach (var descriptor in state)
		{
			Scope!.SetOrCreateInput(descriptor.Variable, descriptor.Handle, descriptor.Handle.Format);
		}
	}
}