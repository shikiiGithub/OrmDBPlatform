﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotNetLab.Data.Orm 
{
   public  class SQLiteTextTypeAttribute : DBTypeAttribute
    {
        /* Mysql
    TEXT	        0-65535字节	    长文本数据
    MEDIUMTEXT	0-16777215字节	中等长度文本数据
    LONGTEXT	    0-4294967295字节	极大文本数据
          */
        /*
         sqlite 最大为SHORTTEXT=“text”
              */
        public static readonly String VERYSHORTTEXT = "nvarchar(255)";
        public static readonly String SHORTTEXT = "Text";
        public static readonly String MEDIUMTEXT = "MEDIUMTEXT";
        public static readonly String LONGTEXT = "LONGTEXT";
        public SQLiteTextTypeAttribute(String LengthType = "nvarchar(255)")
        {
            this.DataType = LengthType;
        }
    }
}
