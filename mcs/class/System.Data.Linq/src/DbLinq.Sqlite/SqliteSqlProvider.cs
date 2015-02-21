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

using System.Collections.Generic;
using DbLinq.Data.Linq.Sql;
using DbLinq.Data.Linq.Sugar.Expressions;
using DbLinq.Vendor.Implementation;
using System;

namespace DbLinq.Sqlite
{
#if !MONO_STRICT
    public
#endif
    class SqliteSqlProvider : SqlProvider
    {
        public override SqlStatement GetInsertIds(SqlStatement table, IList<SqlStatement> autoPKColumn, IList<SqlStatement> inputPKColumns, IList<SqlStatement> inputPKValues, IList<SqlStatement> outputColumns, IList<SqlStatement> outputParameters, IList<SqlStatement> outputExpressions)
        {
            return "SELECT last_insert_rowid()";
        }

        protected override SqlStatement GetLiteralStringLength(SqlStatement a)
        {
            return SqlStatement.Format("LENGTH({0})", a);
        }

        protected override SqlStatement GetLiteralStringToUpper(SqlStatement a)
        {
            return SqlStatement.Format("UPPER({0})", a);
        }

        protected override SqlStatement GetLiteralStringToLower(SqlStatement a)
        {
            return string.Format("LOWER({0})", a);
        }

        protected override SqlStatement GetLiteralCount(SqlStatement a)
        {
            return "COUNT(*)";
        }

        public override SqlStatement GetLiteral(bool literal)
        {
            if (literal)
                return "1";
            return "0";
        }

        protected override char SafeNameEndQuote
        {
            get
            {
                return '`';
            }
        }

        protected override char SafeNameStartQuote
        {
            get
            {
                return '`';
            }
        }


        protected override SqlStatement GetLiteralDateTimePart(SqlStatement dateExpression, SpecialExpressionType operationType)
        {
            string qualifier;
            switch (operationType)
            {
                case SpecialExpressionType.Year: qualifier = "Y"; break;
                case SpecialExpressionType.Month: qualifier = "m"; break; // Yes M and m have opposite meaning compared to .NET
                case SpecialExpressionType.Day: qualifier = "d"; break;
                case SpecialExpressionType.Hour: qualifier = "H"; break;
                case SpecialExpressionType.Minute: qualifier = "M"; break;
                case SpecialExpressionType.Second: qualifier = "S"; break;
                default: throw new NotSupportedException("Not supported by SQLite: " + operationType);
            }

            return "strftime('%" + qualifier + "', datetime(("+ dateExpression + " - 621355968000000000) / 10000000, 'unixepoch'))";
        }


        protected override SqlStatement GetLiteralDateTimeGranularity(SqlStatement dateExpression, SpecialExpressionType operationType)
        {
            string qualifier;
            switch (operationType)
            {
                case SpecialExpressionType.YearGranularity: qualifier = "%Y-01-01"; break;
                case SpecialExpressionType.MonthGranularity: qualifier = "%Y-%m-01"; break; // Yes M and m have opposite meaning compared to .NET
                case SpecialExpressionType.DayGranularity: qualifier = "%Y-%m-%d"; break;
                case SpecialExpressionType.HourGranularity: qualifier = "%Y-%m-%d %H:00:00"; break;
                case SpecialExpressionType.MinuteGranularity: qualifier = "%Y-%m-%d %H:%M:00"; break;
                case SpecialExpressionType.SecondGranularity: qualifier = "%Y-%m-%d %H:%M:%S"; break;
                default: throw new NotSupportedException("Not supported by SQLite: " + operationType);
            }

            return "(strftime('%s', datetime(strftime('" + qualifier + "', datetime((" + dateExpression + " - 621355968000000000) / 10000000, 'unixepoch')))) * 10000000 + 621355968000000000)";

        }



    }
}
