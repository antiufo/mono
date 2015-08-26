#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DbLinq.Data.Linq.Sugar.ExpressionMutator;
using DbLinq.Data.Linq.Sugar.Expressions;

namespace DbLinq.Data.Linq.Sugar.ExpressionMutator
{
    /// <summary>
    /// Extensions to Expression, to enumerate and dynamically change operands in a uniformized way
    /// </summary>
    internal static class ExpressionMutatorExtensions
    {
        /// <summary>
        /// Enumerates all subexpressions related to this one
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IEnumerable<Expression> GetOperands(this Expression expression)
        {
            if (expression is MutableExpression)
                return new List<Expression>(((MutableExpression)expression).Operands);
            return ExpressionMutatorFactory.GetMutator(expression).Operands;
        }

        public static IEnumerable<Expression> GetOperandsBorrowed(this Expression expression)
        {
            if (expression is MutableExpression)
                return ((MutableExpression)expression).Operands;
            return ExpressionMutatorFactory.GetMutator(expression).Operands;
        }

        /// <summary>
        /// Changes all operands
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="operands"></param>
        /// <param name="checkForChanges"></param>
        /// <returns>A potentially new expression with new operands</returns>
        public static T ChangeOperands<T>(this T expression, IList<Expression> operands, bool checkForChanges)
            where T : Expression
        {
            bool haveOperandsChanged = checkForChanges && HaveOperandsChanged(expression, operands);
            if (!haveOperandsChanged)
                return expression;
            var mutableExpression = expression as IMutableExpression;
            if (mutableExpression != null)
                return (T)mutableExpression.Mutate(operands);
            return (T)ExpressionMutatorFactory.GetMutator(expression).Mutate(operands);
        }

        /// <summary>
        /// Determines if operands have changed for a given expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="operands"></param>
        /// <returns></returns>
        private static bool HaveOperandsChanged<T>(T expression, IList<Expression> operands)
            where T : Expression
        {
            var oldOperands = GetOperandsBorrowed(expression);
            if (operands.Count != oldOperands.Count())
                return true;

            var operandIndex = 0;
            foreach (var old in oldOperands)
            {
                if (operands[operandIndex] != old)
                {
                    return true;
                }
                operandIndex++;
            }
            return false;
        }

        /// <summary>
        /// Changes all operands
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="operands"></param>
        /// <returns>A potentially new expression with new operands</returns>
        public static T ChangeOperands<T>(this T expression, IList<Expression> operands)
            where T : Expression
        {
            return ChangeOperands(expression, operands, true);
        }

        /// <summary>
        /// Changes all operands
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="operands"></param>
        /// <returns>A potentially new expression with new operands</returns>
        public static T ChangeOperands<T>(this T expression, params Expression[] operands)
            where T : Expression
        {
            return ChangeOperands(expression, operands, true);
        }

        /// <summary>
        /// Returns the expression result
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static object Evaluate(this Expression expression)
        {
            var executableExpression = expression as IExecutableExpression;
            if (executableExpression != null)
                return executableExpression.Execute();
            try
            {
                // here, we may have non-evaluable expressions, so we "try"/"catch"
                // (maybe should we find something better)
                var lambda = Expression.Lambda(expression);
                var compiled = lambda.CompileDebuggable();
                var value = compiled.DynamicInvoke();
                return value;
            }
            catch
            {
                throw new ArgumentException();
            }
        }


        public static Expression Recurse(this Expression expression, Func<Expression, Expression> analyzer)
        {
            var baseIndex = NextRecList;
            try
            {

                if (RecList == null) RecList = new List<RecursionList>();
                return expression.RecurseInternal(analyzer);
            }
            finally
            {
                NextRecList = baseIndex;
            }
        }


        /// <summary>
        /// Down-top pattern analysis.
        /// </summary>
        /// <param name="expression">The original expression</param>
        /// <param name="analyzer"></param>
        /// <returns>A new QueryExpression or the original one</returns>
        private static Expression RecurseInternal(this Expression expression, Func<Expression, Expression> analyzer)
        {
            if (RecList.Count == NextRecList) RecList.Add(new RecursionList());
            var newOperands = RecList[NextRecList];
            newOperands._count = 0;
            // first, work on children (down)
            foreach (var operand in GetOperandsBorrowed(expression))
            {
                if (operand != null)
                {
                    NextRecList++;
                    newOperands.Add(RecurseInternal(operand, analyzer));
                    NextRecList--;
                }
                else
                {
                    newOperands.Add(null);
                }
            }
            // then on expression itself (top)
            return analyzer(expression.ChangeOperands(newOperands));
        }


        [ThreadStatic]
        private static List<RecursionList> RecList;
        [ThreadStatic]
        private static int NextRecList;

        internal class RecursionList : IList<Expression>
        {

            public RecursionList()
            {
                Data = new Expression[64];
            }

            public Expression[] Data;
            public int _count;

            public int Count
            {
                get
                {
                    return _count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public Expression this[int index]
            {
                get
                {
                    return Data[index];
                }

                set
                {
                    throw new NotSupportedException();
                }
            }

            public int IndexOf(Expression item)
            {
                return Array.IndexOf(Data, item, 0, _count);
            }

            public void Insert(int index, Expression item)
            {
                throw new NotSupportedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotSupportedException();
            }

            public void Add(Expression item)
            {
                if (Data.Length == _count)
                {
                    Array.Resize(ref Data, _count * 2);
                }
                Data[_count++] = item;
            }

            public void Clear()
            {
                _count = 0;
            }

            public bool Contains(Expression item)
            {
                return IndexOf(item) != -1;
            }

            public void CopyTo(Expression[] array, int arrayIndex)
            {
                for (int i = 0; i < _count; i++)
                {
                    array[arrayIndex++] = Data[i];
                }
            }

            public bool Remove(Expression item)
            {
                throw new NotSupportedException();
            }

            public IEnumerator<Expression> GetEnumerator()
            {
                for (int i = 0; i < _count; i++)
                {
                    yield return Data[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}