using ExpressionAnalyzer;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

string[] testCases =
			{
			  "a+b+c+d+e+f+g+h",
			  "a+b/c+d/e+f/g/h+g*k+d*t",
			  "a/b/c/d/e/f/g/h",
			  "a+b+c+d+e+f+g+h+i+j+k+l+m*n+o+p+q+a+a+t+u+v+w+x+y+z"
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