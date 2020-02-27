﻿﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace dotNetLab.Data
{
  public partial  class DBPlatform
    {
        public string DefaultTable { get { return "App"; } }
        public void Write(string strTableName, string strName, String strValue)
        {
             
            if (IsExistRecord(strName, strTableName))
                this.Update(strTableName, string.Format("Val='{0}' Where Name='{1}' ", strValue, strName));
            else
                this.NewRecord(strTableName, string.Format("'{0}','{1}'", strName, strValue));
           
        }
        public void Write(string strName, string strValue)
        {
            Write(DefaultTable, strName, strValue);
        }
        public string FetchValue( String strTableName,String strLabelName, bool NewWhenNoFound = true,
            String strDefaultValue = "0")
        {

            cc:;
            String temp = this.UniqueResult(
                string.Format("select Val from {0} where Name='{1}'; "
                , strTableName, strLabelName));
            if (temp == null && NewWhenNoFound)
            {
                Write(strTableName, strLabelName, strDefaultValue);
                goto cc;
            }
            else
            {
                return temp;
            }
        }
        public String FetchValue(String strLabelName, bool NewWhenNoFound = true, String strDefaultValue = "0")
        {
            return FetchValue(DefaultTable, strLabelName,  NewWhenNoFound, strDefaultValue);
        }
        public int FetchIntValue(string strLabelName, bool NewWhenNoFound = true, String strDefaultValue = "0")
        {
            string temp = FetchValue(DefaultTable, strLabelName,  NewWhenNoFound,  strDefaultValue );
            try
            {
                return int.Parse(temp);
            }
            catch (Exception e)
            {
                ErrorHandler(this, e);
                return nError;
            }


        }
        public int FetchIntValue(string strTableName, string strLabelName,  bool NewWhenNoFound = true, String strDefaultValue = "0")
        {
            string temp = FetchValue( strTableName,strLabelName, NewWhenNoFound, strDefaultValue);
            try
            {
                return int.Parse(temp);
            }
            catch (Exception e)
            {
                ErrorHandler(this, e);
                return nError;
            }


        }
        public float FetchFloatValue(string strLabelName, bool NewWhenNoFound = true, String strDefaultValue = "0")
        {
            string temp = FetchValue(DefaultTable, strLabelName, NewWhenNoFound, strDefaultValue);
            try
            {
                return float.Parse(temp);
            }
            catch (Exception e)
            {
                ErrorHandler(this, e);
                return nError;
            }
        }
        public double FetchDoubleValue(string strLabelName, bool NewWhenNoFound = true, String strDefaultValue = "0")
        {
            string temp = FetchValue(DefaultTable, strLabelName, NewWhenNoFound, strDefaultValue);
            try
            {
                return double.Parse(temp);
            }
            catch (Exception e)
            {
                ErrorHandler(this, e);
                return nError;
            }

        }
        public DateTime FetchDateTimeValue(string strLabelName, bool NewWhenNoFound = true, String strDefaultValue = "0")
        {
            string temp = FetchValue(DefaultTable, strLabelName,  NewWhenNoFound, strDefaultValue);
            try
            {
                return DateTime.Parse(temp);
            }
            catch (Exception e)
            {
                ErrorHandler(this, e);
                return DateTime.MinValue;
            }
        }
        public bool FetchBoolValue(string strLabelName, bool NewWhenNoFound = true, String strDefaultValue = "0")
        {
            string temp = FetchValue(DefaultTable, strLabelName, NewWhenNoFound, strDefaultValue);
            try
            {
                return Boolean.Parse(temp);
            }
            catch (Exception e)
            {
                ErrorHandler(this, e);
                return false;
            }
        }
        public void WriteArray<T>(String strTableName, String strName, T[] tArr)
        {
            for (int i = 0; i < tArr.Length; i++)
            {
                this.AppendItem(strTableName, strName, tArr[i].ToString());
            }
        }
        public void WriteArray<T>(String strName, T[] tArr)
        {

            WriteArray<T>(DefaultTable, strName, tArr);

        }
        public void AppendItem(string strLabelName, String obj)
        {
            AppendItem(DefaultTable, strLabelName, obj);
        }

        //添加一个数组元素，这个数据元素可以与原有的数组元素相同。
        //如果这个键值对不存在，创建这个键值对并返回。
        //如果这个键值对存在则将之前的值与新增的值组成一个数组再写入到这个键所对应的值 
        public void AppendItem(string strTableName, string strLabelName, String obj)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                String[] strArr_Write = null;
                //先尝试取出字符串数组
                String[] strArr = FetchArray(strTableName, strLabelName);
                if (obj == null)
                    return;
                //如果不是数组
                if (strArr == null)
                {
                    
                    //取出这个值（键值对）
                    String str = FetchValue(strTableName, strLabelName,false,"0");
                   //如果这个键值对不存在
                    if (str == null || str.Equals(""))
                    {
                        //创建这个键值对
                        Write(strTableName, strLabelName, obj.ToString());
                        return;
                    }
                    //如果这个键值对存在则将之前的值与新增的值组成一个数组。
                    else
                    {
                        
                        strArr_Write = new String[] { str, obj };
                    }

                }
                else
                {
                    strArr_Write = new String[strArr.Length + 1];
                    for (int i = 0; i < strArr.Length; i++)
                    {
                        strArr_Write[i] = strArr[i];
                    }
                    strArr_Write[strArr_Write.Length - 1] = obj;
                }

                //将新增的数据写入
                for (int i = 0; i < strArr_Write.Length; i++)
                {
                    if (i == 0)
                    {
                        sb.Append(strArr_Write[i]);
                    }
                    else
                        sb.Append(String.Format("{0}{1}", SPLITMARK, strArr_Write[i]));
                }
                this.Write(strTableName, strLabelName, sb.ToString());
                sb.Remove(0, sb.Length);
                sb = null;
            }
            catch (Exception ex)
            {

                ErrorHandler(this, ex);

            }
           

        }
        //添加一个数组元素，这个数据元素不能与原有的数组元素相同。
        //如果这个键值对不存在，创建这个键值对并返回。
        //如果这个键值对存在则将之前的值与新增的值组成一个数组再写入到这个键所对应的值 
        public void AppendUniqueItem(String strTableName, String strLabelName, String obj)
        {
            String[] strArr_Write = null;
            String[] strArr = FetchArray(strTableName, strLabelName);
            if (obj == null)
                return;
            if (strArr == null)
            {
               
                String str = FetchValue(strTableName, strLabelName,false,"-69");
                if (str == null || str.Equals(""))
                {
                    Write(strTableName, strLabelName, obj.ToString());
                    return;
                }
                else
                {
                    strArr_Write = new String[] { str, obj };
                }
            }
            else
            {
                strArr_Write = new String[strArr.Length + 1];
                for (int i = 0; i < strArr.Length; i++)
                {
                    strArr_Write[i] = strArr[i];
                }
                strArr_Write[strArr_Write.Length - 1] = obj;
            }
            StringBuilder sb = new StringBuilder();
            List<String> lst_Arr = new List<String>();
           
            if (strArr == null)
            {
                for (int i = 0; i < strArr_Write.Length; i++)
                {
                    if (lst_Arr.Contains(strArr_Write[i]))
                        continue;
                    lst_Arr.Add(strArr_Write[i]);
                }
            }
            else
            {
                for (int j = 0; j < strArr_Write.Length; j++)
                {
                    if (!lst_Arr.Contains(strArr_Write[j]))
                        lst_Arr.Add(strArr_Write[j]);
                }
            }


            for (int i = 0; i < lst_Arr.Count; i++)
            {
                if (i == 0)
                {
                    sb.Append(lst_Arr[i]);
                }
                else
                    sb.Append(String.Format("{0}{1}", SPLITMARK, lst_Arr[i]));
            }
            this.Write(strTableName, strLabelName, sb.ToString());
            sb.Remove(0, sb.Length);
            sb = null;
        }
        public void AppendUniqueItem( String strLabelName, String obj)
        {
            AppendUniqueItem(DefaultTable, strLabelName, obj);
        }
#if NET4
        public void AppendDynamicItem(String strTableName, String strLabelName, dynamic obj,bool ElementCanRepeat)
        {
            
            Type type =  obj.GetType();
            PropertyInfo[] pifs = type.GetProperties();

            foreach (PropertyInfo item in pifs)
            {
                Object o = item.GetValue(obj, null);
                if(ElementCanRepeat)
                  AppendItem( strTableName,strLabelName,o.ToString());
                else
                    AppendUniqueItem(strTableName, strLabelName, o.ToString());
            }
        }

        public void AppendDynamicItem(String strLabelName, dynamic obj, bool ElementCanRepeat)
        {
            Type type = obj.GetType();
            PropertyInfo[] pifs = type.GetProperties();


            foreach (var item in pifs)
            {
                Object o = item.GetValue(obj, null);
                if (ElementCanRepeat)
                    AppendItem(DefaultTable, strLabelName, o.ToString());
                else
                    AppendUniqueItem(DefaultTable, strLabelName, o.ToString());
            }
        }
#endif

        public String[] FetchArray(string strLabelName)
        {

            /* String str = FetchValue(strLabelName);
             if (str.Contains(SPLITMARK.ToString()))
                 return str.Split(new char[] { SPLITMARK });
             else
                 return null;*/
            return FetchArray("App_Extension_Data_Table", strLabelName);
        }
        public String[] FetchArray(String strTableName, string strLabelName )
        {

            
            String str = FetchValue(strLabelName, strTableName,false,"-89");

            if (str == null)
                return null;
            else
            {
                if (str.Contains(SPLITMARK.ToString()))
                    try
                    {
                        String[] str_ = str.Split('^');
                        return str_;
                    }
                    catch (Exception e)
                    {

                        return null;
                    }
                else
                    return new String[] { str };
            }
        }
        public void FetchFloatArray(string strLabelName, out float[] fArr)
        {
            String[] pArr = FetchArray(strLabelName);
            fArr = new float[pArr.Length];
            for (int i = 0; i < fArr.Length; i++)
            {
                fArr[i] = Convert.ToSingle(pArr[i]);
            }

        }
        public void FetchDoubleArray(string strLabelName, out double[] lfArr)
        {
            String[] pArr = FetchArray(strLabelName);
            lfArr = new double[pArr.Length];
            for (int i = 0; i < lfArr.Length; i++)
            {
                lfArr[i] = Convert.ToDouble(pArr[i]);
            }
        }
        public void FetchIntArray(String strLabelName, out int[] arr)
        {
            String[] pArr = FetchArray(strLabelName);
            arr = new int[pArr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = Convert.ToInt32(pArr[i]);
            }
        }
        protected bool IsExistRecord(string strName, string Tablename)
        {
         
            string temp = this.UniqueResult(
                string.Format("select count(*) from {0} where Name='{1}'"
                 , Tablename, strName
                 ));
            try
            {
                if (int.Parse(temp) > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception e)
            {

                return false;
            }

        }
       
    }
}
