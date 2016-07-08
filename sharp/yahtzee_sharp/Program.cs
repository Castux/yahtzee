class MainClass
{
	public static void Main(string[] args)
	{
		var ruleset = new Yahtzee();
		var solver = new Solver(ruleset);

		solver.Solve();
	}
}