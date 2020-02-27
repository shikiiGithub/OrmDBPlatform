﻿using System;

namespace dotNetLab.Data.Orm
{
  public  class MysqlTextTypeAttribute:DBTypeAttribute
    {

        /* Mysql
       TEXT	        0-65535字节	    长文本数据
       MEDIUMTEXT	0-16777215字节	中等长度文本数据
       LONGTEXT	    0-4294967295字节	极大文本数据
             */
        /*
         sqlite 最大为SHORTTEXT=“text”
              */

        public static readonly String VERYSHORTTEXT = "varchar(255)";
        public static readonly  String SHORTTEXT = "Text";
        public static readonly String MEDIUMTEXT = "MEDIUMTEXT";
        public static readonly String LONGTEXT = "LONGTEXT";
         String FIXEDTEXT = "CHAR({0})";
        public MysqlTextTypeAttribute(String LengthType = "varchar(255)")
        {
            this.DataType = LengthType;
        }

        public MysqlTextTypeAttribute(int char_n)
        {
            this.DataType = String.Format(FIXEDTEXT,char_n);
        }
    }
}
