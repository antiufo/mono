using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class ExpressionExtensions
    {


        public static Delegate CompileDebuggable(this LambdaExpression lambda)
        {
            return lambda.CompileDebuggable(false);
        }

        public static Delegate CompileDebuggable(this LambdaExpression lambda, bool singleUse)
        {
            if (singleUse)
            {
               return (new System.Linq.Expressions.Interpreter.LightCompiler()).CompileTop(lambda).CreateDelegate();
            }
            return lambda.Compile();
#if false
            var modifiedLambda = (LambdaExpression)new ReplaceInMemoryObjectsVisitor(lambda).Visit(lambda);

            lambdas.Add(modifiedLambda);
            var asm = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("Emitted"), AssemblyBuilderAccess.Save, @"c:\temp");
            var mod = asm.DefineDynamicModule("Emitted", @"Emitted.dll");
            var type = mod.DefineType("EmittedCode", TypeAttributes.Public);

            var i = 0;
            foreach (var item in lambdas)
            {


                var met = type.DefineMethod("Lambda_" + i, MethodAttributes.Public | MethodAttributes.Static, item.ReturnType, item.Parameters.SelectToArray(x => x.Type));
                try
                {
                    item.CompileToMethod(met);
                }
                catch (Exception)
                {
                }
                
                i++;
            }
            type.CreateType();
            asm.Save(@"Emitted.dll");
            
            return lambda.Compile();
#endif
        }
    }

    public class ReplaceInMemoryObjectsVisitor : ExpressionVisitor
    {
        private LambdaExpression authorizedLambda;
        public ReplaceInMemoryObjectsVisitor(LambdaExpression authorizedLambda) {
            this.authorizedLambda = authorizedLambda;
        }
        protected override Expression VisitConstant(ConstantExpression node)
        {
            var value = node.Value;
            
            if (value != null && !value.GetType().GetTypeInfo().IsPrimitive && !(value is string) && !value.GetType().GetTypeInfo().IsEnum)
            {
                return Expression.Constant(null, node.Type);
            }
            return base.VisitConstant(node);
        }

    }
}
