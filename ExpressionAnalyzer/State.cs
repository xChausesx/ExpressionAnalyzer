﻿namespace ExpressionAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

enum State
{
	Start,
	Constant,
	ConstantDot,
	ConstantDotConstant,
	ConstantWithSuffix,
	VariableOrFunction,
	FunctionOpenBracket,
	OpenBracket,
	CloseBracket,
	Operation,
	UnaryOperator,
	End
}