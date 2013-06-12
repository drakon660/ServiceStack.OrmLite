using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceStack.OrmLite.DB2
{
    public class DB2Dialect
    {
        public static IOrmLiteDialectProvider Provider { get { return DB2OrmLiteDialectProvider.Instance; } }
    }
}
