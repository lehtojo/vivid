using System;

public class Chain
{
	private System.Type[] Phases { get; set; }

	public Chain(params System.Type[] phases)
	{
		Phases = phases;
	}

	public void Execute(Bundle bundle)
	{
		foreach (var template in Phases)
		{
			bool multithreaded = bundle.Get("multithreaded", true);

			try
			{
				var phase = Activator.CreateInstance(template) as Phase;
				phase.Multithread = multithreaded;

				var status = phase.Execute(bundle);

				phase.Sync();

				if (status.IsProblematic)
				{
					Console.Error.WriteLine($"Terminated: {status.Description}");
					break;
				}
				else if (phase.Failed)
				{
					break;
				}
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("Internal error: " + e.Message);
				return;
			}
		}
	}
}