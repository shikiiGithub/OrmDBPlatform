﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotNetLab.Data.Orm
{
   public class FireBirdTextTypeAttribute : DBTypeAttribute
    {
        /*
         nchar(n)	固定长度的 Unicode 数据。最多 4,000 个字符。	 
nvarchar(n)	可变长度的 Unicode 数据。最多 4,000 个字符。	 
nvarchar(max)	可变长度的 Unicode 数据。最多 536,870,912 个字符。	 
ntext	可变长度的 Unicode 数据。最多 2GB 字符数据。
             */

        public static readonly String VERYSHORTTEXT = "CHARACTER VARYING(255)";

        protected String FIXEDTEXT = "CHARACTER({0})";
        public FireBirdTextTypeAttribute(String LengthType = "CHARACTER VARYING(255)")
        {
            this.DataType = LengthType;
        }

        public FireBirdTextTypeAttribute(int char_n)
        {
            this.DataType = String.Format(FIXEDTEXT, char_n);
        }
    
    }
}
