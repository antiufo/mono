using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DbLinq
{
    internal class RowCreator
    {
        private LambdaExpression lambdaSelectExpression;
        private Delegate compiled;

        public RowCreator(LambdaExpression lambdaSelectExpression)
        {
            this.lambdaSelectExpression = lambdaSelectExpression;
        }

        internal Delegate Compile()
        {
            return compiled ?? (compiled = lambdaSelectExpression.CompileDebuggable());
        }
    }
}
