using dotNetLab.Data;
using dotNetLab.Data.Orm;
using System;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MyApp
{
    public partial class App : Application
    {
         public static Assembly asm_Sqlite = null ;
        public static OrmDBPlatform DbContext;
        public App()
        {
            InitializeComponent();

             if(asm_Sqlite != null)
            {
                DbContext = new OrmDBPlatform();
                SQLiteDBEngine.assembly_Sqlite_Connection = asm_Sqlite;
                bool b  = DbContext.Connect("你的sqlite 数据库文件路径");
               if(b)
                    Console.WriteLine("sqlite 数据库已经连接");
               else
                    Console.WriteLine("sqlite 数据库未连接");
            }

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
