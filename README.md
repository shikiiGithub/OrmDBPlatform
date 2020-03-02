# OrmDBPlatform (文档持续更新)

跨平台极轻.net Orm框架（联系：shikii@outlook.com 或者在Issues提问）

## 支持特性
### 1.net framework 最小支持到4.0(.net 2.0功能不完整,请勿使用)， .net core/standard 支持到2.0 

### 2 支持桌面/移动（Xamarin）开发
### 3.支持关系数据库有 SQL Server/LocalDB/Firebird/MySQL/Postgresql/SQLCE/Sqlite

## 库的选择
### 对于Xamarin 请选择使用OrmDBPlatform.NETStandard项目
### 传统桌面请使用.net_4.0 /.net core 的项目

## 开始使用
   ### 1.最简单的使用是使用nuget添加你想连接数据库的Data Provider的引用，比如搜索 Mysql 然后安装，安装完成后初始化
```c#
       //这种方式针对于没有并发的，简单同步的场合
       //应用于并发，异步的场合请使用 “OrmDB = new OrmDBPlatform(false); ”
        OrmDB = new OrmDBPlatform();

         //监控错误，将日志同时写入Console/db/文本文件中 
         //下面使用默认方式，默认方式为同步模式
         OrmDB. ErrorInvoker = null;
         Orm.InfoInvoker=null; 


        //shikii 为数据库名 root 为用户名 123为密码
        //默认使用本地的数据库
        //EntitySourceAssemblies 表示实体类所在的Assembly,默认使用当前程序集
        bool isConnected = OrmDB.Connect("shikii",  "root","123");
        
        //远程访问则请使用
         bool isConnected = OrmDB.Connect(  typeof(MySQLDBEngine),  ip,  port,"shikii",  "123",  "root") ;

         //如果使用sqlite/sqlce 
          bool isConnected = OrmDB.Connect( dbPath) ;

        //对于SQL Server /LocalDB
           Assembly asm_SQLSEVER = typeof(System.Data.SqlClient.SqlConnection) ;
         //本地连接localhost
         bool isConnected = OrmDB.Connect(   asm_SQLSEVER,  "shikii", "sa",  "123") ;
         //远程连接
         bool isConnected = OrmDB.Connect(   asm_SQLSEVER, ip,port, "shikii",  "123", "sa") ;
   
```
   ### 2.新建立Entities（可自己命名） 文件夹，在Entities下新建 SampleEntity 类，这将在OrmDBPlatform.Connect 后创建名为Sample的表，请注意必须继续自EntityBase
   ```c#
      //可以使用  [Entity("MANUAL_CREATE_TABLE")] 来手动创建此表 
      //默认自动创建
      //不支持命名自定义表名
      public class SampleEntity : EntityBase
      {
          [DBKey] //设置主键，也可以设置外键
          public String Name{get;set;}  
          //指定字符串长度（不需要显示使用，默认使用[MySqlTextTypeAttribute("varchar(255)")]）
          [MySqlTextTypeAttribute("MEDIUMTEXT")] 
          public String XDesc{get;set;}
        

      }
   ```
### 3.增
 ```c#
          //这种方法针对于没有并发的，简单同步的场合
          //比如在Xamarin 中使用
              SampleEntity se = new SampleEntity() ;
              se.Name="Google" ;
              se.XDesc="Fuck Google" ;
              se.Save() ;
            //在并发/异步的场合：
            SampleEntity se =  OrmDB.GetEntity<SampleEntity>() ;
              se.Name="Google" ;
              se.XDesc="Fuck Google" ;
              se.Save() ;
          //需要注意的是如果需要在高速场合下请使用：
            se.Save(SaveMode.INSERT) ;
 ```

### 4.删
 ```c#
 //具体请参考方法上的注释
            OrmDB.Delete<SampleEntity>(x=>x.Name=="Google") ;
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
      //具体请参考方法上的注释
      //不明白可以给我留言
      //返回实体集
     List<SampleEntity> lst  = OrmDB.Where<SampleEntity>(x=>x.Name=="Google") ;
      //返回为DataTable
      OrmDB.InternalExecuteNonQuery();
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



