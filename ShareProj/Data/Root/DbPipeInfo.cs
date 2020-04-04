﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;


namespace dotNetLab.Data
{
    public class DbPipeInfo:IDisposable
    {
        public Dictionary<int, DbCommand> ThreadId_DbCommandPairs = new Dictionary<int, DbCommand>();
        public List<int> ThreadIDs = new List<int>();
        public List<Thread> Threads = new List<Thread>();
        public static Queue <DbCommand> ReservedDbCommands = new Queue<DbCommand>();
        Type ConnectionType;
        String connectionString;
        public DbConnection MainDbConnection;
        public DbCommand MainDbCommand;
        private Timer tmr;
        public int CycleCmdGapTimeInMillSecs = 1000;

      
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


        public void PrepareFirstUse(DbConnection ThisDbConnection)
        {
            DbCommand command = ThisDbConnection.CreateCommand();
            MainDbCommand = command;
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
            ReservedDbCommands.Enqueue(command);
            return command;
        }

        void CycleCommands()
        {
            
            //回收已经完成的线程，Dbcommand
            for (int i = 0; i < Threads.Count; i++)
            {
                if(!Threads[i].IsAlive)
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
                   if( _cmd.Connection.State != System.Data.ConnectionState.Connecting &&
                        _cmd.Connection.State != System.Data.ConnectionState.Executing && 
                        _cmd.Connection.State != System.Data.ConnectionState.Fetching )
                    {
                        ReservedDbCommands.Enqueue(_cmd);
                        ThreadId_DbCommandPairs.Remove(nId);
                        Threads.RemoveAt(i);
                        ThreadIDs.Remove(nId);
                    }

                }
            }

        }
        public DbCommand AvailableCommand  
        {

            get
            {
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
                return command;
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