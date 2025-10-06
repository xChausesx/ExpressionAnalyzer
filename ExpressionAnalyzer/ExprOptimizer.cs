using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ExpressionAnalyzer
{
    public class ExprOptimizer
    {
        private string _expr;

        public ExprOptimizer(string expr)
        {
            _expr = expr;
        }

        public string Optimize()
        {
            var tokens = Tokenize(_expr);
            var optimizedTokens = SimplifyTokens(tokens);
            AnalyzeBrackets(optimizedTokens);


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
            string[] firstPriorOperators = { "*", "/" };
            string[] operators = { "+", "-", "*", "/" };

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
                        && (i == tokens.Count - 1 || !firstPriorOperators.Contains(tokens[i + 1])))
                    {
                        string op = tokens[i - 1];

                        var resultValue = op switch
                        {
                            "+" => value + secondValue,
                            "-" => value - secondValue,
                            "*" => value * secondValue,
                            "/" => value / secondValue,
                            _  => 0,
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

        private void AnalyzeBrackets(List<string> tokens)
        {
            var bracketSeq = new List<string>();
            bool inBrack = false;
            var j = 0;

            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i] == ")")
                {
                    j++;
                    inBrack = false;
                }

                if (inBrack)
                {
                    if(bracketSeq.Count == j)
                    {
                        bracketSeq.Add("");
                    }
                    bracketSeq[j] += tokens[i];
                }

                if (tokens[i] == "(")
                {
                    inBrack = true;
                }
            }
        }
    }
}
