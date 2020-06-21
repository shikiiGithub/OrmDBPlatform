using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
namespace dotNetLab.Web
{
    public abstract class HttpServerBase : IDisposable
    {
        private readonly HttpListener _listener;                        // HTTP 协议侦听器
        private readonly Thread _listenerThread;                        // 监听线程
        private readonly Thread[] _workers;                             // 工作线程组
        private readonly ManualResetEvent _stop, _ready;                // 通知停止、就绪
        private Queue<HttpListenerContext> _queue;                      // 请求队列
        private event Action<HttpListenerContext> ProcessRequest;       // 请求处理委托
        String Ip  = null;
        public String HeadHttpAddr = null;
        public HttpServerBase(int maxThreads,String ip)
        {
            Ip = ip;
            _workers = new Thread[maxThreads];
            _queue = new Queue<HttpListenerContext>();
            _stop = new ManualResetEvent(false);
            _ready = new ManualResetEvent(false);
            _listener = new HttpListener();
            _listenerThread = new Thread(HandleRequests);
        }

        public void Start(int port)
        {
            // 注册处理函数
            ProcessRequest += ProcessHttpRequest;
            String str = String.Format("http://{0}:{1}/",Ip, port);
            HeadHttpAddr = str;
            Console.WriteLine(str);
            // 启动Http服务
            _listener.Prefixes.Add( str);
            _listener.Start();
            _listenerThread.Start();

            // 启动工作线程
            for (int i = 0; i < _workers.Length; i++)
            {
                _workers[i] = new Thread(Worker);
                _workers[i].Start();
            }
        }

        // 请求处理函数
        protected abstract void ProcessHttpRequest(HttpListenerContext ctx);

        // 释放资源
        public void Dispose()
        {
            Stop();
        }

        // 停止服务
        public void Stop()
        {
            _stop.Set();
            _listenerThread.Join();
            foreach (Thread worker in _workers)
            {
                worker.Join();
            }
            _listener.Stop();
        }

        // 处理请求
        private void HandleRequests()
        {
            while (_listener.IsListening)
            {
                var context = _listener.BeginGetContext(ContextReady, null);
                if (0 == WaitHandle.WaitAny(new[] { _stop, context.AsyncWaitHandle }))
                {
                    return;
                }
            }
        }

        // 请求就绪加入队列
        private void ContextReady(IAsyncResult ar)
        {
            try
            {
                lock (_queue)
                {
                    _queue.Enqueue(_listener.EndGetContext(ar));
                    _ready.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("[HttpServerBase::ContextReady]err:{0}", e.Message));
            }
        }

        // 处理一个任务
        private void Worker()
        {
            WaitHandle[] wait = new[] { _ready, _stop };
            while (0 == WaitHandle.WaitAny(wait))
            {
                HttpListenerContext context;
                lock (_queue)
                {
                    if (_queue.Count > 0)
                        context = _queue.Dequeue();
                    else
                    {
                        _ready.Reset();
                        continue;
                    }
                }

                try
                {
                    ProcessRequest(context);
                }
                catch (Exception e)
                {
                    Console.WriteLine(string.Format("[HttpServerBase::Worker]err:{0}", e.Message));
                }
            }
        }
    }
   [Obsolete("已废弃")]
    public class CoreWebServer : HttpServerBase
    {
        Dictionary<String, MethodInfo> Routers;
        Dictionary<String, Type> RouterInfo;
        Dictionary<String, Object> RouterInfo_Active;
        public CoreWebServer(int maxThreads) : base(maxThreads,"127.0.0.5")
        {
            Routers = new Dictionary<string, MethodInfo>();
            RouterInfo = new Dictionary<string, Type>();
            RouterInfo_Active = new Dictionary<string, object>(); 
        }
        //可以使用* 自动匹配ip
        public CoreWebServer(int maxThreads,String ip) : base(maxThreads, ip)
        {
            Routers = new Dictionary<string, MethodInfo>();
            RouterInfo = new Dictionary<string, Type>();
            RouterInfo_Active = new Dictionary<string, object>();
        }
        

        /// <summary>
        /// 注册路由(如果需要使用到Request/Respones 请将类继承自ApiController)
        /// </summary>
        /// <param name="route">为所调用的C# 方法</param>
        /// <param name="ActiveOnce">是否只初始化一次调用的C#方法所在的类</param>
        public void RegisterWebApiMethod(String route, Type ClassType,bool ActiveOnce)
        {
            MethodInfo mif = ClassType.GetMethod(route);
            if (mif == null)
            {
                throw new Exception("未找到路由方法");

            }
            else
            {
                RouterInfo.Add(route.ToLower(), ClassType);
                Routers.Add(route.ToLower(), mif);
                if(ActiveOnce)
                this.RouterInfo_Active.Add(route.ToLower(), System.Activator.CreateInstance(ClassType));
                else
                    this.RouterInfo_Active.Add(route.ToLower(), null);
            }

        }

        protected override void ProcessHttpRequest(HttpListenerContext context)
        {

            String url =context.Request.Url.ToString();
            Console.WriteLine( url);
            String rawUrl = url.Replace(HeadHttpAddr,"");
            if (rawUrl == "favicon.ico")
                return;
            String[] urlParts =rawUrl.ToLower().Split('?');
            String MethodName = urlParts[0] ;

            Object obj = this.RouterInfo_Active[MethodName];
            if(obj == null)
               obj =System.Activator.CreateInstance(RouterInfo[MethodName]);
            
            if(obj is ApiController)
            {
                ApiController controller = obj as ApiController;
                controller.HttpContext = context;
                controller.Request = context.Request;
                controller.Response = context.Response;
            }

            MethodInfo mif = Routers[MethodName];
            ParameterInfo[] parInfos = mif.GetParameters();

            Object[] objs = new object[parInfos.Length];

            String [] urlParams = urlParts[1].Split('&');

            Dictionary<String, String> dct_UrlPars = new System.Collections.Generic.Dictionary<String, String>();
            for (int i = 0; i < urlParams.Length; i++)
            {
                String [] arr =urlParams[i].Split('=');
                dct_UrlPars.Add(arr[0].Trim(),arr[1].Trim());
            }
            for (int i = 0; i < parInfos.Length; i++)
            {
                string parName = parInfos[i].Name.ToLower();
                string val = dct_UrlPars[parName];
               Type type =  parInfos[i].ParameterType;
                if (type == typeof(String))
                    objs[i] = val;
                else if (type == typeof(int))
                    objs[i] = int.Parse(val);
                else if (type == typeof(float))
                    objs[i] = float.Parse(val);
                else if (type == typeof(double))
                    objs[i] = double.Parse(val);
                else if (type == typeof(decimal))
                    objs[i] = decimal.Parse(val);
                else if (type == typeof(char))
                    objs[i] = char.Parse(val);
                else if (type == typeof(DateTime))
                    objs[i] = DateTime.Parse(val);
                else if (type == typeof(TimeSpan))
                    objs[i] = TimeSpan.Parse(val);
                else if (type == typeof(bool))
                    objs[i] = bool.Parse(val);
                else if (type == typeof(long))
                    objs[i] = long.Parse(val);

            }

            context.Response.StatusCode = 200;
            string response = mif.Invoke(obj,objs).ToString();
            byte[] buffer = Encoding.UTF8.GetBytes(response);
            /* 允许跨域的主机地址 */
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            /* 允许跨域的请求方法GET, POST, HEAD 等 */
            context.Response.Headers.Add("Access-Control-Allow-Methods", "*");
            /* 重新预检验跨域的缓存时间 (s) */
            context.Response.Headers.Add("Access-Control-Max-Age", "3600");
            /* 允许跨域的请求头 */
            context.Response.Headers.Add("Access-Control-Allow-Headers", "*");
            /* 是否携带cookie */
            context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
            context.Response.Close();
        }
    }
    public abstract  class ApiController
    {
      public  HttpListenerContext HttpContext { get; set; }
      public  HttpListenerRequest Request { get; set; }
      public  HttpListenerResponse Response { get; set; }
    }
}
