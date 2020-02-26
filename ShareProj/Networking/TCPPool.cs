//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace dotNetLab.Networking
//{
//    public class TCPPool
//    {
//       public TCPBase ThisTCPEndPort;

//        public TServer Server
//        {
//            get => ThisTCPEndPort as TServer;
            
//        }

//        public TClient Client
//        {
//            get => ThisTCPEndPort as TClient;
//        }
//        public void InitServer(Action<int> clientConnected,
//             Action<String> clientDisconnected, Action<int, byte[]> routeMessage,
//              int nBufferSize, String ip, int nPort, int loopGapTime, Encoding encoding)
//        {
//            ThisTCPEndPort = new TServer();
//            InitCommon(routeMessage,nBufferSize, ip, nPort, loopGapTime, encoding);
//            ((TServer)ThisTCPEndPort).ClientConnected += clientConnected;
//            ((TServer)ThisTCPEndPort).ClientDisconnected += clientDisconnected;
//            ((TServer)ThisTCPEndPort).Boot();
//        }

//        public void InitClient(Action<int, byte[]> routeMessage,
//              int nBufferSize, String ip, int nPort, int loopGapTime, Encoding encoding)
//        {
//            ThisTCPEndPort = new TClient();
//            InitCommon(routeMessage,nBufferSize, ip, nPort, loopGapTime, encoding);
          
//            ((TClient)ThisTCPEndPort).Connect();
//        }

//        public void InitServer(Action<int> clientConnected,
//            Action<String> clientDisconnected, Action<int, byte[]> routeMessage,
//             int nBufferSize, String ip, int nPort, int loopGapTime )
//        {
//            InitServer(clientConnected, clientDisconnected, routeMessage, nBufferSize, 
//                ip, nPort, loopGapTime, Encoding.UTF8);
//        }

//        public void InitClient(Action<int, byte[]> routeMessage,
//              int nBufferSize, String ip, int nPort, int loopGapTime )
//        {
//            InitClient(routeMessage, nBufferSize,
//               ip, nPort, loopGapTime, Encoding.UTF8);
//        }

//        public void Send(byte [] byts)
//        {
          
//        }

//        //void CallAPI(String MethodName, Func<Object, byte[]> ConvertToByts, params object[] pars)
//        //{
//        //    List<byte[]> ArgsByts = new List<byte[]>();
//        //    List<int> ArgsLens = new List<int>();

//        //    for (int i = 0; i < pars.Length; i++)
//        //    {
//        //        SerializableAttribute attribute = (SerializableAttribute)Attribute.GetCustomAttribute(pars[i].GetType(), typeof(SerializableAttribute));
//        //        if (attribute == null)
//        //        {
//        //            byte[] buf = ConvertToByts(pars[i]);
//        //            ArgsByts.Add(buf);
//        //            ArgsLens.Add(buf.Length);
//        //        }
//        //        else
//        //        {
//        //            byte[] buf = ThisTCPEndPort.ObjectToBytes(pars[i]);
//        //            ArgsLens.Add(buf.Length);
//        //            ArgsByts.Add(buf);
//        //        }
//        //    }


//        //}
//        void InitCommon(Action<int,byte [] > routeMessage, int nBufferSize, String ip, int nPort, int loopGapTime, Encoding encoding)
//        {
//            ThisTCPEndPort.BufferSize = (uint)nBufferSize;
//            ThisTCPEndPort.IP = ip;
//            ThisTCPEndPort.Port = nPort;
//            ThisTCPEndPort.TextEncode = encoding;
//            ThisTCPEndPort.LoopGapTime = loopGapTime;
//            ThisTCPEndPort .Route = routeMessage;
//        }
//    }
//}
