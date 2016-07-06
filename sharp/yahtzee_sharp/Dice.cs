using System;
using System.Linq;
using System.Collections.Generic;

public static class Dice
{
	public const int NumRerollPatterns = 32; 

	private static Dictionary<string, float> Roll(int count)
	{
		var outcomes = new Dictionary<string, float>();

		var basicProba = (float)(1.0 / Math.Pow(6, count));

		var faces = new byte[count];
		for (int i = 0; i < faces.Length; i++)
			faces[i] = 1;

		while (true)
		{
			// Accumulate outcomes

			var clone = (byte[])faces.Clone();
			Array.Sort(clone);
			var hash = string.Concat(clone);

			float previous = 0;
			outcomes.TryGetValue(hash, out previous);
			outcomes[hash] = previous + basicProba;

			// Iterate

			faces[0]++;
			for (var i = 0; i < faces.Length; i++)
			{
				if (faces[i] > 6)
				{
					if (i < faces.Length - 1)
					{
						faces[i] = 1;
						faces[i + 1]++;
					}
					else
					{
						return outcomes;
					}
				}
			}
		}
	}

	private static string SortString(string s)
	{
		var arr = s.ToArray();
		Array.Sort(arr);
		return new string(arr);
	}

	private static Dictionary<string, Dictionary<string, float>> rerollMemo = new Dictionary<string, Dictionary<string, float>>();

	private static Dictionary<string, float> Reroll(string keep)
	{
		if (rerollMemo.ContainsKey(keep))
			return rerollMemo[keep];

		var outcomes = new Dictionary<string, float>();

		if (keep.Length == 5)
		{
			outcomes[keep] = 1;
			return outcomes;
		}

		foreach (var roll in Roll(5 - keep.Length))
		{
			var outcome = SortString(roll.Key + keep);

			float previous = 0;
			outcomes.TryGetValue(outcome, out previous);
			outcomes[outcome] = previous + roll.Value;
		}

		rerollMemo[keep] = outcomes; 
		return outcomes;
	}

	private static string Keep(string roll, byte keepPattern)
	{
		var keep = "";

		for (var j = 0; j < roll.Length; j++)
		{
			if ((keepPattern & (1 << j)) != 0)
				keep += roll[j];
		}

		return keep;
	}

	private static List<string> Keeps(string roll)
	{
		var result = new HashSet<string>();

		for (byte keepPattern = 0; keepPattern < Math.Pow(2, roll.Length); keepPattern++)
		{
			var keep = Keep(roll, keepPattern);
		}

		return new List<string>(result);
	}

	public static byte[] Faces(string roll)
	{
		return roll.ToArray().Select(c => byte.Parse(c.ToString())).ToArray();
	}

	public static Dictionary<string, float>[] Rerolls(string roll)
	{
		var result = new Dictionary<string, float>[NumRerollPatterns];

		for (byte keepPattern = 0; keepPattern < NumRerollPatterns; keepPattern++)
		{
			var keep = Keep(roll, keepPattern);
			result[keepPattern] = Reroll(keep);
		}

		return result;
	}

	public static Dictionary<string, float> FirstRoll()
	{
		return Reroll("");
	}
}