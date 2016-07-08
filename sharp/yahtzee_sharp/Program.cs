using System;

class MainClass
{
	public static void Main(string[] args)
	{
		if (args.Length < 1)
		{
			PrintUsage();
			return;
		}

		var rulesetArg = args[0];
		Ruleset ruleset;

		switch (rulesetArg)
		{
			case "yahtzee":
				ruleset = new Yahtzee();
				break;
			case "yatzy":
				ruleset = new Yatzy();
				break;
			default:
				Console.WriteLine("Unknown ruleset " + rulesetArg);
				return;
		}

		var solver = new Solver(ruleset);

		if (args.Length > 1)
		{
			var startStep = int.Parse(args[1]);
			solver.Solve(startStep);
		}
		else
		{
			solver.Solve();
		}
	}

	public static void PrintUsage()
	{
		Console.WriteLine("Usage: mono yahtzee.exe ruleset [startStep]");
	}
}