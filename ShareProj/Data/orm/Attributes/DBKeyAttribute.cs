﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNetLab.Data.Orm
{
    public class DBKeyAttribute : OrmAttribute
    {
        public String  KeyDescription ;
         
        public static readonly String PRIMARYKEY=" Primary Key ";
        public static readonly String UNIQUE = " UNIQUE ";
        public static readonly String FOREIGN_KEY = " FOREIGN KEY ";
        public static readonly string POSTGRESQL_FOREIGN_KEY_FSTRING = " references {0}({1}) ";
        public static readonly string MYSQL_FOREIGN_KEY_FSTRING = " FOREIGN KEY({0}) references {1}({2}) ";
        public static readonly string SQLSERVER_FOREIGN_KEY_FSTRING = "FOREIGN KEY REFERENCES {1}({2}) ";
        public String ForeignKeyTable, PrimaryKeyName;

        public DBKeyAttribute(String KeyDescription = " Primary Key " )
        {
            this.KeyDescription = KeyDescription;
           
        }

        /// <summary>
        /// 为除sqlite,sqlce,firedb创建外键
        /// </summary>
        /// <param name="ForeignKeyTable">另一个表名</param>
        /// <param name="PrimaryKeyName">另一个表的主键名（字段名）</param>
        public DBKeyAttribute(String ForeignKeyTable,String PrimaryKeyName)
        {
            this.ForeignKeyTable = ForeignKeyTable; this.PrimaryKeyName = PrimaryKeyName;

            this.KeyDescription = FOREIGN_KEY;
           
        }
    }
}
