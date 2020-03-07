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
        public Dictionary<int,List< DbCommand> > ThreadId_DbCommandPairs = new Dictionary<int, List <DbCommand> >();
        public List<int> ThreadIDs = new List<int>();
        public List<Thread> Threads = new List<Thread>();
        public Queue<DbCommand> ReservedDbCommands = new Queue<DbCommand>();

        int MainThreadId;
        
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

        public void PrepareFirstUse(DbConnection ThisDbConnection)
        {
            DbCommand command = ThisDbConnection.CreateCommand();
            MainDbCommand = command;
            ReservedDbCommands.Enqueue(command);
            MainThreadId = Thread.CurrentThread.ManagedThreadId;

        }

         DbCommand NewCommand( )
        {

            DbConnection conn = System.Activator.CreateInstance(ConnectionType) as DbConnection;
            conn.ConnectionString = connectionString;
            conn.Open();

            DbCommand command = conn.CreateCommand();
        

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
        
            ReservedDbCommands.Enqueue(command);
            return command;
        }

        void CycleCommands()
        {
            List<DbCommand> cmds = null;
            //回收已经完成的线程，Dbcommand
            for (int i = 0; i < Threads.Count; i++)
            {
                if (!Threads[i].IsAlive)
                {

                    int nId = Threads[i].ManagedThreadId;
                     cmds = ThreadId_DbCommandPairs[nId];
                    for (int j = 0; j < cmds.Count; j++)
                    {
                        ReservedDbCommands.Enqueue(cmds[j]);
                    }
                    Threads.RemoveAt(i);
                    ThreadIDs.Remove(nId);
                    ThreadId_DbCommandPairs.Remove(nId);

                }
            }
            try
            {
                cmds = ThreadId_DbCommandPairs[MainThreadId];
                for (int j = 1; j < cmds.Count; j++)
                {
                    if (cmds[j].Connection.State != System.Data.ConnectionState.Executing && cmds[j].Connection.State != System.Data.ConnectionState.Fetching)
                    {
                        ReservedDbCommands.Enqueue(cmds[j]);
                        cmds.RemoveAt(j);
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message + " "+ex.StackTrace);
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
                      
                    Threads.Add(Thread.CurrentThread);
                    List<DbCommand> lstDbCommand = new List<DbCommand>();
                    lstDbCommand.Add(command);
                    ThreadId_DbCommandPairs.Add(threadid, lstDbCommand);
                    ThreadIDs.Add(threadid);
                }
               else
                { 
                    command = ThreadId_DbCommandPairs[threadid][0];
                    if(command.Connection.State == System.Data.ConnectionState.Executing|| command.Connection.State == System.Data.ConnectionState.Fetching)
                    {
                        if (ReservedDbCommands.Count > 0)
                            command = ReservedDbCommands.Dequeue();
                        else
                            command = NewCommand();
                        ThreadId_DbCommandPairs[threadid].Add(command);
                    }

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
                         item.Value.ForEach(x => x.Dispose());
                        item.Value.ForEach(x => x.Connection.Close()) ;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        
                    }
                    
                    
                }
                
             
           

            

        }
    }

   
}