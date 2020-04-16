public interface IResolvable
{
	/// <summary>
	/// Tries to resolve the internal problems
	/// </summary>
	/// <returns>Returns a resolved node tree on success or null when no changes are needed</returns>
	Node? Resolve(Context context);
	Status GetStatus();
}