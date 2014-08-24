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

        private static List<LambdaExpression> lambdas = new List<LambdaExpression>();


        public static Delegate CompileDebuggable(this LambdaExpression lambda)
        {
            return lambda.Compile();
            var modifiedLambda = (LambdaExpression)new ReplaceInMemoryObjectsVisitor(lambda).Visit(lambda);

            lambdas.Add(modifiedLambda);
            var asm = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("Emitted"), AssemblyBuilderAccess.Save, @"c:\temp");
            var mod = asm.DefineDynamicModule("Emitted", @"Emitted.dll");
            var type = mod.DefineType("EmittedCode", TypeAttributes.Public);

            var i = 0;
            foreach (var item in lambdas)
            {


                var met = type.DefineMethod("Lambda_" + i, MethodAttributes.Public | MethodAttributes.Static, item.ReturnType, item.Parameters.Select(x => x.Type).ToArray());
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
            
            if (value != null && !value.GetType().IsPrimitive && !(value is string) && !value.GetType().IsEnum)
            {
                return Expression.Constant(null, node.Type);
            }
            return base.VisitConstant(node);
        }

    }
}
