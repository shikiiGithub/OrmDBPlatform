﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using dotNetLab.Common;
namespace dotNetLab.Data.Orm
{
   public abstract class Entry
    {
        public OrmDBPlatform OrmHost;
        public PropertyInfo[] pifs;
        public List<String> PropertyNameSet;
        public String PrimaryKeyPropertyName;
        public bool PrimaryKeyPropertyQuote = false;
        public enum SaveMode { INSERT, UPDATE, MIXED }
       
        static OrmDBPlatform InternalOrmDBPlatform = null;
       
        protected PropertyInfo PrimaryPropertyInfo = null;
        
        
        public Entry()
        {

            if (InternalOrmDBPlatform != null)
            {
                OrmHost = InternalOrmDBPlatform;
                
            }

            InternalInit();
        }


        public abstract void InternalInit();
        
        public void AssignValue(String PropertyName, Object obj)
        {
            int nIndex = PropertyNameSet.IndexOf(PropertyName);
            PropertyInfo pif=null;
            if (nIndex == -1)
            {
                foreach (var item in pifs)
                {
                    if (item.Name.ToLower().Equals(PropertyName.ToLower()))
                        pif = item;

                }
            }
            else
                pif = this.pifs[nIndex];
            try
            {
                if (PropertyName.Equals("Id") )
                {
                    if(this.OrmHost.AdonetContext.GetType().Equals("SQLiteDBEngine"))
                      pif.SetValue(this, (int)obj, null);
                    else 
                        pif.SetValue(this, (int)obj, null);


                }
                else
                    pif.SetValue(this, obj, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"从数据库中取出实体的{PropertyName}字段的值失败，位于{ex.StackTrace}");
                OrmHost?.AdonetContext.PerformErrorHandler(this, ex);
            }
        }

        public virtual String GetPrimaryKeyValue()
        {
            PropertyInfo pif = PrimaryPropertyInfo;
            String tmp = pif.GetValue(this,null).ToString();
            if (PrimaryKeyPropertyQuote)
            {
                tmp = String.Format("'{0}'", tmp);
            }
            return tmp;
        }


        public abstract void Save(SaveMode mode = SaveMode.MIXED, String tableName = null);


        public string MapJson<T>( string [] arr , params Expression<Func<T,string>> [] Properties ) where T: Entry
        {
            if (arr.Length != Properties.Length)
                return "arr.Length != Properties.Length";
            System.Dynamic.ExpandoObject internalObject = new System.Dynamic.ExpandoObject();
            
                IDictionary<String, object> dict = internalObject;

            Type type = this.GetType();
            for (int i = 0; i < arr.Length; i++)
            {
                String propertyName = Properties[i].Body.ToString();
                int nindex =PropertyNameSet.FindIndex(x => x == propertyName);
                 Object val = pifs[nindex].GetValue(this);
                dict.Add(arr[i], val);

            }
             return  LitJson.JsonMapper.ToJson(internalObject);
        }

        ~Entry()
        {
            this.OrmHost?.LogInfo("Entity 销毁了");
        }

    }
}
