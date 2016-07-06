using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

public class Solver
{
	// Precomputed static info

	public List<string> rolls;
	public Dictionary<string, int> rollIndices;

	public Dictionary<string, Dictionary<string, double>> rerolls;
	public Dictionary<string, List<string>> keeps;
	public Dictionary<string, Dictionary<Box, int>> scores;

	public List<List<BoxSet>> boxsets;
	public Dictionary<Box, int> boxIndices;

	public Solver()
	{
		ComputeStaticInfo();
	}

	private void ComputeStaticInfo()
	{
		rerolls = new Dictionary<string, Dictionary<string, double>>();
		keeps = new Dictionary<string, List<string>>();
		scores = new Dictionary<string, Dictionary<Box, int>>();

		// First all possible 5 dice rolls

		rerolls[""] = Dice.Reroll("");

		// Keep a flat index of rolls to use for later storage

		rolls = rerolls[""].Keys.ToList();
		rolls.Sort();

		rollIndices = new Dictionary<string, int>();
		for (int i = 0; i < rolls.Count; i++)
			rollIndices[rolls[i]] = i;

		// Then: rerolls and scores

		foreach (var roll in rolls)
		{
			// All rerolls

			keeps[roll] = Dice.Keeps(roll);

			foreach (var keep in keeps[roll])
			{
				if (!rerolls.ContainsKey(keep))
					rerolls[keep] = Dice.Reroll(keep);
			}

			// and scores

			scores[roll] = BoxUtils.Scores(roll);
		}

		// Boxsets by number of boxes

		boxsets = new List<List<BoxSet>>();

		for (var i = 0; i <= BoxSet.NumBoxes; i++)
			boxsets.Add(new List<BoxSet>());

		foreach (var boxset in BoxSet.AllSets)
		{
			boxsets[boxset.Contents.Count()].Add(boxset);
		}

		// Box indices

		boxIndices = new Dictionary<Box, int>();
		for (var i = 0; i <= BoxSet.NumBoxes; i++)
			boxIndices[(Box)(1 << i)] = i;
	}

	// States

	public struct Result
	{
		public double value;
		public string action;
	}

	public const int NumPhases = 3;
	public const int NumUpperScores = 64;

	private Result[][,,] results;

	private int numBoxsetsSolved = 0;
	private DateTime startTime;

	private void MakeStorageForBoxSet(BoxSet boxset)
	{
		results[boxset.bits] = new Result[NumPhases, NumUpperScores, rolls.Count];
	}

	// Solve one state

	private void Solve(BoxSet boxset, int phase, int upperScore, string roll)
	{
		// Phase 2: scoring

		if (phase == 2)
		{
			double maxValue = -1;
			Box? bestBox = null;

			foreach (Box box in boxset.Contents)
			{
				// What's the score for this box?

				var score = scores[roll][box];

				// Is there an upper bonus?

				var newUpperScore = upperScore;
				if (box.IsUpper())
				{
					newUpperScore = Math.Min(63, upperScore + score);

					if (upperScore < 63 && newUpperScore >= 63)
						score += 35;
				}

				// Add expected future score: rerolling all dice

				var newBoxSet = boxset;
				newBoxSet.Remove(box);

				double futureScore = 0;

				if (!newBoxSet.IsEmpty)
				{
					foreach (var reroll in rerolls[""])
					{
						futureScore += reroll.Value * results[newBoxSet.bits][0, newUpperScore, rollIndices[reroll.Key]].value;
					}
				}

				// Total value for this action

				var totalScore = score + futureScore;
				if (totalScore >= maxValue)
				{
					maxValue = totalScore;
					bestBox = box;
				}
			}

			// Save result

			results[boxset.bits][phase, upperScore, rollIndices[roll]] = new Result { action = boxIndices[bestBox.Value].ToString(), value = maxValue };
		}

		// Other phases: rerolling

		else
		{
			double maxValue = -1;
			string bestKeep = null;

			foreach (string keep in keeps[roll])
			{
				double futureScore = 0;

				foreach (var reroll in rerolls[keep])
				{
					futureScore += reroll.Value * results[boxset.bits][phase + 1, upperScore, rollIndices[reroll.Key]].value;
				}

				if (futureScore > maxValue)
				{
					maxValue = futureScore;
					bestKeep = keep;
				}
			}

			// Save result

			results[boxset.bits][phase, upperScore, rollIndices[roll]] = new Result { action = bestKeep, value = maxValue };
		}
	}

	private void SolveBoxSet(BoxSet boxset)
	{
		MakeStorageForBoxSet(boxset);

		for (var phase = NumPhases - 1; phase >= 0; phase--)
			for (var upperScore = 0; upperScore < NumUpperScores; upperScore++)
				foreach (var roll in rolls)
					Solve(boxset, phase, upperScore, roll);
		
		DumpResult(boxset);

		lock(this)
		{
			numBoxsetsSolved++;

			var averageBoxSetTime = (DateTime.Now - startTime).TotalSeconds / numBoxsetsSolved;
			var remainingTime = (BoxSet.NumBoxSets - numBoxsetsSolved) * averageBoxSetTime;

			Console.WriteLine(string.Format("Solved {0}/{1}, {2:F1}s left", numBoxsetsSolved, BoxSet.NumBoxSets, remainingTime));
		}
	}

	// Solve a round, assuming the next round is already solved
	// and results are in results.

	private void SolveRound(int round)
	{
		Console.WriteLine(string.Format("==== Round {0} ====", round));

		if (results == null)
			results = new Result[BoxSet.NumBoxSets][,,];

		var boxsetsForRound = boxsets[14 - round];

		ParallelOptions po = new ParallelOptions
		{
			MaxDegreeOfParallelism = Environment.ProcessorCount
		};

		Parallel.ForEach(boxsetsForRound, po, SolveBoxSet);
	}

	// The big baddy

	public void SolveAll()
	{
		numBoxsetsSolved = 0;
		startTime = DateTime.Now;

		for (var round = 13; round >= 1; round--)
		{
			SolveRound(round);

			// Free unused memory

			if (round <= 11)
			{
				foreach (var boxset in boxsets[14 - (round + 2)])
				{
					results[boxset.bits] = null;
				}
			}
		}
	}

	// Utils

	public void DumpResult(BoxSet boxset)
	{
		using (var writer = new StreamWriter(File.Open("result" + boxset.bits, FileMode.Create)))
		{
			foreach (var foo in results[boxset.bits])
			{
				writer.Write(string.Format("{0},{1:F2};", foo.action, foo.value));
			}
		}
	}
}

