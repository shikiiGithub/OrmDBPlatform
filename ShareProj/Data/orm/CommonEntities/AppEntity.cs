using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace dotNetLab.Data.Orm
{
    /// <summary>
    /// 兼容以前的键值对操作方式
    /// </summary>
    public  class AppEntity : EntityBase 
    {
        [DBKey]
        public String Name {get;set;}
        public String Val  {get;set;}   
    }
}