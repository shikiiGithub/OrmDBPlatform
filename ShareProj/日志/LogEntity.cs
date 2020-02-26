using System;
using System.Collections.Generic;
using System.Text;

namespace dotNetLab.Common.Logging
{
  public  class LogEntity
    {
        Exception ErrorDescription = null;
        String InfoString;
        public string Time { get => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); }
        public string Status { get; set; }
        public String Description
        {
            get {

                if (ErrorDescription == null)
                    return "Status OK !";
                else
                    return BuildErrorString();
            }
        }

        public String Tag
        {
            get;set;
        }
        String BuildErrorString()
        {
            try
            {
                Exception e = ErrorDescription;
                return $"{e.Message} {e.StackTrace}";
            }
            catch (Exception ex)
            {

                return "未能获得出错信息";
            }
            
        }
        public LogEntity(Exception e)
        {
            this.ErrorDescription = e;
        }

        public LogEntity(string Info)
        {
            this.InfoString = Info;
        }

        public String JsonFormatString()
        {
            
                try
                {

               return  LitJson.JsonMapper.ToJson(this);
                }
                catch (Exception ex)
                {

                    return "在构建Json 字符串时发生错误！";
                }
             
        }

    }
}
