using System;
using System.Linq;
using System.Collections.Generic;

public static class Dice
{
	private static Dictionary<string, double> Roll(int count)
	{
		var outcomes = new Dictionary<string, double>();

		var basicProba = 1.0 / Math.Pow(6, count);

		var faces = new byte[count];
		for (int i = 0; i < faces.Length; i++)
			faces[i] = 1;

		while (true)
		{
			// Accumulate outcomes

			var clone = (byte[])faces.Clone();
			Array.Sort(clone);
			var hash = string.Concat(clone);

			double previous = 0;
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

	public static Dictionary<string, double> Reroll(string keep)
	{
		var outcomes = new Dictionary<string, double>();

		if (keep.Length == 5)
		{
			outcomes[keep] = 1;
			return outcomes;
		}

		foreach (var roll in Roll(5 - keep.Length))
		{
			var outcome = SortString(roll.Key + keep);

			double previous = 0;
			outcomes.TryGetValue(outcome, out previous);
			outcomes[outcome] = previous + roll.Value;
		}

		return outcomes;
	}

	public static List<string> Keeps(string roll)
	{
		var result = new HashSet<string>();

		for (var i = 0; i < Math.Pow(2, roll.Length); i++)
		{
			var keep = "";

			for (var j = 0; j < roll.Length; j++)
			{
				if ((i & (1 << j)) != 0)
					keep += roll[j];
			}

			result.Add(keep);
		}

		return new List<string>(result);
	}

	public static byte[] Faces(string roll)
	{
		return roll.ToArray().Select(c => byte.Parse(c.ToString())).ToArray();
	}
}