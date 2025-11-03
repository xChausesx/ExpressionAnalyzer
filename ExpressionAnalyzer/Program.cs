using ExpressionAnalyzer;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

string[] testCases =
			{
			  "a+b*c + k - x - d - e - f/g/h/q",
			  "a+b+c+d+e+f-d*f*g*c*s",
			  "0/b/c/v/d/e/g*t-v-b-d-s-e-g",
			  "a*(b+(c+d)/e)+b*0+5+4-1*n",
			  "0+b*0+0*a+a*b+1",
			  "2+3+4+5+6+7+8*s-p",
			  "(a+b+5)*2+0*(0/5-(6+3+d))",
			  "a*(b+c-1)*d",
			  "(a-c)*(b-k+1)"
			};

foreach (var test in testCases)
{
	Console.WriteLine($"Початковий вираз: {test}");
	var analyzer = new Analyzer(test);
	var errors = analyzer.Analyze();
	var treeBuilder = new TreeBuilder(test);

	if (errors.Count == 0)
	{
		Console.WriteLine("✅ Вираз коректний\n");

		try
		{
			var groupedTest = treeBuilder.Build(true);
			Console.WriteLine("Дистрибутивний вираз: " + groupedTest.Item1);
			Console.WriteLine("Згрупований вираз: " + groupedTest.Item2 + "\n");
			var modeler = new MatrixSystemModeler();
			var resulttt = modeler.Simulate(treeBuilder.Root);
			resulttt.PrintMetrics();
			resulttt.PrintGanttChart();
			treeBuilder.PrintTree();
			Console.WriteLine("\n\n");
		}
		catch (DivideByZeroException ex)
		{
			Console.WriteLine("Помилка: " + ex.Message + "\n");
		}
	}
	else
	{
		Console.WriteLine("Знайдені помилки:");
		foreach (var err in errors)
			Console.WriteLine(err);
		Console.WriteLine();
	}
}