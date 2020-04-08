using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
 

namespace dotNetLab.Common
{
    public static class ExtensionFuncs
    {
      
        public static void AllExecute<T>(this IEnumerable<T> array, Action<T> Predicate)
        {
            int n = array.Count();
            if (n > 0)
            {
                IEnumerator en = array.GetEnumerator();
                while (en.MoveNext())
                {
                    Predicate((T)en.Current);

                }

            }

        }
        public static int IndexOf<T>(this Array array, T obj)
        {
          return Array.IndexOf<T>((T[])array,  obj);
        }
        public static T  GetCustomAttribute<T>(this PropertyInfo pif) where T : Attribute
        {
            
            
            Type AttributeType = typeof(T);
             
            Object  temp = Attribute.GetCustomAttribute(pif, AttributeType) ;

            if (temp != null)
                return (T)temp;
            else
                return null;
        }

        public static T GetCustomAttribute<T>(this Type type) where T : Attribute
        {
            Object [] objs = type.GetCustomAttributes(true);

            foreach (var item in objs)
            {
               Object obj  = (T)item ;
                if (obj != null)
                    return (T)item;
            }

            return null;
        
        }

        public static T[] GetCustomAttributes<T>(this Type type) where T : Attribute
        {
            Object[] objs = type.GetCustomAttributes(true);
            if (objs == null ||objs.Length <1)
                return null;

            T[] arr = new T[objs.Length];
            int n = 0;
            foreach (var item in objs)
            {
                arr[n++] = (T)item;
                
            }

            return arr;

        }

        /// <summary>
        /// 可用于分页，分割数组
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="ElementAmountPerBlock">每个块的元素数量</param>
        /// <param name="BlockIndex">块的索引</param>
        /// <returns>数组</returns>
        public static T[] ToIndexArray<T>(this IEnumerable<T> ts,
               int ElementAmountPerBlock, int BlockIndex)
        {
            int SourceAmount = ts.Count();
            if (SourceAmount == 0)
                return null;

            //总页数

            int parts = SourceAmount / ElementAmountPerBlock;
            if (SourceAmount % ElementAmountPerBlock != 0)
                parts += 1;
            if (BlockIndex >= parts)
                BlockIndex = parts - 1;

            if (BlockIndex <= 0)
                BlockIndex = 0;


            int nBegineIndex = ElementAmountPerBlock * BlockIndex;
            int ncount = 0;

            if (0 == BlockIndex && parts == 1)
            {
                if (SourceAmount < ElementAmountPerBlock)
                    ncount = SourceAmount;

            }
            else if (BlockIndex == parts - 1)
            {
                if (SourceAmount < ElementAmountPerBlock * parts)
                {
                    ncount = SourceAmount - (parts - 1) * ElementAmountPerBlock;
                }
                else
                    ncount = ElementAmountPerBlock;
            }

            else
            {
                ncount = ElementAmountPerBlock;
            }

            

            T[] arr = new T[ncount];

            T[] src = ts.ToArray();
            
            for (int i = nBegineIndex; i < nBegineIndex + ncount; i++)
            {
                arr[i - nBegineIndex] = src[i];
            }
            return arr;

        }


        /// <summary>
        /// 截取字符串
        /// </summary>
        /// <param name="s"></param>
        /// <param name="index">首索引</param>
        /// <param name="end">终索引</param>
        /// <param name="ExceptEndIndex">终索引是否包含在内（默认包含）</param>
        /// <returns></returns>
        public static String SubString(this String s,long index,long end,bool ExceptEndIndex = false)
        {
            if (ExceptEndIndex)
                return s.Substring((int)index, (int)(end - index + 1));
            else
                return s.Substring((int)index, (int)(end - index));

        }

        /// <summary>
        /// 形成sql 无意义
        /// </summary>
        public static bool Like(this String s,String x)
        {
            return s.Contains(x);
        }

        public static object First(this DataTable dt)
        {
            if (dt == null)
                return null;
            if (dt.Rows.Count > 0)
                return dt.Rows[0][0];
            else
                return null;
        }
        public static bool NotLike(this String s,String x)
        {
            return !s.Contains(x);
        }


        /// <summary>
        /// 是否不为null 或empty
        /// </summary>
        public static bool IsValideString(this String str, bool CheckWhiteSpace = false )
        {
            return !String.IsNullOrEmpty(str) && !String.IsNullOrWhiteSpace(str);
        }

        public static String ConnectAll<T>(this IEnumerable<T> array,String gapStr)
        {
           int n =  array.Count();
            StringBuilder sb = new StringBuilder();
            if (n > 0)
            {
                IEnumerator en = array.GetEnumerator();
                while (en.MoveNext())
                {
                    String str = en.Current.ToString();
                    sb.AppendFormat("{0}{1}", str, gapStr);
                }
                String s = sb.ToString().Replace(gapStr,"");
                return s;
            } 
            return null;
        }

        public static List<T> FirstColumn<T>(this DataTable dt)
        {
            List<T> lst = new List<T>();
            if (dt == null)
                return lst;
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        lst.Add((T)dt.Rows[i][j]);
                    }

                }
            }
            return lst;
        }

        //C:\Users\shikii\Desktop\CFGS -> CFGS
        //C:\Users\shikii\Desktop\CFGS\a.txt -> a.txt
        public static String[] GetShortNames (this IEnumerable<String> array )
        {
            int n = array.Count();
            if (n == 0)
                return null;

            String[] arr = new string[n];
            int i = 0;
           
                IEnumerator en = array.GetEnumerator();
                while (en.MoveNext())
                {
                    String str = en.Current.ToString();
                    arr[i++] = Path.GetFileName(str);
                }
            

                return arr;
        }
        //C:\Users\shikii\Desktop\CFGS\a.txt -> C:\Users\shikii\Desktop\CFGS
        //removeStr要移除的字符串
        //newReplaceStr 替代的字符串
        public static String[] GetDirs(this IEnumerable<String> array,String removeStr =null,String newReplaceStr= null)
        {
            int n = array.Count();
            if (n == 0)
                return null;

            String[] arr = new string[n];
            int i = 0;

            IEnumerator en = array.GetEnumerator();
            while (en.MoveNext())
            {
                String str = en.Current.ToString();
               if(removeStr.IsValideString())
                arr[i++] = Path.GetDirectoryName(str).Replace(removeStr, newReplaceStr);
            }


            return arr;
        }

        public static int ToInt(this String str)
        {

            try
            {
                return int.Parse(str);
            }
            catch (Exception ex)
            {

                throw;
              
            }
        }
        public static float ToFloat(this String str)
        {

            try
            {
                return float.Parse(str);
            }
            catch (Exception ex)
            {

                throw;

            }
        }
        public static double ToDouble(this String str)
        {

            try
            {
                return double.Parse(str);
            }
            catch (Exception ex)
            {

                throw;

            }
        }

    }

   
}


// public static int IndexOf(this Array array,Object obj)
// {
//    Type type = obj.GetType();
//    int nIndex = -1;
//    if (type == typeof(int))
//    {
//        nIndex =  Array.IndexOf<int>( (int[]) array,  (int) obj );
//    }
//    else if (type == typeof(double))
//    {
//        nIndex = Array.IndexOf<double>((double[])array, (double)obj);
//    }
//    else if (type == typeof(String))
//    {
//        nIndex = Array.IndexOf<String>((String[])array, obj.ToString());
//    }
//    else if (type == typeof(bool))
//    {
//        nIndex = Array.IndexOf<bool>((bool[])array, (bool)obj);
//    }
//    else if (type == typeof(float))
//    {
//        nIndex = Array.IndexOf<float>((float[])array, (float)obj);
//    }
//    else if (type == typeof(uint))
//    {
//        nIndex = Array.IndexOf<uint>((uint[])array, (uint)obj);
//    }
//    else if (type == typeof(long))
//    {
//        nIndex = Array.IndexOf<long>((long[])array, (long)obj);
//    }
//    return nIndex;
//}