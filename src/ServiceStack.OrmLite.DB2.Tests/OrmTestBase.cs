using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceStack.OrmLite.DB2.Tests
{
    public abstract class OrmTestBase
    {
        protected string ConnectionString = "Server=localhost:50000;Database=SAMPLE;UID=admin;PWD=fruy4brEC;";
        protected OrmLiteConnectionFactory factory;

        public OrmTestBase()
        {
            OrmLiteConfig.DialectProvider = DB2OrmLiteDialectProvider.Instance;
        }
    }
}
