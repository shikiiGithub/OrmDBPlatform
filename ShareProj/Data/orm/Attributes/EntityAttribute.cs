﻿using System;
using System.Collections.Generic;
using System.Text;

namespace dotNetLab.Data.Orm
{
   public class EntityAttribute: Attribute
    {
        public String EntityDescription;
        public ActionType  EntityActionType = ActionType.None;
        public static readonly String MANUAL_CREATE_TABLE = "MANUAL_CREATE_TABLE";
        public static readonly String AUTO_CREATE_TABLE = "AUTO_CREATE_TABLE";
        public EntityAttribute(String EntityDescription = "AUTO_CREATE_TABLE")
        {
            this.EntityDescription = EntityDescription;

        }
        public EntityAttribute(ActionType actionType,String EntityDescription )
        {
            this.EntityDescription = EntityDescription;
            this.EntityActionType = actionType;

        }

        public String GetSqlTemplateString()
        {
            String str = null;
            switch (EntityActionType)
            {
               
                case ActionType.Alter:
                    str = "alter table {0} " + EntityDescription;
                    break;
               
            }
            return str;
        }

        public enum ActionType { None,Alter}

    }
}
