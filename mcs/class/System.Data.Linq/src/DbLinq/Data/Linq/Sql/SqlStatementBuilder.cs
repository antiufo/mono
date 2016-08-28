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
using DbLinq.Util;
using Shaman.Runtime;

namespace DbLinq.Data.Linq.Sql
{
    /// <summary>
    /// An SqlStatement is a literal SQL request, composed of different parts (SqlPart)
    /// each part being either a parameter or a literal string
    /// </summary>
#if !MONO_STRICT
    public
#endif
    class SqlStatementBuilder
    {
        public readonly List<SqlPart> Parts = new List<SqlPart>();

        /// <summary>
        /// Returns part at given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public SqlPart this[int index]
        {
            get { return Parts[index]; }
        }

        /// <summary>
        /// Creates a new SqlStatement based on the current and appending new SqlParts
        /// </summary>
        /// <param name="newParts"></param>
        /// <returns></returns>
        public void Append(IList<SqlPart> newParts)
        {
            foreach (var part in newParts)
                AddPart(Parts, part);
        }
        

        /// <summary>
        /// Adds the part to the given parts list.
        /// </summary>
        /// <param name="parts">The parts.</param>
        /// <param name="part">The part.</param>
        public static void AddPart(IList<SqlPart> parts, SqlPart part)
        {
            parts.Add(part);
        }

        /// <summary>
        /// Joins statements, separated by a given statement
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="sqlStatements"></param>
        /// <returns></returns>
        public void AppendJoin(SqlStatement sqlStatement, IList<SqlStatement> sqlStatements)
        {
            for (int index = 0; index < sqlStatements.Count; index++)
            {
                if (index > 0)
                    Append(sqlStatement);
                Append(sqlStatements[index]);
            }
        }

        /// <summary>
        /// Creates an SQL statement based on a format string and SqlStatements as arguments
        /// </summary>
        /// <param name="format"></param>
        /// <param name="sqlStatements"></param>
        /// <returns></returns>
        public void AppendFormat(string format, IList<SqlStatement> sqlStatements)
        {
            var beginOfCurrentString = 0;
            int i = 0;
            for (; i < format.Length; i++)
            {
                var ch = format[i];
                if (ch == '{')
                {
                    var end = format.IndexOf('}', i);
                    if (end == -1) throw new FormatException();
                    
                    var num = ValueString.ParseInt32(format.AsValueString().Substring(i + 1, end - i - 1));
                    if (i - beginOfCurrentString != 0)
                    {
                        Append(format.Substring(beginOfCurrentString, i - beginOfCurrentString));
                    }
                    Append(sqlStatements[num]);
                    beginOfCurrentString = end + 1;
                    i = end;
                }
            }
            if (i - beginOfCurrentString != 0)
            {
                Append(format.Substring(beginOfCurrentString, i - beginOfCurrentString));
            }
        }

        /// <summary>
        /// Formats an SqlStatement from a given string format
        /// </summary>
        /// <param name="format"></param>
        /// <param name="sqlStatements"></param>
        /// <returns></returns>
        public void AppendFormat(string format, params SqlStatement[] sqlStatements)
        {
            AppendFormat(format, (IList<SqlStatement>)sqlStatements);
        }

        /// <summary>
        /// Appends a bunch of sqlStatements to the current one
        /// </summary>
        /// <param name="sqlStatements"></param>
        /// <returns></returns>
        public void Append(IList<SqlStatement> sqlStatements)
        {
            foreach (var sqlStatement in sqlStatements)
            {
                foreach (var sqlPart in sqlStatement)
                {
                    AddPart(Parts, sqlPart);
                }
            }
        }

#if UNTESTED

        /// <summary>
        /// Inserts statements at given position
        /// </summary>
        /// <param name="index"></param>
        /// <param name="sqlStatements"></param>
        public void Insert(int index, IList<SqlStatement> sqlStatements)
        {
            for (int statementIndex = sqlStatements.Count - 1; statementIndex >= 0; statementIndex--)
            {
                var sqlStatement = sqlStatements[statementIndex];
                for (int partIndex = sqlStatement.Count - 1; partIndex >= 0; partIndex++)
                {
                    var sqlPart = sqlStatement[partIndex];
                    InsertPart(Parts, index, sqlPart);
                }
            }
        }

        /// <summary>
        /// Inserts statements at given position
        /// </summary>
        /// <param name="index"></param>
        /// <param name="sqlStatements"></param>
        public void Insert(int index, params SqlStatement[] sqlStatements)
        {
            Insert(index, (IList<SqlStatement>)sqlStatements);
        }

#endif

        /// <summary>
        /// Appends sqlStatements to the current one
        /// </summary>
        /// <param name="newStatements"></param>
        /// <returns></returns>
        public void Append(params SqlStatement[] newStatements)
        {
            Append((IList<SqlStatement>)newStatements);
        }

        public void Append(SqlStatement newStatement)
        {
            foreach (var sqlPart in newStatement)
            {
                AddPart(Parts, sqlPart);
            }
        }

        /// <summary>
        /// Replaces the specified text, optionally ignoring the case.
        /// The method does not replace cross-parts text
        /// </summary>
        /// <param name="oldText">The old text.</param>
        /// <param name="newText">The new text.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        public void Replace(string oldText, string newText, bool ignoreCase)
        {
            for (int partIndex = 0; partIndex < Parts.Count; partIndex++)
            {
                var part = Parts[partIndex];
                if (part.Sql.ContainsCase(oldText, ignoreCase))
                {
                    // we know how to process only on literal strings
                    if (part is SqlLiteralPart)
                    {
                        Parts[partIndex] = new SqlLiteralPart(part.Sql.ReplaceCase(oldText, newText, ignoreCase));
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlStatementBuilder"/> class.
        /// </summary>
        public SqlStatementBuilder()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlStatementBuilder"/> class.
        /// </summary>
        /// <param name="sqlStatements">The SQL statements.</param>
        public SqlStatementBuilder(params SqlStatement[] sqlStatements)
        {
            Append(sqlStatements);
        }

        /// <summary>
        /// Gets the built SqlStatement.
        /// </summary>
        /// <returns></returns>
        public SqlStatement ToSqlStatement()
        {
            return new SqlStatement(Parts);
        }
    }
}
