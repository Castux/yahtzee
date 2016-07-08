using System;
using System.Linq;
using System.Collections.Generic;

public class Yahtzee : Ruleset
{
    public Yahtzee()
    {
		Boxes = new Boxes(new List<string>
		{
			"ones",
			"twos",
			"threes",
			"fours",
			"fives",
			"sixes",
			"three of kind",
			"four of kind",
			"full house",
			"small straight",
			"large straight",
			"yahtzee",
			"chance"
		});

		UpperBonusThreshold = 63;
		UpperBonus = 35;
		NumPhases = 3;
    }

	public override int Score(string roll, Box box)
	{
		var faces = Dice.Faces(roll);
		var sum = faces.Select(x => (int)x).Sum();
		var longestSequence = LongestSequence(faces);

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
			case "three of kind":
				return NOfKind(faces, 3) ? sum : 0;
			case "four of kind":
				return NOfKind(faces, 4) ? sum : 0;
			case "full house":
				return (NOfKind(faces, 3, true) && NOfKind(faces, 2, true)) ? 25 : 0;
			case "small straight":
				return longestSequence >= 4 ? 30 : 0;
			case "large straight":
				return longestSequence >= 5 ? 40 : 0;
			case "yahtzee":
				return NOfKind(faces, 5) ? 50 : 0;
			case "chance":
				return sum;
		}

		throw new Exception("Wat");
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
}

