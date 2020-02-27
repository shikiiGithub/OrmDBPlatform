﻿﻿using dotNetLab.Common;
using dotNetLab.Data.Orm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;

using System.Reflection;

namespace dotNetLab.Data 
{
    public class SQLCEDBEngine : DBPlatform,  ITableInfo
    {

        public bool Connect(String DBFilePath, String dlldir = null)
        {
            try
            {
                String strCmpactDBFilePath = DBFilePath;

                String s = Assembly.GetExecutingAssembly().Location;
                String dir = Path.GetDirectoryName(s);
                dir = dir + "\\";

                if (dlldir != null)
                    dir = dlldir + "\\";

                CheckSQLCERefFiles(dir);
                DbConnection args = GetDbConnection(dir);
                
                args.ConnectionString = $"data source = {strCmpactDBFilePath};";
                if (!File.Exists(args.Database))
                {
                    Object objEngine = GetReflectOject(dir + "System.Data.SqlServerCe.dll", "System.Data.SqlServerCe.SqlCeEngine");
                    objEngine.GetType().GetProperty("LocalConnectionString").SetValue(objEngine, args .ConnectionString, null);
                    objEngine.GetType().GetMethod("CreateDatabase").Invoke(objEngine, null);
                    IDisposable disposeObj = objEngine as IDisposable;
                    disposeObj.Dispose();
                }

                bool b = Connect(args);
                if(!b)
                {
                    Console.WriteLine(
                    "未能连接数据库,请检测VC++2010是否安装？");
                    
                }
                return b;
            }
            catch (Exception ex)
            {
 
                return false;
            }
        }

        public DbConnection GetDbConnection(String dir = "")
        {
            object obj = this.GetReflectOject(dir + "System.Data.SqlServerCe.dll", "System.Data.SqlServerCe.SqlCeConnection");
            DbConnection ThisDbConnection =
                obj
                as DbConnection;
            return ThisDbConnection;

        }
        /// <summary>
        /// 检查必须的dll（兼容以前的版本）
        /// </summary>
        /// <param name="dir">dll所在的文件夹</param>
        protected virtual void CheckSQLCERefFiles(String dir = "")
        {
        }

        //{

        //    if (!File.Exists(dir + "System.Data.SqlServerCe.dll"))
        //    {
        //        AddRef(dir + "System.Data.SqlServerCe.dll", dotNetLab.Plugin.VSPackage.System_Data_SqlServerCe);
        //    }
        //    if (!File.Exists(dir + "sqlceqp40.dll"))
        //    {
        //        AddRef(dir + "sqlceqp40.dll", dotNetLab.Plugin.VSPackage.sqlceqp40);
        //    }
        //    if (!File.Exists(dir + "sqlcese40.dll"))
        //    {
        //        AddRef(dir + "sqlcese40.dll", dotNetLab.Plugin.VSPackage.sqlcese40);
        //    }
        //    if (!File.Exists("sqlceme40.dll"))
        //    {
        //        AddRef(dir + "sqlceme40.dll", dotNetLab.Plugin.VSPackage.sqlceme40);
        //    }
        //    if (!File.Exists(dir + "sqlceer40EN.dll"))
        //    {
        //        AddRef(dir + "sqlceer40EN.dll", dotNetLab.Plugin.VSPackage.sqlceer40EN);
        //    }
        //    if (!File.Exists(dir + "sqlceca40.dll"))
        //    {
        //        AddRef(dir + "sqlceca40.dll", dotNetLab.Plugin.VSPackage.sqlceca40);
        //    }
        //    if (!File.Exists(dir + "sqlcecompact40.dll"))
        //    {
        //        AddRef(dir + "sqlcecompact40.dll", dotNetLab.Plugin.VSPackage.sqlcecompact40);
        //    }
        //}
        public static void AddRef(string strFileName, byte[] bytArr)
        {
            FileStream fs = new FileStream(strFileName, FileMode.Create);

            fs.Write(bytArr, 0, bytArr.Length);
            fs.Flush();
            fs.Close();
            fs.Dispose();

        }

        public List<string> GetAllViewNames( )
        {
            return null;
        }

        public List<string> GetAllTableNames( )
        {
            DataTable dt = null;
            List<String> TableNames = new List<string>();
            dt = this.ProvideTable("select Table_Name from information_schema.TABLES" );
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String str = dt.Rows[i][0].ToString();
                TableNames.Add(str);
                 
            }
            return TableNames;
        }

        public List<string> GetTableColumnNames(string tableName )
        {
            try
            {

                DataTable dt;
                List<String> AllColumnNames = new List<string>();

                string SQLFormat = $"select COLUMN_NAME from information_schema.COLUMNS where table_name = '{tableName}';";
                dt = this.ProvideTable(String.Format(SQLFormat, tableName) );
                for (int i = 0; i < dt.Rows.Count; i++)
                    AllColumnNames.Add(dt.Rows[i][0].ToString());
                return AllColumnNames;
            }
            catch (Exception ex)
            {

                return null;
            }


        }

        public List<string> GetTableColumnTypes(string tableName, bool isRawSqlType = false )
        {

            string sql = String.Format("select data_type from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME ='{0}'  ", tableName);
            DataTable dt = ProvideTable(sql);
            List<String> typeList = new List<string>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String type = dt.Rows[i][0].ToString().ToLower();
                if (!isRawSqlType)
                    type = TypeInfer(type);
                typeList.Add(type);
            }
            return typeList;
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
                        if ((textLengthAttribute as DBTypeAttribute).DataType != SQLCETextTypeAttribute.VERYSHORTTEXT)
                            return "ntext";
                        else
                            return "national character varying(512)";
                    }
                    else
                    {
                        return "national character varying(512)";
                    }

                case "DateTime": return "DateTime";


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

