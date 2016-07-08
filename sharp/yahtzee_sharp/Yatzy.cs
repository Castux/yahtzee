using System;
using System.Linq;
using System.Collections.Generic;

public class Yatzy : Ruleset
{
	public Yatzy()
	{
		Boxes = new Boxes(new List<string>
		{
			"ones",
			"twos",
			"threes",
			"fours",
			"fives",
			"sixes",
			"one pair",
			"two pairs",
			"three of kind",
			"four of kind",
			"small straight",
			"large straight",
			"full house",
			"chance",
			"yatzy"
		});

		UpperBonusThreshold = 63;
		UpperBonus = 50;
		NumPhases = 3;
	}

	public override int Score(string roll, Box box)
	{
		var faces = Dice.Faces(roll);

		var boxname = Boxes.GetName(box);

		switch (boxname)
		{
			case "ones":
				return ScoreUpper(faces, 1);
			case "twos":
				return ScoreUpper(faces, 2);
			case "threes":
				return ScoreUpper(faces, 3);
			case "fours":
				return ScoreUpper(faces, 4);
			case "fives":
				return ScoreUpper(faces, 5);
			case "sixes":
				return ScoreUpper(faces, 6);
			case "one pair":
				return NOfKind(faces, 2) * 2;
			case "two pairs":
				return TwoPairs(faces);
			case "three of kind":
				return NOfKind(faces, 3) * 3;
			case "four of kind":
				return NOfKind(faces, 4) * 4;
			case "full house":
				return FullHouse(faces);
			case "small straight":
				return roll == "12345" ? 15 : 0;
			case "large straight":
				return roll == "23456" ? 20 : 0;
			case "chance":
				return faces.Select(x => (int)x).Sum();
			case "yatzy":
				return (NOfKind(faces, 5) > 0) ? 50 : 0;
		}

		throw new Exception("Wat");
	}

	private byte NOfKind(byte[] faces, int n, byte? exclude = null)
	{
		for (byte face = 6; face >= 1; face--)
		{
			var count = faces.Count(f => f == face);
			if (count >= n && face != exclude)
				return face;
		}

		return 0;
	}

	private int TwoPairs(byte[] faces)
	{
		var first = NOfKind(faces, 2);

		if (first > 0)
		{
			var second = NOfKind(faces, 2, first);
			if (second > 0)
				return first * 2 + second * 2;
		}

		return 0;
	}

	private int FullHouse(byte[] faces)
	{
		var three = NOfKind(faces, 3);
		if (three > 0)
		{
			var two = NOfKind(faces, 2, three);
			if (two > 0)
			{
				return two * 2 + three * 3;
			}
		}

		return 0;
	}

}

