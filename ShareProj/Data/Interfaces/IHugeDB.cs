﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNetLab.Data 
{
   public interface IHugeDB
    {
        void ChangeColumnName(string tableName, string colName_Old, string colName_New, string ColTypeDefine);

        void ChangeColumnType(string tableName, string colName, string ColTypeDefine);


        void AddColumn(string tableName, String ColumnName, string FieldType,String Extra=null);
       

        void DropColumn(string tableName, string columnName);

        bool BootServer(String ServerDirPath);
        void ShutDownServer(String ServerDirPath);
        
        bool Connect(string DBName, string UserName, string Pwd);
        bool Connect(String ip, int port, string  DBName, string  UserName, string Pwd);
    }
}
