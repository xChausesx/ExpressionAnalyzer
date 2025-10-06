using ExpressionAnalyzer;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

string[] testCases =
			{
				"-3+12c**d/e-d*f2/cd**(a+2.2*4)",
				"-(b+c)+func1((1a*baa+1bj_ko2*(j-e))",
				"-a+b2*0-nm",
				"g2*(b-17.3)))+(6-cos(5)))",
				"-(215.01+312,2)b)+(1c",
				"/a*b**c + m)*a*b + a*c - a*smn(j*k/m + m",
				"-cos(-&t))/(*(*f)(127.0.0.1, \"/dev/null\", (t==0)?4more_errors:b^2) - .5",
				"//(*0)- an*0p(a+b)-1.000.5//6(*f(-b, 1.8-0*(2-6) %1 + (++a)/(6x^2+4x-1) +\r\nd/dt*(smn(at+q)/(4cos(at)-ht^2)",
				"-(-5x((int*)exp())/t - 3.14.15k/(2x^2-5x-1)*y - A[N*(i++)+j]",
				"-(-exp(3et/4.0.2, 2i-1)/L + )((void*)*f()) + ((i++) + (++i/(i--))/k//) + 6.000.500.5",
				"**f(*k, -p+1, ))2.1.1 + 1.8q((-5x ++ i)",
				"/.1(2x^2-5x+7)-(-i)+ (j++)/0 - )(*f)(2, 7-x, )/q + send(-(2x+7)/A[j, i], 127.0.0.1 ) + )/",
				"*101*1#(t-q)(t+q)//dt - (int*)f(8t, -(k/h)A[i+6.]), exp(), ))(t-k*8.00.1/.0"
			};

/*for (int i = 0; i < testCases.Length; i++)
{
	Console.WriteLine($"Тест {i + 1}: {testCases[i]}");
	var analyzer = new Analyzer(testCases[i]);
	var errors = analyzer.Analyze();

	if (errors.Count == 0)
		Console.WriteLine("✅ Вираз коректний\n");
	else
	{
		Console.WriteLine("Знайдені помилки:");
		foreach (var err in errors)
			Console.WriteLine(err);
		Console.WriteLine();
	}
}*/

//string expr = "-3*0+12+0+0*3+a+1*b+24+36+12+36+12*c*d/e-d*f2/cd*(a+2.2*4)";

string expr = "(a+b+5)*2+0*(0/5-(6+3+d))";
var optimizer = new ExprOptimizer(expr);

try
{
    string optimized = optimizer.Optimize(false);
    Console.WriteLine("Оптимізований вираз: " + optimized);
}
catch (DivideByZeroException ex)
{
    Console.WriteLine("Помилка: " + ex.Message);
}