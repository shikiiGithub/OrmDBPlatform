using System;
using System.Collections.Generic;
using System.Text;

namespace dotNetLab.Common 
{
   public delegate void ErrorCallback(Object obj, Exception ex);
    public delegate void InfoCallback(Object obj, Object info);
    public  interface IReadableLog
    { 
       
       event ErrorCallback ErrorHandler;
       event InfoCallback InfoHandler;
    }
}
