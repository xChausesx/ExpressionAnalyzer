namespace ExpressionAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Analyzer
{
	private readonly string _expression;
	private readonly List<string> _errors = new List<string>();
	private readonly Stack<char> _brackets = new Stack<char>();
	private State _state = State.Start;

	public Analyzer(string expression)
	{
		_expression = expression.Replace(" ", "");
	}

	public List<string> Analyze()
	{
		for (int i = 0; i < _expression.Length; i++)
		{
			char ch = _expression[i];
			ProcessChar(ch, i);
		}

		if (_state == State.Operation || _state == State.ConstantDot || _state == State.FunctionOpenBracket)
			_errors.Add("❌ Вираз не може закінчуватися на оператор чи некоректний символ.");

		if (_brackets.Count > 0)
			_errors.Add("❌ Непарна кількість відкритих дужок.");

		if (_errors.Count == 0)
			_state = State.End;

		return _errors;
	}

	private void ProcessChar(char ch, int pos)
	{
		switch (_state)
		{
			case State.Start:
				if (char.IsLetter(ch)) _state = State.VariableOrFunction;
				else if (char.IsDigit(ch)) _state = State.Constant;
				else if (ch == '(') { _state = State.OpenBracket; _brackets.Push('('); }
				else _errors.Add($"❌ [{pos}] Вираз не може починатись із '{ch}'");
				break;

			case State.Constant:
				if (char.IsDigit(ch)) { }
				else if (ch == '.') _state = State.ConstantDot;
				else if (IsOperator(ch)) _state = State.Operation;
				else if (ch == ')') { _state = State.CloseBracket; PopBracket(pos); }
				else _errors.Add($"❌ [{pos}] Некоректний символ після числа: '{ch}'");
				break;

			case State.ConstantDot:
				if (char.IsDigit(ch)) _state = State.ConstantDotConstant;
				else _errors.Add($"❌ [{pos}] Після '.' повинна йти цифра.");
				break;

			case State.ConstantDotConstant:
				if (char.IsDigit(ch)) { }
				else if (IsOperator(ch)) _state = State.Operation;
				else if (ch == ')') { _state = State.CloseBracket; PopBracket(pos); }
				else _errors.Add($"❌ [{pos}] Некоректний символ після числа: '{ch}'");
				break;

			case State.VariableOrFunction:
				if (char.IsLetter(ch)) { }
				else if (ch == '(') { _state = State.FunctionOpenBracket; _brackets.Push('('); }
				else if (IsOperator(ch)) _state = State.Operation;
				else if (ch == ')') { _state = State.CloseBracket; PopBracket(pos); }
				else _errors.Add($"❌ [{pos}] Некоректний символ після змінної/функції: '{ch}'");
				break;

			case State.FunctionOpenBracket:
				if (char.IsLetter(ch)) _state = State.VariableOrFunction;
				else if (char.IsDigit(ch)) _state = State.Constant;
				else if (ch == ')') { _state = State.CloseBracket; PopBracket(pos); }
				else _errors.Add($"❌ [{pos}] Некоректний символ після відкриття дужки у функції: '{ch}'");
				break;

			case State.OpenBracket:
				if (char.IsLetter(ch)) _state = State.VariableOrFunction;
				else if (char.IsDigit(ch)) _state = State.Constant;
				else if (ch == '(') { _state = State.OpenBracket; _brackets.Push('('); }
				else if (ch == '+' || ch == '-') _state = State.Operation;
				else _errors.Add($"❌ [{pos}] Некоректний символ після '(' : '{ch}'");
				break;

			case State.CloseBracket:
				if (IsOperator(ch)) _state = State.Operation;
				else if (ch == ')') { _state = State.CloseBracket; PopBracket(pos); }
				else _errors.Add($"❌ [{pos}] Некоректний символ після ')': '{ch}'");
				break;

			case State.Operation:
				if (char.IsLetter(ch)) _state = State.VariableOrFunction;
				else if (char.IsDigit(ch)) _state = State.Constant;
				else if (ch == '(') { _state = State.OpenBracket; _brackets.Push('('); }
				else _errors.Add($"❌ [{pos}] Некоректний символ після оператора: '{ch}'");
				break;
		}
	}

	private void PopBracket(int pos)
	{
		if (_brackets.Count == 0)
			_errors.Add($"❌ [{pos}] Зайва закриваюча дужка ')'");
		else
			_brackets.Pop();
	}

	private bool IsOperator(char ch) => ch == '+' || ch == '-' || ch == '*' || ch == '/';
}