using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceStack.OrmLite.DB2.Tests.Mappings
{
    [Schema("Drakon")]
    public class Employee
    {
        public string EmpNo { get; set; }
        [Alias("FIRSTNME")]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MidInit { get; set; }

        //[ForeignKey(typeof(Depratment)), OnDelete = "SET NULL"]
        public string WorkDept { get; set; }
        public string PhoneNo { get; set; }
        public DateTime HireDate { get; set; }
        public Job Job { get; set; }
        public Sex Sex { get; set; }
        public int EdLevel { get; set; }
        public DateTime BirthDate { get; set; }
        public Decimal Salary { get; set; }
        public Decimal Bonus { get; set; }
        public Decimal Comm { get; set; }
    }
    [Schema("Drakon")]
    public class Depratment
    {
        public string DeptNo { get; set; }
        public string DeptName { get; set; }
        public string MgrNo { get; set; }
        public string AdmrDept { get; set; }
        public string Location { get; set; }
    }

    public enum Sex
    {
        F,
        M
    }
    public enum Job
    {
        Analyst,
        Clerk,
        Designer,
        FieldRep,
        Manager,
        Operator,
        Pres,
        SalesRep
    }
}
