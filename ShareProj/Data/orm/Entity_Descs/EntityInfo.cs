﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace dotNetLab.Data.Orm
{
    public class EntityInfo
    {
        public PropertyInfo[] pifs;
        public List<String> PropertyNames;
        public String PrimaryKeyPropertyName;
        public bool PrimaryKeyPropertyQuote;
        public PropertyInfo PrimaryPropertyInfo;
        public EntityInfo()
        {
            PropertyNames = new List<string>();
        }
    }

}
