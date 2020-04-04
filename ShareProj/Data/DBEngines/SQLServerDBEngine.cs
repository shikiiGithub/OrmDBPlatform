﻿﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Data;
using dotNetLab.Data.Orm;
using dotNetLab.Common;

namespace dotNetLab.Data 
{
    public class SqlServerDBEngine : DBPlatform,  ITableInfo,  IHugeDB
    {
        public readonly String SQLSERVERCONNECTIONSTRING = "server={0},{1};Initial Catalog={2};User ID={3};Password ={4}";
        public readonly String LOCALDBCONNECTIONSTRING = "server=(localdb)\\MSSQLLocalDB;AttachDBFilename={0} ;";
        public static Assembly asm_SQLSEVER = null;

        public SqlServerDBEngine()
        {
             
        }
        public static void CopyMasterDB(String copyToDir, String dbName = "shikii")
        {
            String path = @"C:\Users\{0}\AppData\Local\Microsoft\Microsoft SQL Server Local DB\Instances\MSSQLLocalDB\master.mdf";
            path = String.Format(path, Environment.UserName);
            File.Copy(path, Path.Combine(copyToDir, dbName + ".mdf"));
            path = String.Format(@"C:\Users\{0}\AppData\Local\Microsoft\Microsoft SQL Server Local DB\Instances\MSSQLLocalDB\mastlog.ldf", Environment.UserName);
            //mastlog.ldf
            File.Copy(path, Path.Combine(copyToDir, dbName + ".ldf"));
        }

        protected virtual DbConnection GetDbConnection( )
       {
           try
            {
                object obj = asm_SQLSEVER.CreateInstance("System.Data.SqlClient.SqlConnection");
              DbConnection  ThisDbConnection = obj as DbConnection;
              

               return ThisDbConnection;
           }
           catch (Exception ex)
           {
               PerformErrorHandler(this, ex);
               return null;
           }

       }

        /// <summary>
        /// 连接到LocalDB
        /// </summary>
        /// <param name="DBFilePath">数据库文件路径</param>
        /// <returns></returns>
        public bool Connect(String DBFilePath)
        {
            try
            {
                DbConnection args = GetDbConnection();
                String strCmpactDBFilePath = DBFilePath;
                args .ConnectionString = String.Format(LOCALDBCONNECTIONSTRING, DBFilePath);
                bool b = Connect(args);
                if (!b)
                {
                    Console.WriteLine("未能连接数据库");
                  
                }
                return b;

            }
            catch (Exception exx)
            {

                PerformErrorHandler(this, exx);

                return false;
            }

        }

        /// <summary>
        /// 连接到SQL Server （localhost）
        /// </summary>
        public bool Connect(string DBName, string UserName, string Pwd)
        {
            if (UserName == null || UserName == "")
            {
                return Connect(DBName);
            }

            DbConnection args = GetDbConnection();
          
            args .ConnectionString = String.Format(SQLSERVERCONNECTIONSTRING, "localhost", 1433, "master", UserName, Pwd);
    
            bool b = this.Connect(args);
            if (!b)
            {
                Console.WriteLine("未能连接数据库");
                
            }
             
            return b;
        }
        /// <summary>
        /// 连接到SQL Server （ip,port）
        /// </summary>
        public bool Connect(string ip, int port, string DBName, string UserName, string Pwd)
        {
            DbConnection args = GetDbConnection();
            args. ConnectionString = String.Format(SQLSERVERCONNECTIONSTRING, ip, port, "master", UserName, Pwd);
            bool b = this.Connect(args);
            if (!b)
            {
                Console.WriteLine("未能连接数据库");
                args = null;
            }
            else
            {
                this.ExecuteNonQuery(String.Format("use {0};", DBName));
            }
            return b;
        }

        public void AddColumn(string tableName, string ColumnName, string FieldType)
        {
            String sql = String.Format("ALTER TABLE {0} ADD {1} {2}", tableName, ColumnName, FieldType);
            this.ExecuteNonQuery(sql);
        }
        /// <summary>
        /// 修改列名
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="colName_Old"></param>
        /// <param name="colName_New"></param>
        /// <param name="ColTypeDefine">ColTypeDefine 始终为空</param>
        /// <param name="args"></param>

        public void ChangeColumnName(string tableName, string colName_Old, string colName_New, string ColTypeDefine)
        {
            String sql = String.Format("EXEC sp_rename '{0}.{1}', '{2}' , 'COLUMN';", tableName, colName_Old, colName_New);
            this.ExecuteNonQuery(sql);
        }

        public void ChangeColumnType(string tableName, string colName, string ColTypeDefine )
        {
            String sql = String.Format("ALTER TABLE {0} ALTER COLUMN {1} {2}", tableName, colName, ColTypeDefine);
            this.ExecuteNonQuery(sql );
        }



        public void DropColumn(string tableName, string columnName )
        {
            String sql = String.Format("ALTER TABLE {0} DROP COLUMN {1}", tableName, columnName);
            this.ExecuteNonQuery(sql );
        }

        public List<string> GetAllTableNames( )
        {
            List<String> AllTableNames = new List<string>();
            string DBName = ThisDbPipeInfo.MainDbConnection.Database;
            DataTable dt = this.ProvideTable(String.Format("select Table_Name from information_schema.TABLES where table_catalog = '{0}'", DBName) );

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String strTableName = dt.Rows[i][0].ToString();

                AllTableNames.Add(strTableName);
            }
            return AllTableNames;

        }

        public List<string> GetAllViewNames( )
        {
            throw new NotImplementedException();
        }

        public List<String> GetAllDBNames( )
        {
            
            List<string> AllDBNames = new List<string>();
            

            DataTable dt = this.ProvideTable("SELECT  SCHEMA_NAME  FROM information_schema.SCHEMATA"  );
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String strTableName = dt.Rows[i][0].ToString();
                AllDBNames.Add(strTableName);
               
            }
            return AllDBNames;
        }
        public List<string> GetTableColumnNames(string tableName )
        {
            List<String> AllColumnNames = new List<string>();
            string DBName = ThisDbPipeInfo.MainDbConnection.Database ;

           
            DataTable dt = this.ProvideTable(String.Format("select COLUMN_NAME from information_schema.COLUMNS where table_name = '{0}' and table_catalog ='{1}';", tableName, DBName) );

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                AllColumnNames.Add(dt.Rows[i][0].ToString());
            }
            return AllColumnNames;
        }

        public List<string> GetTableColumnTypes(string tableName, bool isRawSqlType = false )
        {

            List<String> ColumnTypes = new List<string>();
            string DBName = ThisDbPipeInfo.MainDbConnection.Database ;

            DataTable dt = this.ProvideTable(String.Format("select data_type,CHARACTER_MAXIMUM_LENGTH from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME ='{0}' and TABLE_CATALOG='{1}'", tableName, DBName) );
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String str = dt.Rows[i][0].ToString().ToLower();

                if (!isRawSqlType)
                    str = this.TypeInfer(str);
                else
                {

                    if (str.Contains("char"))
                    {
                        int charNum = Convert.ToInt32(dt.Rows[i][1]);
                        str = String.Format("{0}({1})", str, charNum);
                    }
                }

            }
            return ColumnTypes;
        }

        public string InferDataType(PropertyInfo pif, string csType)
        {


            switch (csType)
            {


                case "Byte": return "tinyint";
                case "Int16": return "smallint";
                case "Int32": return "int";
                case "Int64": return "bigint";
                case "String":
                    Attribute[] attributes = Attribute.GetCustomAttributes(pif);
                    Attribute attribute = null;
                    if (attributes != null && attributes.Length > 1)
                    {
                        for (int i = 0; i < attributes.Length; i++)
                        {
                            if (attributes[i] is SQLServerTextTypeAttribute)
                            {
                                attribute = attributes[i];
                                break;
                            }
                        }
                    }
                    Attribute textLengthAttribute = attribute;
                    if (textLengthAttribute != null)
                    {
                        return (textLengthAttribute as DBTypeAttribute).DataType;
                    }
                    else { return SQLServerTextTypeAttribute.VERYSHORTTEXT; }
                case "DateTime": return "DateTime";
                case "Single": return "float(24)";
                case "Double": return "float(53)";
                case "Decimal": return "DECIMAL";

                /*Sql 数据库里的Bit类型 读出来是 True 或者 False
而写入数据库 则是 0 和1 1表式true 0表式false
判断一下啊，如果条件成立 数据库里的bit字段就等于1 否则等于0 不就行了！*/
                case "Boolean": return "TINYINT";

            }

            return null;
        }

        public bool BootServer(string ServerDirPath)
        {
            return true;
        }

        public void ShutDownServer(string ServerDirPath)
        {
            
        }
    }
}
