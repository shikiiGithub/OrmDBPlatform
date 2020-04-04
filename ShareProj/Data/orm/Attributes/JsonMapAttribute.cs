using System;
using System.Collections.Generic;
using System.Text;
namespace dotNetLab.Data.Orm
{
    public class JsonMapAttribute : OrmAttribute
    {
        public String Key, JsonPropertyName;
       public JsonMapAttribute(String key,String jsonPropertyName)
        {
            this.Key = key;
            this.JsonPropertyName = jsonPropertyName;
        }
    }
}
