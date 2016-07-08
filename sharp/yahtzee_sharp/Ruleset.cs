using System.Linq;
using System.Collections.Generic;

public abstract class Ruleset
{
	public Boxes Boxes;

	public int UpperBonusThreshold;
	public int UpperBonus;

	public int NumPhases;

	public abstract int Score(string roll, Box2 box);

	public bool IsUpper(Box2 box)
	{
		return box.bits <= Boxes.GetBox("sixes").bits;
	}

	protected static int ScoreUpper(byte[] faces, int face)
	{
		return faces.Count(f => f == face) * face;
	}

	protected static bool NOfKind(byte[] faces, int n, bool exact = false)
	{
		for (int i = 1; i <= 6; i++)
		{
			var count = faces.Count(f => f == i);

			if ((exact && count == n) || (!exact && count >= n))
				return true;
		}

		return false;
	}

	public Dictionary<Box2, int> Scores(string roll)
	{
		var scores = new Dictionary<Box2, int>();

		foreach (Box2 box in Boxes.AllBoxes)
			scores[box] = Score(roll, box);

		return scores;
	}
}