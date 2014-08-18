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
            var modifiedLambda = (LambdaExpression)new ReplaceInMemoryObjectsVisitor().Visit(lambda);

            lambdas.Add(modifiedLambda);
            var asm = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Emitted"), AssemblyBuilderAccess.Save);
            var mod = asm.DefineDynamicModule("Emitted", @"Emitted.dll");
            var type = mod.DefineType("EmittedCode", TypeAttributes.Public);

            var i = 0;
            foreach (var item in lambdas)
            {


                var met = type.DefineMethod("Lambda_" + i, MethodAttributes.Public | MethodAttributes.Static, lambda.ReturnType, lambda.Parameters.Select(x => x.Type).ToArray());
                modifiedLambda.CompileToMethod(met);
                i++;
            }
            type.CreateType();
            asm.Save(@"Emitted");
            return lambda.Compile();
        }
    }

    public class ReplaceInMemoryObjectsVisitor : ExpressionVisitor
    {
        protected override Expression VisitConstant(ConstantExpression node)
        {
            var value = node.Value;
            if (value != null && !value.GetType().IsPrimitive && !(value is string))
            {
                Console.WriteLine(value.GetType());
                return Expression.Constant(null, node.Type);
            }
            return base.VisitConstant(node);
        }
    }
}
