using System.Collections.Generic;

public class Size
{
	private static readonly Dictionary<int, Size> Map = new Dictionary<int, Size>();

	static Size()
	{
		Map.Add(BYTE.Bytes, BYTE);
		Map.Add(WORD.Bytes, WORD);
		Map.Add(DWORD.Bytes, DWORD);
		Map.Add(QWORD.Bytes, QWORD);
	}

	public static readonly Size BYTE = new Size("byte", "db", 1);
	public static readonly Size WORD = new Size("word", "dw", 2);
	public static readonly Size DWORD = new Size("dword", "dd", 4);
	public static readonly Size QWORD = new Size("qword", "dq", 8);

	public static Size Get(int bytes)
	{
		return Map[bytes];
	}

	public string Identifier { get; private set; }
	public string Data { get; private set; }
	public int Bytes { get; private set; }

	public Size(string identifier, string data, int bytes)
	{
		Identifier = identifier;
		Data = data;
		Bytes = bytes;
	}

	public override string ToString()
	{
		return Identifier;
	}
}