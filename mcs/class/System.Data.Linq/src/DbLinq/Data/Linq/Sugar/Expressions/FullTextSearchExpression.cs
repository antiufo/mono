using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DbLinq.Util;
using DbLinq.Data.Linq.Sugar.ExpressionMutator;
using DbLinq.Data.Linq.Sugar.Implementation;
using System.Linq.Expressions;
using System.Diagnostics;

namespace DbLinq.Data.Linq.Sugar.Expressions
{
    /// <summary>
    /// A GroupExpression holds a grouped result
    /// It is usually transparent, except for return value, where it mutates the type to IGrouping
    /// </summary>
    [DebuggerDisplay("FullTextSearchExpression")]
#if !MONO_STRICT
    public
#endif
    class FullTextSearchExpression : MutableExpression
    {
        public const ExpressionType ExpressionType = (ExpressionType)CustomExpressionType.EntitySet;

        public FullTextSearchExpression(string language, string searchTerms, int searchQueryId)
            : base(ExpressionType, typeof(VirtualSearchTable))
        {
            this.Language = language;
            this.SearchTerms = searchTerms;
            this.SearchQueryId = searchQueryId;
        }

        public string Language;
        public string SearchTerms;
        public int SearchQueryId;
    }
}
