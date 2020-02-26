﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 

 namespace dotNetLab.Data 
{
   public class DBManager
    {
       public DBPlatform  ThisDBPlatform;
       
        /// <summary>
        /// 连接到Sqlite/SQLCE
        /// </summary>
        /// <param name="SqliteDbFilePath"></param>
        /// <returns></returns>
        public bool Connect(String SqliteDbFilePath,bool isSQLCE=false)
        {
            if (ThisDBPlatform == null)
            {
                if (!isSQLCE)
                {
                   ThisDBPlatform = new SQLiteDBEngine();
                    
                }
                else
                {
                    ThisDBPlatform = new SQLCEDBEngine();
                }

            }

            bool b =false ;
            if(!isSQLCE)
              b  = ((SQLiteDBEngine)ThisDBPlatform).Connect(SqliteDbFilePath);
            else
              b = ((SQLCEDBEngine)ThisDBPlatform).Connect(SqliteDbFilePath);
            return b;
        }

        /// <summary>
        /// 连接到MySQL/Postgresql/SQL SERVER/LOCALDB/FireBird
        /// </summary>
        /// <param name="DBName"></param>
        /// <param name="UserName"></param>
        /// <param name="Pwd"></param>
        /// <returns></returns>
        public bool Connect (Type type_DBEngine,string DBName, string UserName, string Pwd)  
        {
            if (ThisDBPlatform == null)
                ThisDBPlatform = (DBPlatform) System.Activator.CreateInstance(type_DBEngine);
            IHugeDB alterDB = ThisDBPlatform as IHugeDB;
            bool b = alterDB.Connect(DBName, UserName, Pwd);
         
            return b;
        }

        /// <summary>
        /// 远程连接到MySQL/Postgresql/SQL SERVER/LOCALDB/FireBird
        /// </summary>

        public bool Connect(Type type_DBEngine,String ip, int port, string  DBName, string UserName, string  Pwd)  
        {
            if (ThisDBPlatform == null)
                ThisDBPlatform = (DBPlatform)System.Activator.CreateInstance(type_DBEngine);
            IHugeDB alterDB = ThisDBPlatform as IHugeDB;
            bool b = alterDB.Connect(ip, port,   DBName ,UserName,  Pwd );
            return b;
        }


       

    }
}
