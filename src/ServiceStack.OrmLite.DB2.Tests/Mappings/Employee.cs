using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceStack.OrmLite.DB2.Tests.Mappings
{
    public class Employee
    {
        public string EmpNo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MidInit { get; set; }
        public string WorkDept { get; set; }
        public string PhoneNo { get; set; }
        public DateTime HireDate { get; set; }
        public string Job { get; set; }
        public string Sex { get; set; }
        public DateTime BirthDate { get; set; }
        public Decimal Salary { get; set; }
        public Decimal Bonus { get; set; }
        public Decimal Comm { get; set; }
    }
}
