using ExpressionAnalyzer.Models;
using System.ComponentModel.Design;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ExpressionAnalyzer
{
	public class TreeBuilder
	{
		private string _expr;
		private string[] firstPriorOperators = { "*", "/" };
		private string[] secondPriorOperators = { "+", "-" };
		private string[] operators = { "+", "-", "*", "/" };
		public TreeNode Root { get; private set; }

		public TreeBuilder(string expr)
		{
			_expr = expr;
		}

		public (string, string) Build(bool isDistributive)
		{
			var optimizer = new ExprOptimizer(_expr);
			var optimizedList = optimizer.Optimize(isDistributive);

			var optimizedExp = string.Join("", optimizedList);

			var result = MakeBrackets(optimizedList);

			BuildTreeFromTokens(result);

			return (optimizedExp, result);
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
						tokens[i + 1] = tokens[i + 1].Replace("/", "*");

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

		private void BuildTreeFromTokens(string expr)
		{
			Root = new TreeNode(expr);
			var pattern = @"\((?>[^()]+|(?<open>\()|(?<-open>\)))*(?(open)(?!))\)"
						 + @"|(?:(?<=^|[\(\+\-\*/])-\s*(?:\d+(?:\.\d+)?|[a-zA-Z_]\w*))"
						 + @"|\d+(?:\.\d+)?"
						 + @"|[a-zA-Z_]\w*"
						 + @"|[\+\-\*/]";

			var res = new List<string>();

			void Dfs(TreeNode n)
			{
				if (n == null) return;
				if (n.Value.StartsWith('(') && n.Value.EndsWith(')'))
				{
					n.Value = n.Value.Remove(n.Value.Length - 1, 1).Remove(0, 1);
					var matches = Regex.Matches(n.Value, pattern);

					for (int i = 0; i < matches.Count; i++) 
					{
						if (operators.Any(x => x == matches[i].Value))
						{
							n.Value = matches[i].Value;
							n.Left = new TreeNode(matches[i - 1].Value);

							if (matches[i].Value == "-")
							{
								n.Right = new TreeNode(matches[i + 1].Value.Replace("+", "§").Replace("-", "+").Replace("§", "-"));
							}
							else if (matches[i].Value == "/")
							{
								n.Right = new TreeNode(matches[i + 1].Value.Replace("*", "/"));
							}
							else
							{
								n.Right = new TreeNode(matches[i + 1].Value);
							}
						}
					}
				}
				Dfs(n.Left);
				Dfs(n.Right);
			}

			Dfs(Root);
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
