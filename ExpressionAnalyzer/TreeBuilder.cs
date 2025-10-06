using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionAnalyzer
{
    public class TreeBuilder
    {
		private string _expr;

		public TreeBuilder(string expr)
		{
			_expr = expr;
		}

		public void Build(bool isDistributive)
		{
			var optimizer = new ExprOptimizer(_expr);

			_expr = optimizer.Optimize(isDistributive);
		}
	}
}
