﻿using System;
using System.IO;
using System.Reflection;
using dotNetLab.Common;

namespace dotNetLab.Data.Orm
{
    /// <summary>
    /// 记录应用程序产生的错误日志到数据库中
    /// </summary>
  public  class AppLogEntity : EntityBase
    {
        public String LogFileDirPath = "Logs";
        public static EntityInfo LogEntityInfo = null;

        

        public AppLogEntity(String LogFileDirPath)
        {
            if(LogFileDirPath!=null)
            this.LogFileDirPath = LogFileDirPath;

        }

        public enum LogLevels
        {
              ERROR,INFO,DEBUG
        }

    
        [DBKey]
        public int Id { get; set; }
        public DateTime LogFiredTime { get; set; }
        public String Status { get; set; }
        [MysqlTextType("LONGTEXT")]
        [PostgresqlTextTypeAttribute("TEXT")]
        [SQLiteTextType("Text")]
        [SQLCETextType("ntext")]
        public String Message { get; set; }
        EntityInfo GetEntityInfo(PropertyInfo[] pifs)
        {
            EntityInfo entityInfo = new EntityInfo();
            entityInfo.pifs = pifs;

            foreach (var item in pifs)
            {
                entityInfo.PropertyNames.Add(item.Name);

                DBKeyAttribute attr = item.GetCustomAttribute<DBKeyAttribute>();

                if (attr != null)
                {
                    if (attr.KeyDescription.Equals(DBKeyAttribute.PRIMARYKEY))
                    {
                        entityInfo.PrimaryKeyPropertyName = item.Name;
                        entityInfo.PrimaryPropertyInfo = item;
                        String propertyTypeName = item.PropertyType.Name;
                        if (propertyTypeName.Equals("DateTime") || propertyTypeName.Equals("String"))
                            entityInfo.PrimaryKeyPropertyQuote = true;
                        else
                            entityInfo.PrimaryKeyPropertyQuote = false;
                    }
                }
            }

            return entityInfo;
        }
        public override void InternalInit()
        {
            if (OrmHost != null)
            {
                this.pifs = this.GetType().GetProperties();
                if (LogEntityInfo == null)
                {
                  
                    LogEntityInfo = GetEntityInfo(pifs);
                }

                EntityInfo eif = LogEntityInfo;
                this.PropertyNameSet = eif.PropertyNames;
                this.PrimaryKeyPropertyName = eif.PrimaryKeyPropertyName;
                this.PrimaryKeyPropertyQuote = eif.PrimaryKeyPropertyQuote;
                this.PrimaryPropertyInfo = eif.PrimaryPropertyInfo;
            }
            
        }


        /// <summary>
        /// 保存或者更新一条记录(速度慢)
        /// 对于日志保存不需要指定日志表名
        /// 默认使用混合模式
        /// </summary>
        /// <param name="mode">INSERT,UPDATE,MIXED</param>
        /// <param name="tableName">另外指定表名</param>
        /// <param name="args"></param>
        public override void Save(SaveMode mode = SaveMode.MIXED, String tableName = null )
        {
            lock (OrmHost)
            {
                try
                {
                    if (tableName != null && tableName.EndsWith("Entity") && tableName != "Entity")
                    {
                        if (tableName.EndsWith("_Entity"))
                        {

                            tableName = tableName.Remove(tableName.Length - "_Entity".Length, "_Entity".Length);

                        }
                        if (tableName.EndsWith("Entity"))
                            tableName = tableName.Remove(tableName.Length - "Entity".Length, "Entity".Length);
                    }


                    this.TableName = tableName;
                    if (TableName == null)
                    {
                        string formatedTimeString = LogFiredTime.ToString("yyyy-MM-dd HH:mm:ss");
                        String msgContent = String.Format("{0} {1} {2}\r\n", formatedTimeString
                            , Status, Message);
                        Console.WriteLine(msgContent);
                        String _tableName = String.Format("_{0}_Log", DateTime.Now.ToString("yyyy_MM"));
                        TableName = _tableName;

                        if (!Directory.Exists(LogFileDirPath))
                            Directory.CreateDirectory(LogFileDirPath);
                        String txtLogFilePath = Path.Combine(LogFileDirPath, TableName + ".txt");

                        File.AppendAllText(txtLogFilePath, msgContent);
                        OrmHost.AddTable(typeof(AppLogEntity) , TableName, false);
                    }

                    switch (mode)
                    {
                        case SaveMode.INSERT:
                            OrmHost?.ISave(this );
                            break;
                        case SaveMode.UPDATE:
                            OrmHost?.USave(this );
                            break;
                        case SaveMode.MIXED:
                            OrmHost?.Save(this );
                            break;

                    }

                    TableName = null;
                }
                catch (Exception ex)
                {

                }
      
        }

    }

        

    }
}
