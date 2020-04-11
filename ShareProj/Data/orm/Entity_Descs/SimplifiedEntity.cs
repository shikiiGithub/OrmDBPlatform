﻿using dotNetLab.Common;
using dotNetLab.Data.Orm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace dotNetLab.Data.Orm
{
   public abstract class SimplifiedEntity:Entry
    {
        public Type HostEntityType;
        String insertInComplatedValuesColumns = null;
        public Object PrimaryKey = null;
        
    

        public override void InternalInit()
        {
            if (this.OrmHost != null)
            {
                Reflect();
                StringBuilder sb = new StringBuilder();
                sb.Append(" ( ");
                for (int i = 0; i < PropertyNameSet.Count; i++)
                {
                    OrmAttribute attr = pifs[i].GetCustomAttribute<DBKeyAttribute>();
                    if (attr != null && !(pifs[i].PropertyType == typeof(String) ||
                                          pifs[i].PropertyType == typeof(DateTime)))
                        continue;
                    sb.Append(PropertyNameSet[i] + ",");
                }

                sb.Remove(sb.Length - 1, 1);
                sb.Append(" ) ");
                insertInComplatedValuesColumns = sb.ToString();
            }
        }

        public override String GetPrimaryKeyValue()
        {
            //PropertyInfo pif = this.GetType().GetProperty(PrimaryKeyPropertyName);
            //String tmp = pif.GetValue(this).ToString();
            if (PrimaryKey == null)
            {
                return null;
            }


            String tmp = PrimaryKey.ToString();

            if (PrimaryKeyPropertyQuote)
            {
                tmp = String.Format("'{0}'", tmp);
            }
            return tmp;
        }
        protected  void Reflect()
        {
            pifs = this.GetType().GetProperties();
            PropertyNameSet = new List<string>();
            foreach (var item in pifs)
            {
                PropertyNameSet.Add(item.Name);
            }
        }
        //确定entity 中的属性类型的值转为sql 时要不要带上‘’
        String GetEntityPropertyValue(Type type, Object obj)
        {
            String typeName = type.Name;
            switch (typeName)
            {

                case "String":

                case "DateTime":
                    return $"'{obj}'";
                default:
                    return obj.ToString();

            }


        }

        /// <summary>
        /// 更新一条记录,注意请保持SaveMode为SaveMode.UPDATE
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="TableName"></param>
        /// <param name="args"></param>
        public override void Save( EntitySaveMode mode = EntitySaveMode.UPDATE,String TableName =null )
        {
            try
            {
                SimplifiedEntity entity = this;
                String tableName = null;
                Type EntityType = null;
                PropertyInfo[] pifs;
                StringBuilder sb;
                EntityType = entity.GetType();
                if (TableName == null)
                {
                    String __tableName = OrmHost.GetTableName(this.HostEntityType, null);
                    tableName = __tableName;
                }
                else
                {
                    tableName = TableName;
                    if (tableName != null && tableName.EndsWith("Entity") && tableName != "Entity")
                    {
                        if (tableName.EndsWith("_Entity"))
                        {

                            tableName = tableName.Remove(tableName.Length - "_Entity".Length, "_Entity".Length);

                        }
                        if (tableName.EndsWith("Entity"))
                            tableName = tableName.Remove(tableName.Length - "Entity".Length, "Entity".Length);

                    }
                }

                pifs = entity.pifs;
                sb = new StringBuilder();
                sb.Clear();
                foreach (var item in pifs)
                {
                      if (item.Name.Equals("Id"))
                        {
                            continue;
                        }
                        DBKeyAttribute attr = item.GetCustomAttribute<DBKeyAttribute>();
                        if (attr != null && attr.KeyDescription.Equals(DBKeyAttribute.PRIMARYKEY))
                            continue;
                        Object obj = item.GetValue(entity,null);
                    String value = obj?.ToString();


                    if (value != null && value.Contains("'"))
                        value = value.Replace("\'", "\'\'");

                      value = GetEntityPropertyValue(item.PropertyType, obj);
                        sb.Append(
                            String.Format("{0}={1},", item.Name, value)
                            );
                 }
                    String sql = sb.ToString().TrimEnd(',');
                    OrmHost.AdonetContext.Update(tableName, sql +
                        String.Format(" where {0}={1}", entity.PrimaryKeyPropertyName, entity.GetPrimaryKeyValue())  
                        );
            
            }
            catch (Exception e)
            {
                OrmHost?.AdonetContext.PerformErrorHandler(this, e);
            }
        }

    }
}
