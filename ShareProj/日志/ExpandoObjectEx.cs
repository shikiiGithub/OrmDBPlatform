#if NET4
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace dotNetLab.Common
{
    /// <summary>
    /// 暂时不用
    /// </summary>
    public class ExpandoObjectEx : DynamicObject
    {
        //保存对象动态定义的属性值
        private Dictionary<string, object> _values;
        protected String Name;
        public ExpandoObjectEx(String dynamicObjectName)
        {
            _values = new Dictionary<string, object>();
            
            Name = dynamicObjectName;
        }
        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public object GetPropertyValue(string propertyName)
        {
            if (_values.ContainsKey(propertyName) == true)
            {
                return _values[propertyName];
            }
            return null;
        }
        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public void SetPropertyValue(string propertyName, object value)
        {
            if (_values.ContainsKey(propertyName) == true)
            {
                _values[propertyName] = value;
            }
            else
            {
                _values.Add(propertyName, value);
            }
        }
        /// <summary>
        /// 实现动态对象属性成员访问的方法，得到返回指定属性的值
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = GetPropertyValue(binder.Name);
            return result == null ? false : true;
        }
        /// <summary>
        /// 实现动态对象属性值设置的方法。
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            SetPropertyValue(binder.Name, value);
            return true;
        }
        /// <summary>
        /// 动态对象动态方法调用时执行的实际代码
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = new object();
            //var theDelegateObj = GetPropertyValue(binder.Name) as DynamicDelegate<T>;
            //if (theDelegateObj == null || theDelegateObj.CallMethod == null)
            //{
            //    result = null;
            //    return false;
            //}
            //result = theDelegateObj.CallMethod(this, args);
            return true;
        }
        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            return base.TryInvoke(binder, args, out result);
        }
    }
    /// <summary>
    /// 包装lambda 方法用
    /// </summary>
    internal class DynamicDelegate<T>
    {
        private T _delegate;

        public T CallMethod
        {
            get { return _delegate; }
        }
        private DynamicDelegate(T D)
        {
            _delegate = D;
        }
        /// <summary>
        /// 构造委托对象，让它看起来有点javascript定义的味道.
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        public static DynamicDelegate<T> Function(T D)
        {
            return new DynamicDelegate<T>(D);
        }
    }

}

#endif