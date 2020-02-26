using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace dotNetLab.Common 
{
    public abstract class Invoker : IDisposable
    {
        protected Type type;
          Object host;
        protected BindingFlags NoPublicBindingFlag = BindingFlags.Instance | BindingFlags.NonPublic;
        public String Name;

        public string DllPath = "";
        public string FullClassName = "";
        public object Host
        {
            get { return host; }
            set
            {
                host = value;
                if (this.type == null)
                {
                    type = Host.GetType();
                    GetMembers();

                }
            }
        }
        protected abstract void GetMembers();

        protected MethodInfo GetMethod(string Name, params Type[] types)
        {
            return type.GetMethod(Name, types);
        }
        protected EventInfo GetEvent(string Name)
        {

            return type.GetEvent(Name);
        }
        
        public void Dispose()
        {
            host = null;
        }



    }
}
