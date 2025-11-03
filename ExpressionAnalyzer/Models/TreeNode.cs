namespace ExpressionAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TreeNode
{
	public string Value { get; set; }
	public TreeNode Left { get; set; }
	public TreeNode Right { get; set; }

	public TreeNode(string value)
	{
		Value = value;
	}

	public TreeNode(string value, TreeNode? left = null, TreeNode? right = null)
	{
		Value = value;
		Left = left;
		Right = right;
	}

	/// <summary>
	/// Перевіряє, чи є вузол операцією
	/// </summary>
	public bool IsOperation() => "+-*/".Contains(Value);

	/// <summary>
	/// Перевіряє, чи є вузол операндом (листом)
	/// </summary>
	public bool IsOperand() => !IsOperation() && Left == null && Right == null;
}
