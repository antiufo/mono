using DbLinq.Data.Linq.Sugar.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace System.Data.Linq.Sugar.ExpressionMutator.Implementation
{
    class BlockExpressionMutator : IMutableExpression
    {
        private BlockExpression expression;

        public BlockExpressionMutator(BlockExpression expression)
        {
            this.expression = expression;
        }

        public IEnumerable<Expression> Operands
        {
            get
            {
                return expression.Variables.Concat(expression.Expressions);
            }
        }

        public Expression Mutate(IList<Expression> operands)
        {
            var vars = operands.TakeWhile(x => x is ParameterExpression).Cast<ParameterExpression>();

            return Expression.Block(vars, operands.Skip(vars.Count()));

        }
    }
}
