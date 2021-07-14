using System;

public class Chain
{
	private System.Type[] Phases { get; set; }

	/// <summary>
	/// Creates a chain of phases
	/// </summary>
	public Chain(params System.Type[] phases)
	{
		Phases = phases;
	}

	/// <summary>
	/// Executes the configured phases with the given bundle
	/// </summary>
	public bool Execute(Bundle bundle)
	{
		foreach (var template in Phases)
		{
			var multithreaded = bundle.Get("multithreaded", true);

			try
			{
				if (Activator.CreateInstance(template) is not Phase phase)
				{
					throw new ApplicationException("Could not create the next phase");
				}

				phase.Multithread = multithreaded;

				// Record the start of execution of the next phase
				var start = DateTime.Now;

				var status = phase.Execute(bundle);

				if (status.IsProblematic)
				{
					Console.Error.WriteLine($"Terminated: {status.Description}");
					return false;
				}

				phase.Sync();

				// Record the end of the execution
				var end = DateTime.Now;

				if (bundle.Get(ConfigurationPhase.OUTPUT_TIME, false))
				{
					Console.WriteLine($"{phase.GetName()}: {(end - start).TotalMilliseconds} ms");
				}

				if (status.IsProblematic)
				{
					Console.Error.WriteLine($"Terminated: {status.Description}");
					return false;
				}
				else if (phase.Failed)
				{
					Console.Error.WriteLine($"Terminated: {phase.GetTaskErrors()}");
					return false;
				}
			}
			catch (SourceException e)
			{
				Console.Error.WriteLine(e.Message);
				return false;
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("Internal error: " + e.Message);
				return false;
			}
		}

		return true;
	}
}