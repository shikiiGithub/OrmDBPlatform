﻿﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace dotNetLab.Data 
{
    public interface ITableInfo 
    {

        String InferDataType(PropertyInfo pif,  String csType);
        List<String> GetAllViewNames();

        List<String> GetAllTableNames();
        List<String> GetTableColumnNames(String tableName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="isRawSqlType">是否将数据库中的类型转换到C# 类型</param>
        /// <param name="args"></param>
        List<String> GetTableColumnTypes(String tableName, bool isRawSqlType = false);
    }
}
