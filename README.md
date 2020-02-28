# OrmDBPlatform (文档持续更新)

跨平台极轻.net Orm框架（联系：shikii@outlook.com 或者在Issues提问）

## 支持特性
### 1.net framework 最小支持到4.0(.net 2.0功能不完整,请勿使用)， .net core/standard 支持到2.0 

### 2 支持桌面/移动（Xamarin）开发
### 3.支持关系数据库有 SQL Server/LocalDB/SQLCE Firebird MySQL Postgresql Sqlite

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
          //这种方式针对于没有并发的，简单同步的场合
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

### 4.删除
 ```c#
 //具体请参考方式上的注释
            OrmDB.Delete() ;
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
      //具体请参考方式上的注释
      //不明白可以给我留言
      //返回实体集
      OrmDB.Where() ;
      //返回为DataTable
      OrmDB.InternalExecuteNonQuery();
 ```

### 7.如何使用原生ADO.NET
```C#
 OrmDB.AdonetContext.ExecuteNonQuery(sql,这个参数可以不给);
 //查询
DataTable dt = OrmDB.AdonetContext.ProvideTable(  sql,这个参数可以不给);
//查询单个值 
String str = UniqueResult(string sql, DbCommand cmd = null)
//更多请参考
DBPlatform.cs 源码

```
