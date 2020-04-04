﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotNetLab.Data.Orm
{
   public static class EntryExtensions
    {

        /// <summary>
        /// 无意义，只为生成sql
        /// </summary>
        public static Entry From(this Entry entry,object obj)
        {
            return null;
        }
        /// <summary>
        /// 无意义，只为生成sql
        /// </summary>
        public static Entry Select(this Entry entry,params object[] obj)
        {
            return null;
        }
        /// <summary>
        /// 无意义，只为生成sql
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Entry Max(this Entry entry,object obj)
        {
            return null;
        }
        /// <summary>
        /// 无意义，只为生成sql (空白连接)
        /// </summary>
        public static Entry With(this Entry entry,params object[] obj)
        {
            return null;
        }

        /// <summary>
        /// 无意义，只为生成sql(逗号连接)
        /// </summary>
        public static Entry WithComma(this Entry entry,params object[] obj)
        {
            return null;
        }
        /// <summary>
        /// 无意义，只为生成sql(空白连接)
        /// </summary>
        public static bool WhereWith(this Entry entry,params object[] obj)
        {
            return false;
        }

        /// <summary>
        /// 无意义，只为生成sql(逗号连接)
        /// </summary>
        public static bool WhereWithComma(this Entry entry,params object[] obj)
        {
            return false;
        }

        /// <summary>
        /// 无意义，只为生成sql
        /// </summary>
        public static Entry Min(this Entry entry,object obj)
        {
            return null;
        }
        /// <summary>
        /// 无意义，只为生成sql
        /// </summary>
        public static Entry Count(this Entry entry,object obj)
        {
            return null;

        }
        /// <summary>
        /// 无意义，只为生成sql
        /// </summary>
        public static Entry Avg(this Entry entry,object obj)
        {
            return null;
        }
        /// <summary>
        /// 无意义，只为生成sql
        /// </summary>
        public static Entry Sum(this Entry entry,object obj)
        {
            return null;
        }
        /// <summary>
        /// 无意义，只为生成sql
        /// </summary>

        public static Entry LCase(this Entry entry,object obj)
        {
            return null;
        }
        /// <summary>
        /// 无意义，只为生成sql
        /// </summary>
        public static Entry UCase(this Entry entry,object obj)
        {
            return null;
        }
        /// <summary>
        /// 无意义，只为生成sql
        /// </summary>
        public static Entry Round(this Entry entry,object obj, int decimals)
        {
            return null;
        }
        /// <summary>
        /// 无意义，只为生成sql
        /// </summary>
        public static Entry As(this Entry entry,Object asObject)
        {
            return null;
        }



        /// <summary>
        /// 无意义，只为生成sql
        /// 只要前x条数据
        /// 数据库中行的索引是从0开始
        /// 列的索引是从1开始
        /// </summary>
        /// <param name="index">行索引</param>
        /// <returns></returns>
        public static Entry Limit(this Entry entry,int index)
        {
            return null;
        }
        /*
         格式二:分页查询
            select * from 表名 limit m,n;
            m:每页数据的开始行数,变化的
            n:每页显示的数量,固定的
 
         */
        /// <summary>
        /// 分页查询,无意义，只为生成sql
        /// </summary>
        /// <param name="m">每页数据的开始行数,变化的</param>
        /// <param name="n">每页显示的数量,固定的</param>
        /// <returns></returns>
        public static Entry Limit(this Entry entry,int m, int n)
        {
            return null;
        }
        /// <summary>
        /// 无意义，只为生成sql
        /// </summary>
        public static Entry Distinct(this Entry entry,Object col)
        {
            return null;
        }



        /// <summary>
        /// 形成sql 无意义
        /// 从上到下降序，这意味着最上面一个是最大的值 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="Target"></param>
        /// <returns></returns>
        public static Entry OrderByDESC(this Entry entry,object Target)
        {
            return null;
        }
        /// <summary>
        /// 形成sql 无意义
        /// </summary>
        public static Entry Update(this Entry entry,object tableName)
        {
            return null;
        }

        /// <summary>
        /// 形成sql 无意义
        /// </summary>
        public static Entry Drop(this Entry entry,params object[] objs)
        {
            return null;
        }
        /// <summary>
        /// 形成sql 无意义
        /// </summary>
        public static Entry InsertInto(this Entry entry,object tableName)
        {
            return null;
        }
        /// <summary>
        /// 形成sql 无意义
        /// </summary>
        public static Entry Alter(this Entry entry,object tableName)
        {
            return null;
        }
        /// <summary>
        /// 形成sql 无意义 set 字段1=字段1的值
        /// </summary>
        /// <param name="pars">字段1,字段1的值,字段2,字段2的值 </param>
        /// <returns></returns>
        public static Entry UpdateSet(this Entry entry,params Object[] pars)
        {
            return null;
        }

        /// <summary>
        /// 形成sql 无意义
        /// 从上到下升序，这意味着最下面一个是最大的值 
        /// </summary>
        public static Entry OrderByASC(this Entry entry,object Target)
        {
            return null;
        }
        /// <summary>
        /// 内连接(要有from 语句)
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="leftTable"></param>
        /// <param name="rightTable"></param>
        /// <param name="WhereSql"></param>
        /// <returns></returns>
        public static Entry InnerJoin(this Entry entry,String leftTable,String rightTable,String WhereSql)
        {
            return null;
        }

    }
}
