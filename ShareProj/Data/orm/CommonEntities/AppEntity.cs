using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace dotNetLab.Data.Orm
{
    /// <summary>
    /// ������ǰ�ļ�ֵ�Բ�����ʽ
    /// </summary>
    public  class AppEntity : EntityBase 
    {
        [DBKey]
        public String Name {get;set;}
        public String Val  {get;set;}   
    }
}