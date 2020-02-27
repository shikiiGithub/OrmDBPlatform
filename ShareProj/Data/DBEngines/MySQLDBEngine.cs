﻿﻿using dotNetLab.Common;
using dotNetLab.Data.Orm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
 
 
namespace dotNetLab.Data 
{
    //Mysql x86 msi下载地址 https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.62-win32.msi
    //Mysql x64 msi下载地址 https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.62-winx64.msi
    //Mysql x86 zip下载地址 https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.62-win32.zip
    //Mysql x64 zip下载地址 https://dev.mysql.com/get/Downloads/MySQL-5.5/mysql-5.5.62-winx64.zip
    public class MySQLDBEngine : DBPlatform, ITableInfo, IHugeDB 
    {
        //连接本地,请注意可能需要指定this.DbInteractiveArgs = {返回值}
        public bool Connect(string dbName, string userName, string pwd)
        {
            String localhostConstr = "server =localhost; database ={0}; user={1}; password = {2};Charset=utf8;pooling=True;minpoolsize=1;maxpoolsize=100";

            String connectstring = String.Format(localhostConstr,"mysql" ,userName, pwd);
            DbConnection dbconnection = GetDbConnection();
            dbconnection.ConnectionString = connectstring;
            if (!this.Connect(dbconnection))
                return false;
            List<String> allDBNames = this.GetAllDBNames() ;
                bool b = allDBNames.Contains(dbName.ToLower());
                if (!b)
                    NewDB(dbName );

                ThisDbPipeInfo.Dispose();
                ThisDbPipeInfo = null;
                connectstring = String.Format(localhostConstr,dbName, userName, pwd);
                dbconnection = GetDbConnection();
                dbconnection.ConnectionString = connectstring;
                b = this.Connect(dbconnection);
                 return b;
        }

        //远程连接，请注意可能需要指定this.DbInteractiveArgs = {返回值}
        public bool Connect(string ip, int port, string dbName, string userName, string pwd)
        {
            String connectstring = String.Format(
                "server={0};user={1};database=mysql;port={2};password={3};Charset=utf8;"
                , ip, userName, port, pwd);

            DbConnection dbconnection = GetDbConnection();
            dbconnection.ConnectionString = connectstring;
            bool bconn = this.Connect(dbconnection);
            if (bconn)
            {
                List<String> allDBNames = this.GetAllDBNames() ;
                bool b = allDBNames.Contains(dbName.ToLower());
                if (!b)
                    NewDB(dbName);
                ThisDbPipeInfo.Dispose();
                ThisDbPipeInfo = null;
                connectstring = String.Format("server ={0}; database ={1}; user={2}; password = {3};Charset=utf8", ip,
                    dbName, userName, pwd);
                dbconnection = GetDbConnection();
                dbconnection.ConnectionString = connectstring;
                bconn = this.Connect(dbconnection);
            }

            return bconn;
        }
        //获得一个DbInteractiveArgs（未连接）
        private  DbConnection GetDbConnection()
        {
            if (!File.Exists("MySql.Data.dll"))
            {
                PerformErrorHandler(this, new Exception("未能找到Mysql.Data.dll"));
            }
            object obj = this.GetReflectOject("MySql.Data.dll", "MySql.Data.MySqlClient.MySqlConnection");
            DbConnection   ThisDbConnection = obj as DbConnection;
            
            return ThisDbConnection;
        }

        public List<String> GetAllTableNames( )
        {
            string DBName = ThisDbPipeInfo.MainDbConnection.Database;
             List<string> AllTableNames = new List<string>();
 
            DataTable dt = this.ProvideTable(String.Format("select Table_Name from information_schema.TABLES where table_schema = '{0}'", DBName) );


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String strTableName = dt.Rows[i][0].ToString();
               // yield return strTableName;
                AllTableNames.Add(strTableName);
            }
           // yield break;
           return AllTableNames;
        }

        public List<String> GetAllDBNames( )
        {
             
             List<string> AllDBNames = new List<string>();
             DataTable dt = this.ProvideTable("SELECT lcase(SCHEMA_NAME) FROM information_schema.SCHEMATA" );


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String strTableName = dt.Rows[i][0].ToString();
                 AllDBNames.Add(strTableName);
               // yield return strTableName;
            }
            return AllDBNames;
        }
        public List<string> GetTableColumnNames(string tableName )
        {
             List<string> AllColumnNames = new List<string>();
            String SQLFormat = "select COLUMN_NAME from information_schema.COLUMNS where table_name = '{0}' and TABLE_SCHEMA ='{1}'; ";
            string DBName = this.ThisDbPipeInfo.MainDbConnection.Database;
           
            DataTable dt = this.ProvideTable(String.Format(SQLFormat, tableName, DBName) );
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String str = dt.Rows[i][0].ToString();
                // yield return str  ;
                AllColumnNames.Add(str);
            }
             return AllColumnNames;
        }
      

        public List<string> GetTableColumnTypes( string tableName, bool isRawSqlType= false  )
        {
            List<string> Types = new List<string>();
            string DBName = this.ThisDbPipeInfo.MainDbConnection.Database;
             
            DataTable dt = this.ProvideTable(String.Format("select data_type,CHARACTER_MAXIMUM_LENGTH from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME ='{0}' and TABLE_SCHEMA='{1}'", tableName, DBName) );
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String str = dt.Rows[i][0].ToString().ToLower();

                if (!isRawSqlType)
                    str = this.TypeInfer(str);
                else
                {

                    if (str.Equals("char")||str.Equals("varchar"))
                    {
                        int charNum = Convert.ToInt32(dt.Rows[i][1]);
                        str = String.Format("{0}({1})",str,charNum);
                    }
                }


                Types.Add(str);

            }
             return Types;
        }
        public List<String> GetAllViewNames( )
        {
            List<string> AllTableNames = new List<string>();
            String strSQL = "select table_Name from INFORMATION_SCHEMA.VIEWS ;";
            DataTable dt = this.ProvideTable(strSQL );

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                AllTableNames.Add(dt.Rows[i][0].ToString());
            }
            return AllTableNames;
        }

        public void ChangeColumnName(string tableName, string colName_Old, string colName_New,string ColTypeDefine )
        {
           String sql = String.Format("alter table {0} change  {1} {2} {3} ",tableName,colName_Old, colName_New,ColTypeDefine);
            ExecuteNonQuery(sql );              
        }

        public void ChangeColumnType(string tableName, string colName , string ColTypeDefine )
        {
            String sql = String.Format("alter table {0} change  {1} {2} {3} ", tableName, colName, colName, ColTypeDefine);
            ExecuteNonQuery(sql );
        }

        public void AddColumn(string tableName, String ColumnName, string FieldType )
        {
            ExecuteNonQuery(String.Format("alter table {0} add column {1} {2} "
                , tableName, ColumnName, FieldType
                ) );
        }

        public void DropColumn(string tableName, string columnName )
        {
            this.ExecuteNonQuery(String.Format("alter table {0} drop column {1}", tableName, columnName) );
        }

        public string InferDataType(PropertyInfo pif,string csType)
        {

            switch (csType)
            {

                case "Char": return "TINYINT";
                case "Byte": return "TINYINT UNSIGNED";
                case "Int16": return "SMALLINT";
                case "UInt16": return "SMALLINT UNSIGNED";
                case "Int32": return "int";
                case "Int64": return "bigint";
                case "UInt32": return "int UNSIGNED";
                case "UInt64": return "bigint UNSIGNED";
                case "String":

                    //Attribute[] attributes = Attribute.GetCustomAttributes(pif, typeof(MysqlTextTypeAttribute));
                    //DBTypeAttribute textLengthAttribute = attributes == null ? null : attributes[0] as DBTypeAttribute;
                    Attribute[] attributes = Attribute.GetCustomAttributes(pif);
                    Attribute attribute = null;
                    if (attributes != null && attributes.Length > 1)
                    {
                        for (int i = 0; i < attributes.Length; i++)
                        {
                            if (attributes[i] is MysqlTextTypeAttribute)
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
                    else {return MysqlTextTypeAttribute.VERYSHORTTEXT; }
                case "DateTime":return "DateTime";
                case "Single": return "float";
                case "Double": return "double";
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
            // Process.GetProcessesByName()
            Process[] ps = Process.GetProcessesByName("mysqld");
            if (ps != null && ps.Length > 0)
                return true;
            List<String> lst = new List<string>();

            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序

            //向cmd窗口发送输入信息
            p.StandardInput.WriteLine(String.Format("{0}\\bin\\mysqld.exe \"--defaults-file={0}\\my.ini\" --console&exit", ServerDirPath));
            p.StandardInput.AutoFlush = true;
            ps = Process.GetProcessesByName("mysqld");
            if (ps != null && ps.Length > 0)
                return true;
            else
                return false;
        }

        public void ShutDownServer(string ServerDirPath)
        {
           
        }
    }
}
