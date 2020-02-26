﻿using System;
using System.Reflection;

namespace dotNetLab
{

    public class OrmAttribute : Attribute
    {
        public OrmAttribute()
        {

        }
        public  String GetAttributeValue(String fieldName)
        {
            FieldInfo fif = this.GetType().GetField(fieldName);
            Object val = fif.GetValue(this);
            return val.ToString();
        }
    }
}
