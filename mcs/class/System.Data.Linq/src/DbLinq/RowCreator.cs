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
        private bool singleUse;
        public RowCreator(LambdaExpression lambdaSelectExpression, bool singleUse)
        {
            this.lambdaSelectExpression = lambdaSelectExpression;
            this.singleUse = singleUse;
        }

        internal Delegate Compile()
        {
            return compiled ?? (compiled = lambdaSelectExpression.CompileDebuggable(singleUse));
        }
    }
}
