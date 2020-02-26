﻿using System;
namespace dotNetLab.Data.Orm
{

    public class PostgresqlTextTypeAttribute : DBTypeAttribute
    {
        public static readonly String VERYSHORTTEXT = "character varying(255)";
        public static readonly String LONGTEXT = "Text";
        String FIXEDTEXT = "character({0})";
        public PostgresqlTextTypeAttribute(String LengthType = "character varying(255)")
        {
            this.DataType = LengthType;
        }
        public PostgresqlTextTypeAttribute(int char_n)
        {
            this.DataType = String.Format(FIXEDTEXT, char_n);
        }
       

    }
}
