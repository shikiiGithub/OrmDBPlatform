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
    public class SQLiteDBEngine :  DBPlatform,  ITableInfo
    {
        private String DefaultTable = "App";
        //仅用于Xamarin
        public static Assembly assembly_Sqlite_Connection = null;


        bool InternalMobileConnect(String SqliteDBFilePath)
        {
            try
            {

                String name_lower = null;
                DbConnection dbConnection = null;
                if (assembly_Sqlite_Connection != null)
                {

                    Type[] types = assembly_Sqlite_Connection.GetTypes();

                    for (int i = 0; i < types.Length; i++)
                    {
                        name_lower = types[i].Name.ToLower();
                        if (name_lower.Equals("sqliteconnection"))
                        {
                            dbConnection = assembly_Sqlite_Connection.CreateInstance(types[i].FullName) as DbConnection;
                            break;
                        }


                    }

                }
                else
                    return false;

                String strCmpactDBFilePath = SqliteDBFilePath;
                dbConnection.ConnectionString = $"data source={strCmpactDBFilePath}";

                bool b = Connect(dbConnection);
                if (!b)
                    PerformErrorHandler(this, new Exception("未能连接数据库"));

                return b;

            }
            catch (Exception exx)
            {


                return false;
            }

        }

        public virtual bool Connect (String  SqliteDBFilePath)
        {
            try
            {
                if (assembly_Sqlite_Connection != null)
                  return   InternalMobileConnect(SqliteDBFilePath);
                
                String strThisDllPath = this.GetType().Assembly.Location;
                string dir = Path.GetDirectoryName(strThisDllPath);
                String dllPath = dir + "\\System.Data.SQLite.dll";
                if (!File.Exists(dllPath))
                {
                    Console.WriteLine( "未找到‘System.Data.SQLite.dll’");
                    return false ;
                }
               DbConnection ThisDbConnection =
                   this.GetReflectOject(dllPath, "System.Data.SQLite.SQLiteConnection")
                    as DbConnection;
               
                String strCmpactDBFilePath = SqliteDBFilePath;
            
               
                 
                 ThisDbConnection.ConnectionString = $"data source={strCmpactDBFilePath}";
             
                bool b = Connect(ThisDbConnection);
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
        public List<String> GetAllTableNames(   )
        {
            DataTable dt = null;List<String> TableNames = new List<string>() ;
            try
            {
                
                dt = this.ProvideTable("select name from sqlite_master where type='table' or type='view' order by name;" );

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    TableNames.Add(dt.Rows[i][0].ToString());
                }
                return TableNames;
            }
            catch (Exception e)
            {
                PerformErrorHandler(dt, e);
                return null;
            }
        }

        public List<String> GetAllViewNames(  )
        {
            List<String> AllTableNames = new List<string>();
            DataTable dt = null;
            try
            {
                string sql = "select name from sqlite_master where  type='view' order by name;";
                 dt = this.ProvideTable(sql );

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    AllTableNames.Add(dt.Rows[i][0].ToString());
                }
                return AllTableNames;
            }
            catch (Exception ex)
            {

                PerformErrorHandler(dt, ex);
                return null;
            }
            

        }
        public List<String> GetTableColumnNames(String tableName )
        {
            try
            {

            DataTable dt;
            List<String> AllColumnNames = new List<string>();

            string SQLFormat = "PRAGMA table_info({0})";
            dt = this.ProvideTable(String.Format(SQLFormat, tableName) );
            for (int i = 0; i < dt.Rows.Count; i++)
                AllColumnNames.Add(dt.Rows[i][1].ToString());
                return AllColumnNames;
            }
            catch (Exception ex)
            {

                return null;
            }
        }
        public List<String> GetTableColumnTypes(string tableName, bool isRawSqlType = false )
        {
            string sql = $"PRAGMA table_info ({tableName})";
            DataTable dt =  ProvideTable(sql );
            List<String> typeList = new List<string>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String type = dt.Rows[i]["type"].ToString().ToLower();
                if(!  isRawSqlType )
                type = TypeInfer(type);
                typeList.Add(type);
            }
            return typeList;
        }
         //导出表结构
        public String GetCreatedTableSQL(string tableName )
        {
            string sql = $"select sql from sqlite_master where type = 'table' and name='{tableName}';";
            String str = UniqueResult(sql );
            return str;
        }

        //不能导出字段为null的
        public string GetTable_Sql_Data(string tableName)
        {
            try
            {
                List<string> AllTableTypes = GetTableColumnTypes(tableName,false ) ;
                StringBuilder sb = new StringBuilder();
                StringBuilder sb_All = new StringBuilder();
                int i = 0;
                Action<Object> QueryAction = (o) =>
                {
                    if (i == AllTableTypes.Count)
                        i = 0;
                    if (NeedQuote(AllTableTypes[i++]))
                        sb.Append($"'{o}'");
                    else
                        sb.Append($"{o},");
                    
                };
                Action EndQueryRow  = ()=>
                    {
                        sb.Remove(sb.Length - 1, 1);
                        sb_All.Append($"insert into {tableName} values({sb.ToString()});\r\n");
                        sb.Clear();
                    };
                this.FastQueryDataEx($"select * from {tableName} ;", QueryAction,null,EndQueryRow );
                return sb_All.ToString().Trim('\r','\n');
            }
            catch (Exception ex)
            {

                return null;
            }
            
        }

        //导出整个数据库中的数据为sql语句
        public string ExportDBToSql( )
        {
           var TableNames  = GetAllTableNames( );
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < TableNames.Count; i++)
            {
                if(TableNames[i].ToLower() != this.DefaultTable.ToLower())
                sb.AppendLine(GetCreatedTableSQL(TableNames[i] ) );
                sb.AppendLine(GetTable_Sql_Data(TableNames[i] ));
            }
            return sb.ToString().Trim('\r', '\n');
        }

        public List<string> GetEmbeddedSqlFile(string namespaceName,String folderName)
        {
            List<string> lst = new List<string>();
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{namespaceName}.{folderName}.sqlite.sql");
            StreamReader sr = new StreamReader(stream,Encoding.UTF8);
            string temp;
            while((temp = sr.ReadLine()) != null)
            {
                lst.Add(temp);
            }

            return lst;
        }

        public bool ExecuteEmbeddedSqlFile(string namespaceName, String folderName)
        {
            try
            {
                List<string> lst = GetEmbeddedSqlFile(namespaceName, folderName);
                foreach (var item in lst)
                {
                    this.ExecuteNonQuery(item);
                }
                return true;
            }
            catch (Exception ex)
            {
                
                Console.WriteLine("At ExecuteEmbeddedSqlFile " + ex.Message + " " + ex.StackTrace);
                return false;
            }
        }

        public string InferDataType(PropertyInfo pif, string csType)
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
                    Attribute[] attributes = Attribute.GetCustomAttributes(pif);
                    Attribute attribute = null;
                    if (attributes != null && attributes.Length > 1)
                    {
                        for (int i = 0; i < attributes.Length; i++)
                        {
                            if (attributes[i] is SQLiteTextTypeAttribute)
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
                        return SQLiteTextTypeAttribute.VERYSHORTTEXT;
                    }
               
                case "DateTime":
                    SQLiteTextTypeAttribute timeTypeAttribute = pif.GetCustomAttribute<SQLiteTextTypeAttribute>();
                    if (timeTypeAttribute == null)
                        return "DateTime";
                    else
                        return timeTypeAttribute.DataType;

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
    }
}
