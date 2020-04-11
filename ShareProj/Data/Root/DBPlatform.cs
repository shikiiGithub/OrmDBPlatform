﻿﻿using dotNetLab.Common;
using dotNetLab.Data.Orm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace dotNetLab.Data
{
    public  partial  class DBPlatform: DbPlatformRoot,IReadableLog
    {
        public event ErrorCallback ErrorHandler;
        public event InfoCallback InfoHandler;
        public delegate void OnDBDataChangedCallback(String text);
        public event OnDBDataChangedCallback OnDBDataChanged;
        public DbPipeInfo ThisDbPipeInfo;
        public void PerformErrorHandler(Object o, Exception e)
        {
            this.ErrorHandler?.Invoke(o, e);
        }
        public bool Connect(DbConnection conn)
        {
            try
            {
                if(ThisDbPipeInfo == null)
                    this.ThisDbPipeInfo = new DbPipeInfo(conn);
                conn.Open();
                ThisDbPipeInfo.PrepareFirstUse(conn);

                return true;
            }
            catch (Exception ex)
            {
               
                this.PerformErrorHandler(conn, ex);
                return false;

            }


        }

        public Exception ExecuteNonQuery(string sql,DbCommand cmd=null)
        {

            try
            {
               
                    if (cmd == null)
                        cmd = ThisDbPipeInfo.AvailableCommand;
                    cmd.CommandText = sql;
               
                    cmd.ExecuteNonQuery();
               
               return null;
            }
            catch (Exception e)
            {  
                this.ErrorHandler?.Invoke(null, e);
                
               
                return e;
            }

        }
        /// <summary>
        /// <para>批量事务处理(适用于大数据传递)</para>
        /// 不需要显示使用transaction,内置使用Transaction
        /// 可以使用orm方式对数据库写操作
        /// 也可以使用传入的DbCommand 对象写操作
        /// </summary>
        /// <param name="_constr">连接字符串,null则使用默认连接</param>
        /// <param name="actions">多个线程要执行的Action(每个Action 一个线程)</param>
        public void BatchExecuteNonQuery(String _constr=null,params Action<DbCommand>[] actions)
        {
            int nActions = actions.Length;
            ParameterizedThreadStart BatchExecuteNonQueryAction = (obj) =>
            {
                Object[] objs = obj as Object[];
                DbCommand cmd = objs[0] as DbCommand;
                Action<DbCommand> action = objs[1] as Action<DbCommand>;
                DbTransaction transaction = cmd.Connection.BeginTransaction();
                action(cmd);
                transaction.Commit();

            };
            for (int i = 0; i < nActions; i++)
            {
                DbCommand dbCommand  = ThisDbPipeInfo.NewCommandOrReuseDbCommand(_constr);
                Thread thd =new Thread(BatchExecuteNonQueryAction);
                int nId= thd.ManagedThreadId;
                ThisDbPipeInfo.Threads.Add(thd);
                ThisDbPipeInfo.ThreadIDs.Add(nId);
                ThisDbPipeInfo.ThreadId_DbCommandPairs.Add(nId, dbCommand);
                thd.Start( new Object[] { dbCommand, actions[i] });
            }
        }
        public DbDataReader FastQueryData(String sql,DbCommand cmd=null)
        {

            try
            {

                if(cmd== null)
                  cmd 
                    = ThisDbPipeInfo.AvailableCommand;
                cmd.CommandText = sql;
                DbDataReader reader = cmd.ExecuteReader();
                return reader;


            }
            catch (Exception ex)
            {
                ErrorHandler?.Invoke(null, ex);
                return null;
            }

        }

        public Exception FastQueryDataEx(string sql,
            Action<Object> QueriedDataCallback, DbCommand cmd = null,
            Action EndQueryedRowCallback = null)
        {

            try
            {
                if (cmd == null)
                    cmd = ThisDbPipeInfo.AvailableCommand;
                cmd.CommandText = sql;
                DbDataReader reader = cmd.ExecuteReader();
                int nFileCount = reader.FieldCount;

                if (reader.HasRows)//如果有数据
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < nFileCount; i++) //逐个字段的遍历
                        {
                            //示例
                            //if (NeedQuote(AllTableTypes[i]))
                            //    sb.Append($"'{reader[i].ToString()},'");
                            //else
                            //    sb.Append($"'{reader[i]}',");

                            QueriedDataCallback(reader.GetValue(i));
                        }

                        EndQueryedRowCallback?.Invoke();

                    }
                }

                //先关闭Reader
                reader.Close();




                return null;
            }
            catch (Exception ex)
            {

                ErrorHandler?.Invoke(null, ex);
                return ex;
            }
        }
        public DataTable ProvideTable(string sql,  DbCommand cmd = null)
        {

            DataTable dt = null;
            try
            {
                dt = new DataTable();
              if(cmd== null)
                cmd = ThisDbPipeInfo.AvailableCommand;
                cmd.CommandText = sql;

                DbDataReader reader = cmd.ExecuteReader();
                dt.Load(reader);
                reader.Close();

            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(cmd, e);
                return null;
            }

            return dt;

        }
        public string UniqueResult(string sql, DbCommand cmd = null)
        {
            if (cmd == null)
                  cmd = ThisDbPipeInfo.AvailableCommand;
            String strResult = null;
            try
            {

                cmd.CommandText = sql;
                Object obj = cmd.ExecuteScalar();
                if (obj != null)
                    strResult = obj.ToString();
               
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(cmd, e);
                return null;
            }
            return strResult;
        }
        public Exception RemoveTable(string strTableName, DbCommand cmd = null)
        {
            try
            {


                if (cmd == null)
                    cmd = ThisDbPipeInfo.AvailableCommand;
                cmd.CommandText = string.Format("drop table  {0} ;", strTableName);

                cmd.ExecuteNonQuery();

                return null;


            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(null, e);
                return e;
            }
        }
        public Exception RemoveRecord(string TableName, String strRequirement, DbCommand cmd = null)
        {
            try
            {


                if (cmd == null)
                    cmd = ThisDbPipeInfo.AvailableCommand;
                cmd.CommandText = string.Format("Delete from  {0} where {1} ;", TableName, strRequirement);
                cmd.ExecuteNonQuery();
                return null;
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(null, e);
                return e;
            }
        }
        public Exception Update(string TableName, string strColumnAssignAndRequirment, DbCommand cmd = null)
        {

            try
            {

                if (cmd == null)
                    cmd = ThisDbPipeInfo.AvailableCommand;
                cmd.CommandText =
                            string.Format
                            ("update  {0} set {1}",
                                TableName, strColumnAssignAndRequirment);

                cmd.ExecuteNonQuery();
                return null;

            }
            catch (Exception e)
            {

                ErrorHandler?.Invoke(null, e);
                return e;
            }
        }
        public virtual Exception NewDB(string DBName, DbCommand cmd = null)
        {

            try
            {
                if (cmd == null)
                
                cmd = ThisDbPipeInfo.AvailableCommand;
                cmd.CommandText = string.Format("create database {0} ;", DBName);

                cmd.ExecuteNonQuery();


                return null;

            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(null, e);
                return e;
            }

        }
        public Exception RemoveDB(string strDBName, DbCommand cmd = null)
        {

            try
            {
                if (cmd == null)
                    cmd = ThisDbPipeInfo.AvailableCommand;
                cmd.CommandText = string.Format("DROP database {0} ;", strDBName);
                cmd.ExecuteNonQuery();
                return null;

            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(null, e);
                return e;
            }
        }
        public Exception NewTable(string tablename, string tableDef, DbCommand cmd = null)
        {

            try
            {
                if (cmd == null)
                    cmd = ThisDbPipeInfo.AvailableCommand;
                cmd.CommandText = string.Format("create table {0}({1}) ;",
                            tablename, tableDef);

                cmd.ExecuteNonQuery();
                return null;
            }
            catch (Exception e)
            {

                ErrorHandler?.Invoke(null, e);
                return e;
            }

        }
     
        public Exception NewRecord(string strTableName, string strValue, DbCommand cmd = null)
        {

            try
            {
                if (cmd == null)
                    cmd = ThisDbPipeInfo.AvailableCommand;
                cmd.CommandText = string.Format("insert into {0} values({1})", strTableName, strValue);
                cmd.ExecuteNonQuery();
                return null;
            }
            catch (Exception e)
            {
                ErrorHandler?.Invoke(null, e);
                return e;
            }

        }
        /// <summary>
        /// 未添加Lock
        /// </summary>
        /// <param name="ViewName"></param>
        /// <param name="strTableName"></param>
        /// <param name="strColumnNames">such as " age,sex,... "</param>
        /// <param name="args"></param>
        public Exception NewView(string ViewName, string strTableName, string strColumnNames, DbCommand cmd = null)
        {
            try
            {


                if (cmd == null)
                    cmd = ThisDbPipeInfo.AvailableCommand;
                cmd.CommandText =
                            string.Format(CREATEVIEW, ViewName, strColumnNames, strTableName);
                cmd.ExecuteNonQuery();
                return null;


            }
            catch (Exception ex)
            {

                ErrorHandler(null, ex);

                return ex;
            }

        }
        public Exception NewKeyValueView(string ViewName, string strFromTableName,
            string strColumnName_Name, string strColumnName_Val, DbCommand cmd = null)
        {
            string str = string.Format("{0} as Name,{1} as Val",
                strColumnName_Name, strColumnName_Val
                );
            return NewView(ViewName, strFromTableName, str,cmd);
        }
    }
}
