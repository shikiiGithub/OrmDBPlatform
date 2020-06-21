﻿﻿using dotNetLab.Common;
using dotNetLab.Data.Orm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using dotNetLab.Data;
namespace dotNetLab.Data 
{
    public class FireBirdEngine : DBPlatform,  IHugeDB,  ITableInfo
    {
        public String FireBirdConnectionStr_Server = "data source={0};user id={1};password={2};initial catalog={3};character set=UTF8;port number={4};server type=Default; pooling=True;min pool size=1;max pool size=100";
        public String FireBirdConnectionStr = "User=SYSDBA;Password=masterkey;Database={0};Charset=utf8;ServerType=1";
        public static int PackingSize = 16384;
        public static bool EmbeddedMode = true;
        
        public int GetAuto_IncrementID(string tableName, String filedName)
        {
            String str = this.UniqueResult(String.Format("select max ({0}) from {1}", filedName.ToUpper(), tableName));
            if (str.IsValideString())
                return int.Parse(str) + 1;
            else
            {
                PerformErrorHandler(this, new Exception("出现错误，未得到自增id"));
                return 0;
            }
        }

        public void AddColumn(string tableName, string ColumnName, string FieldType,String Extra=null)
        {
            String sql = String.Format("ALTER TABLE {0} ADD {1} {2}", tableName.ToUpper(), ColumnName.ToUpper(), FieldType.ToUpper());
            this.ExecuteNonQuery(sql);
        }

        public void ChangeColumnName(string tableName, string colName_Old, string colName_New, string ColTypeDefine )
        {
            String sql = String.Format("ALTER TABLE {0} ALTER COLUMN {1} TO {2}", tableName, colName_Old.ToUpper(), colName_New.ToUpper());
            this.ExecuteNonQuery(sql);
        }

        public void ChangeColumnType(string tableName, string colName, string ColTypeDefine )
        {
            String sql = String.Format("ALTER TABLE {0} ALTER COLUMN {1} TYPE {2}", tableName.ToUpper(), colName.ToUpper(), ColTypeDefine.ToUpper());
            this.ExecuteNonQuery(sql);
        }

        public bool Connect(String dbPath)
        {
            try
            {
                String con = String.Format(FireBirdConnectionStr, dbPath);

                object obj = this.GetReflectOject("FirebirdSql.Data.FirebirdClient.dll", "FirebirdSql.Data.FirebirdClient.FbConnection");
                DbConnection ThisDbConnection = obj as DbConnection;
                ThisDbConnection.ConnectionString = String.Format(this.FireBirdConnectionStr, dbPath);

                if (!File.Exists(dbPath))
                {
                    obj.GetType().GetMethod("CreateDatabaseImpl", BindingFlags.NonPublic | BindingFlags.Static).Invoke(obj, new object[] {  ThisDbConnection.ConnectionString, PackingSize, true, true });
                }
                bool b  = this.Connect(ThisDbConnection);
                return b;
            }
            catch (System.Exception ex)
            {
                PerformErrorHandler(this, ex);
                return  false;
            }

        }

        public bool Connect(string DBName, string UserName, string Pwd)
        {
            return Connect("127.0.0.1",3050, DBName,   UserName,   Pwd);
        }

        public bool Connect(string ip, int port, string DBName, string UserName, string Pwd)
        {
            try
            {
                if (EmbeddedMode)
                    return Connect(DBName);

                if (String.IsNullOrEmpty(UserName))
                    UserName = "SYSDBA";
                String con = String.Format(FireBirdConnectionStr_Server, ip, UserName, Pwd,DBName,port);
               
                object obj = this.GetReflectOject("FirebirdSql.Data.FirebirdClient.dll", "FirebirdSql.Data.FirebirdClient.FbConnection");

                DbConnection ThisDbConnection =
                    obj
                        as DbConnection;
                ThisDbConnection.ConnectionString = String.Format(this.FireBirdConnectionStr, DBName);
                 if (!File.Exists(DBName))
                {
                    obj.GetType().GetMethod("CreateDatabaseImpl", BindingFlags.NonPublic | BindingFlags.Static).Invoke(obj, new object[] {   ThisDbConnection.ConnectionString, PackingSize, true, true });
                }

               bool b= this.Connect(ThisDbConnection);
               return b;
            }
            catch (System.Exception ex)
            {
                PerformErrorHandler(this, ex);
                return false;
            }
        }

        public void DropColumn(string tableName, string columnName )
        {
            String sql = String.Format("ALTER TABLE {0} DROP {1}", tableName.ToUpper(), columnName.ToUpper());
            this.ExecuteNonQuery(sql );
        }

        public List<string> GetAllTableNames( )
        {
            List<String> AllTableNames = new List<string>();

            DataTable dt =
        ProvideTable("SELECT RDB$RELATION_NAME AS TABLE_NAME FROM RDB$RELATIONS WHERE RDB$SYSTEM_FLAG = 0 or RDB$SYSTEM_FLAG = 1  AND RDB$VIEW_SOURCE IS NULL;" );

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i][0].ToString().Contains("$"))
                    continue;
                AllTableNames.Add(dt.Rows[i][0].ToString().Trim());
            }
            return AllTableNames;

        }

        public List<string> GetAllViewNames( )
        {
            throw new NotImplementedException();
        }

        

        public List<string> GetTableColumnNames(string tableName)
        {

            List<string> AllColumnNames = new List<string>();
            String strFormat = "SELECT RDB$FIELD_NAME FROM RDB$RELATION_FIELDS WHERE  RDB$RELATION_NAME= '{0}' ;";
            DataTable dt = this.ProvideTable(String.Format(strFormat, tableName.ToUpper()) );
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                AllColumnNames.Add(dt.Rows[i][0].ToString().Trim());
            }
            return AllColumnNames;
        }

        public List<string> GetTableColumnTypes(string tableName, bool isRawSqlType = false)
        {
            List<String> ColumnTypes = new List<string>();
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT A.RDB$FIELD_NAME, B.RDB$FIELD_TYPE, B.RDB$CHARACTER_LENGTH, B.RDB$FIELD_PRECISION,C.RDB$TYPE_NAME,C.RDB$FIELD_NAME");
            sb.Append(" FROM RDB$RELATION_FIELDS A, RDB$FIELDS B, RDB$TYPES C WHERE A.RDB$RELATION_NAME = '{0}'");
            sb.Append(" AND A.RDB$FIELD_SOURCE = B.RDB$FIELD_NAME  AND C.RDB$TYPE = B.RDB$FIELD_TYPE AND C.RDB$FIELD_NAME='RDB$FIELD_TYPE';");
            DataTable dt = this.ProvideTable(String.Format(sb.ToString(), tableName.ToUpper()) );


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                String str = dt.Rows[i][4].ToString().ToLower().Trim();

                if (!isRawSqlType)
                    str = this.TypeInfer(str);
                else
                {

                    if (str.Contains("varying"))
                    {
                        int charNum = Convert.ToInt32(dt.Rows[i][2]);
                        str = String.Format("character {0}({1})", str, charNum);
                    }
                    else if (str.Equals("long"))
                    {
                        str = "INTEGER";
                    }
                    ColumnTypes.Add(str);
                }

            }
            return ColumnTypes;
        }

        public string InferDataType(PropertyInfo pif, string csType)
        {
            switch (csType)
            {
                case "Byte[]": return "BLOB";
                case "Int16": return "SMALLINT";
                case "Int32": return "INTEGER";
                case "Int64": return "BIGINT";
                case "String":
                    //DBTypeAttribute textLengthAttribute = pif.GetCustomAttribute<FireBirdTextTypeAttribute>();

                    Attribute[] attributes = Attribute.GetCustomAttributes(pif);
                    Attribute attribute = null;
                    if (attributes != null && attributes.Length > 1)
                    {
                        for (int i = 0; i < attributes.Length; i++)
                        {
                            if (attributes[i] is FireBirdTextTypeAttribute)
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
                    else { return FireBirdTextTypeAttribute.VERYSHORTTEXT; }
                case "DateTime": return "TIMESTAMP";
                case "Single": return "FLOAT";
                case "Double": return "DOUBLE PRECISION";
                case "Decimal": return "DECIMAL";
                /*Sql 数据库里的Bit类型 读出来是 True 或者 False
而写入数据库 则是 0 和1 1表式true 0表式false
判断一下啊，如果条件成立 数据库里的bit字段就等于1 否则等于0 不就行了！*/
                case "Boolean": return "SMALLINT";

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

