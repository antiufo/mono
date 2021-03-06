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

using DbLinq.Util;
using DbLinq.Data.Linq.Database;
using System.Collections.Generic;

using DbLinq.Data.Linq.Sugar.Expressions;
using DbLinq.Data.Linq.Sql;
using System.Threading.Tasks;

#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
using System.Data;
using System.Data.Common;
#endif

namespace DbLinq.Data.Linq.Sugar
{
    /// <summary>
    /// Base class for all query types (select, insert, update, delete, raw)
    /// </summary>
    internal abstract class AbstractQuery
    {
        public abstract Task<ITransactionalCommand> GetCommandAsync(bool synchronous);

        protected Task<ITransactionalCommand> GetCommandTrAsync(bool createTransaction, bool synchronous)
        {
            return DbLinq.Data.Linq.Database.Implementation.TransactionalCommand.CreateAsync(Sql.ToString(), createTransaction, DataContext, QueryContext.Transaction != null ? QueryContext.Transaction.Connection : null, synchronous);
        }

        /// <summary>
        /// The DataContext from which the request originates
        /// </summary>
        public DataContext DataContext { get; private set; }

        /// <summary>
        /// SQL command
        /// </summary>
        public SqlStatement Sql { get; private set; }

        protected AbstractQuery(DataContext dataContext, SqlStatement sql, QueryContext queryContext)
        {
            DataContext = dataContext;
            Sql = sql;
            QueryContext = queryContext;
        }


        public QueryContext QueryContext { get; private set; }
    }
}
