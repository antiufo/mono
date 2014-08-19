using DbLinq.Data.Linq.Sugar.Expressions;
using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DbLinq.Data.Linq
{
    public class FieldAssignment
    {
        public FieldAssignment(ColumnExpression column, MemberInfo memberInfo, MetaDataMember member, Expression expression)
        {
            this.MemberInfo = memberInfo;
            this.Column = column;
            this.DataMember = member;
            this.ParameterColumn = expression;
        }

        public ColumnExpression Column { get; private set; }
        public MetaDataMember DataMember { get; private set; }
        public MemberInfo MemberInfo { get; private set; }
        public Expression ParameterColumn { get; private set; }
    }
}
