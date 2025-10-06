using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ExpressionAnalyzer
{
	public class ExprOptimizer
	{
		private string _expr;
		private string[] operators = { "+", "-", "*", "/" };
		private string[] firstPriorOperators = { "*", "/" };

		public ExprOptimizer(string expr)
		{
			_expr = expr;
		}

		public string Optimize(bool isDistributive)
		{
			var tokens = Tokenize(_expr);

			var optimizedTokens = SimplifyTokens(tokens);

			if (isDistributive)
			{
				SimplifyBrackets(optimizedTokens);

				optimizedTokens = SimplifyTokens(tokens);

			}

			return string.Join("", optimizedTokens);
		}

		private List<string> Tokenize(string expr)
		{
			var pattern = @"(?<=^)-\d+(\.\d+)?|\d+(\.\d+)?|[a-zA-Z_]\w*|[\+\-\*/\(\)]";
			var matches = Regex.Matches(expr, pattern);
			var tokens = new List<string>();
			foreach (Match m in matches)
				tokens.Add(m.Value);
			return tokens;
		}

		private List<string> SimplifyTokens(List<string> tokens)
		{
			bool changed;

			do
			{
				changed = false;

				for (int i = 0; i < tokens.Count; i++)
				{
					var test = tokens[i];

					if (tokens[i] == "0" && i > 0 && tokens[i - 1] == "*")
					{
						tokens[i] = "0";
						tokens.RemoveAt(i - 2);
						tokens.RemoveAt(i - 2);
						changed = true;
						break;
					}

					if (tokens[i] == "0" && i + 1 < tokens.Count && tokens[i + 1] == "*" && (i + 1 == tokens.Count || tokens[i + 2] != "("))
					{
						tokens.RemoveAt(i + 2);
						tokens.RemoveAt(i + 1);
						changed = true;
						break;
					}

					if (tokens[i] == "0" && i + 1 < tokens.Count && tokens[i + 1] == "/" && (i + 1 == tokens.Count || tokens[i + 2] != "("))
					{
						tokens.RemoveAt(i + 2);
						tokens.RemoveAt(i + 1);
						changed = true;
						break;
					}

					if (tokens[i] == "/" && i + 1 < tokens.Count && tokens[i + 1] == "0")
						throw new DivideByZeroException($"Ділення на нуль на позиції {i + 1}");

					if (tokens[i] == "1" && i > 0 && tokens[i - 1] == "*")
					{
						tokens[i] = "0";
						tokens.RemoveAt(i - 1);
						tokens.RemoveAt(i - 1);
						changed = true;
						break;
					}

					if (tokens[i] == "1" && i + 1 < tokens.Count && tokens[i + 1] == "*")
					{
						tokens.RemoveAt(i);
						tokens.RemoveAt(i);
						changed = true;
						break;
					}

					if (tokens[i] == "1" && i > 0 && tokens[i - 1] == "/")
					{
						tokens.RemoveAt(i - 1);
						tokens.RemoveAt(i - 1);
						changed = true;
						break;
					}

					if (i > 1 &&
						double.TryParse(tokens[i], NumberStyles.Any, CultureInfo.InvariantCulture, out double secondValue)
						&& operators.Contains(tokens[i - 1]) &&
						double.TryParse(tokens[i - 2], NumberStyles.Any, CultureInfo.InvariantCulture, out double value)
						&& (i == tokens.Count - 1 || !firstPriorOperators.Contains(tokens[i + 1]))
						&& (i == 2 || !firstPriorOperators.Contains(tokens[i - 3])))
					{
						string op = tokens[i - 1];

						var resultValue = op switch
						{
							"+" => value + secondValue,
							"-" => value - secondValue,
							"*" => value * secondValue,
							"/" => value / secondValue,
							_ => 0,
						};

						tokens[i] = resultValue.ToString();

						tokens.RemoveAt(i - 2);
						tokens.RemoveAt(i - 2);
						changed = true;
						break;
					}

					if (i > 1 && tokens[i] == ")" && tokens[i - 2] == "(")
					{
						tokens[i] = tokens[i - 1];

						tokens.RemoveAt(i - 2);
						tokens.RemoveAt(i - 2);
						changed = true;
						break;
					}
				}

			} while (changed);

			return tokens;
		}

		private void SimplifyBrackets(List<string> tokens)
		{
			var bracketSeq = new Dictionary<string, string>();
			bool inBrack = false;
			var key = "";

			var nestedBracketsCount = 0;
			var nestedBracketsAmount = 1;
			var indexOfFirstBracket = 0;

			do
			{
				for (int i = 0; i < tokens.Count; i++)
				{
					if (tokens[i] == ")")
					{
						if (nestedBracketsCount != 0)
						{
							nestedBracketsCount--;
						}
						else if (!string.IsNullOrEmpty(key))
						{
							RemoveBrackets(tokens, bracketSeq, key, indexOfFirstBracket, i);
							i-= 2;
							key = "";
							inBrack = false;
						}
					}

					if (i < tokens.Count - 1 && tokens[i] == "(")
					{
						if (inBrack)
						{
							nestedBracketsCount++;
							nestedBracketsAmount++;
						}
						else
						{
							indexOfFirstBracket = i;
						}

						inBrack = true;
					}

					if (inBrack)
					{
						if (string.IsNullOrEmpty(key))
						{
							if (i > 1)
							{
								key = tokens[i - 2] + tokens[i - 1];
							}
							else
							{
								key = "+";
							}

							bracketSeq.Add(key, "");
							continue;
						}

						bracketSeq[key] += tokens[i];
					}
				}

				nestedBracketsAmount--;
			}
			while (nestedBracketsAmount > 0);
		}

		private void RemoveBrackets(List<string> tokens, Dictionary<string, string> bracketSeq, string key, int startIndex, int currentIndex)
		{
			var sequence = Tokenize(bracketSeq[key]);

			var count = sequence.Count;

			sequence = SimplifyTokens(sequence);

			char op = key.Last();

			string seqAfterDiv = "";

			var resultValue = op switch
			{
				'+' => PlusOpSequence(sequence),
				'-' => MinusOpSequence(sequence),
				'*' => MulOpSequence(sequence, key),
				'/' => DivOpSequence(sequence, key),
				_ => new List<string>(),
			};

			if (firstPriorOperators.Contains(op.ToString()))
			{
				startIndex-=2;

				count+=2;
			}

			if (currentIndex < tokens.Count - 1 && firstPriorOperators.Contains(tokens[currentIndex + 1]))
			{
				currentIndex++;

				while (tokens[currentIndex] != "+" && tokens[currentIndex] != "-")
				{
					seqAfterDiv += tokens[currentIndex];
					currentIndex++;
				}

				for (int i = 0; i < resultValue.Count; i++)
				{
					if (resultValue[i] != "+" && resultValue[i] != "-" && i != resultValue.Count - 1)
					{
						continue;
					}

					if (i != resultValue.Count - 1 && !(resultValue[i] == ")"))
					{
						resultValue.Insert(i, seqAfterDiv);
					}
					else
					{
						resultValue.Add(seqAfterDiv);
					}

					i++;
				}

				count+= seqAfterDiv.Length;

				var resultStr = string.Join("", resultValue);

				resultValue = Tokenize(resultStr);
			}

			tokens.RemoveRange(startIndex, count + 2);

			tokens.InsertRange(startIndex, resultValue);
		}

		private List<string> PlusOpSequence(List<string> sequence) => sequence;

		private List<string> MinusOpSequence(List<string> sequence)
		{
			for (int i = 0; i < sequence.Count; i++)
			{
				if (!operators.Contains(sequence[i]) && sequence[i] != "(" && sequence[i] != ")")
				{
					sequence[i] = "-" + sequence[i];
				}
			}

			return sequence;
		}

		private List<string> MulOpSequence(List<string> sequence, string key)
		{
			var keyVaue = key.TrimEnd('*');

			for (int i = 0; i<sequence.Count; i++)
			{
				if (!operators.Contains(sequence[i]) && sequence[i] != "(" && sequence[i] != ")" && (i <= 0 || sequence[i - 1] != "/"))
				{
					sequence.Insert(i, "*");
					sequence.Insert(i, keyVaue);
					i+=2;
				}
			}

			return sequence;
		}

		private List<string> DivOpSequence(List<string> sequence, string key)
		{
			var keyVaue = key.TrimEnd('/');

			for (int i = 0; i<sequence.Count; i++)
			{
				if (!operators.Contains(sequence[i]) && sequence[i] != "(" && sequence[i] != ")")
				{
					sequence.Insert(i, keyVaue);
					sequence.Insert(i, "/");
				}
			}

			return sequence;
		}
	}
}
