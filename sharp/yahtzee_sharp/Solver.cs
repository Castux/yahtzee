﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Solver
{
	// Rule set

	private Ruleset ruleset;
	private int NumUpperScores;

	// Precomputed static info

	private List<string> rolls;
	private Dictionary<string, int> rollIndices;

	private Dictionary<string, Dictionary<string, float>[]> rerolls;
	private Dictionary<string, Dictionary<Box2, int>> scores;

	private List<List<BoxSet2>> boxsetsByCount;

	public Solver(Ruleset ruleset)
	{
		this.ruleset = ruleset;
		NumUpperScores = ruleset.UpperBonusThreshold + 1;

		PrecomputeStaticInfo();
	}

	private void PrecomputeStaticInfo()
	{
		// Keep a flat index of rolls to use for later storage

		rolls = Dice.FirstRoll().Keys.ToList();
		rolls.Sort();

		rollIndices = new Dictionary<string, int>();
		for (int i = 0; i < rolls.Count; i++)
			rollIndices[rolls[i]] = i;

		// Then: rerolls and scores

		rerolls = new Dictionary<string, Dictionary<string, float>[]>();
		scores = new Dictionary<string, Dictionary<Box2, int>>();

		foreach (var roll in rolls)
		{
			// All rerolls

			rerolls[roll] = Dice.Rerolls(roll);

			// and scores

			scores[roll] = ruleset.Scores(roll);
		}

		// Boxsets by number of boxes

		boxsetsByCount = new List<List<BoxSet2>>();

		for (var i = 0; i <= ruleset.Boxes.NumBoxes; i++)
			boxsetsByCount.Add(new List<BoxSet2>());

		foreach (var boxset in ruleset.Boxes.AllBoxSets)
		{
			boxsetsByCount[boxset.Contents.Count()].Add(boxset);
		}
	}

	// States

	public struct Result
	{
		public float value;
		public byte action;
	}

	private Result[][][,] data;
	private DateTime startTime;

	public void Solve()
	{
		var numSteps = ruleset.NumPhases * ruleset.Boxes.NumBoxes;
		data = new Result[numSteps][][,];

		var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
		startTime = DateTime.Now;

        for (var step = numSteps - 1; step >= 0; step--)
		{
            var round = step / ruleset.NumPhases;

			Console.WriteLine(string.Format("==== Step {0}/round {1} ====", step, round + 1));

			var boxsetsForRound = boxsetsByCount[ruleset.Boxes.NumBoxes - round];

			data[step] = new Result[BoxSet.NumBoxSets][,];

			Parallel.ForEach(boxsetsForRound, options, boxset => Solve(step, boxset));

			// Free unused memory

			if (step + 2 < numSteps)
			{
				data[step + 2] = null;
			}
		}
	}

	private int solveCount = 0;

	private void Solve(int step, BoxSet2 boxset)
	{
		data[step][boxset.bits] = new Result[NumUpperScores, rolls.Count];

		for (var upperScore = 0; upperScore < NumUpperScores; upperScore++)
			foreach (var roll in rolls)
				Solve(boxset, step, upperScore, roll);

		DumpResult(step, boxset);

		lock (this)
		{
			solveCount++;

			var totalCount = BoxSet.NumBoxSets * ruleset.NumPhases;

			var averageSolveTime = (DateTime.Now - startTime).TotalSeconds / solveCount;
			var remainingTime = (totalCount - solveCount) * averageSolveTime;

			Console.WriteLine(string.Format("Solved {0:F2}%, {1:F1}s left", (double)solveCount / totalCount * 100, remainingTime));
		}
	}

	private void Solve(BoxSet2 boxset, int step, int upperScore, string roll)
	{
		var phase = step % ruleset.NumPhases;

		// Phase 2: scoring

		if (phase == ruleset.NumPhases - 1)
		{
			float maxValue = -1;
			Box2? bestBox = null;

			foreach (Box2 box in boxset.Contents)
			{
				// What's the score for this box?

				var score = scores[roll][box];

				// Is there an upper bonus?

				var newUpperScore = upperScore + (ruleset.IsUpper(box) ? score : 0);
				if (newUpperScore >= ruleset.UpperBonusThreshold && upperScore < ruleset.UpperBonusThreshold)
					score += ruleset.UpperBonus;

				if (newUpperScore > ruleset.UpperBonusThreshold)
					newUpperScore = ruleset.UpperBonusThreshold;

				// Add expected future score: rerolling all dice

				var newBoxSet = boxset;
				newBoxSet.Remove(box);

				float futureScore = 0;

				if (!newBoxSet.IsEmpty)
				{
					foreach (var reroll in rerolls[roll][0]) // reroll by keeping nothing (keep pattern = 0)
					{
						futureScore += reroll.Value * data[step + 1][newBoxSet.bits][newUpperScore, rollIndices[reroll.Key]].value;
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

			data[step][boxset.bits][upperScore, rollIndices[roll]] = new Result { action = (byte)bestBox.Value.Index, value = maxValue };
		}

		// Other phases: rerolling

		else
		{
			float maxValue = -1;
			byte bestKeepPattern = 0;

			for (byte keepPattern = 0; keepPattern < Dice.NumRerollPatterns; keepPattern++)
			{
				if (rerolls[roll][keepPattern] != null)
				{
					float futureScore = 0;

					foreach (var reroll in rerolls[roll][keepPattern])
					{
						futureScore += reroll.Value * data[step + 1][boxset.bits][upperScore, rollIndices[reroll.Key]].value;
					}

					if (futureScore > maxValue)
					{
						maxValue = futureScore;
						bestKeepPattern = keepPattern;
					}
				}
			}

			// Save result

			data[step][boxset.bits][upperScore, rollIndices[roll]] = new Result { action = bestKeepPattern, value = maxValue };
		}
	}

	private void DumpResult(int step, BoxSet2 boxset)
	{
		var path = string.Format("step{0:D}", step);

		Directory.CreateDirectory(path);

		using (var writer = new StreamWriter(File.Open(path + "/data" + boxset.bits, FileMode.Create)))
		{
			foreach (var foo in data[step][boxset.bits])
			{
				writer.Write(string.Format("{0},{1:F2},", foo.action, foo.value));
			}
		}
	}
}

