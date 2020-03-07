﻿﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
#if NET4
using System.Linq;
using System.Linq.Expressions;
#endif
using System.Reflection;
using System.Text;
using System.Threading;
using dotNetLab.Common;
 
namespace dotNetLab.Data.Orm
{
     
  public  class OrmDBPlatform:IDisposable
  {

        
        public Type type_EngineType = null;
        /// <summary>
        /// LightMode轻模式使用
        /// </summary>
        public bool LightMode = false;
        //数据库对象管理中心
        public dotNetLab.Data.DBManager ThisDBManager;
        /// <summary>
        /// 默认赋值用于简化实体.save
        /// </summary>
        FieldInfo Entry_InternalOrmDBPlatform_FieldInfo = null;

        /// <summary>
        /// 托管所有实体所对应的表信息
        /// </summary>
        public Dictionary<String, EntityInfo> TableManager;
        /// <summary>
        /// 是否允许应用程序可以实例化多个OrmDBPlatform对象
        /// </summary>
        public bool FixThisOrmDBPlatformObject = true;

        public ErrorCallback ErrorInvoker = null;
        public InfoCallback InfoInvoker=null; 
        public Action<OrmDBPlatform> BootDBAction = null;
         Stopwatch MyStopwatch = new Stopwatch();
        /// <summary>
        /// 内部测时
        /// </summary>
        void BeginMeasureTime()
        {
            MyStopwatch.Reset();
            MyStopwatch.Start();
             
        }
        /// <summary>
        /// 内部测时
        /// </summary>
        void EndMeasureTime()
        {
            MyStopwatch.Stop();

            Console.WriteLine("耗时：" + MyStopwatch.Elapsed.TotalMilliseconds + " ms");
        }

        /// <summary>
        /// 兼容ado.net 原生操作
        /// 和键值对的操作
        /// </summary> 
        public DBPlatform AdonetContext
        {
            get { return ThisDBManager?.ThisDBPlatform; }
        }
        /// <summary>
        /// true --禁止应用程序可以实例化多个OrmDBPlatform对象
        /// false --允许应用程序可以实例化多个OrmDBPlatform对象
        /// 如果非并发情况下请默认使用true
        /// 在创建Entity时可以不使用GetEntity<T>
        /// </summary>
        public OrmDBPlatform(bool FixThisOrmDBPlatformObject=true)
        {
            this.FixThisOrmDBPlatformObject = FixThisOrmDBPlatformObject;
            Console.WriteLine("OrmDBPlatform 初始化了");
        }

        public  void LogError(Exception ex, String LogFileDirPath = null)
        {
            if (AppLogEntity.LogEntityInfo != null)
            {
                lock (AppLogEntity.LogEntityInfo)
                {

                    AppLogEntity entity = null;
                    if (!this.FixThisOrmDBPlatformObject)
                        entity = GetEntity<AppLogEntity>(LogFileDirPath);
                    else
                        entity = new AppLogEntity(LogFileDirPath);
                    entity.LogFiredTime = DateTime.Now;
                    entity.Status = AppLogEntity.LogLevels.ERROR.ToString();
                    entity.Message = ex.Message + " " + ex.StackTrace;
                
                   
                        entity.Save(Entry.SaveMode.INSERT);

                }

            }

            
        }

        public  void LogInfo(String msg, String LogFileDirPath = null)
        {
            if (AppLogEntity.LogEntityInfo == null&& !this.FixThisOrmDBPlatformObject)
            {
                AppLogEntity entity = null;
                    entity = GetEntity<AppLogEntity>(LogFileDirPath);
                
            }
                lock (AppLogEntity.LogEntityInfo)
                {
                    AppLogEntity entity = null;
                    if (!this.FixThisOrmDBPlatformObject)
                        entity = GetEntity<AppLogEntity>(LogFileDirPath);
                    else
                        entity = new AppLogEntity(LogFileDirPath);

                    entity.LogFiredTime = DateTime.Now;
                    entity.Status = AppLogEntity.LogLevels.INFO.ToString();
                    entity.Message = msg;
                entity.Save(Entry.SaveMode.INSERT);
            }
             
        }

        /// <summary>
        /// 在允许应用程序可以实例化多个OrmDBPlatform对象时
        /// 必须使用用下列方法来获得Entity
        /// </summary>
        /// <typeparam name="T">Entity</typeparam>
        /// <returns></returns>
        public virtual T GetEntity<T>(params object [] args) where T : Entry
        {
            Entry entry = null;
            entry = (Entry) System.Activator.CreateInstance(typeof(T),args);
            entry.OrmHost = this;
            entry.InternalInit();
            return (T)entry;
        }

        /// <summary>
        /// 省得第将都要对Entity 实例赋值 OrmHost = this ;
        ///  Entity.Save(...)就不用考虑那么多东西
        /// </summary>
        void AssignMeToEntry()
        {
            if (FixThisOrmDBPlatformObject)
            {
                if (Entry_InternalOrmDBPlatform_FieldInfo == null)
                    Entry_InternalOrmDBPlatform_FieldInfo = typeof(Entry).GetField("InternalOrmDBPlatform", BindingFlags.NonPublic | BindingFlags.Static);
                Entry_InternalOrmDBPlatform_FieldInfo.SetValue(null, this);
            }
           
            TableManager = new Dictionary<string, EntityInfo>();
           
        }
        /// <summary>
        /// 注册日志输出,如果没有指定日志事件（Error/Info）
        /// 则使用默认的日志处理（这将导致一定耗时）
        /// </summary>
        void EndInit()
        {
            if(ErrorInvoker==null)
                ErrorInvoker = (obj, ex) =>
                {
                    Console.WriteLine("使用默认日志处理异常事件");
                    LogError(ex);
                };
            if (InfoInvoker == null)
                InfoInvoker = (obj, info) =>
                    {
                        Console.WriteLine("使用默认日志处理异常事件");
                        LogInfo(info.ToString());
                    };
            AdonetContext.ErrorHandler += ErrorInvoker;
            AdonetContext.InfoHandler += InfoInvoker;
        }
        /// <summary>
        /// 指定文件的详细信息
         /// </summary>
         /// <param name="path">指定文件的路径</param>
         String GetADONETDllProductName(string path)
            {
                System.IO.FileInfo fileInfo = null;
                try
                {
                    fileInfo = new System.IO.FileInfo(path);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    // 其他处理异常的代码
                    return null;
                }
                // 如果文件存在
                if (fileInfo != null && fileInfo.Exists)
                {
                    System.Diagnostics.FileVersionInfo info = System.Diagnostics.FileVersionInfo.GetVersionInfo(path);
                if (info.ProductName != "")
                    return info.ProductName;
                else
                    return info.InternalName;
                }
                else
                {
                    Console.WriteLine("指定的文件路径不正确!");
                  
                }

                return null;
            }
  
        /// <summary>
        /// Sqlite / MySQL 数据库可不必调用此方法
        /// 创建所有已知表,只是为非Sqlite / MySQL 数据库提供支持
        /// </summary>
        /// <param name="EntitySourceAssembly">包含Entity类文件的程序集</param>
        public void CreateAllTable(Assembly EntitySourceAssembly)
        {
            //创建应用程序表（键值对（用于存储配置等数据））
             AddTable<dotNetLab.Data.Orm.AppEntity>() ;
            //如果LightMode 为true 则只收集表信息
            if (LightMode)
            {
                if (this.TableManager.Count == 0)
                {
                    if (BootDBAction == null)
                        throw new Exception("在轻模式下需要手动将已经存在的\r\nOrmDBPlatform 对象的 TableManager 赋值过来！是否未启动数据库？");
                    
                       
                }
                return;
            }
            if (EntitySourceAssembly != null)
            {
                Type[] types = EntitySourceAssembly.GetTypes();
                for (int i = 0; i < types.Length; i++)
                {
                   
                    if (types[i].BaseType == null)
                        return;
                    if (types[i].BaseType.Equals(typeof(EntityBase)))
                    {
                        //检测实体类上的特性，如果特性中提示为手动创建表，则该表不会自动创建
                        //而应该手动创建
                        EntityAttribute entityAttribute = types[i].GetCustomAttribute<EntityAttribute>();
                        if (entityAttribute != null && entityAttribute.EntityDescription == EntityAttribute.MANUAL_CREATE_TABLE)
                        {
                            continue;
                        }
                        //自动创建表
                        this.AddTable(types[i], null, true);
                    }
                }

            }
        }

        void Init(Type type_DBEngine, String DBName = "shikii", String usrName = "root",   String pwd = "123", params Assembly[] EntitySourceAssemblies)
        {
            ThisDBManager = new DBManager();
           
            //SQLITE
            if (type_DBEngine == typeof(SQLiteDBEngine))
            {

                if (!DBName.EndsWith(".db"))
                    DBName = DBName + ".db";
                ThisDBManager.Connect(DBName );
            
            }
           
            else if (type_DBEngine.Name == "SQLCEDBEngine")
            {
                if (!DBName.EndsWith(".sdf"))
                    DBName = DBName + ".sdf";
                ThisDBManager.Connect(DBName,true);
               
            }
            else
            {
                ThisDBManager.Connect(type_DBEngine, DBName, usrName, pwd);
             
            }

            if (EntitySourceAssemblies == null)
                EntitySourceAssemblies = new Assembly[] { Assembly.GetEntryAssembly() };
            
            for (int i = 0; i < EntitySourceAssemblies.Length; i++)
            {
             CreateAllTable(EntitySourceAssemblies[i]);

            }

        }

        /// <summary>
        /// 初始化连接远程的数据库
        /// </summary>
        void Init(Type type_DBEngine, String ip, int port, String DBName = "shikii", String pwd = "123", String usrName = "root", params Assembly[] EntitySourceAssemblies)
        {
            ThisDBManager =  new DBManager();
      
            bool temp = false;
            if (type_DBEngine == typeof(SQLiteDBEngine))
            {

                if (!DBName.EndsWith(".db"))
                    DBName = DBName + ".db";
                temp = ThisDBManager.Connect(DBName );
           
            }
            else if (type_DBEngine.Name == "SQLCEDBEngine")
            {
                if (!DBName.EndsWith(".sdf"))
                    DBName = DBName + ".sdf";
                  temp = ThisDBManager.Connect(DBName, true);
                
            }

            else
            {
                   temp = ThisDBManager.Connect (type_DBEngine,ip, port, DBName, usrName, pwd);
                     
            }
            if (EntitySourceAssemblies == null)
                EntitySourceAssemblies = new Assembly[] { Assembly.GetEntryAssembly() };
            for (int i = 0; i < EntitySourceAssemblies.Length; i++)
            {
                CreateAllTable(EntitySourceAssemblies[i]);

            }

        }
       

        //C#数据类型转数据库类型
        public String IdentifyEntityPropertyType(PropertyInfo pif,Type type)
        {
            String typeName = type.Name;
            ITableInfo kv = this.AdonetContext as ITableInfo;
            return kv.InferDataType(pif, typeName).ToLower();
        }

        //确定entity 中的属性类型的值转为sql 时要不要带上‘’
          String GetEntityPropertyValue (Type type,Object obj)
        {
            String typeName = type.Name;
            switch (typeName)
            {
               
                case "String":
                    
                case "DateTime":
                     String val  =  String.Format("'{0}'", MakeSingleQuotesSenseToDB(obj));
                    return val;
                default:
                    return obj.ToString();

            }
            

        }
        //entity的数据必须为属性
        public void AddTable<T> () where T : EntityBase
        {
            AddTable<T>(null );
        }
        /// <summary>
        /// 添加一张表，表结构与T 实体相同，但是不与T实体类名相同或者完全不同
        /// </summary>
        /// <typeparam name="T">T 实体</typeparam>
        /// <param name="TableName"></param>
        public void AddTable<T>(String TableName) where T : EntityBase
        {
            AddTable (typeof(T),TableName);
        }

        /// <summary>
        /// 获得表名，传入Entity对象的原因是Entity对象的TableName不为null
        /// </summary>
        /// <param name="EntityType">Entity 的数据类型</param>
        /// <param name="entity">Entity 对象</param>
        /// <returns></returns>
        public String GetTableName(Type EntityType,EntityBase entity)
        {
            String tableName = EntityType.Name;
            
            if(entity !=null && entity.TableName!= null)
            {
                tableName = entity.TableName;
            }
            else if (tableName.EndsWith("Entity") && tableName != "Entity")
            {
                if (tableName.EndsWith("_Entity"))
                {

                    tableName = tableName.Remove(tableName.Length - "_Entity".Length, "_Entity".Length);

                }
                if (tableName.EndsWith("Entity"))
                    tableName = tableName.Remove(tableName.Length - "Entity".Length, "Entity".Length);

            }

            return tableName;
        }
         /// <summary>
         /// 向上文注册表信息（列）
         /// </summary>
         /// <param name="EntityType">实体类Type</param>
         /// <param name="_TableName">表名</param>
        public void RegisterTable(Type EntityType, String _TableName)
        {
            String tableName = _TableName;
            if (_TableName == null)
                tableName = GetTableName(EntityType, null);
            PropertyInfo[] pifs = EntityType.GetProperties();
                this.TableManager.Add(tableName, GetEntityInfo(pifs));
        }
        /// <summary>
        /// 向数据库中添加表，如果表存在则检测实体信息变更
        /// </summary>
        /// <param name="EntityType">实体类Type</param>
        /// <param name="_TableName">表名(为空则为实体类名去掉Entity)</param>
        /// <param name="recordEntityInfo">是否需要注册表的列信息（内部使用，不必传值）</param>
        public void AddTable(Type EntityType , String _TableName,bool recordEntityInfo=true)
        {
            try
            {

                String tableName = _TableName;
                if (_TableName == null)
                    tableName = GetTableName(EntityType, null);
                ITableInfo kv = AdonetContext as ITableInfo;
                List<String> tbls = kv.GetAllTableNames( );
                PropertyInfo[] pifs = EntityType.GetProperties();
                if(recordEntityInfo)
                this.TableManager.Add(tableName, GetEntityInfo(pifs));
                

                String str = null;

                //检测表结构发生了变更或者表已经存在
                for (int i = 0; i < tbls.Count; i++)
                {
                    if (tbls[i].ToLower().Equals(tableName.ToLower()))
                    {
                        //检测表结构发生了变更
                        //需要注意的是删除列1.用dbms 删除数据库表中的列2.需要在Entity中删除相应的属性
                        if (this.AdonetContext is IHugeDB)
                        {
                            List<String> lstColNames = kv.GetTableColumnNames(tableName);
                            List<String> lstColTypes = kv.GetTableColumnTypes(tableName, true);
                            //List<String> EntityColumnNames = new List<string>();
                            //List<String> EntityColumnTypes = new List<string>();
                            IHugeDB alterDB = this.AdonetContext as IHugeDB;
                            if (alterDB == null)
                                return;
                            //是否有更改属性名的情况
                            for (int j = 0, z = 0; j < lstColNames.Count; j++, z++)
                            {

                                if (z == pifs.Length)
                                {
                                    alterDB.DropColumn(tableName, lstColNames[j]);
                                    break;
                                }
                                Type type = pifs[z].PropertyType;
                                str = IdentifyEntityPropertyType(pifs[z], type);


                                if (lstColNames[j].ToLower() != pifs[z].Name.ToLower())
                                {
                                    if (lstColNames.Count > pifs.Length)
                                    {
                                        alterDB.DropColumn(tableName, lstColNames[j]);
                                        z--;
                                        continue;
                                    }
                                    else
                                        alterDB.ChangeColumnName(tableName, lstColNames[j], pifs[z].Name, str);
                                }


                                if (!lstColTypes[j].Equals(str))
                                {
                                    // this.AdonetContext.TypeInfer()
                                    if (alterDB.GetType().Name.Equals("MySQLDBEngine") && lstColTypes[j].Contains("varchar") &&
                                         str.Contains("varchar") && lstColTypes[j].Contains("255"))
                                    {
                                        continue;
                                    }
                                    else if (alterDB.GetType().Name.Equals("PostgreSQLEngine")  &&
                                         lstColTypes[j].Contains("varying") &&
                                         str.Contains("varying") && lstColTypes[j].Contains("255")
                                         )
                                    {
                                        continue;
                                    }

                                    alterDB.ChangeColumnType(tableName, lstColNames[j], str);
                                }

                            }

                            lstColNames = kv.GetTableColumnNames(tableName);
                            //是否有增加属性名的情况
                            if (lstColNames.Count != pifs.Length && pifs.Length > lstColNames.Count)
                            {
                                for (int j = lstColNames.Count; j < pifs.Length; j++)
                                {

                                    Type type = pifs[j].PropertyType;
                                    str = IdentifyEntityPropertyType(pifs[j], type);

                                    String attrStr = "";

                                    DBKeyAttribute attr = pifs[j].GetCustomAttribute<DBKeyAttribute>();
                                    if (attr != null)
                                        attrStr = attr.KeyDescription;
                                    if (pifs[j].Name.Equals("Id"))
                                    {
                                        attrStr = GetAutoIncrementMark();
                                        if (AdonetContext.GetType().Name.Equals("PostgreSQLEngine"))
                                            str = " serial ";
                                    }
                                    alterDB.AddColumn(tableName, pifs[j].Name, str + attrStr);
                                }
                            }
                        }
                        return;

                    }
                }
                List<String> ColumnNames = new List<string>();
                List<String> ColumnTypes = new List<string>();
                List<String> AttributeSet = new List<string>();

                String MysqlForeignKey = null;

                foreach (var item in pifs)
                {
                    ColumnNames.Add(item.Name);
                    Type type = item.PropertyType;
                    str = IdentifyEntityPropertyType(item, type);
                    ColumnTypes.Add(str);


                    DBKeyAttribute att = item.GetCustomAttribute<DBKeyAttribute>();
                    if (att != null)
                    {
                        if (att.KeyDescription.Equals(DBKeyAttribute.PRIMARYKEY) || att.KeyDescription.Equals(DBKeyAttribute.UNIQUE))
                            AttributeSet.Add(att.KeyDescription);
                        else if (att.KeyDescription.Equals(DBKeyAttribute.FOREIGN_KEY))
                        {
                            if (this.AdonetContext.GetType().Name.Equals("MySQLDBEngine")  )
                            {
                                MysqlForeignKey = String.Format(DBKeyAttribute.MYSQL_FOREIGN_KEY_FSTRING, item.Name, att.ForeignKeyTable, att.PrimaryKeyName);

                            }
                            else if (AdonetContext.GetType().Name.Equals("PostgreSQLEngine"))
                            {
                                AttributeSet.Add(
                                    String.Format(DBKeyAttribute.POSTGRESQL_FOREIGN_KEY_FSTRING, att.ForeignKeyTable, att.PrimaryKeyName)
                                    );
                            }

                            
                        }


                    }
                    else
                        AttributeSet.Add("");

                    if (item.Name.Equals("Id"))
                    {
                        if (this.AdonetContext.GetType().Name.Equals("PostgreSQLEngine"))
                        {
                            ColumnTypes[ColumnTypes.Count - 1] = " serial ";

                        }
                        else if (AdonetContext is SQLiteDBEngine)
                            ColumnTypes[ColumnTypes.Count - 1] = " INTEGER ";

                        if(att != null)
                            AttributeSet[AttributeSet.Count-1] = AttributeSet[AttributeSet.Count - 1]+ GetAutoIncrementMark();
                        else
                       AttributeSet.Add(GetAutoIncrementMark());
                    }

                }

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < ColumnNames.Count; i++)
                {
                    sb.Append(ColumnNames[i] + " ");
                    sb.Append(ColumnTypes[i] + " ");
                    sb.Append(AttributeSet[i] + ",");

                }
                //除掉逗号
                sb.Remove(sb.Length - 1, 1);

                if (MysqlForeignKey != null)
                    sb.AppendFormat(",{0} ", MysqlForeignKey);
            
                AdonetContext.NewTable(tableName, sb.ToString() );
            }
            catch (Exception ex)
            {
                 AdonetContext.PerformErrorHandler(AdonetContext, ex);
            }
           
        }
        /// <summary>
        /// 获得实体类属性信息
        /// </summary>
        /// <param name="pifs">属性集合</param>
        /// <returns></returns>
        protected EntityInfo GetEntityInfo(PropertyInfo [] pifs)
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
      
        /// <summary>
        /// 获得自增字段的sql 关键字
        /// </summary>
        /// <returns></returns>
        String GetAutoIncrementMark()
        {
            if (AdonetContext is SQLiteDBEngine)
                return " AUTOINCREMENT ";
             
            else if (AdonetContext.GetType().Name.Equals("MySQLDBEngine"))
            {
                return " AUTO_INCREMENT ";
            }
            
            else
                return " ";


        }
        /// <summary>
        /// 引号 ‘ 的转义
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        String MakeSingleQuotesSenseToDB(Object obj)
        {

            String value = obj?.ToString();

            if (value != null && value.Contains("'"))
                value = value.Replace("\'", "\'\'");
            return value;
        }

        /// <summary>
        /// 保存或者更新一条记录
        ///（速度较慢,调试时/数据量少时使用，高速场合请使用ISave/USave）
        /// </summary>
        public Exception Save(EntityBase entity )
        {
            try
            {
                entity.OrmHost = this;
                String tableName = null;
                Type EntityType = null;
                PropertyInfo[] pifs;
                StringBuilder sb;
                EntityType = entity.GetType();
                  tableName = GetTableName(EntityType, entity);

                 pifs = entity.pifs;
                String tmp  = AdonetContext.UniqueResult(String.Format("select count(*) from {0} where {1}={2}",
                        tableName, entity.PrimaryKeyPropertyName, entity.GetPrimaryKeyValue()) 
                        );
                int cnt = 0;
                int.TryParse(tmp, out cnt);

                sb = new StringBuilder();

                StringBuilder _sbPropertyNames = new StringBuilder();
                if (cnt == 0)
                {
                    
                    foreach (var item in pifs)
                    {
                       // OrmAttribute attr = item.GetCustomAttribute<DBKeyAttribute>();

                        //if (attr != null && !(item.PropertyType == typeof(String) || item.PropertyType == typeof(DateTime)))
                        //{
                            
                        //    else
                        //    continue;
                        //}
                        if(item.Name.Equals("Id"))
                        {
                            //自动设置自增值
                            if (this.AdonetContext.GetType().Name.Equals("FireBirdEngine"))
                            {
                                FireBirdEngine fireBird = this.AdonetContext as FireBirdEngine;
                                int id = fireBird.GetAuto_IncrementID(tableName, item.Name );
                                item.SetValue(entity, id, null);
                            }
                            else
                            continue;
                        }

                        Object obj = item.GetValue(entity,null);

                        
                         String value = GetEntityPropertyValue(item.PropertyType, obj);
                        sb.Append(value + ",");
                        _sbPropertyNames.AppendFormat("{0},", item.Name);
                    }
                    _sbPropertyNames.Remove(_sbPropertyNames.Length - 1, 1);
                    String sql = sb.ToString().TrimEnd(',');

                    sql = String.Format("insert into {0} ({1}) values({2}) ; ",
                        tableName, _sbPropertyNames.ToString(), sql);
                    return AdonetContext.ExecuteNonQuery(sql );
                }
                else
                {
                    sb.Remove(0, sb.Length);
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
                        String value = GetEntityPropertyValue(item.PropertyType, obj);
                        sb.Append( 
                            String.Format("{0}={1},",item.Name,value)
                            );
                    }
                    String sql = sb.ToString().TrimEnd(',');
                 return    AdonetContext.Update(tableName, sql +
                        String.Format(" where {0}={1}", entity.PrimaryKeyPropertyName, entity.GetPrimaryKeyValue()) 
                        );
                }
            }
            catch (Exception e )
            {
                this.AdonetContext.PerformErrorHandler(this, e);
                return e;
            }
        }
        protected void AssignValue(Object Host, String PropertyName, Object Value)
        {
            PropertyInfo pif = Host.GetType().GetProperty(PropertyName);
            pif.SetValue(Host, Value, null);
        }
        /// <summary>
        /// 高速场合保存新增记录（insert)
        /// </summary>
        public Exception ISave(EntityBase entity )
        {
            try
            {

                

              

                entity.OrmHost = this;
                String tableName = null;
                Type EntityType = null;
                PropertyInfo[] pifs;
                StringBuilder sb;
                EntityType = entity.GetType();
                tableName = GetTableName(EntityType, entity);

                pifs = entity.pifs;
                sb = new StringBuilder();

                StringBuilder _sbPropertyNames = new StringBuilder();
                foreach (var item in pifs)
                    {
                        // OrmAttribute attr = item.GetCustomAttribute<DBKeyAttribute>();

                        //if (attr != null && !(item.PropertyType == typeof(String) || item.PropertyType == typeof(DateTime)))
                        //{

                        //    else
                        //    continue;
                        //}
                        if (item.Name.Equals("Id"))
                        {
                            //自动设置自增值 
                            if (this.AdonetContext.GetType().Name=="FireBirdEngine")
                            {
                                FireBirdEngine fireBird = this.AdonetContext as FireBirdEngine ;
                                int id = fireBird.GetAuto_IncrementID(tableName, item.Name );
                                item.SetValue(entity, id, null);
                            }
                            else
                                continue;
                        }

                        Object obj = item.GetValue(entity, null);
                       String value = GetEntityPropertyValue(item.PropertyType, obj);
                        sb.Append(value + ",");
                        _sbPropertyNames.AppendFormat("{0},", item.Name);
                    }
                    _sbPropertyNames.Remove(_sbPropertyNames.Length - 1, 1);
                    String sql = sb.ToString().TrimEnd(',');
                    sql = String.Format("insert into {0} ({1}) values({2}) ; ",
                        tableName, _sbPropertyNames.ToString(), sql);
                return AdonetContext.ExecuteNonQuery(sql );
                
            }
            catch (Exception e)
            {
                this.AdonetContext.PerformErrorHandler(this, e);
                return e;
            }
        }
        /// <summary>
        /// 高速场合保存更改的记录(update)
        /// </summary> 
        public Exception USave(EntityBase entity )
        {
            try
            {
                entity.OrmHost = this;
                String tableName = null;
                Type EntityType = null;
                PropertyInfo[] pifs;
                StringBuilder sb;
                EntityType = entity.GetType();
                tableName = GetTableName(EntityType, entity);

                pifs = entity.pifs;
                sb = new StringBuilder();
                StringBuilder _sbPropertyNames = new StringBuilder();
                foreach (var item in pifs)
                {
                    if (item.Name.Equals("Id"))
                    {
                        continue;
                    }
                    DBKeyAttribute attr = item.GetCustomAttribute<DBKeyAttribute>();
                    if (attr != null && attr.KeyDescription.Equals(DBKeyAttribute.PRIMARYKEY))
                        continue;
                    Object obj = item.GetValue(entity, null);
                    String value = GetEntityPropertyValue(item.PropertyType, obj);
                    sb.Append(
                        String.Format("{0}={1},", item.Name, value)
                        );
                }
                String sql = sb.ToString().TrimEnd(',');
              Exception exx_  =  AdonetContext.Update(tableName, sql +
                    String.Format(" where {0}={1}", entity.PrimaryKeyPropertyName, entity.GetPrimaryKeyValue()) 
                    );
                sb.Clear();
                return exx_;
            }
            catch (Exception e)
            {
                this.AdonetContext.PerformErrorHandler(this, e);
                return e;
            }
        }

        /// <summary>
        /// 不需要考虑是否带‘’,TableName=null 时使用例如：FuckGoogleEntity则取FuckGoogle作为表名
        /// </summary>
        /// <typeparam name="T">Entity 类</typeparam>
        /// <param name="PrimaryKeyValue"></param>
        /// <param name="TableName">表名称</param>
        public void Delete<T>(Object PrimaryKeyValue,String TableName=null )
        {
            try
            {
                Type type = typeof(T);
                String tableName = TableName;
                if (tableName == null)
                  tableName = GetTableName(type, null);
                PropertyInfo[] pifs = type.GetProperties();
                String PrimaryKeyPropertyName = null;
                String PrimaryKeyPropertyVal = null;

                foreach (var item in pifs)
                {

                    DBKeyAttribute attr = item.GetCustomAttribute<DBKeyAttribute>();

                    if (attr != null)
                    {
                        if (attr.KeyDescription.Equals(DBKeyAttribute.PRIMARYKEY))
                        {
                            PrimaryKeyPropertyName = item.Name;
                            String propertyTypeName = item.PropertyType.Name;
                            if (propertyTypeName.Equals("DateTime") || propertyTypeName.Equals("String"))
                                PrimaryKeyPropertyVal = String.Format("'{0}'", PrimaryKeyValue);
                            else
                                PrimaryKeyPropertyVal = PrimaryKeyValue.ToString();
                        }
                    }
                }
                String whereSQL = String.Format("{0}={1}", PrimaryKeyPropertyName, PrimaryKeyPropertyVal);
                AdonetContext.RemoveRecord(tableName, whereSQL );
            }
            catch (Exception ex)
            {
                this.AdonetContext.PerformErrorHandler(this, ex);
            }
          
       
                
       }

#if NET4
        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <typeparam name="T">Entity 类型</typeparam>
        /// <param name="HowToDelete">比如“delete from test where name='sfs'”则HowToDelete=(x)=> x.name='sfs'</param>
        /// <param name="TableName"></param>
        /// <param name="args"></param>
        public void Delete<T>(Expression<Func<T, bool>> HowToDelete, String TableName = null) where T : EntityBase
        {
            Expression2SQL expression2SQL = new Expression2SQL();


            String sql = expression2SQL.GetRawSql<T>(HowToDelete);

            String tableName = TableName;
            if (tableName == null)
                tableName = GetTableName(typeof(T), null);
            AdonetContext.RemoveRecord(tableName, sql);

        }
        public virtual List<T> Where<T>  (Expression<Func<T, bool>> WhererExpression
            ,String TableName=null  
            ) where T : EntityBase
        {
           
           String tableName = TableName;
            if (tableName == null)
                tableName = GetTableName(typeof(T), null);
            Expression2SQL expression2SQL = new Expression2SQL();
            String sql = expression2SQL.GetRawSql<T>(WhererExpression);
            
            sql = "select * from " + tableName + " where " + sql;
            List<T> EntitySet = new List<T>();
            ITableInfo keyValueDB = AdonetContext as ITableInfo;
            List<String> lstColNames = keyValueDB.GetTableColumnNames(tableName );

            DbDataReader reader = AdonetContext.FastQueryData(sql );
            int nFileCount = reader.FieldCount;

            if (reader.HasRows)//如果有数据
            {
                while (reader.Read())
                {
                    EntityBase entity = (EntityBase)System.Activator.CreateInstance(typeof(T));
                    for (int i = 0; i < nFileCount; i++) //逐个字段的遍历
                    {
                        Object obj = reader.GetValue(i);
                        entity.OrmHost = this;
                        entity.AssignValue(lstColNames[i], obj);
                    }
                    EntitySet.Add((T)entity);
                }
            }
            //先关闭Reader
            reader.Close();
            return EntitySet ;
        }
#endif
        /// <summary>
        /// 取出所有数据
        /// </summary>
        /// <typeparam name="T">Entity 数据类型</typeparam>
        /// <returns></returns>
        public virtual List<T> Where<T>( String TableName = null ) where T : EntityBase
        {
            String tableName = TableName;
            if (tableName == null)
                tableName = GetTableName(typeof(T), null);

            String sql = "select * from " + tableName ;
            ITableInfo keyValueDB = AdonetContext as ITableInfo;
            List<String> lstColNames = keyValueDB.GetTableColumnNames(tableName ) ;
            DbDataReader reader = AdonetContext.FastQueryData(sql );
            List<T> EntitySet = new List<T>();
            int nFileCount = reader.FieldCount;

            if (reader.HasRows)//如果有数据
            {
                while (reader.Read())
                {
                    EntityBase entity = (EntityBase)System.Activator.CreateInstance(typeof(T));
                    entity.OrmHost = this;
                    for (int i = 0; i < nFileCount; i++) //逐个字段的遍历
                    {
                        Object obj = reader.GetValue(i);
                        
                        entity.AssignValue(lstColNames[i], obj);
                    }
                    EntitySet.Add((T)entity);
                }
            }
            //先关闭Reader
            reader.Close();
            return EntitySet;
        }

#if NET4
        /// <summary>
        /// 取出一行数据
        /// </summary>
        /// <typeparam name="T">Entity 数据类型</typeparam>
        /// <param name="selectSQLExpression"></param>
        /// <param name="FromSQLExpression"></param>
        /// <param name="WhererExpression">筛选条件</param>
        /// <param name="tableName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual T WhereUniqueEntity<T>(Expression<Func<T, Entry>> selectSQLExpression, 
            Expression<Func<T, Entry>> FromSQLExpression = null, 
            Expression<Func<T, bool>> WhererExpression = null,String tableName = null
             
            ) where T : EntityBase
        {
            
            if (tableName == null)
                tableName = GetTableName(typeof(T), null);

            DataTable dt = InternalQuery(selectSQLExpression, FromSQLExpression, WhererExpression );
            List<T> EntitySet = new List<T>();

            if (dt == null)
                return null;
            int nRows = (int)dt?.Rows.Count;
            int nCols = (int)dt?.Columns.Count;

            ITableInfo keyValueDB = AdonetContext as ITableInfo;
            List<String> lstColNames = keyValueDB.GetTableColumnNames(tableName ) ;
            if (nRows ==1)
            {
                EntityBase entity = (EntityBase)System.Activator.CreateInstance(typeof(T));
                for (int j = 0; j < nCols; j++)
                {
                    entity.AssignValue(lstColNames[j], dt.Rows[0][j]);
                    entity.OrmHost = this;
                }

                dt.Clear();
                dt.Dispose();
                return (T)entity;
            }

            else
            {
                try
                {
                    dt.Clear();
                    dt.Dispose();
                }
                catch (Exception ex)
                {

                     
                }
                
                return null;
            }
        }
      
       
        /// <summary>
        /// 查询表中部分列（需要自定义部分列类!请勿包含主键,可以更新数据）
        /// </summary>
        /// <typeparam name="X">部分列类</typeparam>
        /// <typeparam name="T">完整表类</typeparam>
        /// <param name="WhererExpression">where 查询条件</param>
        /// <returns></returns>
        public virtual List<X> Where<X,T> (Expression<Func<T, bool>> WhererExpression, string TableName= null ) where T :EntityBase where X : SimplifiedEntity
        {
            Type typeHost = typeof(T);
            PropertyInfo[] _pifs = typeHost.GetProperties();
            //获得T的主键名及主键值是否需要quote('')
            String PrimaryKeyPropertyName = null;
            bool PrimaryKeyNeedQuote = false;
            for (int i = 0; i < _pifs.Length; i++)
            {
                DBKeyAttribute attr = _pifs[i].GetCustomAttribute<DBKeyAttribute>();
                if (attr != null && attr.KeyDescription.Equals(DBKeyAttribute.PRIMARYKEY))
                    PrimaryKeyPropertyName = _pifs[i].Name;

                String propertyTypeName = _pifs[i].PropertyType.Name;
                if (propertyTypeName.Equals("DateTime") || propertyTypeName.Equals("String"))
                    PrimaryKeyNeedQuote = true;
                else
                    PrimaryKeyNeedQuote = false;
            }
            Type type = typeof(X);
            PropertyInfo[] pifs = type.GetProperties();
            List<String> PropertyNames = new List<String>();
            StringBuilder sb = new StringBuilder();
            sb.Append("select ");
            sb.Append(PrimaryKeyPropertyName + ",");
            PropertyNames.Add(PrimaryKeyPropertyName);
            for (int i = 0; i < pifs.Length; i++)
            {
                sb.Append(pifs[i].Name + ",");
                PropertyNames.Add(pifs[i].Name);
            }
          
            sb.Remove(sb.Length - 1, 1);
            sb.Append(" ");
            String tableName = TableName;
            if (tableName == null)
                tableName = GetTableName(typeof(T), null);
            sb.Append(" from " + tableName + " ");

            Expression2SQL expression2SQL = new Expression2SQL();
            String sql = expression2SQL.GetRawSql<T>(WhererExpression);

            sql = sb.ToString() + " where " + sql;

            ITableInfo keyValueDB = AdonetContext as ITableInfo;
            List<String> lstColNames = keyValueDB.GetTableColumnNames(tableName ) ;
            List<X> Xset = new List<X>();

            DbDataReader reader = AdonetContext.FastQueryData(sql);
            int nFileCount = reader.FieldCount;

            if (reader.HasRows)//如果有数据
            {
               
                while (reader.Read())
                { 
                SimplifiedEntity entity = (SimplifiedEntity)System.Activator.CreateInstance(typeof(X));
              
                entity.HostEntityType = typeof(T);
                entity.PrimaryKeyPropertyName = PrimaryKeyPropertyName;
                entity.PrimaryKeyPropertyQuote = PrimaryKeyNeedQuote;
                   
                    for (int i = 0; i < nFileCount; i++) //逐个字段的遍历
                    {
                        Object obj = reader.GetValue(i);
                        ////赋值主键值,不明白为什么可以查看第830行
                        if (i == 0)
                        {
                            entity.PrimaryKey = obj;
                            continue;
                        }
                        AssignValue(entity, PropertyNames[i], obj);
                    }
                    Xset.Add((X)entity);
                }
            }
            //先关闭Reader
            reader.Close();
            return Xset;
        }
        /// <summary>
        /// 兼容以前的查询方式，灵活度最高
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectSQLExpression"></param>
        /// <param name="FromSQLExpression"></param>
        /// <param name="WhererExpression"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual DataTable InternalQuery<T>(Expression<Func<T, Entry>> selectSQLExpression  ,
            Expression<Func<T, Entry>> FromSQLExpression  = null,
            Expression<Func<T, bool>> WhererExpression=null
             
            ) where T : EntityBase
        {
            StringBuilder sqlStringBuilder = new StringBuilder();
            Expression2SQL expression2SQL = new Expression2SQL();
            
            String selectSQL = expression2SQL.GetRawSql(selectSQLExpression);
            String fromSQL = expression2SQL.GetRawSql(FromSQLExpression);
            String whereSQL = expression2SQL.GetRawSql(WhererExpression);
            if (!selectSQL.IsValideString())
                selectSQL = " select * ";
            if (!fromSQL.IsValideString())
                fromSQL = " from " + GetTableName(typeof(T), null);
            
             sqlStringBuilder.Append(selectSQL);
            sqlStringBuilder.Append(" "+fromSQL);
            if (whereSQL.IsValideString())
                sqlStringBuilder.Append(" where " + whereSQL);

            DataTable dt = AdonetContext.ProvideTable(sqlStringBuilder.ToString() );
            sqlStringBuilder.Clear();
            return dt;
        }

        /// <summary>
        /// 执行update操作(处理一些特殊update,没有指定的Entity的表)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="UpdateSQLExpression">写成例如：Entity.Update(tableName)</param>
        /// <param name="WhererExpression">写成例如：（x）=>x.id=8</param>
        /// <param name="args"></param>
        public virtual void InternalExecuteNonQuery<T>(Expression<Func<T, Entry>> UpdateSQLExpression,
              Expression<Func<T, bool>> WhererExpression = null
               
            ) where T : EntityBase
        {

            //AdonetContext.ExecuteNonQuery
            StringBuilder sqlStringBuilder = new StringBuilder();
            Expression2SQL expression2SQL = new Expression2SQL();
            String UpdateSQL = expression2SQL.GetRawSql(UpdateSQLExpression);
        
            String whereSQL = expression2SQL.GetRawSql(WhererExpression);


           
            sqlStringBuilder.Append(UpdateSQL);
           
            if (whereSQL.IsValideString())
                sqlStringBuilder.Append(" where " + whereSQL);

            AdonetContext.ExecuteNonQuery(sqlStringBuilder.ToString() );

            
            sqlStringBuilder.Clear();
            
        }
#endif
        /// <summary>
        /// 是否包含该表
        /// </summary>
        /// <typeparam name="T">Entity 数据类型</typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool ContainTable<T>(  string TableName = null)
        {
            String tableName = TableName;
            if (tableName == null)
                tableName = GetTableName(typeof(T), null);
            ITableInfo kv = AdonetContext as ITableInfo;
            List<String> tbls = kv.GetAllTableNames( ) ;
            for (int i = 0; i < tbls.Count; i++)
            {
                if (tbls[i].ToLower().Equals(tableName.ToLower()))
                    return true;  
            }
            return false;

        }
        /// <summary>
        /// 批量事务处理(适用于大数据传递)
        /// 不需要显示使用transaction,内置使用Transaction
        /// 可以使用orm方式对数据库写操作
        /// 也可以使用传入的DbCommand 对象写操作
        /// </summary>
        /// <param name="actions">多个线程要执行的Action(每个Action 一个线程)</param>
        public void BatchExecuteNonQuery(String _conn=null,params Action<DbCommand>[] actions)
        {
            this.AdonetContext.BatchExecuteNonQuery(_conn,actions);
        }
#if NET4

        /// <summary>
            /// 从数据库App表中取出json数据并转化为dynamic对象
            /// </summary>
            /// <param name="dynamicObjectName">dynamic对象名请使用nameof(...)</param>
            /// <param name="dyn_Obj">dynamic对象</param>
        public void FetchDynamicObject(System.Dynamic.ExpandoObject dyn_Obj )
        {

            String UniqueName = null;

             
            IDictionary<String,object> dict = dyn_Obj;
            if (dict.Keys.Contains("Name"))
                UniqueName = dict["Name"].ToString();
            else
            {

                throw new Exception("用于存储到数据库dynamic 对象必须有Name属性，且必须唯一");
            }
            String jsonString = AdonetContext.FetchValue(UniqueName, true,"0" );

            if (jsonString != "0" && jsonString != "{}")
            {
                LitJson.JsonMapper.ToObject(jsonString, dyn_Obj);
            }
        }


        /// <summary>
        /// 保存dynamic 数据
        /// 往数据库的App表中写入转化json数据的dynamic对象
        /// </summary>
        /// <param name="dynamicObjectName">dynamic对象名请使用nameof(...)</param>
        /// <param name="dyn_Obj">dynamic对象</param>
        public void WriteDynamicObject(System.Dynamic.ExpandoObject dyn_Obj )
        {
            String UniqueName = null;
            IDictionary<String, object> dict = dyn_Obj;
            if (dict.Keys.Contains("Name"))
                UniqueName = dict["Name"].ToString();
            else
            {

                throw new Exception("用于存储到数据库dynamic 对象必须有Name属性，且必须唯一");

            }
            AdonetContext.Write(UniqueName, LitJson.JsonMapper.ToJson(dyn_Obj));
        }
#endif

        /// <summary>
        /// 仅为作者使用(可以用来连接sqlite/firebird 嵌入式数据库);
        /// 默认用户名
        /// Mysql->root Postgresql->postgres SQL Server ->sa
        /// <param name="pwd_or_DbFilePath">密码或者数据库文件路径</param>
        public bool Connect(String pwd_or_DbFilePath = "123")
        {
            String usrName = "";
            Func<Type> GetEngineType = () =>
            {
                if (this.type_EngineType != null)
                    return this.type_EngineType;

                String path = Assembly.GetCallingAssembly().Location;
                path = Path.GetDirectoryName(path);
                String[] dllFiles = Directory.GetFiles(path, "*.dll");

                Type type_DBEngine = null;

                for (int i = 0; i < dllFiles.Length; i++)
                {

                    String dllProductName = GetADONETDllProductName(dllFiles[i]);
                    if (dllProductName.Contains("Npgsql"))
                    {
                        type_DBEngine = typeof(PostgreSQLEngine);
                        break;

                    }
                    else if (dllProductName.Contains("SQLite"))
                    {
                        type_DBEngine = typeof(SQLiteDBEngine);
                        break;
                    }
                    else if (dllProductName.Contains("MySql"))
                    {
                        type_DBEngine = typeof(MySQLDBEngine);
                        break;
                    }
                    else if (dllProductName.Contains("FirebirdClient"))
                    {
                        type_DBEngine = typeof(FireBirdEngine);
                        break;
                    }
                    else if (dllProductName.Contains("Microsoft") && dllProductName.Contains("Compact"))
                    {
                        type_DBEngine = typeof(SQLCEDBEngine);
                        break;
                    }

                }

                return type_DBEngine;
            };
            Type type = GetEngineType();
            if (type == typeof(MySQLDBEngine))
                usrName = "root";
            else if (type == typeof(PostgreSQLEngine))
                usrName = "postgres";
            else if (type == typeof(SqlServerDBEngine))
                usrName = "sa";
                return Connect(usrName, pwd_or_DbFilePath);
        }
        /// <summary>
        /// 仅为作者使用
        /// </summary>
        public bool Connect( String usrName = "root",String pwd = "123")
        {
            return Connect("shikii", usrName, pwd);
        }
        /// <summary>
        /// 不需要指定存储引擎，只要在程序根目录下放相关数据连接的dll
        /// 或者在引用中添加相关引用如：System.Data.SQLite
        /// 支持的数据库有sqlite,sqlce,postgresql,
        /// firebird(默认使用嵌入模式，服务器模式需要先设置 FireBirdEngine EmbeddedMode = false;),mysql
        /// 注意不支持sql server,localdb
        /// </summary>
        public bool Connect(String DBName = "shikii", String usrName = "root", String pwd = "123")
        {
            return Connect(DBName, usrName, pwd,null);
        }

        /// <summary>
        /// 不需要指定存储引擎，只要在程序根目录下放相关数据连接的dll
        /// 或者在引用中添加相关引用如：System.Data.SQLite
        /// 支持的数据库有sqlite,sqlce,postgresql,
        /// firebird(默认使用嵌入模式，服务器模式需要先设置 FireBirdEngine EmbeddedMode = false;),mysql
        /// 注意不支持sql server,localdb
        /// </summary>
        /// <param name="type_DBEngine">比如：typeof(SQLiteDBEngine)</param>
        /// <param name="EntitySourceAssemblies">存在Entity的程序集(可以为空)</param>
        public bool Connect(String DBName = "shikii", String usrName = "root", String pwd = "123",
            params Assembly[] EntitySourceAssemblies)
        {
        BootDBEngine:;
            Func<Type> GetEngineType = () =>
            {
                if (this.type_EngineType != null)
                    return this.type_EngineType;

                String path = Assembly.GetCallingAssembly().Location;
                path = Path.GetDirectoryName(path);
                String[] dllFiles = Directory.GetFiles(path, "*.dll");

                Type type_DBEngine = null;

                for (int i = 0; i < dllFiles.Length; i++)
                {

                    String dllProductName = GetADONETDllProductName(dllFiles[i]);
                    if (dllProductName.Contains("Npgsql"))
                    {
                        type_DBEngine = typeof(PostgreSQLEngine);
                        break;

                    }
                    else if (dllProductName.Contains("SQLite"))
                    {
                        type_DBEngine = typeof(SQLiteDBEngine);
                        break;
                    }
                    else if (dllProductName.Contains("MySql"))
                    {
                        type_DBEngine = typeof(MySQLDBEngine);
                        break;
                    }
                    else if (dllProductName.Contains("FirebirdClient"))
                    {
                        type_DBEngine = typeof(FireBirdEngine);
                        break;
                    }
                    else if (dllProductName.Contains("Microsoft") && dllProductName.Contains("Compact"))
                    {
                        type_DBEngine = typeof(SQLCEDBEngine);
                        break;
                    }

                }

                return type_DBEngine;
            };

            bool temp = false;
            if (ThisDBManager == null)
            {
                AssignMeToEntry();
                Type type = GetEngineType();
                Init(type, DBName, usrName, pwd, EntitySourceAssemblies);
                EndInit();
                temp = IsConnected;
            }

            else
            {

                Type type_DBEngine = GetEngineType();

                if (type_DBEngine == typeof(SQLiteDBEngine))
                {

                    if (!DBName.EndsWith(".db"))
                        DBName = DBName + ".db";
                    temp = ThisDBManager.Connect(DBName);
                    
                }
                else if (type_DBEngine.Name == "SQLCEDBEngine")
                {
                    if (!DBName.EndsWith(".sdf"))
                        DBName = DBName + ".sdf";
                    temp = ThisDBManager.Connect(DBName, true);
                    
                }
                else
                {
                    temp = ThisDBManager.Connect(type_DBEngine, DBName, usrName, pwd);
                    
                }
              
            }
            if (BootDBAction != null && temp == null)
            {
                BootDBAction.Invoke(this);
                goto BootDBEngine;
            }
            return temp;
        }
        /// <summary>
        /// ！！！注意不支持sql server,localdb！！！
        /// 初始化数据平台(不必手动调用AddTable,会自动查找继承自EntityBase的Entity,然后再创建相应的表)
        /// firebird(默认使用嵌入模式，服务器模式需要先设置 FireBirdEngine EmbeddedMode = false;),mysql
        /// </summary>
        /// <param name="type_DBEngine">比如：typeof(SQLiteDBEngine)</param>
        /// <param name="EntitySourceAssemblies">存在Entity的程序集</param>
        public bool Connect(Type type_DBEngine, String DBName = "shikii", String usrName = "root", String pwd = "123", params Assembly[] EntitySourceAssemblies)
        {
            BootDBEngine:;
            bool temp = false;
            if (ThisDBManager == null)
            {
                AssignMeToEntry();
                Init(type_DBEngine, DBName, usrName, pwd, EntitySourceAssemblies);
                EndInit();
                temp = IsConnected;
            }
            else
            {
               
                if (type_DBEngine == typeof(SQLiteDBEngine))
                {

                    if (!DBName.EndsWith(".db"))
                        DBName = DBName + ".db";
                    temp = ThisDBManager.Connect(DBName);
                   
                }
                else if (type_DBEngine.Name == "SQLCEDBEngine")
                {
                    if (!DBName.EndsWith(".sdf"))
                        DBName = DBName + ".sdf";
                    temp = ThisDBManager.Connect(DBName, true);
                   
                }
                else
                {
                    temp = ThisDBManager.Connect(type_DBEngine, DBName, usrName, pwd);
                    
                }
            }

            if (BootDBAction != null && temp == null)
            {
                BootDBAction.Invoke(this);
                goto BootDBEngine;
            }
            return temp;
        }
        /// <summary>
        /// 注意连接的是远程数据库 ！！！注意不支持sql server,localdb！！！
        /// firebird(默认使用嵌入模式，服务器模式需要先设置 FireBirdEngine EmbeddedMode = false;),mysql
        /// 初始化数据平台(不必手动调用AddTable,会自动查找继承自EntityBase的Entity,然后再创建相应的表)
        /// </summary>
        /// <param name="type_DBEngine">比如：typeof(SQLiteDBEngine)</param>
        /// <param name="EntitySourceAssemblies">存在Entity的程序集</param>
        public bool Connect(Type type_DBEngine, String ip, int port, String DBName = "shikii", String pwd = "123", String usrName = "root", params Assembly[] EntitySourceAssemblies)
        {
            BootDBEngine:;
            bool temp = false;
            if (ThisDBManager == null)
            {
                AssignMeToEntry();
                Init(type_DBEngine, ip, port, DBName, usrName, pwd, EntitySourceAssemblies);
               
                 
                EndInit();
                temp = IsConnected;
            }
            else
            {
                if (type_DBEngine == typeof(SQLiteDBEngine))
                {

                    if (!DBName.EndsWith(".db"))
                        DBName = DBName + ".db";
                    temp = ThisDBManager.Connect(DBName);
                    
                }
                else if (type_DBEngine.Name == "SQLCEDBEngine")
                {
                    if (!DBName.EndsWith(".sdf"))
                        DBName = DBName + ".sdf";
                    temp = ThisDBManager.Connect(DBName, true);
                  
                }
                else
                {
                    temp = ThisDBManager.Connect(type_DBEngine, ip, port, DBName, usrName, pwd);
                    
                }
            }
             if (BootDBAction != null && temp == null)
                {
                BootDBAction.Invoke(this);
                goto BootDBEngine;
            }

            return temp;

        }

        /// <summary>
        /// ！！！注意只支持sql server,localdb！！！
        /// 初始化数据平台(不必手动调用AddTable,会自动查找继承自EntityBase的Entity,然后再创建相应的表)
        /// </summary>
        /// <param name="EntitySourceAssemblies">存在Entity的程序集</param>
        public bool Connect(Assembly asm_SQLSEVER, String DBName = "shikii", String usrName = "root", String pwd = "123", params Assembly[] EntitySourceAssemblies)
        {
            if (this.AdonetContext == null)
            {
                AssignMeToEntry();
                SqlServerDBEngine.asm_SQLSEVER = asm_SQLSEVER;
                Init(typeof(SqlServerDBEngine), DBName, usrName, pwd, EntitySourceAssemblies);
                EndInit();
                return IsConnected;
            }
            else
            {
                bool temp = false;
                SqlServerDBEngine.asm_SQLSEVER = asm_SQLSEVER;
            
                Type type_DBEngine = typeof(SqlServerDBEngine);
                //SQLITE
                if (type_DBEngine == typeof(SQLiteDBEngine))
                {

                    if (!DBName.EndsWith(".db"))
                        DBName = DBName + ".db";
                    temp = ThisDBManager.Connect(DBName);
                
                }
                else if (type_DBEngine.Name == "SQLCEDBEngine")
                {
                    if (!DBName.EndsWith(".sdf"))
                        DBName = DBName + ".sdf";
                    temp = ThisDBManager.Connect(DBName, true);
                  
                }
                else
                {
                    temp = ThisDBManager.Connect(type_DBEngine, DBName, usrName, pwd);
             
                }

                return temp;
            }
        }
        /// <summary>
        /// ！！！注意只支持sql server,localdb！！！
        /// 初始化数据平台(不必手动调用AddTable,会自动查找继承自EntityBase的Entity,然后再创建相应的表)
        /// </summary>
        /// <param name="EntitySourceAssemblies">存在Entity的程序集</param>
        public bool Connect(Assembly asm_SQLSEVER, String ip, int port, String DBName = "shikii", String pwd = "123", String usrName = "root", params Assembly[] EntitySourceAssemblies)
        {
            if (this.AdonetContext == null)
            {
                AssignMeToEntry();
                SqlServerDBEngine.asm_SQLSEVER = asm_SQLSEVER;
                Init(typeof(SqlServerDBEngine), ip, port, DBName, usrName, pwd, EntitySourceAssemblies);
                EndInit();
                return IsConnected;
            }
            else
            {
                SqlServerDBEngine.asm_SQLSEVER = asm_SQLSEVER;
                bool temp = false;
                Type type_DBEngine = typeof(SqlServerDBEngine);
                //SQLITE
                if (type_DBEngine == typeof(SQLiteDBEngine))
                {

                    if (!DBName.EndsWith(".db"))
                        DBName = DBName + ".db";
                    temp = ThisDBManager.Connect(DBName);
                    
                }
                else if (type_DBEngine.Name == "SQLCEDBEngine")
                {
                    if (!DBName.EndsWith(".sdf"))
                        DBName = DBName + ".sdf";
                    temp = ThisDBManager.Connect(DBName, true);
                
                }
                else
                {

                    temp = ThisDBManager.Connect(type_DBEngine, ip, port, DBName, usrName, pwd);
             
                }

                return temp;
            }
        }
        /// <summary>
/// 内部使用
/// </summary>
        bool IsConnected
        {
            get
            {
                return AdonetContext.ThisDbPipeInfo.MainDbConnection.State == ConnectionState.Open;
            }
        }
        
        /// <summary>
        /// 打印输出是否连接到数据的状态信息
        /// </summary>
        /// <param name="args">Connect 的返回值</param>
        /// <param name="LogFileDirPath">日志文件夹的路径，Web/桌面不用给值，移动端一定要给值</param>
        public  void LogConnectionStatus( String LogFileDirPath=null)
        {
            if (AdonetContext.ThisDbPipeInfo.MainDbConnection.State == ConnectionState.Open)
                  LogInfo($"连接{AdonetContext.GetType().Name}数据库引擎成功",LogFileDirPath);
            else
                 LogError(new Exception($"连接{AdonetContext.GetType().Name}数据库引擎失败"),LogFileDirPath);
        }

        public void Dispose()
        {
       
            this.ThisDBManager.ThisDBPlatform = null;
            this.ThisDBManager = null;
            this.LogInfo ("数据库上下文被销毁");
        }

        /// <summary>
        /// 关闭数据库
        /// </summary>
        ~OrmDBPlatform()
        {
           
        }

    }
}