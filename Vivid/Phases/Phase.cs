using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Represents one phase in compilation
/// </summary>
public abstract class Phase
{
	private List<Task<Status>> Tasks { get; } = new List<Task<Status>>();
	public bool Failed => Tasks.Any(i => !i.IsCompleted || i.Result.IsProblematic);

	/// <summary>
	/// Returns the fullname without the postfix 'Phase'
	/// </summary>
	public string GetName()
	{
		var name = GetType().Name;
		var postfix = "Phase";
		
		return name.EndsWith(postfix) ? name[0..(name.Length - postfix.Length)] : name;
	}

	/// <summary>
	/// Executes the phase with the given data 
	/// </summary>
	/// <returns>Status returned from the phase</returns>
	public abstract Status Execute();

	/// <summary>
	/// Executes runnable on another thread if multithreading is enabled, otherwise executes locally
	/// </summary>
	public void Run(Func<Status> task)
	{
		Status status;

		try
		{
			status = task();
		}
		catch (Exception e)
		{
			status = new Status(e.Message);
		}

		Tasks.Add(Task.FromResult(status));
	}

	/// <summary>
	/// Waits for all tasks to finish
	/// </summary>
	public void Sync()
	{
		var i = 0;

		while (i < Tasks.Count)
		{
			Tasks[i++]?.Wait();
		}

		Tasks.RemoveAll(i => !i.Result.IsProblematic);
	}

	/// <summary>
	/// Returns all tasks errors that occurred during execution
	/// </summary>
	public string GetTaskErrors()
	{
		var builder = new StringBuilder("\n");

		foreach (var task in Tasks)
		{
			if (task.Result.IsProblematic)
			{
				builder.Append(task.Result.Message).Append('\n');
			}
		}

		return builder.ToString();
	}
}