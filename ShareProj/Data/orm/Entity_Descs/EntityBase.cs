﻿using System;

namespace dotNetLab.Data.Orm
{
    /// <summary>
    /// 1.entity的数据必须为属性
    /// 2.可以检测表结构发生了变更
    ///  需要注意的是要在Entity中删除相应的属性会删除列!!!要先备份!!!
    /// 3.不允许更改entity 属性的上下次序（数据库的表已经存在的情况下）
    /// </summary>
    public abstract class EntityBase : Entry
    {
        public String TableName = null;
        public Exception ex;
     
        public EntityBase()
        {
            InternalInit();
        }

        /// <summary>
        /// 快速获得反射信息
        /// </summary>
        public override void InternalInit()
        {
            if (this.OrmHost != null)
            {
                String tblName = OrmHost.GetTableName(this.GetType(), this);
                EntityInfo eif = OrmHost.TableManager[tblName];
                this.pifs = eif.pifs;
                this.PropertyNameSet = eif.PropertyNames;
                this.PrimaryKeyPropertyName = eif.PrimaryKeyPropertyName;
                this.PrimaryKeyPropertyQuote = eif.PrimaryKeyPropertyQuote;
                this.PrimaryPropertyInfo = eif.PrimaryPropertyInfo;
            }
        }

        /// <summary>
        /// 保存或者更新一条记录(速度慢)
        /// 默认使用混合模式
        /// </summary>
        /// <param name="mode">INSERT,UPDATE,MIXED</param>
        /// <param name="tableName">另外指定表名</param>
        /// <param name="args"></param>
        public override void Save(SaveMode mode= SaveMode.MIXED,String tableName=null  )
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
            switch (mode)
            {
                case SaveMode.INSERT:
                    ex= OrmHost?.ISave(this );
                    break;
                case SaveMode.UPDATE:
                   ex= OrmHost?.USave(this );
                    break;
                case SaveMode.MIXED:
                   ex= OrmHost?.Save(this );
                    break;
               
            }
           
            TableName = null;
        }
      
    }
}
