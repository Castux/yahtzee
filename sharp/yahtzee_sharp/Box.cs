using System;
using System.Linq;
using System.Collections.Generic;

public enum Box
{
	Ones = 1 << 0,
	Twos = 1 << 1,
	Threes = 1 << 2,
	Fours = 1 << 3,
	Fives = 1 << 4,
	Sixes = 1 << 5,
	ThreeOfKind = 1 << 6,
	FourOfKind = 1 << 7,
	FullHouse = 1 << 8,
	SmallStraight = 1 << 9,
	LargeStraight = 1 << 10,
	Yahtzee = 1 << 11,
	Chance = 1 << 12
}

public struct BoxSet
{
	public const int NumBoxes = 13;
	public const int NumBoxSets = (1 << NumBoxes);

	public int bits { private set; get; }

	public static BoxSet Empty = new BoxSet { bits = 0 };

	public static IEnumerable<BoxSet> AllSets
	{
		get
		{
			for (int i = 0; i < NumBoxSets; i++)
				yield return new BoxSet { bits = i };
		}
	}

	public void Add(Box box)
	{
		bits |= (int)box;
	}

	public void Remove(Box box)
	{
		bits &= ~(int)box;
	}

	public bool Contains(Box box)
	{
		return (bits & (int)box) != 0;
	}

	public IEnumerable<Box> Contents
	{
		get
		{
			for (int i = 0; i < NumBoxes; i++)
			{
				var box = (Box)(1 << i);
				if (Contains(box))
					yield return box;
			}
		}
	}

	public bool IsEmpty
	{
		get { return bits == 0; }
	}
}

public static class BoxUtils
{
	public static bool IsUpper(this Box box)
	{
		return box >= Box.ThreeOfKind;
	}

	private static int ScoreUpper(byte[] faces, int face)
	{
		return faces.Count(f => f == face) * face;
	}

	private static bool NOfKind(byte[] faces, int n, bool exact = false)
	{
		for (int i = 1; i <= 6; i++)
		{
			var count = faces.Count(f => f == i);

			if ((exact && count == n) || (!exact && count >= n))
				return true;
		}

		return false;
	}

	private static int LongestSequence(byte[] faces)
	{
		var hash = new HashSet<byte>(faces);
		int longest = 1;

		for (byte face = 1; face <= 6; face++)
		{
			var length = 0;
			for (var i = face; i <= 6; i++)
			{
				if (hash.Contains(i))
					length++;
				else
					break;
			}

			if (length > longest)
				longest = length;
		}

		return longest;
	}

	private static int Score(string roll, Box box)
	{
		var faces = Dice.Faces(roll);
		var sum = faces.Select(x => (int)x).Sum();
		var longestSequence = LongestSequence(faces);

		switch(box)
		{
			case Box.Ones:
				return ScoreUpper(faces, 1);
			case Box.Twos:
				return ScoreUpper(faces, 2);
			case Box.Threes:
				return ScoreUpper(faces, 3);
			case Box.Fours:
				return ScoreUpper(faces, 4);
			case Box.Fives:
				return ScoreUpper(faces, 5);
			case Box.Sixes:
				return ScoreUpper(faces, 6);
			case Box.ThreeOfKind:
				return NOfKind(faces, 3) ? sum : 0;
			case Box.FourOfKind:
				return NOfKind(faces, 4) ? sum : 0;
			case Box.FullHouse:
				return (NOfKind(faces, 3, true) && NOfKind(faces, 2, true)) ? 25 : 0;
			case Box.SmallStraight:
				return longestSequence >= 4 ? 30 : 0;
			case Box.LargeStraight:
				return longestSequence >= 5 ? 40 : 0;
			case Box.Yahtzee:
				return NOfKind(faces, 5) ? 50 : 0;
			case Box.Chance:
				return sum;
		}

		throw new Exception("Wat");
	}

	public static Dictionary<Box, int> Scores(string roll)
	{
		var scores = new Dictionary<Box, int>();

		foreach (Box box in Enum.GetValues(typeof(Box)))
			scores[box] = Score(roll, box);

		return scores;
	}
}