﻿using System;
using System.Collections.Generic;
using System.Text;

namespace dotNetLab.Data.Orm
{
   public class EntityAttribute: Attribute
    {
        public String EntityDescription;

        public String TableName;

        public static readonly String MANUAL_CREATE_TABLE = "MANUAL_CREATE_TABLE";
        public static readonly String AUTO_CREATE_TABLE = "AUTO_CREATE_TABLE";
        public EntityAttribute(String EntityDescription = "AUTO_CREATE_TABLE")
        {
            this.EntityDescription = EntityDescription;

        }
    }
}
