using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.DB2.Tests.Mappings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ServiceStack.OrmLite.DB2.Terminal
{
    class Program
    {
        protected static string ConnectionString = "Server=192.168.141.137:50000;Database=SAMPLE;UID=admin;PWD=fruy4brEC;";
        protected OrmLiteConnectionFactory factory;
        private static object Employee;

        static void Main(string[] args)
        {
           OrmLiteConfig.DialectProvider = DB2OrmLiteDialectProvider.Instance;
           using (IDbConnection conn = ConnectionString.OpenDbConnection())
           {
               //var emp = conn.Select<Employee>(q=>q.Sex == Sex.F);

               //string lastSql = conn.GetLastSql();
               try
               {
                   BatchLog log = conn.Id<BatchLog>(1);
                   log.BatchStart = DateTime.Now;
                   //log.Parameters = "TCS4";

                   conn.Save(log);

                   //BatchLog log = BatchLog.Default;
                   //log.BatchType = "TCS";
                   //log.Parameters = "TCS4";

                   //conn.Insert(log);
               }
               catch (Exception e)
               {
                   var t = e;
               }
               //conn.InsertParam(new Employee { EmpNo = "00035", FirstName = "Jo", LastName = "D" });

               //if (!conn.TableExists("Fake"))
               //{
               //    conn.CreateTable<Fake>();
               //}
               //DateTime val = conn.SqlScalar<DateTime>("select current timestamp from sysibm.sysdummy1");

               //conn.Insert(new Employee { EmpNo = "00035", FirstName = "Jo", LastName = "D" });

               //var list = conn.SelectParam<Employee>(q => q.EmpNo == "000340");


               //var list = conn.Select<Employee>(q => q.FirstName.StartsWith("b"));
               //var list2 = conn.Select<Employee>(q=>Sql.In(q.FirstName,"BRUCE"));
               //var list3 = conn.Select<Employee>(q => q.HireDate > DateTime.Now.AddYears(-10));
               //var list4 = conn.Select<Employee>(q => q.LastName.Contains("WA") && q.FirstName == "SALLY");

               //var gosc = conn.Select<Employee>(q => q.Salary == 100000).First();
               //gosc.PhoneNo = "6617";
               //gosc.Salary = 100001;

               //using (var trans = conn.OpenTransaction(IsolationLevel.ReadCommitted))
               //{
               //    //conn.Update(new Employee { }, p=>p.Salary );
               //    conn.UpdateOnly(gosc,f=>new { f.PhoneNo , f.Salary}, p=>p.EmpNo == gosc.EmpNo);
               //    //conn.UpdateOnly(new Employee { Salary = 99000 }, p => p.Salary, p => p.LastName == "THOMPSON");
               //    var sql = conn.GetLastSql();
               //    trans.Commit();
               //}

               //conn.Insert(new Employee
               //{
               //    EmpNo = "000067",
               //    FirstName = "Drakon",
               //    LastName = "Drakonis",
               //    PhoneNo = "6666",
               //    EdLevel = 15
               //});

               //var sql = conn.GetLastSql();
               //conn.UpdateOnly(new Employee { WorkDept = "E66" }, p => p.WorkDept, p => p.FirstName.Contains("Drakon"));

               
           }

           Expression<Func<Employee,bool>> expression = q=>q.Sex == Sex.M;

           var func = expression.Compile(); 
        }
    }

    [Schema("Admin")]
    public class BatchLog
    {
        [PrimaryKey]
        [Sequence("BATCHLOGID_SEQ")]
        public int BatchId { get; set; }
        public string BatchType { get; set; }
		public DateTime BatchStart { get; set; }
		public DateTime? BatchEnd  { get; set; }
		public int DocumentsProcessed { get; set; }
		public int DocumentsWithError { get; set; }
		public string Status { get; set; }
		public string StatusMessage { get; set; }
		public string Parameters { get; set; }

        public static BatchLog Default
        {
            get { return new BatchLog() { BatchStart = DateTime.Now, BatchEnd = null, Status = "RUNNING", StatusMessage = "RUNNING" }; }
        }
    }

    [Schema("Drakon")]
    public class Fake
    {
        [Alias("PK_ID")]
        [PrimaryKey()] 
        public string Id { get; set; }

        [ForeignKey(typeof(Fakes), OnDelete="CASCADE")]
        public string Fid { get; set; }
    }

    public class Fakes
    {
        [Alias("PK_ID")]
        [PrimaryKey()]
        public string Id { get; set; }
    }


}
