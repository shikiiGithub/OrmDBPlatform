﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using dotNetLab.Common;

namespace dotNetLab.Data.Orm
{
    public class OrmDBPlatformManager
    {
        public OrmDBPlatform MainDbOrmPlatform;
        //public Dictionary<int, OrmDBPlatform> DBPipeHashCodes;
       // public Dictionary<OrmDBPlatform, bool> DBPipes;
        //public int MaxDBPipeCount = 200;
        //public int MinDBPipeCount = 0;
        //public int DBPipeCount = 0;
      //  static readonly Object lockDBPipeInjection = new Object();
        public Dictionary<Type, PropertyInfo[]> InjectingPropertyInfos;
        Func<OrmDBPlatform, bool> ConnectDBAction;
        public Queue<Exception> ErrorMessageQueue;
        public Queue<String> InfoLogMessageQueue;
        Stopwatch DBConnectStopwatch = new Stopwatch();
        Type type_App;
        void GetInjectingPropertyInfos(Type WebApiControllerBaseType, Type AttributeType)
        {
            Type[] types = type_App.Assembly.GetTypes();
            foreach (Type item in types)
            {
                if (WebApiControllerBaseType == item)
                    continue;
                bool b = WebApiControllerBaseType.IsAssignableFrom(item);
                if (b)
                {
                    List<PropertyInfo> propertyInfos = new List<PropertyInfo>();
                    PropertyInfo[] pifs = item.GetProperties();
                    foreach (var pif in pifs)
                    {
                        Attribute attribute = Attribute.GetCustomAttribute(pif, AttributeType);
                        if (attribute != null)
                        {
                            propertyInfos.Add(pif);
                        }
                    }
                    InjectingPropertyInfos.Add(item, propertyInfos.ToArray());
                }

            }


        }



        #region 数据库相关
        public void InitOrmDBPlatformManager(Func<OrmDBPlatform, bool> ConnectDBAction, Type type_App, Type WebApiControllerBaseType, Type AttributeType)
        {
            ErrorMessageQueue = new Queue<Exception>();
            InfoLogMessageQueue = new Queue<string>();

            this.ConnectDBAction = ConnectDBAction;
            
            this.type_App = type_App;
            //DBPipes = new Dictionary<OrmDBPlatform, bool>();
            //DBPipeHashCodes = new Dictionary<int, OrmDBPlatform>();
            InjectingPropertyInfos = new Dictionary<Type, PropertyInfo[]>();

            GetMainDbContext();
            GetInjectingPropertyInfos(WebApiControllerBaseType, AttributeType);
            //for (int i = 0; i < MinDBPipeCount; i++)
            //{
            //    GetFastDbContext(false);
            //}
        }

        void AssignLogHandler(OrmDBPlatform orm)
        {
            orm.InfoInvoker = (obj, info) =>
            {
                InfoLogMessageQueue.Enqueue(info.ToString());
            };

            orm.ErrorInvoker = (obj, ex) =>
            {
                ErrorMessageQueue.Enqueue(ex);
            };
        }

        void BeginMeasureDBConnectTime()
        {
            DBConnectStopwatch.Restart();
        }

        void EndMeasureDBConnectTime()
        {
            DBConnectStopwatch.Stop();

            Console.WriteLine("连接数据库耗时：" + DBConnectStopwatch.ElapsedMilliseconds + " ms");
        }


        /// <summary>
        /// 初始化主数据库用于全局使用数据库
        /// 这个对象会把所有实体映射为表
        /// 监测实体变更，将变更应用到数据库表中
        /// </summary>
        void GetMainDbContext()
        {
            MainDbOrmPlatform = new OrmDBPlatform();
            AssignLogHandler(MainDbOrmPlatform);
            BeginMeasureDBConnectTime();
            bool args = (bool)ConnectDBAction?.Invoke(MainDbOrmPlatform);
            EndMeasureDBConnectTime();
            if (args  )
                InfoLogMessageQueue.Enqueue($"连接{MainDbOrmPlatform.AdonetContext.GetType().Name}数据库引擎成功");
            else
                ErrorMessageQueue.Enqueue( new Exception($"连接{MainDbOrmPlatform.AdonetContext.GetType().Name}数据库引擎失败"));
           
        }

     



        #endregion
    }
}


/*
    #region 弃用的
        [Obsolete("请不要使用这个方法，因为使用这个方法会导致很严重的bug")]
        public OrmDBPlatform AutoGetOrmDBPlatform()
        {

            lock (lockDBPipeInjection)
            {
                IEnumerator<OrmDBPlatform> iter = DBPipes.Keys.GetEnumerator();
                OrmDBPlatform _orm = null;

                for (int i = 0; i < DBPipes.Count; i++)
                {
                    iter.MoveNext();
                    OrmDBPlatform orm = iter.Current;
                    bool b = DBPipes[orm];

                    if (b)
                    {
                        continue;
                    }
                    else
                    {
                        _orm = orm;
                        DBPipes[orm] = true;
                        break;
                    }
                }
                if (_orm == null)
                {
                    _orm = GetFastDbContext();
                    Thread thd = new Thread(
                      () =>
                      {
                          GetFastDbContext(false);
                      }
                       );
                    thd.Name = "备用连接数据库对象";
                    thd.Start();
                }
                return _orm;
            }

        }

        /// <summary>
        /// 初始化主数据库用于局部的数据库上下文
        /// 用于各WebAPI
        /// </summary>
        [Obsolete("请不要使用这个方法，因为使用这个方法会导致很严重的bug")]
        public OrmDBPlatform GetFastDbContext(bool _lock = true)
        {
            OrmDBPlatform orm = new OrmDBPlatform(false);
            AssignLogHandler(orm);
            orm.type_EngineType = MainDbOrmPlatform.AdonetContext.GetType();
            orm.TableManager = MainDbOrmPlatform.TableManager;
            orm.LightMode = true;
            BeginMeasureDBConnectTime();
            bool bconn = (bool)ConnectDBAction?.Invoke(orm);
            EndMeasureDBConnectTime();
            if (bconn)
                InfoLogMessageQueue.Enqueue($"连接{MainDbOrmPlatform.AdonetContext.GetType().Name}数据库引擎成功");
            else
                ErrorMessageQueue.Enqueue(new Exception($"连接{MainDbOrmPlatform.AdonetContext.GetType().Name}数据库引擎失败"));
            //DBPipes.Add(orm, _lock);
            //DBPipeHashCodes.Add(orm.GetHashCode(), orm);
            return orm;
        }

        #endregion

     */
