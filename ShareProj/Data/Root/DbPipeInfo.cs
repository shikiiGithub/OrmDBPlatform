﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;


namespace dotNetLab.Data
{
    public class DbPipeInfo:IDisposable
    {
        public Dictionary<int, DbCommand> ThreadId_DbCommandPairs = new Dictionary<int, DbCommand>();
        public List<int> ThreadIDs = new List<int>();
        public List<Thread> Threads = new List<Thread>();
        public  Queue <DbCommand> ReservedDbCommands = new Queue<DbCommand>();
        Type ConnectionType;
        String connectionString;
        public DbConnection MainDbConnection;
        public DbCommand MainDbCommand;
        private Timer tmr;
        public int CycleCmdGapTimeInMillSecs = 1000;
        //超时30秒
        public int CmdExecutingTimeout = 30;
      
        public DbPipeInfo(DbConnection ThisDbConnection)
        {
            if(ThisDbConnection==null)
                throw new Exception("ThisDbConnection 不能为空");
            tmr= new Timer((obj)=> {
                CycleCommands();
            },
            null,
            0,
            CycleCmdGapTimeInMillSecs);

            MainDbConnection = ThisDbConnection;
            ConnectionType = ThisDbConnection.GetType();
            connectionString = ThisDbConnection.ConnectionString;
        }

         public int CommandCycleGapTime
        {
            get { return this.CycleCmdGapTimeInMillSecs; }
            set { CycleCmdGapTimeInMillSecs = value;
                tmr.Change(0, value);
            }
        }


        public void ManualRemoveDbCmd(DbCommand dbCommand)
        {
            try
            {
                if (ReservedDbCommands.Contains(dbCommand))
                    ReservedDbCommands.Dequeue();
                if (ThreadId_DbCommandPairs.Values.Contains(dbCommand))
                {
                    int index_v = ThreadId_DbCommandPairs.Values.ToList().FindIndex(x => x == dbCommand);
                    int nId = ThreadId_DbCommandPairs.Keys.ToList()[index_v];
                    ThreadId_DbCommandPairs.Remove(nId);
                    Threads.RemoveAt(index_v);
                    ThreadIDs.Remove(nId);
                    dbCommand.Connection.Close();
                    dbCommand.Connection.Dispose();
                    dbCommand.Dispose();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                File.AppendAllText(String.Format("DbPipeInfo_{0}_Error", DateTime.Now.ToString("yyyy_MM")),
                    "手动移除DbCommand 时出错位于ManualRemoveDbCmd " + ex.Message + " " + ex.StackTrace, Encoding.UTF8);

            }

        }

        public void PrepareFirstUse(DbConnection ThisDbConnection)
        {
            DbCommand command = ThisDbConnection.CreateCommand();
            MainDbCommand = command;
            MainDbCommand.CommandTimeout = CmdExecutingTimeout;
            ReservedDbCommands.Enqueue(command);

        }

        DbCommand NewCommand ( )
        {
            //利用已有的闲置DbCommand
            if (ReservedDbCommands.Count > 0)
            {

                DbCommand cmd = ReservedDbCommands.Dequeue();
                return cmd;
            }
            DbConnection conn = System.Activator.CreateInstance(ConnectionType) as DbConnection;

            conn.ConnectionString =   connectionString  ;
            conn.Open();
            DbCommand command = conn.CreateCommand();
            command.CommandTimeout = CmdExecutingTimeout;
            Console.WriteLine("创建了一个连接");
            return command;
        }

        /// <summary>
        /// 获得一个连接（如果可以复用则复用）
        /// </summary>
        /// <param name="_connectionString"></param>
        /// <returns></returns>
        public DbCommand NewCommandOrReuseDbCommand(String _connectionString=null )
        {
            //利用已有的闲置DbCommand
            if (ReservedDbCommands.Count > 0)
            {

               DbCommand cmd = ReservedDbCommands.Dequeue();
               return cmd;
            }
            DbConnection conn = System.Activator.CreateInstance(ConnectionType) as DbConnection;
            
            conn.ConnectionString = _connectionString==null? connectionString:_connectionString ;
            conn.Open();
            DbCommand command = conn.CreateCommand();
            command.CommandTimeout = CmdExecutingTimeout;
            Console.WriteLine("创建了一个连接");
            return command;
        }
        /// <summary>
        /// 创建一个连接以备用（注意不会复用已经存在的闲置的DbCommand）
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public DbCommand NewCommandForReserving(  String _connectionString= null)
        {
            DbConnection conn = System.Activator.CreateInstance(ConnectionType) as DbConnection;
            conn.ConnectionString = _connectionString == null ? connectionString : _connectionString;
            conn.Open();
            DbCommand command = conn.CreateCommand();
            Console.WriteLine("创建了一个连接");
            command.CommandTimeout = CmdExecutingTimeout;
            ReservedDbCommands.Enqueue(command);
            return command;
        }

        void CycleCommands()
        {
            try
            {
                //回收已经完成的线程，Dbcommand
                for (int i = 0; i < Threads.Count; i++)
                {
                    if (!Threads[i].IsAlive)
                    {

                        int nId = Threads[i].ManagedThreadId;
                        DbCommand _cmd = ThreadId_DbCommandPairs[nId];
                        ReservedDbCommands.Enqueue(_cmd);
                        ThreadId_DbCommandPairs.Remove(nId);
                        Threads.RemoveAt(i);
                        ThreadIDs.Remove(nId);
                    }
                    else
                    {
                        int nId = Threads[i].ManagedThreadId;
                        DbCommand _cmd = ThreadId_DbCommandPairs[nId];
                        if (_cmd.Connection.State != System.Data.ConnectionState.Connecting &&
                             _cmd.Connection.State != System.Data.ConnectionState.Executing &&
                             _cmd.Connection.State != System.Data.ConnectionState.Fetching)
                        {
                            ReservedDbCommands.Enqueue(_cmd);
                            ThreadId_DbCommandPairs.Remove(nId);
                            Threads.RemoveAt(i);
                            ThreadIDs.Remove(nId);
                        }

                    }
                }
            }
            catch ( Exception ex)
            {

            
            }
          

        }
        public DbCommand AvailableCommand  
        {

            get
            {
                restart:;
                DbCommand command = null;
                int threadid = Thread.CurrentThread.ManagedThreadId;
                if (!this.ThreadIDs.Contains(threadid))
                {
                    if (ReservedDbCommands.Count > 0)
                        command = ReservedDbCommands.Dequeue();
                    else
                     command = NewCommand();
                    Threads.Add(Thread.CurrentThread);
                    ThreadId_DbCommandPairs.Add(threadid, command);
                    ThreadIDs.Add(threadid);
                }
               else
                { 
                    command = ThreadId_DbCommandPairs[threadid];
                }

               if(command.Connection.State != System.Data.ConnectionState.Closed  &&
                    command.Connection.State != System.Data.ConnectionState.Broken
                    )
                return command;
               else
                {
                    bool isMainDbCommand = false;
                    if (command == MainDbCommand)
                        isMainDbCommand = true;
                    ManualRemoveDbCmd(command);
                    if (isMainDbCommand)
                    {
                        MainDbCommand = NewCommand();
                        MainDbConnection = MainDbCommand.Connection;
                        ReservedDbCommands.Enqueue(command);

                    }

                    goto restart;
                }
           }
            
        }
      
        public void Dispose()
        {

            tmr.Dispose();
            foreach (var item in ThreadId_DbCommandPairs)
                {
                    try
                    {
                        item.Value.Dispose();
                        item.Value.Connection?.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        
                    }
                    
                    
                }
                
             
           

            

        }
    }

   
}