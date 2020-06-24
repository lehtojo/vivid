using System;

public class Chain
{
    private System.Type[] Phases { get; set; }

    /// <summary>
    /// Creates a chain of phases
    /// </summary>
    /// <param name="phases">Phases to execute in order when invoked</param>
    public Chain(params System.Type[] phases)
    {
        Phases = phases;
    }

    /// <summary>
    /// Executes the configured phases with the given bundle
    /// </summary>
    /// <param name="bundle">Bundle to pass to the phases</param>
    public bool Execute(Bundle bundle)
    {
        foreach (var template in Phases)
        {
            var multithreaded = bundle.Get("multithreaded", true);

            try
            {
                if (!(Activator.CreateInstance(template) is Phase phase))
                {
                    throw new ApplicationException("Couldn't create the next phase");
                }

                phase.Multithread = multithreaded;

                var status = phase.Execute(bundle);

                if (status.IsProblematic)
                {
                    Console.Error.WriteLine($"Terminated: {status.Description}");
                    return false;
                }

                phase.Sync();

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
            catch (Exception e)
            {
                Console.Error.WriteLine("Internal error: " + e.Message);
                return false;
            }
        }


		return true;
	}
}