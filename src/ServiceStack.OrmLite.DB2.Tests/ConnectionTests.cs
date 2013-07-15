using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;

namespace ServiceStack.OrmLite.DB2.Tests
{
    [TestClass]
    public class ConnectionTests : OrmTestBase
    {                
        [TestMethod]
        public void Connection()
        {
            IDbConnection conn;

            using (conn = ConnectionString.OpenDbConnection())
            {
                Assert.IsTrue(conn.State == ConnectionState.Open);
            }
            
            Assert.IsTrue(conn.State == ConnectionState.Closed);
        }
    }
}
