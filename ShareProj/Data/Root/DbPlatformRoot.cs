﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Reflection;
using dotNetLab.Common;

namespace dotNetLab.Data
{
    public class DbPlatformRoot
    {
        public readonly string CREATEVIEW = "CREATE VIEW {0} AS SELECT {1} FROM  {2};";
        public const string EMBEDDEDTABLEDEF = "Name nvarchar(128) primary key,Val nvarchar(512) not null";
        public const char SPLITMARK = '^';
        public const string FIELD_VALUE = "Val";
        public const string FIELD_NAME = "Name";
        protected const string BADCONNECTION = "未能连接本地数据库";
        protected int nError = -99999;

      
        
 
    
        public void FreeDataTable( DataTable dt)
        {
           
            if (dt != null)
            {
                dt.Clear();
                dt.Dispose();

            }
            
        }
        public bool NeedQuote(string type)
        {
            type = type.ToLower();
            if (type.Contains("text") || type.Contains("char") || type == "datetime" || type.Contains("time") || type.Contains("date"))
                return true;
            else
              return  false;
        }
        public string TypeInfer(string s)
        {
            string type = s;
            if (type.Contains("text") || type.Contains("char"))
                type = "string";
            if (type == "bigint")
                type = "long";
            if (type == "smallint")
                type = "short";
            if (type == "tinyint")
            {
                type = "byte";
            }
            if (type == "real")
                type = "double";
            if (type == "datetime" || type.Contains("time") || type.Contains("date"))
                type = "DateTime";
            if (type.Contains("blob") || type.Contains("binary") || type.Contains("image"))
                type = "byte []";
            return type;
        }
        protected bool IsDouble(string str)
        {
            try
            {
                double i = Convert.ToDouble(str);
                return true;
            }
            catch
            {
                return false;
            }
        }
        protected bool IsDateTime(string str)
        {
            try
            {
                DateTime ui = Convert.ToDateTime(str);
                return true;
            }
            catch
            {
                return false;
            }

        }
        public String BinaryToBase64Str(String strFileName)
        {


            FileStream fs = new FileStream(strFileName, FileMode.Open);
            byte[] arr = new byte[fs.Length];
            fs.Read(arr, 0, (int)fs.Length);

            fs.Close();
            fs.Dispose();
            return Convert.ToBase64String(arr);
        }
        public byte[] Base64StrToBinary(String strBase64Binary)
        {

            return Convert.FromBase64String(strBase64Binary);
        }
        
        public virtual object GetReflectOject(string strDllPath, string strObjectFullName)
        {
            String dir = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            String fileName = Path.GetFileName(strDllPath);
            strDllPath = Path.Combine(dir, fileName);
            Assembly asm = Assembly.LoadFrom(strDllPath);
            Object obj =  asm.CreateInstance(strObjectFullName);
            return obj;

        }
 
      
    }
}