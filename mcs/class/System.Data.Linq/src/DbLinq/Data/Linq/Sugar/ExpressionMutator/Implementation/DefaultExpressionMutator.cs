using DbLinq.Data.Linq.Sugar.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace System.Data.Linq.Sugar.ExpressionMutator.Implementation
{
    class DefaultExpressionMutator : IMutableExpression
    {
        private DefaultExpression expression;

        public DefaultExpressionMutator(DefaultExpression expression)
        {
            this.expression = expression;
        }

        public IEnumerable<Expression> Operands
        {
            get
            {
                return Enumerable.Empty<Expression>();
            }
        }

        public Expression Mutate(IList<Expression> operands)
        {
            return expression;
        }
    }
}
