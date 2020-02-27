#if NET4
using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Linq;
using System.Linq.Expressions;

using System.Text;
 

namespace dotNetLab.Data.Orm
{
    public class SqlParaModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string name { set; get; }

        /// <summary>
        /// 
        /// </summary>
        public object value { set; get; }
    }


    /// <summary>
    /// 作者博客连接 https://www.cnblogs.com/maiaimei/p/7147049.html
    /// </summary>
    public class Expression2SQL
    {
        /// <summary>
        /// NodeType枚举
        /// </summary>
        private enum EnumNodeType
        {
            /// <summary>
            /// 二元运算符
            /// </summary>
            [Description("二元运算符")] BinaryOperator = 1,

            /// <summary>
            /// 一元运算符
            /// </summary>
            [Description("一元运算符")] UndryOperator = 2,

            /// <summary>
            /// 常量表达式
            /// </summary>
            [Description("常量表达式")] Constant = 3,

            /// <summary>
            /// 成员（变量）
            /// </summary>
            [Description("成员（变量）")] MemberAccess = 4,

            /// <summary>
            /// 函数
            /// </summary>
            [Description("函数")] Call = 5,

            /// <summary>
            /// 未知
            /// </summary>
            [Description("未知")] Unknown = -99,

            /// <summary>
            /// 不支持
            /// </summary>
            [Description("不支持")] NotSupported = -98
        }

        /// <summary>
        /// 判断表达式类型
        /// </summary>
        /// <param name="exp">lambda表达式</param>
        /// <returns></returns>
        private EnumNodeType CheckExpressionType(Expression exp)
        {
            switch (exp.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                case ExpressionType.Equal:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.LessThan:
                case ExpressionType.NotEqual:
                    return EnumNodeType.BinaryOperator;
                case ExpressionType.Constant:
                    return EnumNodeType.Constant;
                case ExpressionType.MemberAccess:
                    return EnumNodeType.MemberAccess;
                case ExpressionType.Call:
                    return EnumNodeType.Call;
                case ExpressionType.Not:
                case ExpressionType.Convert:
                    return EnumNodeType.UndryOperator;
                default:
                    return EnumNodeType.Unknown;
            }
        }

        /// <summary>
        /// 表达式类型转换
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string ExpressionTypeCast(ExpressionType type)
        {
            switch (type)
            {
                 
                
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return " and ";
                case ExpressionType.Equal:
                    return " = ";
                case ExpressionType.GreaterThan:
                    return " > ";
                case ExpressionType.GreaterThanOrEqual:
                    return " >= ";
                case ExpressionType.LessThan:
                    return " < ";
                case ExpressionType.LessThanOrEqual:
                    return " <= ";
                case ExpressionType.NotEqual:
                    return " <> ";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return " or ";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return " + ";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return " - ";
                case ExpressionType.Divide:
                    return " / ";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return " * ";

                default:
                    return null;
            }
        }

        private string BinarExpressionProvider(Expression exp)
        {
            BinaryExpression be = exp as BinaryExpression;
            Expression left = be.Left;
            Expression right = be.Right;
            ExpressionType type = be.NodeType;
            string sb = "";
            //先处理左边
            sb += ExpressionRouter(left);
            sb += ExpressionTypeCast(type);
            //再处理右边
            string sbTmp = ExpressionRouter(right);
            if (sbTmp == "null")
            {
                if (sb.EndsWith(" = "))
                    sb = sb.Substring(0, sb.Length - 2) + " is null";
                else if (sb.EndsWith(" <> "))
                    sb = sb.Substring(0, sb.Length - 2) + " is not null";
            }
            else
                sb += sbTmp;

            return sb;
        }

        private string ConstantExpressionProvider(Expression exp)
        {
            ConstantExpression ce = exp as ConstantExpression;
            if (ce.Value == null)
            {
                return "null";
            }
            //else if (ce.Value is ValueType && ce.Type.Name.Equals("Byte[]"))
            //{
            //    GetSqlParaModel(listSqlParaModel, GetValueType(ce.Value));
            //    return "@para" + listSqlParaModel.Count;
            //}
            else if (ce.Value.Equals("*"))
                return "*";
            else if (ce.Value is string || ce.Value is DateTime || ce.Value is char)
            {
                return String.Format("'{0}'", ce.Value.ToString());
            }

            return ce.Value.ToString();
        }

        private string LambdaExpressionProvider(Expression exp)
        {
            LambdaExpression le = exp as LambdaExpression;
            return ExpressionRouter(le.Body);
        }

        private string MemberExpressionProvider(Expression exp)
        {
            try
            {
                MemberExpression me = exp as MemberExpression;
                if (me.Expression.NodeType == ExpressionType.Constant)
                {
                    Object obj = Expression.Lambda(exp).Compile().DynamicInvoke();
                    String str = "";
                    var _type = obj.GetType().Name;
                    switch (_type)
                    {
                        case "DateTime":
                        case "String":
                        case "Char":
                            str = String.Format(" '{0}' ", obj);
                            break;
                        default:
                            str = obj.ToString();
                            break;
                    }

                    return str;
                }
                else
                {
                    return me.Member.Name;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("shikii Error: ");
                return "";
            }
        }

        private string MethodCallExpressionProvider(Expression exp)
        {
            MethodCallExpression mce = exp as MethodCallExpression;
            StringBuilder sb = new StringBuilder();
            String SecondStr = "";
            if (mce.Object is MethodCallExpression)
                sb.Append(MethodCallExpressionProvider(mce.Object));

            switch (mce.Method.Name)
            {
                case "Contains":
                    if (mce.Object == null)
                    {
                        return string.Format("{0} in ({1})", ExpressionRouter(mce.Arguments[1]),
                            ExpressionRouter(mce.Arguments[0]));
                    }
                    else
                    {
                        if (mce.Object.NodeType == ExpressionType.MemberAccess)
                        {
                            var _name = ExpressionRouter(mce.Object);
                            var _value = ExpressionRouter(mce.Arguments[0]).Replace("'", "").Trim();

                            return string.Format("{0} like '%{1}%'", _name, _value);
                        }
                    }

                    break;
                case "Equals":
                    if (mce.Object == null)
                    {
                        return string.Format("{0} in ({1})", ExpressionRouter(mce.Arguments[1]),
                            ExpressionRouter(mce.Arguments[0]));
                    }
                    else
                    {
                        if (mce.Object.NodeType == ExpressionType.MemberAccess)
                        {
                            var _name = ExpressionRouter(mce.Object);
                            var _value = ExpressionRouter(mce.Arguments[0]);
                            return string.Format("{0} = {1}", _name, _value);
                        }
                    }

                    break;

                case "UpdateSet":
                    if (mce.Arguments.Count == 0)
                        return " ";
                    //StringBuilder _xsb = new StringBuilder();
                    //for (int i = 0; i <mce.Arguments.Count ; i+=2)
                    //{
                    //    _xsb.AppendFormat(" {1}={0} ,", ExpressionRouter(mce.Arguments[i], listSqlParaModel).Replace("'", "")
                    //        , ExpressionRouter(mce.Arguments[i+1], listSqlParaModel)
                    //        );
                    //}
                    //_xsb.Remove(_xsb.Length - 1, 1);
                    SecondStr = string.Format(" set {0} ", ExpressionRouter(mce.Arguments[1]));
                    SecondStr = SecondStr.Replace(",", "=");
                    break;
                case "InsertInto":
                    SecondStr = string.Format(" insert into {0} ", ExpressionRouter(mce.Arguments[1]).Replace("'", ""));
                    break;
                case "Drop":
                    SecondStr = string.Format(" drop {0} ",
                        ExpressionRouter(mce.Arguments[1]).Replace("'", "").Replace(",", " "));
                    break;
                case "Alter":
                    SecondStr = string.Format(" alter {0} ", ExpressionRouter(mce.Arguments[1]).Replace("'", ""));
                    break;
                case "Update":
                    SecondStr = string.Format(" update {0} ", ExpressionRouter(mce.Arguments[1]).Replace("'", ""));
                    break;
                case "OrderByASC":
                    SecondStr = string.Format(" order by {0} asc", ExpressionRouter(mce.Arguments[1]));
                    break;
                case "OrderByDESC":
                    SecondStr = string.Format(" order by  {0} desc", ExpressionRouter(mce.Arguments[1]));
                    break;
                case "Select":
                    SecondStr = string.Format(" Select {0} ", ExpressionRouter(mce.Arguments[1]).Replace("'", ""));
                    break;
                case "From":
                    SecondStr = string.Format(" From {0} ", ExpressionRouter(mce.Arguments[1]).Replace("'", ""));
                    break;
                case "Max":
                    SecondStr = string.Format(" max({0}) ", ExpressionRouter(mce.Arguments[1]));
                    break;
                case "Min":
                    SecondStr = string.Format(" Min({0}) ", ExpressionRouter(mce.Arguments[1]));
                    break;
                case "Count":
                    SecondStr = string.Format(" Count({0}) ", ExpressionRouter(mce.Arguments[1]));
                    break;
                case "Avg":
                    SecondStr = string.Format(" Avg({0}) ", ExpressionRouter(mce.Arguments[1]).Replace("'", ""));
                    break;
                case "Sum":
                    SecondStr = string.Format(" Sum({0}) ", ExpressionRouter(mce.Arguments[1]).Replace("'", ""));
                    break;
                case "Lcase":
                    SecondStr = string.Format(" Lcase({0}) ", ExpressionRouter(mce.Arguments[1]).Replace("'", ""));
                    break;
                case "UCase":
                    SecondStr = string.Format(" UCase({0}) ", ExpressionRouter(mce.Arguments[1]));
                    break;
                case "Round":
                    SecondStr = string.Format(" Round({0},{1}) ", ExpressionRouter(mce.Arguments[1]),
                        ExpressionRouter(mce.Arguments[2]));
                    break;
                case "Like":
                    SecondStr = string.Format("({0} like {1})", ExpressionRouter(mce.Arguments[1]),
                        ExpressionRouter(mce.Arguments[2]).Replace("'", ""));
                    break;
                case "NotLike":
                    SecondStr = string.Format("({0} not like '%{1}%')", ExpressionRouter(mce.Arguments[1]),
                        ExpressionRouter(mce.Arguments[2]).Replace("'", ""));
                    break;
                case "As":
                    SecondStr = string.Format(" As {0} ", ExpressionRouter(mce.Arguments[1]).Replace("'", ""));
                    break;
                case "Limit":
                    if (mce.Arguments.Count > 1)
                        SecondStr = string.Format(" Limit {0},{1} ", ExpressionRouter(mce.Arguments[1]),
                            ExpressionRouter(mce.Arguments[2]));
                    else
                        SecondStr = string.Format(" Limit {0} ", ExpressionRouter(mce.Arguments[1]));
                    break;
                case "Distinct":
                    SecondStr = string.Format(" Distinct {0}  ", ExpressionRouter(mce.Arguments[1]));
                    break;
                case "With":
                case "WhereWith":
                    SecondStr = string.Format(" {0} ", ExpressionRouter(mce.Arguments[1])).Replace(",", " ");
                    break;
                case "WithComma":
                case "WhereWithComma":
                    SecondStr = string.Format(" {0} ", ExpressionRouter(mce.Arguments[1]).Replace("'", ""));
                    break;
                case "InnerJoin":
                    SecondStr = string.Format(" {0} inner join {1} on {2} ",
                        ExpressionRouter(mce.Arguments[1]).Replace("'", "")
                        , ExpressionRouter(mce.Arguments[2]).Replace("'", ""),
                        ExpressionRouter(mce.Arguments[3]).Replace("'", "")
                    );
                    break;
            }


            //else if (mce.Method.Name == "ThenBy")
            //{
            //    return string.Format("{0},{1} asc", MethodCallExpressionProvider(mce.Arguments[0], listSqlParaModel), ExpressionRouter(mce.Arguments[1], listSqlParaModel));
            //}
            //else if (mce.Method.Name == "ThenByDescending")
            //{
            //    return string.Format("{0},{1} desc", MethodCallExpressionProvider(mce.Arguments[0], listSqlParaModel), ExpressionRouter(mce.Arguments[1], listSqlParaModel));
            //}
            //else if (mce.Method.Name == "In")
            //{
            //    return string.Format("{0} in ({1})", ExpressionRouter(mce.Arguments[0], listSqlParaModel), ExpressionRouter(mce.Arguments[1], listSqlParaModel));
            //}
            //else if (mce.Method.Name == "NotIn")
            //{
            //    return string.Format("{0} not in ({1})", ExpressionRouter(mce.Arguments[0], listSqlParaModel), ExpressionRouter(mce.Arguments[1], listSqlParaModel));
            //}
            sb.AppendFormat(" {0} ", SecondStr);
            return sb.ToString();
        }

        private string NewArrayExpressionProvider(Expression exp)
        {
            NewArrayExpression ae = exp as NewArrayExpression;
            StringBuilder sbTmp = new StringBuilder();
            foreach (Expression ex in ae.Expressions)
            {
                sbTmp.Append(ExpressionRouter(ex));
                sbTmp.Append(",");
            }

            return sbTmp.ToString(0, sbTmp.Length - 1);
        }

        private string ParameterExpressionProvider(Expression exp)
        {
            ParameterExpression pe = exp as ParameterExpression;
            return pe.Type.Name;
        }

        private string UnaryExpressionProvider(Expression exp)
        {
            UnaryExpression ue = exp as UnaryExpression;
            var result = ExpressionRouter(ue.Operand);
            ExpressionType type = exp.NodeType;
            if (type == ExpressionType.Not)
            {
                if (result.Contains(" in "))
                {
                    result = result.Replace(" in ", " not in ");
                }

                if (result.Contains(" like "))
                {
                    result = result.Replace(" like ", " not like ");
                }
            }

            return result;
        }

        /// <summary>
        /// 路由计算
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="listSqlParaModel"></param>
        /// <returns></returns>
        private string ExpressionRouter(Expression exp)
        {
            var nodeType = exp.NodeType;


            if (exp is BinaryExpression) //表示具有二进制运算符的表达式
            {
                return BinarExpressionProvider(exp);
            }
            else if (exp is ConstantExpression) //表示具有常数值的表达式
            {
                return ConstantExpressionProvider(exp);
            }
            else if (exp is LambdaExpression) //介绍 lambda 表达式。 它捕获一个类似于 .NET 方法主体的代码块
            {
                return LambdaExpressionProvider(exp);
            }
            else if (exp is MemberExpression) //表示访问字段或属性
            {
                return MemberExpressionProvider(exp);
            }
            else if (exp is MethodCallExpression) //表示对静态方法或实例方法的调用
            {
                //  MethodCallExpression mce = exp as MethodCallExpression;


                return MethodCallExpressionProvider(exp);
            }
            else if (exp is NewArrayExpression) //表示创建一个新数组，并可能初始化该新数组的元素
            {
                return NewArrayExpressionProvider(exp);
            }
            else if (exp is ParameterExpression) //表示一个命名的参数表达式。
            {
                return ParameterExpressionProvider(exp);
            }
            else if (exp is UnaryExpression) //表示具有一元运算符的表达式
            {
                return UnaryExpressionProvider(exp);
            }

            return null;
        }

        /// <summary>
        /// 值类型转换
        /// </summary>
        /// <param name="_value"></param>
        /// <returns></returns>
        private object GetValueType(object _value)
        {
            var _type = _value.GetType().Name;

            switch (_type)
            {
                case "Decimal": return Convert.ToDecimal(_value);
                case "Int32": return Convert.ToInt32(_value);
                case "DateTime": return Convert.ToDateTime(_value);
                case "String": return _value.ToString();
                case "Char": return Convert.ToChar(_value);
                case "Boolean": return Convert.ToBoolean(_value);
                default: return _value;
            }
        }

        //主要使用这个方法
        public String GetRawSql<T>(Expression<Func<T, bool>> expression) where T : EntityBase
        {
            if (expression == null)
                return "";
            Expression exp = expression.Body as Expression;
            List<SqlParaModel> listSqlParaModel = new List<SqlParaModel>();
            String result = ExpressionRouter(exp);
            return result;
        }

        public String GetRawSql<T>(Expression<Func<T, Entry>> expression) where T : EntityBase
        {
            if (expression == null)
                return "";
            Expression exp = expression.Body as Expression;
            List<SqlParaModel> listSqlParaModel = new List<SqlParaModel>();
            String result = ExpressionRouter(exp);
            return result;
        }
    }
}

#endif