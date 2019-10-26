using System;
using System.Collections.Generic;
using System.Linq;

public class FunctionList
{
	public List<Function> Instances { get; private set; } = new List<Function>();

	public void Add(Function function)
	{
		Instances.Add(function);

		if (Instances.Count > 1)
		{
			Instances.First().SetIndex(0);
			function.SetIndex(Instances.Count - 1);
		}
	}

	public void Update()
	{
		foreach (Function function in Instances)
		{
			function.Update();
		}
	}

	private struct Candidate : IComparable
	{
		public Function Function { get; set; }
		public int Casts { get; set; }

		public int CompareTo(object other)
		{
			return Casts.CompareTo(((Candidate)other).Casts);
		}
	}

	/// <summary>
	/// Tries to find the matching function by comparing parameters.
	/// When there are two or more callable functions with the given parameter list, the function with least casts is chosen
	/// </summary>
	/// <param name="parameters">Parameter list used to filter</param>
	/// <returns>Success: Function with same or least castable parameters, Failure: null</returns>
	public Function Get(List<Type> parameters)
	{
		List<Candidate> candidates = new List<Candidate>();

		foreach (Function function in Instances)
		{
			List<Type> types = function.ParameterTypes;

			// Verify that the current function has equal amount of parameters with the given parameter list
			if (parameters.Count != types.Count)
			{
				continue;
			}

			int casts = 0;

			// Verify each given parameter is atleast castable to the required parameter type of the current function
			for (int i = 0; i < parameters.Count; i++)
			{
				if (Resolver.GetSharedType(parameters[i], types[i]) != null)
				{
					if (parameters[i] != types[i])
					{
						casts++;
					}
				}
				else
				{
					goto End;
				}
			}


			Candidate candidate = new Candidate
			{
				Function = function,
				Casts = casts
			};

			candidates.Add(candidate);

		End:;
		}

		if (candidates.Count == 0)
		{
			return null;
		}

		// Return the candidate which has the minium amount of casts in terms of parameters
		return candidates.Min().Function;
	}
}