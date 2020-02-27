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
    public class PostgreSQLEngine : DBPlatform,    ITableInfo,   IHugeDB
    {
        public readonly String POSTGRESQLCONNECTIONSTRING = "server={0};username={1};database={2};port={3};password={4};Pooling=True;Minimum Pool Size=1;Maximum Pool Size=200";
        //  public readonly String POSTGRESQLCONNECTIONSTRING = "Host=${0};User ID={1};Database={2};Port={3};Password={4};Pooling=true;Min Pool Size=0;Max Pool Size=100;Connection Lifetime=15;Unicode=true;Connection Timeout=30";

        /*
         User ID=${userid};Password=${password};Host=${datasource};Port=${port};Database=${database}; 
            Pooling=true;Min Pool Size=0;Max Pool Size=100;Connection Lifetime=15;Unicode=true;Connection Timeout=30;UnpreparedExecute=true;
         */

        Assembly asm_Npgsql;
        bool InternalConnect(String ip, string dbName, string userName, string pwd, int port)
        {
            String connectstring = String.Format(POSTGRESQLCONNECTIONSTRING, ip, userName, dbName, port, pwd);
            DbConnection dbInteractive = GetDbConnection();
            dbInteractive .ConnectionString = connectstring;
            bool b = this.Connect(dbInteractive);
            return b;
        }
        public static bool ShikiiDBExist = false;

        /// <summary>
        /// 连接到Postgresql
        /// </summary>

        public bool Connect(string dbName, string userName, string pwd)
        {
            if (!ShikiiDBExist)
            {
                bool b = InternalConnect("localhost", "postgres", "postgres", pwd, 5432);
                if (b)
                {

                    List<String> allDBNames = this.GetAllDBNames( );
                      b = allDBNames.Contains(dbName.ToLower());
                    if (!b)
                    {
                        NewDB(dbName  );

                    }
                    ThisDbPipeInfo.Dispose();
                    ThisDbPipeInfo = null;
                    b = InternalConnect("localhost", dbName, userName, pwd, 5432);

                    ShikiiDBExist = true;
                }
               
                return b;
            }
            else
            {
                bool  b = InternalConnect("localhost", dbName, userName, pwd, 5432);
                return b;
            }
        }
        /// <summary>
        /// 连接到Postgresql
        /// </summary>
        public bool Connect(String ip, int port, string dbName, string userName, string pwd)
        {
            if (!ShikiiDBExist)
            {
                //String ip,string dbName, string userName, string pwd,int port)
                bool b = InternalConnect(ip, "postgres", "postgres", pwd, port);

                if (b)
                {

                    List<String> allDBNames = this.GetAllDBNames( );
                      b = allDBNames.Contains(dbName.ToLower());
                    if (!b)
                    {
                        NewDB(dbName   );
                    }
                    ThisDbPipeInfo.Dispose();
                    ThisDbPipeInfo = null;
                   
                    b = InternalConnect(ip, dbName, userName, pwd, port);
                    ShikiiDBExist = true;
                }
                return b;
            }
            else
            {
                 bool  b = InternalConnect("localhost", dbName, userName, pwd, 5432);
               
                return b;
            }
        }
        public List<String> GetAllDBNames( )
        {
            // SELECT lcase(datname) FROM pg_database;

        
            List<string> AllDBNames = new List<string>();
             
            DataTable dt = this.ProvideTable(" SELECT  datname  FROM pg_database;" );
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String strTableName = dt.Rows[i][0].ToString();
                AllDBNames.Add(strTableName);
                // yield return strTableName;
            }
            return AllDBNames;

        }
        //远程连接，请注意可能需要指定this.DbInteractiveArgs = {返回值}
        //获得一个DbInteractiveArgs（未连接）
        protected virtual DbConnection GetDbConnection()
        {
            try
            {
                String dir = Assembly.GetCallingAssembly().Location;

                dir = Path.GetDirectoryName(dir);

                

                if (!File.Exists(dir + "\\Npgsql.dll"))
                {
                    // Tipper.Error = "Npgsql.dll";
                    PerformErrorHandler(this, new Exception("Npgsql.dd 未能找到！"));
                    return null;
                }

                byte[] assemblyBuffer = File.ReadAllBytes(dir + "\\Npgsql.dll");

                if (asm_Npgsql == null)
                    asm_Npgsql = Assembly.Load(assemblyBuffer);

                object obj = asm_Npgsql.CreateInstance("Npgsql.NpgsqlConnection");
                DbConnection  ThisDbConnection = obj as DbConnection;
                 

                return ThisDbConnection;
            }
            catch (Exception ex)
            {
                PerformErrorHandler(this, ex);
                return null;
            }

        }
        public void AddColumn(string tableName, string ColumnName, string FieldType )
        {
            ExecuteNonQuery(String.Format("alter table {0} add column {1} {2} "
               , tableName, ColumnName, FieldType
               ));
        }

        public void ChangeColumnName(string tableName, string colName_Old, string colName_New, string ColTypeDefine )
        {
            String sql = String.Format("alter table {0} rename  {1} to {2}   ", tableName, colName_Old, colName_New);
            ExecuteNonQuery(sql );
            String sqlx = String.Format("alter table {0} alter COLUMN {1} type {2} ", tableName, colName_New, ColTypeDefine);
            ExecuteNonQuery(sqlx );

        }

        public void ChangeColumnType(string tableName, string colName, string ColTypeDefine )
        {
            String sqlx = String.Format("alter table {0} alter COLUMN {1} type {2} ", tableName, colName, ColTypeDefine);
            ExecuteNonQuery(sqlx );
        }

        public void DropColumn(string tableName, string columnName )
        {
            this.ExecuteNonQuery(String.Format("alter table {0} drop column {1}", tableName, columnName) );
        }

        public List<string> GetAllTableNames( )
        {
            List<string> AllTableNames = new List<string>();
            
            DataTable dt = this.ProvideTable(String.Format("select table_name from information_schema.TABLES  where table_schema='public';"));

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String strTableName = dt.Rows[i][0].ToString();
                AllTableNames.Add(strTableName);
            }
            return AllTableNames;
        }

        public List<string> GetAllViewNames()
        {
            throw new NotImplementedException();
        }

        public List<string> GetTableColumnNames(string tableName)
        {
            String sql = "select column_name from information_schema.COLUMNS where table_name='{0}' ;";
            sql = String.Format(sql, tableName.ToLower());

            List<string> AllColumnNames = new List<string>();

            DataTable dt = this.ProvideTable(sql);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String str = dt.Rows[i][0].ToString();

                AllColumnNames.Add(str);
            }
            return AllColumnNames;

        }

        public List<string> GetTableColumnTypes(string tableName, bool isRawSqlType = false )
        {
            List<string> Types = new List<string>();
            DataTable dt = this.ProvideTable(String.Format("select data_type,CHARACTER_MAXIMUM_LENGTH from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME ='{0}'", tableName.ToLower()) );
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String str = dt.Rows[i][0].ToString().ToLower();

                if (!isRawSqlType)
                    str = this.TypeInfer(str);
                else
                {
                    if (str.Equals("character") || str.Equals("character varying"))
                    {
                        int charNum = Convert.ToInt32(dt.Rows[i][1]);
                        str = String.Format("{0}({1})", str, charNum);
                    }

                }


                Types.Add(str);

            }
            return Types;
        }

        public string InferDataType(PropertyInfo pif, string csType)
        {
            switch (csType)
            {
                case "Int16": return "smallint";
                case "Int32": return "integer";
                case "Int64": return "bigint";
                case "String":
                    Attribute[] attributes = Attribute.GetCustomAttributes(pif);
                    Attribute attribute = null;
                    if (attributes != null && attributes.Length > 1)
                    {
                        for (int i = 0; i < attributes.Length; i++)
                        {
                            if (attributes[i] is PostgresqlTextTypeAttribute)
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
                    else
                    {
                        return PostgresqlTextTypeAttribute.VERYSHORTTEXT;
                    }
                case "DateTime": return "timestamp without time zone";
                case "Single": return "real";
                case "Double": return "double precision";
                case "Decimal": return "decimal";
                /*Sql 数据库里的Bit类型 读出来是 True 或者 False
而写入数据库 则是 0 和1 1表式true 0表式false
判断一下啊，如果条件成立 数据库里的bit字段就等于1 否则等于0 不就行了！*/
                case "Boolean": return "bool";


            }

            return null;
        }


        /// <summary>
        /// 启动PostgreSQL进程
        /// </summary>
        /// <param name="PostgreSQLDirPath">PostgreSQL安装目录如（D:\Programs\pgsql）这个目录下应该有bin目录</param>
        /// <returns></returns>
        public bool BootServer(String PostgreSQLDirPath)
        {
            // Process.GetProcessesByName()
            Process[] ps = Process.GetProcessesByName("postgres");
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
            p.StandardInput.WriteLine(String.Format(" \"{0}\" start -D \"{1}\\data\"&exit",
                PostgreSQLDirPath + "\\bin\\" + "pg_ctl.exe", PostgreSQLDirPath));
            p.StandardInput.AutoFlush = true;

            ps = Process.GetProcessesByName("postgres");
            if (ps != null && ps.Length > 0)
                return true;
            else
                return false;


        }
        /// <summary>
        /// 关闭PostgreSQL进程
        /// </summary>
        /// <param name="PostgreSQLDirPath">PostgreSQL安装目录如（D:\Programs\pgsql）这个目录下应该有bin目录</param>
        /// <returns></returns>
        public void ShutDownServer(String PostgreSQLDirPath)
        {

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
            p.StandardInput.WriteLine(String.Format(" \"{0}\" stop -D \"{1}\\data\"&exit",
                PostgreSQLDirPath + "\\bin\\" + "pg_ctl.exe", PostgreSQLDirPath));
            p.StandardInput.AutoFlush = true;

        }


    }
}
