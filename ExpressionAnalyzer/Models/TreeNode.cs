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
}
