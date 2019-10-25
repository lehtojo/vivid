using System.Collections.Generic;

/// <summary>
/// Efficient way of storing values with names
/// </summary>
public class Bundle : Dictionary<string, object>
{
	public void Put(string name, object element)
	{
		Add(name, element);
	}

	public void PutString(string name, string text)
	{
		Add(name, text);
	}

	public void PutFloat(string name, float number)
	{
		Add(name, number);
	}

	public void PutInt(string name, int number)
	{
		Add(name, number);
	}

	public void PutBool(string name, bool b)
	{
		Add(name, b);
	}

	public T Get<T>(string name, T fallback)
	{
		if (TryGetValue(name, out object value))
		{
			return (T)value;
		}

		return fallback;
	}

	public bool Contains(string name)
	{
		return ContainsKey(name);
	}
}