//
// MetaModel.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using DbLinq.Data.Linq;
using DbLinq.Data.Linq.Sugar.Expressions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using DbLinq.Data.Linq.Database;
using DbLinq.Data.Linq.Mapping;
using DbLinq.Factory;
using DbLinq.Data.Linq.Sugar;
using DbLinq.Data.Linq.Sugar.Implementation;

namespace System.Data.Linq.Mapping
{
    public abstract class MetaModel
    {
        public abstract Type ContextType { get; }
        public abstract string DatabaseName { get; }
        public abstract MappingSource MappingSource { get; }
        public abstract Type ProviderType { get; }

        public abstract MetaFunction GetFunction(MethodInfo method);
        public abstract IEnumerable<MetaFunction> GetFunctions();
        public abstract MetaType GetMetaType(Type type);
        public abstract MetaTable GetTable(Type rowType);
        public abstract IEnumerable<MetaTable> GetTables();

        public virtual Expression CreateObject(Type type, IEnumerable<FieldAssignment> bindings)
        {
            return null;
        }

        internal MetaDataMember GetMetaDataMember(MemberInfo memberInfo)
        {
            if (memberInfo == null) return null;
            var type = GetMetaType(memberInfo.DeclaringType);
            return type != null ? type.GetDataMember(memberInfo) : null;
        }



        public virtual object GetInputParameterValue(object expr)
        {
            return expr;
        }

        public static Expression GetOutputValueReader(Type columnType, int valueIndex, ParameterExpression dataRecordParameter,
                                                  ParameterExpression mappingContextParameter, DataContext context)
        {
            var disp = context.CustomExpressionDispatcher ?? ObjectFactory.Get<IExpressionDispatcher>();

            return ((ExpressionDispatcher)disp).GetOutputValueReader(columnType, valueIndex, dataRecordParameter, mappingContextParameter, context);
        }
    }
}
