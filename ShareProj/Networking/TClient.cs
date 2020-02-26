using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace dotNetLab.Networking
{
   public  class TClient : TCPBase
    {


        /// <summary>
        /// 可参考 (msgMark,nDataLen,nDataStartIndex,buf)=>{};
        /// </summary>
        public event Action<int, int,int,byte[]> Route;
        protected Socket Client;
        protected Byte[] bytArr_MainChannel;
        public event EventHandler Disconnected;
        public Byte[] MainBuffer
        {
            get { return bytArr_MainChannel; }
            set { bytArr_MainChannel = value; }
        }
        protected int nRecievedNum = 0;
        

        public bool Connected {get;set;}
        //Client ID Is Client IP
        public bool Connect(String ip,int loopGapTime= 1000,int bufsize=1029,int port=8040)
        {
            try
            {
                BufferSize = (uint)bufsize;
                LoopGapTime = loopGapTime;
                this.IP = ip;
                Port = port;
                MainBuffer = new byte[BufferSize];
                bEndNetwork = false;

                ServerIP = IPAddress.Parse(IP);
                IPEndPoint ClientEndPoint =
                    new IPEndPoint(this.ServerIP, nPort);
                Client = new
             Socket
             (
             AddressFamily.InterNetwork,
             SocketType.Stream, ProtocolType.IP);
                Client.Connect(ClientEndPoint);
                thd_Main = new Thread(Loop);
                thd_Main.Start();
                Connected = true;
               
                return true;
            }
            catch (System.Exception ex)
            {
                Connected = false;
                this.strErrorInfo = ex.ToString();
                return false;
            }

        }

        public bool Reconnect()
        {
            IPEndPoint ClientEndPoint =
            new IPEndPoint(this.ServerIP, nPort);
            Client.Connect(ClientEndPoint);
            thd_Main = new Thread(Loop);
            thd_Main.Start();
            Connected = true;
            return Connected;
        }
        protected override void Loop()
        {
            while (true)
            {
                if (bEndNetwork)
                    return;
                RecieveAndParse();
                Thread.Sleep(nLoopGapTime);
            }
           
        }

        protected virtual void RecieveFormServerMethod()
        {
            int nCount = Config.ConentStartIndex;
            int nRecievedLen = Client.Receive(MainBuffer, 0, nCount, System.Net.Sockets.SocketFlags.None);
            int nLen = Config.FetchDataLen(MainBuffer);
            

            int nTotalLen =  nLen + nCount;
            while (true)
            {
                if (nCount < nTotalLen)
                {
                    nCount += Client.Receive(MainBuffer, nCount, nTotalLen - nCount, System.Net.Sockets.SocketFlags.None);
                }
                else
                    break;
            }
            nRecievedNum = nRecievedLen;
        }
        protected void RecieveAndParse()
        {
            try
            {
                RecieveFormServerMethod();
                if (nRecievedNum == 0)
                    return;
                int byt_MSG_Mark = Config.FetchMSGMark(MainBuffer);
                int nLen = Config.FetchDataLen(MainBuffer);
                int nDataStartIndex = Config.ConentStartIndex;
                if (Route != null)
                    Route(byt_MSG_Mark,nLen,nDataStartIndex, MainBuffer);
                Thread.Sleep(nLoopGapTime);
            }
            catch (Exception e)
            {
                bEndNetwork = true;
                if (Disconnected != null)
                    Disconnected(this, null);
                Connected = false;
                this.strErrorInfo = e.ToString();
            }

        }
        //Close Socket
        protected override bool Close()
        {
            try
            {
                bEndNetwork = true;
                thd_Main.Abort();
                Client.Disconnect(false);
                Client.Shutdown(SocketShutdown.Both);
                Client.Close();

                return true;
            }
            catch (System.Exception ex)
            {
                this.strErrorInfo = ex.ToString();
                return false;
            }


        }
        public void Dispose()
        {
            Close();
            Connected = false;
        }


        public  void Send(int MSG,byte[] byts)
        {
            byte[] byt_SendContent = new byte[byts.Length + Config.ConentStartIndex];
            Config.StoreMSGMark(byt_SendContent, MSG);
            Config.StoreDataLen(byts.Length, byt_SendContent);
           
                Socket sct = Client;
                Config.StoreData(byts, byt_SendContent);
                int n = sct.Send(byt_SendContent);
                if (n == byt_SendContent.Length)
                {
                    return;
                }
                else
                {
                    Console.WriteLine("TCP 发送数据失败");
                }
            
        }
        public   void Send(byte[] byts)
        {
            byte[] byt_SendContent = new byte[byts.Length + Config.ConentStartIndex];
            Config.StoreDataLen(byts.Length, byt_SendContent);
            
                Socket sct = Client;
                Config.StoreData(byts, byt_SendContent);
                int n = sct.Send(byt_SendContent);
                if (n == byt_SendContent.Length)
                {
                    return;
                }
                else
                {
                    Console.WriteLine("TCP 发送数据失败");
                }
            
        }
        ~TClient()
        {
            Close();
        }
    }
}
