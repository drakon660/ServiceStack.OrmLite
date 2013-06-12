using IBM.Data.DB2;
using ServiceStack.Common.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using ServiceStack.Common;
using System.Globalization;
using System.Threading;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.DB2
{
    public class DB2OrmLiteDialectProvider : OrmLiteDialectProviderBase<DB2OrmLiteDialectProvider>
    {
        public static DB2OrmLiteDialectProvider Instance = new DB2OrmLiteDialectProvider();

        public override System.Data.IDbConnection CreateConnection(string connectionString, Dictionary<string, string> options)
        {
            if (options != null)
            {
                foreach (var option in options)
                {
                    connectionString += option.Key + "=" + option.Value + ";";
                }
            }

            return new DB2Connection(connectionString);
        }
        
        internal long LastInsertId { get; set; }

        public override long GetLastInsertId(IDbCommand dbCmd)
        {
            return LastInsertId;
        }

        private string Quote(string name)
        {
            return string.Format("{0}", name);                
        }

        public override string GetQuotedTableName(ModelDefinition modelDef)
        {
            if (!modelDef.IsInSchema)
                return Quote(NamingStrategy.GetTableName(modelDef.ModelName));

            return Quote(string.Format("{0}.{1}", modelDef.Schema,
                NamingStrategy.GetTableName(modelDef.ModelName)));
        }      

        public override string GetQuotedColumnName(string columnName)
        {
            return Quote(columnName);
        }

        public override string ToSelectStatement(Type tableType, string sqlFilter, params object[] filterParams)
        {
            StringBuilder sql = new StringBuilder();
            const string SelectStatement = "SELECT ";
            ModelDefinition modelDef = GetModel(tableType);
            var isFullSelectStatement =
                !string.IsNullOrEmpty(sqlFilter)
                && sqlFilter.Trim().Length > SelectStatement.Length
                && sqlFilter.Trim().Substring(0, SelectStatement.Length).ToUpper().Equals(SelectStatement);

            if (isFullSelectStatement) return sqlFilter.SqlFormat(filterParams);

            sql.AppendFormat("SELECT {0} \nFROM {1} ",
                             GetColumnNames(modelDef),
                             GetQuotedTableName(modelDef));

            if (!string.IsNullOrEmpty(sqlFilter))
            {
                sqlFilter = sqlFilter.SqlFormat(filterParams);
                if (!sqlFilter.StartsWith("\nORDER ", StringComparison.InvariantCultureIgnoreCase)
                    && !sqlFilter.StartsWith("\nROWS ", StringComparison.InvariantCultureIgnoreCase)) // ROWS <m> [TO <n>])
                {
                    sql.Append("\nWHERE ");
                }
                sql.Append(sqlFilter);
            }
            
            return sql.ToString();
        }

        public override bool DoesTableExist(IDbCommand dbCmd, string tableName)
        {
            //if (!QuoteNames & !RESERVED.Contains(tableName.ToUpper()))
            //{
            //    tableName = tableName.ToUpper();
            //}

            var sql = "SELECT count(*) FROM SYSCAT.TABLES where TABNAME={0}".SqlFormat(tableName);

            dbCmd.CommandText = sql;
            var result = dbCmd.GetLongScalar();

            return result > 0;
        }

        public override bool DoesSequenceExist(IDbCommand dbCmd, string sequencName)
        {
            //if (!QuoteNames & !RESERVED.Contains(sequencName.ToUpper()))
            //{
            //    sequencName = sequencName.ToUpper();
            //}

            var sql = "SELECT count(*) FROM SYSCAT.SEQUENCES WHERE SEQNAME = {0}".SqlFormat(sequencName);
            dbCmd.CommandText = sql;
            var result = dbCmd.GetLongScalar();
            return result > 0;
        }
        
        private object GetNextValue(IDbCommand dbCmd, string schema, string sequence, object value)
        {
            Object retObj;

            if (value.ToString() != "0")
            {
                long nv;
                if (long.TryParse(value.ToString(), out nv))
                {
                    LastInsertId = nv;
                    retObj = LastInsertId;
                }
                else
                {
                    LastInsertId = 0;
                    retObj = value;
                }
                return retObj;

            }
            
            var sql = string.Format("SELECT NEXTVAL FOR {0} FROM SYSIBM.SYSDUMMY1", Quote(string.Format("{0}.{1}",schema,sequence)));
            dbCmd.CommandText = sql;
            var result = dbCmd.GetLongScalar();

            LastInsertId = result;
            return result;
        }

        public override string ToInsertRowStatement(object objWithProperties, IList<string> insertFields, IDbCommand dbCommand)
        {
            var sbColumnNames = new StringBuilder();
            var sbColumnValues = new StringBuilder();

            var tableType = objWithProperties.GetType();
            var modelDef = GetModel(tableType);

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {

                if (fieldDef.IsComputed) continue;
                if (insertFields.Count > 0 && !insertFields.Contains(fieldDef.Name) && fieldDef.Sequence.IsNullOrEmpty()) continue;

                if ((fieldDef.AutoIncrement || !string.IsNullOrEmpty(fieldDef.Sequence)
                    || fieldDef.Name == OrmLiteConfig.IdField)
                    && dbCommand != null)
                {

                    if (fieldDef.AutoIncrement && string.IsNullOrEmpty(fieldDef.Sequence))
                    {
                        fieldDef.Sequence = Sequence(
                            (modelDef.IsInSchema
                                ? modelDef.Schema + "_" + NamingStrategy.GetTableName(modelDef.ModelName)
                                : NamingStrategy.GetTableName(modelDef.ModelName)),
                            fieldDef.FieldName, fieldDef.Sequence);
                    }

                    PropertyInfo pi = tableType.GetProperty(fieldDef.Name,
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);                    

                    var result = GetNextValue(dbCommand, modelDef.Schema, fieldDef.Sequence, pi.GetValue(objWithProperties, new object[] { }));
                    if (pi.PropertyType == typeof(String))
                        ReflectionUtils.SetProperty(objWithProperties, pi, result.ToString());
                    else if (pi.PropertyType == typeof(Int16))
                        ReflectionUtils.SetProperty(objWithProperties, pi, Convert.ToInt16(result));
                    else if (pi.PropertyType == typeof(Int32))
                        ReflectionUtils.SetProperty(objWithProperties, pi, Convert.ToInt32(result));
                    else if (pi.PropertyType == typeof(Guid))
                        ReflectionUtils.SetProperty(objWithProperties, pi, result);
                    else
                        ReflectionUtils.SetProperty(objWithProperties, pi, Convert.ToInt64(result));
                }

                if (sbColumnNames.Length > 0) sbColumnNames.Append(",");
                if (sbColumnValues.Length > 0) sbColumnValues.Append(",");

                try
                {
                    sbColumnNames.Append(string.Format("{0}", GetQuotedColumnName(fieldDef.FieldName)));
                    if (!string.IsNullOrEmpty(fieldDef.Sequence) && dbCommand == null)
                        sbColumnValues.Append(string.Format("@{0}", fieldDef.Name));
                    else
                        sbColumnValues.Append(fieldDef.GetQuotedValue(objWithProperties));
                }
                catch (Exception)
                {
                    throw;
                }
            }

            var sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2}) ",
                                    GetQuotedTableName(modelDef), sbColumnNames, sbColumnValues);

            return sql;
        }

        public override string GetQuotedValue(object value, Type fieldType)
        {
            if (value == null) return "NULL";


            if (fieldType == typeof(DateTime) || fieldType == typeof(DateTime?))
            {               
                var dateValue = (DateTime)value;
                string db224Format = "YYYY-MM-DD HH24:MI:SS.FF3";
                string iso8601Format = "yyyy-MM-dd HH:mm:ss.fff";
                return "TIMESTAMP_FORMAT(" + base.GetQuotedValue(dateValue.ToString(iso8601Format), typeof(string)) + ", " + base.GetQuotedValue(db224Format, typeof(string)) + ")";
            }

            if (!fieldType.UnderlyingSystemType.IsValueType && fieldType != typeof(string))
            {
                if (TypeSerializer.CanCreateFromString(fieldType))
                {
                    return OrmLiteConfig.DialectProvider.GetQuotedParam(TypeSerializer.SerializeToString(value));
                }

                throw new NotSupportedException(
                    string.Format("Property of type: {0} is not supported", fieldType.FullName));
            }

            if (fieldType == typeof(int))
                return ((int)value).ToString(CultureInfo.InvariantCulture);

            if (fieldType == typeof(float))
                return ((float)value).ToString(CultureInfo.InvariantCulture);

            if (fieldType == typeof(double))
                return ((double)value).ToString(CultureInfo.InvariantCulture);

            if (fieldType == typeof(decimal))
                return ((decimal)value).ToString(CultureInfo.InvariantCulture);

            return ShouldQuoteValue(fieldType)
                    ? OrmLiteConfig.DialectProvider.GetQuotedParam(value.ToString())
                    : value.ToString();                    
        }

        private string Sequence(string modelName, string fieldName, string sequence)
        {
            return sequence.IsNullOrEmpty()
                ? Quote(modelName + "_" + fieldName + "_GEN")
                : Quote(sequence);
        }

        public override string ToDeleteRowStatement(object objWithProperties)
        {
            var tableType = objWithProperties.GetType();
            var modelDef = GetModel(tableType);

            var sqlFilter = new StringBuilder();

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                try
                {
                    if (fieldDef.IsPrimaryKey || fieldDef.Name == OrmLiteConfig.IdField)
                    {
                        if (sqlFilter.Length > 0) sqlFilter.Append(" AND ");
                        sqlFilter.AppendFormat("{0} = {1}",
                            GetQuotedColumnName(fieldDef.FieldName),
                            fieldDef.GetQuotedValue(objWithProperties));
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

            var deleteSql = string.Format("DELETE FROM {0} WHERE {1}",
                GetQuotedTableName(modelDef), sqlFilter);

            return deleteSql;
        }

        public override SqlExpressionVisitor<T> ExpressionVisitor<T>()
        {
            return new DB2SqlExpressionVisitor<T>();
        }
    }
}
