using ExpressionAnalyzer.Models;
using System.Text.RegularExpressions;

namespace ExpressionAnalyzer
{
	public class TreeBuilder
	{
		private string _expr;
		private string[] firstPriorOperators = { "*", "/" };
		private string[] secondPriorOperators = { "+", "-" };
		public TreeNode Root { get; private set; }

		public TreeBuilder(string expr)
		{
			_expr = expr;
		}

		public string Build(bool isDistributive)
		{
			var optimizer = new ExprOptimizer(_expr);
			var optimizedList = optimizer.Optimize(isDistributive);

			var optimizedExp = string.Join("", optimizedList);

			var result = MakeBrackets(optimizedList);

			return result;
		}

		public string MakeBrackets(List<string> tokens)
		{
			var candidate = string.Empty;
			var mulOrDivCandidate = string.Empty;

			var expFirst = string.Join("", tokens);

			var matchCount = 0;

			if (tokens[0] == "-")
			{
				tokens.RemoveAt(0);
				tokens[0] = "-" + tokens[0];
			}

			do
			{
				for (int i = 0; i < tokens.Count; i++)
				{
					if (tokens[i] == "*")
					{
						tokens[i] = "(" + tokens[i - 1] + tokens[i] + tokens[i + 1] + ")";

						tokens.RemoveAt(i + 1);
						tokens.RemoveAt(i - 1);
						i++;
					}
					if (i + 1 < tokens.Count && tokens[i] == "/")
					{
						tokens[i] = "(" + tokens[i - 1] + tokens[i] + tokens[i + 1] + ")";

						tokens.RemoveAt(i + 1);
						tokens.RemoveAt(i - 1);
						i++;
					}
					if (i + 1 < tokens.Count && secondPriorOperators.Any(x => x == tokens[i])
						&& (i + 1 < tokens.Count && (i + 2 >= tokens.Count || !firstPriorOperators.Contains(tokens[i + 2])))
						&& (i < 2 || !firstPriorOperators.Any(x => x == tokens[i - 2])))
					{
						if (tokens[i] == "-")
						{
							tokens[i + 1] = tokens[i + 1].Replace("+", "§").Replace("-", "+").Replace("§", "-");
						}

						tokens[i] = "(" + tokens[i - 1] + tokens[i] + tokens[i + 1] + ")";

						tokens.RemoveAt(i + 1);
						tokens.RemoveAt(i - 1);
						i++;
					}
				}

				var pattern = @"\((?>[^()]+|(?<open>\()|(?<-open>\)))*(?(open)(?!))\)|(?<=^)-\d+(\.\d+)?|\d+(\.\d+)?|[a-zA-Z_]\w*|[\+\-\*/]";

				var exp = string.Join("", tokens);
				var matches = Regex.Matches(exp, pattern);
				matchCount = matches.Count;
				tokens.Clear();
				foreach (Match m in matches)
					tokens.Add(m.Value);
			} while (matchCount != 1);

			return string.Join("", tokens);
		}

		private TreeNode BuildTreeFromTokens(List<string> tokens)
		{
			Stack<TreeNode> nodeStack = new Stack<TreeNode>();

			return null;
		}

		public void PrintTree(TreeNode node = null, string indent = "", bool isLeft = true)
		{
			if (node == null)
				node = Root;

			if (node == null)
				return;

			Console.WriteLine(indent + (isLeft ? "├── " : "└── ") + node.Value);

			if (node.Left != null || node.Right != null)
			{
				if (node.Left != null)
					PrintTree(node.Left, indent + (isLeft ? "│   " : "    "), true);

				if (node.Right != null)
					PrintTree(node.Right, indent + (isLeft ? "│   " : "    "), false);
			}
		}
	}
}
