﻿using System.Linq;
using System.Collections.Generic;

public abstract class Ruleset
{
	public Boxes Boxes;

	public int UpperBonusThreshold;
	public int UpperBonus;

	public int NumPhases;

	public abstract int Score(string roll, Box box);

	public bool IsUpper(Box box)
	{
		return box.bits <= Boxes.GetBox("sixes").bits;
	}

	protected static int ScoreUpper(byte[] faces, int face)
	{
		return faces.Count(f => f == face) * face;
	}

	public Dictionary<Box, int> Scores(string roll)
	{
		var scores = new Dictionary<Box, int>();

		foreach (Box box in Boxes.AllBoxes)
			scores[box] = Score(roll, box);

		return scores;
	}
}