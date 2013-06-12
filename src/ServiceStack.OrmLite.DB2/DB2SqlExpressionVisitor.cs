using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ServiceStack.OrmLite.DB2
{
    public class DB2SqlExpressionVisitor<T> : SqlExpressionVisitor<T>
    {
        public override string LimitExpression
        {
            get
            {
                return "";
            }
        }
       

        protected override string ApplyPaging(string sql)
        {
            if (!Rows.HasValue)
                return sql;
            if (!Skip.HasValue)
            {
                Skip = 0;
            }            

            int index = sql.IndexOf("SELECT");
            
            int fromIndex = sql.IndexOf("FROM");

            string subs = sql.Substring(6, fromIndex - 6);

            //sql = UpdateWithOrderByIfNecessary(sql);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SELECT * FROM (");
            ////sb.AppendLine("SELECT \"_ss_ormlite_1_\".*, ROWNUM RNUM FROM (");
            sb.AppendLine("SELECT ROW_NUMBER() OVER() as ROWNUM, ");
            sb.AppendLine(subs);
            sb.AppendLine(sql.Substring(fromIndex));
            //sb.Append(sql);
            sb.AppendFormat(") WHERE ROWNUM BETWEEN {0} AND {1}",Skip.Value,Rows.Value);
            //sb.AppendFormat("WHERE ROWNUM <= {0} + {1}) \"_ss_ormlite_2_\" ", Skip.Value, Rows.Value);
            //sb.AppendFormat("WHERE \"_ss_ormlite_2_\".RNUM > {0}", Skip.Value);

            return sb.ToString();
        }        
    }
}
