# OrmDBPlatform(文档持续更新中)

跨平台极轻.net Orm框架,内置简单的多并发处理（联系：shikii@outlook.com 或者在Issues上提问）

## 支持特性
### 1.net framework 最低支持到4.0，.net core/standard 支持到2.0或者2.0以上

### 2 支持桌面/移动（Xamarin）开发
### 3.支持关系数据库有 SQL Server/LocalDB/Firebird/MySQL/Postgresql/SQLCE/Sqlite

## 库的选择
### 对于Xamarin 请选择使用OrmDBPlatform.NETStandard项目
### 传统桌面请使用.net_4.0 /.net core 的项目

## 开始使用
   ### 1.最简单的使用是使用nuget添加你想连接数据库的Data Provider的引用，比如搜索 Mysql 然后安装，安装完成后初始化
```c#
        //初始化
        OrmDB = new OrmDBPlatform();

         //监控错误，将日志同时写入Console/db/文本文件中 
         //下面使用默认方式，默认方式为同步模式
         OrmDB. ErrorInvoker = null;
         Orm.InfoInvoker=null; 


        //shikii 为数据库名 root 为用户名 123为密码
        //默认使用本地的数据库（不适用SQL Server /LocalDB）
        //EntitySourceAssemblies 表示实体类所在的Assembly,默认使用当前程序集
        bool isConnected = OrmDB.Connect("shikii",  "root","123"); //localhost模式下使用默认的端口
        
        //远程访问则请使用（注意不能用这种方式连接SQL Server /LocalDB，往下有介绍）
         bool isConnected = OrmDB.Connect( ip, "shikii",  "123",  "root") ;        //使用默认端口号 如mysql 3306 
         bool isConnected = OrmDB.Connect( ip,  port,"shikii",  "123",  "root") ;//使用自定义端口号
       
         //如果使用sqlite/sqlce 
          bool isConnected = OrmDB.Connect( dbPath) ;

        //对于SQL Server /LocalDB
           Assembly asm_SQLSEVER = typeof(System.Data.SqlClient.SqlConnection) ;
         //本地连接localhost
         bool isConnected = OrmDB.Connect(   asm_SQLSEVER,  "shikii", "sa",  "123") ;
         //远程连接
          bool isConnected = OrmDB.Connect(   asm_SQLSEVER, ip,port, "shikii",  "123", "sa") ;
         //并发处理(可以提前准备多个备用连接，然后OrmDBPlatform 库会智能管理你所创建的连接)
        //也可以不显式指定，OrmDBPlatform库会根据情况自动增加连接数并循环利用
        //显式指定的好处是可以更快的响应并发。（可以使用并行来察看并发处理的效果）
        for(int i = 0;i< 你定义备用连接数;i++);
        DbCommand dbcom = GetNewDbCommand();
     
        
   
```
   ### 2.新建立Entities（可自己命名） 文件夹，在Entities下新建 SampleEntity 类，这将在OrmDBPlatform.Connect 后创建名为Sample的表，请注意必须继承自EntityBase
   ```c#
      //!!!一定要有主键!!! !!!一定要有主键!!! !!!一定要有主键!!! 重要的事情说三遍
      //可以使用  [Entity("MANUAL_CREATE_TABLE")] 来手动创建此表 
      //默认自动创建，此外使用Entity特性可以在创建表后执行特定任务
      //比如：[Entity(EntityAttribute.ActionType.Alter, "add unique (MessageContent,MessageRecordTime)")]
      //这将在创建完表后执行sql 语句（添加唯一键）
      //不支持命名自定义表名
      
      public class SampleEntity : EntityBase
      {
          //当使用Id作为自增字段时一定要加[DBKey] 
          //并非是我这么规则而是各数据库这么规定
          //当字段中出现Id时会默认视其为自增字段，可以不用给值
          //[DBKey]  
          //public int Id{get;set;}
          
         [DBKey] //设置主键，也可以设置外键
          public String Name{get;set;}  
          //指定字符串长度（不需要显示使用，默认使用[MySqlTextTypeAttribute("varchar(255)")]）
         //可以将多个特性用于一个字段，这样有利于适配更多数据库 
          [MySqlTextType("MEDIUMTEXT")] 
          [SQLiteTextType("Text")]
          public String XDesc{get;set;}
        

      }
   ```
### 3.增
 ```c#
          //一般，数据量少时使用
              SampleEntity se = new SampleEntity() ;
              se.Name="Google" ;
              se.XDesc="Fuck Google" ;
              se.Save() ;
          //需要注意的是如果需要在高速场合下请使用：
            se.Save(SaveMode.INSERT) ;
          //批量（事务）
           /// <summary>
        /// 批量事务处理(适用于大数据传递)
        /// 不需要显示使用transaction,内置使用Transaction
        /// 可以使用orm方式对数据库写操作
        /// 也可以使用传入的DbCommand 对象写操作
        /// </summary>
        /// <param name="actions">多个线程要执行的Action(每个Action 一个线程)</param>
        public void BatchExecuteNonQuery(String _conn=null,params Action<DbCommand>[] actions)
       //例子：
        OrmDB.BatchExecuteNonQuery(null,(cmd)=>{
            for(int i=0;i<100;i++)
            {
               SampleEntity sample  = OrmDB.GetEntity<SampleEntity>() ;
                 sample.Name = "Microsoft" ;
                 sample.XDesc = "炒鸡坑逼" ;
                 sample.Save(SaveMode.INSERT) ;
            }

        },(cmd)=>{ for(int i=0;i<100;i++)
            {
               SampleEntity sample  = OrmDB.GetEntity<SampleEntity>() ;
                 sample.Name = "Google" ;
                 sample.XDesc = "创新垃圾" ;
                 sample.Save(SaveMode.INSERT) ;
            } }) ;


 ```

### 4.删
 ```c#
            //根据条件删除
            OrmDB.Delete<SampleEntity>(x=>x.Name=="Google") ;
           //根据主键值删除
           OrmDB.Delete<SampleEntity>(1) ;
 ```

### 5.改
 ```c#
           
           //数据量少，不要求速度时
            se.Save();
          //需要注意的是如果需要在高速场合下请使用：
            se.Save(SaveMode.UPDATE) ;
 ```

### 6.查
 ```c#
      //返回实体集
     List<SampleEntity> lst  = OrmDB.Where<SampleEntity>(x=>x.Name=="Google") ;
      // 第一个参数null 表示“select  * ”
      // 第二个表示“ from Sample order by ShareItemRecordTime desc ” 其中from Sample 可以不写
     //第三个参数表示无条件筛选
      lst  = OrmDB.Where<SampleEntity>(null,  x.OrderByDESC(x.ShareItemRecordTime) , null);;
   
      //返回单个实体
       SampleEntity entity = OrmDB.WhereUniqueEntity(x=>x.Name=="Google");
    
      //返回为DataTable
        /// <summary>
        /// 兼容以前的查询方式，灵活度最高
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="selectSQLExpression">对应于select 语句</param>
        /// <param name="FromSQLExpression">对应于from sql语句</param>
        /// <param name="WhererExpression">对应于where sql语句</param>
      public virtual DataTable InternalQuery<T>(Expression<Func<T, Entry>> selectSQLExpression  ,Expression<Func<T, Entry>>       FromSQLExpression  = null,Expression<Func<T, bool>> WhererExpression=null)

    //示例
    //统计表中记录总条数
   //需要注意的是 x.Select(x.Count("*")) 不能写成 x.Select().Count("*")
     DataTable dt  =  InternalQuery<SampleEntity>(x=>x.Select(x.Count("*")),null,null) ;
       

 ```

### 7.如何使用“简化”ADO.NET Helper
```C#
 OrmDB.AdonetContext.ExecuteNonQuery(sql,这个参数可以不给);
 //查询
DataTable dt = OrmDB.AdonetContext.ProvideTable(  sql,这个参数可以不给);
//查询单个值 
String str = UniqueResult(string sql, DbCommand cmd = null)
//更多请参考
DBPlatform.cs 源码

```

### 8.写/读键值对,写入/读取Json
```C#
//写/读键值对 
 //写
OrmDB.AdonetContext.Write(key,value) ;
//读
String val = OrmDB.AdonetContext.Fetch(key) ; 

//写入/读取Json
//建议作为读写程序配置使用
//写
System.Dynamic.ExpandoObject dyn_Obj = new System.Dynamic.ExpandoObject() ;
dyn_Obj.Name="google" ; //一定添加Name这个属性，因为WriteDynamicObject这个方法内部会用到
dyn_Obj.Val = "val";
OrmDB.WriteDynamicObject(dyn_Obj) ;

//读
//dyn_Obj 一定要先初始化，这样会到数据库中保存的json 取出 
System.Dynamic.ExpandoObject dyn_Obj = new System.Dynamic.ExpandoObject() ;
dyn_Obj.Name="google" ; //一定添加Name这个属性，因为FetchDynamicObject这个方法内部会用到
OrmDB.FetchDynamicObject(dyn_Obj ) ;

```

### 9.如何使用原生ADO.NET
```C#
  //获得DbCommand
  DbCommand cmd =  OrmDB.AdonetContext.ThisDbPipeInfo.AvailableCommand ;
  //获得DbConnection
  DbConnection cnn = cmd.Connection ;
  //执行任务
  cmd.CommandText = "delete from tbl" ;
  cmd.ExecuteNonQuery() ;
  
 
```



