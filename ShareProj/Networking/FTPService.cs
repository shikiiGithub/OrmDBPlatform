
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
 
namespace dotNetLab.Networking 
{
   public class FTPService
    {

     
        private static string FTPCONSTR = "";//FTP的服务器地址，格式为ftp://192.168.1.234:8021/。ip地址和端口换成自己的，这些建议写在配置文件中，方便修改
        private static string FTPUSERNAME = "";//FTP服务器的用户名
        private static string FTPPASSWORD = "";//FTP服务器的密码

        #region 本地文件上传到FTP服务器
        /// <summary>
        /// 上传文件到远程ftp
        /// </summary>
        /// <param name="path">本地的文件目录</param>
        /// <param name="name">文件名称</param>
        /// <returns></returns>
        public static bool UploadFile(string path, string name)
        {
            string erroinfo = "";
            FileInfo f = new FileInfo(path);
            path = path.Replace("\\", "/");
            path = FTPCONSTR + "/data/uploadFile/photo/" + name;//这个路径是我要传到ftp目录下的这个目录下
            FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(path));
            reqFtp.UseBinary = true;
            reqFtp.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
            reqFtp.KeepAlive = false;
            reqFtp.Method = WebRequestMethods.Ftp.UploadFile;
            reqFtp.ContentLength = f.Length;
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;
            FileStream fs = f.OpenRead();
            try
            {
                Stream strm = reqFtp.GetRequestStream();
                contentLen = fs.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }
                strm.Close();
                fs.Close();
                erroinfo = "完成";
                return true;
            }
            catch (Exception ex)
            {
                erroinfo = string.Format("因{0},无法完成上传", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 上传文件到远程ftp
        /// </summary>
        /// <param name="ftpPath">ftp上的文件路径</param>
        /// <param name="path">本地的文件目录</param>
        /// <param name="id">文件名</param>
        /// <returns></returns>
        public static bool UploadFile(string ftpPath, string path, string id)
        {
            string erroinfo = "";
            FileInfo f = new FileInfo(path);
            path = path.Replace("\\", "/");
            bool b = MakeDir(ftpPath);
            if (b == false)
            {
                return false;
            }
            path = FTPCONSTR + ftpPath + id;
            FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(path));
            reqFtp.UseBinary = true;
            reqFtp.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
            reqFtp.KeepAlive = false;
            reqFtp.Method = WebRequestMethods.Ftp.UploadFile;
            reqFtp.ContentLength = f.Length;
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;
            FileStream fs = f.OpenRead();
            try
            {
                Stream strm = reqFtp.GetRequestStream();
                contentLen = fs.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }
                strm.Close();
                fs.Close();
                erroinfo = "完成";
                return true;
            }
            catch (Exception ex)
            {
                erroinfo = string.Format("因{0},无法完成上传", ex.Message);
                return false;
            }
        }
        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="path">本地的文件目录</param>
        /// <param name="name">文件名称</param>
        /// <param name="pb">进度条</param>
        /// <returns></returns>

        ////上面的代码实现了从ftp服务器下载文件的功能
        public static Stream Download(string ftpfilepath)
        {
            Stream ftpStream = null;
            FtpWebResponse response = null;
            try
            {
                ftpfilepath = ftpfilepath.Replace("\\", "/");
                string url = FTPCONSTR + ftpfilepath;
                FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                reqFtp.UseBinary = true;
                reqFtp.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
                response = (FtpWebResponse)reqFtp.GetResponse();
                ftpStream = response.GetResponseStream();
            }
            catch (Exception ee)
            {
                if (response != null)
                {
                    response.Close();
                }
                //MessageBox.Show("文件读取出错，请确认FTP服务器服务开启并存在该文件");
            }
            return ftpStream;
        }
        #endregion

        #region 从ftp服务器下载文件

        /// <summary>
        /// 从ftp服务器下载文件的功能
        /// </summary>
        /// <param name="ftpfilepath">ftp下载的地址</param>
        /// <param name="filePath">存放到本地的路径</param>
        /// <param name="fileName">保存的文件名称</param>
        /// <returns></returns>
        public static bool Download(string ftpfilepath, string filePath, string fileName)
        {
            try
            {
                filePath = filePath.Replace("我的电脑\\", "");
                String onlyFileName = Path.GetFileName(fileName);
                string newFileName = filePath + onlyFileName;
                if (File.Exists(newFileName))
                {
                    //errorinfo = string.Format("本地文件{0}已存在,无法下载", newFileName);                   
                    File.Delete(newFileName);
                    //return false;
                }
                ftpfilepath = ftpfilepath.Replace("\\", "/");
                string url = FTPCONSTR + ftpfilepath;
                FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                reqFtp.UseBinary = true;
                reqFtp.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
                FtpWebResponse response = (FtpWebResponse)reqFtp.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                long cl = response.ContentLength;
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];
                readCount = ftpStream.Read(buffer, 0, bufferSize);
                FileStream outputStream = new FileStream(newFileName, FileMode.Create);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }
                ftpStream.Close();
                outputStream.Close();
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                //errorinfo = string.Format("因{0},无法下载", ex.Message);
                return false;
            }
        }
        //
       
        #endregion

        #region 获得文件的大小
        /// <summary>
        /// 获得文件大小
        /// </summary>
        /// <param name="url">FTP文件的完全路径</param>
        /// <returns></returns>
        public static long GetFileSize(string url)
        {

            long fileSize = 0;
            try
            {
                FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                reqFtp.UseBinary = true;
                reqFtp.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
                reqFtp.Method = WebRequestMethods.Ftp.GetFileSize;
                FtpWebResponse response = (FtpWebResponse)reqFtp.GetResponse();
                fileSize = response.ContentLength;

                response.Close();
            }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.Message);
            }
            return fileSize;
        }
        #endregion

        #region 在ftp服务器上创建文件目录

        /// <summary>
        ///在ftp服务器上创建文件目录
        /// </summary>
        /// <param name="dirName">文件目录</param>
        /// <returns></returns>
        public static bool MakeDir(string dirName)
        {
            try
            {
                bool b = RemoteFtpDirExists(dirName);
                if (b)
                {
                    return true;
                }
                string url = FTPCONSTR + dirName;
                FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                reqFtp.UseBinary = true;
                // reqFtp.KeepAlive = false;
                reqFtp.Method = WebRequestMethods.Ftp.MakeDirectory;
                reqFtp.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
                FtpWebResponse response = (FtpWebResponse)reqFtp.GetResponse();
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                //errorinfo = string.Format("因{0},无法下载", ex.Message);
                return false;
            }

        }
        /// <summary>
        /// 判断ftp上的文件目录是否存在
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool RemoteFtpDirExists(string path)
        {

            path = FTPCONSTR + path;
            FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(path));
            reqFtp.UseBinary = true;
            reqFtp.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
            reqFtp.Method = WebRequestMethods.Ftp.ListDirectory;
            FtpWebResponse resFtp = null;
            try
            {
                resFtp = (FtpWebResponse)reqFtp.GetResponse();
                FtpStatusCode code = resFtp.StatusCode;//OpeningData
                resFtp.Close();
                return true;
            }
            catch
            {
                if (resFtp != null)
                {
                    resFtp.Close();
                }
                return false;
            }
        }
        #endregion

        #region 从ftp服务器删除文件的功能
        /// <summary>
        /// 从ftp服务器删除文件的功能
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool DeleteFile(string fileName)
        {
            try
            {
                string url = FTPCONSTR + fileName;
                FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                reqFtp.UseBinary = true;
                reqFtp.KeepAlive = false;
                reqFtp.Method = WebRequestMethods.Ftp.DeleteFile;
                reqFtp.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
                FtpWebResponse response = (FtpWebResponse)reqFtp.GetResponse();
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                 
                return false;
            }
        }
        #endregion
    }
}

/*
 * 
 *  /// <summary>
        /// 从ftp服务器下载文件的功能----带进度条
        /// </summary>
        /// <param name="ftpfilepath">ftp下载的地址</param>
        /// <param name="filePath">保存本地的地址</param>
        /// <param name="fileName">保存的名字</param>
        /// <param name="pb">进度条引用</param>
        /// <returns></returns>
        public static bool Download(string ftpfilepath, string filePath, string fileName, ProgressBar pb)
        {
            FtpWebRequest reqFtp = null;
            FtpWebResponse response = null;
            Stream ftpStream = null;
            FileStream outputStream = null;
            try
            {
                filePath = filePath.Replace("我的电脑\\", "");
                String onlyFileName = Path.GetFileName(fileName);
                string newFileName = filePath + onlyFileName;
                if (File.Exists(newFileName))
                {
                    try
                    {
                        File.Delete(newFileName);
                    }
                    catch { }

                }
                ftpfilepath = ftpfilepath.Replace("\\", "/");
                string url = FTPCONSTR + ftpfilepath;
                reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                reqFtp.UseBinary = true;
                reqFtp.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
                response = (FtpWebResponse)reqFtp.GetResponse();
                ftpStream = response.GetResponseStream();
                long cl = GetFileSize(url);
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];
                readCount = ftpStream.Read(buffer, 0, bufferSize);
                outputStream = new FileStream(newFileName, FileMode.Create);

                float percent = 0;
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                    percent = (float)outputStream.Length / (float)cl * 100;
                    if (percent <= 100)
                    {
                        if (pb != null)
                        {
                            pb.Invoke(new updateui(upui), new object[] { cl, (int)percent, pb });
                        }
                    }
                    // pb.Invoke(new updateui(upui), new object[] { cl, outputStream.Length, pb });

                }

                //MessageBoxEx.Show("Download0");
                return true;
            }
            catch (Exception ex)
            {
                //errorinfo = string.Format("因{0},无法下载", ex.Message);
                //MessageBoxEx.Show("Download00");
                return false;
            }
            finally
            {
                //MessageBoxEx.Show("Download2");
                if (reqFtp != null)
                {
                    reqFtp.Abort();
                }
                if (response != null)
                {
                    response.Close();
                }
                if (ftpStream != null)
                {
                    ftpStream.Close();
                }
                if (outputStream != null)
                {
                    outputStream.Close();
                }
            }
        }
   public static bool UploadFile(string path, string name, ProgressBar pb)
   {
            string erroinfo = "";
            float percent = 0;
            FileInfo f = new FileInfo(path);
            path = path.Replace("\\", "/");
            path = FTPCONSTR + "/data/uploadFile/photo/" + name;
            FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(path));
            reqFtp.UseBinary = true;
            reqFtp.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
            reqFtp.KeepAlive = false;
            reqFtp.Method = WebRequestMethods.Ftp.UploadFile;
            reqFtp.ContentLength = f.Length;
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;
            FileStream fs = f.OpenRead();
            int allbye = (int)f.Length;
            if (pb != null)
            {
                pb.Maximum = (int)allbye;

            }
            int startbye = 0;
            try
            {
                Stream strm = reqFtp.GetRequestStream();
                contentLen = fs.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    startbye = contentLen + startbye;
                    if (pb != null)
                    {
                        pb.Value = (int)startbye;
                    }
                    contentLen = fs.Read(buff, 0, buffLength);
                    percent = (float)startbye / (float)allbye * 100;
                }
                strm.Close();
                fs.Close();
                erroinfo = "完成";
                return true;
            }
            catch (Exception ex)
            {
                erroinfo = string.Format("因{0},无法完成上传", ex.Message);
                return false;
            }
        }
        /// <summary>
        /// 文件上传到ftp
        /// </summary>
        /// <param name="ftpPath">ftp的文件路径</param>
        /// <param name="path">本地的文件目录</param>
        /// <param name="name">文件名称</param>
        /// <param name="pb">进度条</param>
        /// <returns></returns>
        public static bool UploadFile(string ftpPath, string path, string name, ProgressBar pb)
        {
            //path = "ftp://" + UserUtil.serverip + path;
            string erroinfo = "";
            float percent = 0;
            FileInfo f = new FileInfo(path);
            path = path.Replace("\\", "/");
            bool b = MakeDir(ftpPath);
            if (b == false)
            {
                return false;
            }
            path = FTPCONSTR + ftpPath + name;
            FtpWebRequest reqFtp = (FtpWebRequest)FtpWebRequest.Create(new Uri(path));
            reqFtp.UseBinary = true;
            reqFtp.Credentials = new NetworkCredential(FTPUSERNAME, FTPPASSWORD);
            reqFtp.KeepAlive = false;
            reqFtp.Method = WebRequestMethods.Ftp.UploadFile;
            reqFtp.ContentLength = f.Length;
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;
            FileStream fs = f.OpenRead();
            int allbye = (int)f.Length;
            //if (pb != null)
            //{
            //    pb.Maximum = (int)allbye;

            //}
            int startbye = 0;
            try
            {
                Stream strm = reqFtp.GetRequestStream();
                contentLen = fs.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    startbye = contentLen + startbye;
                    percent = (float)startbye / (float)allbye * 100;
                    if (percent <= 100)
                    {
                        int i = (int)percent;
                        if (pb != null)
                        {
                            pb.BeginInvoke(new updateui(upui), new object[] { allbye, i, pb });
                        }
                    }

                    contentLen = fs.Read(buff, 0, buffLength);

                    //  Console.WriteLine(percent);
                }
                strm.Close();
                fs.Close();
                erroinfo = "完成";
                return true;
            }
            catch (Exception ex)
            {
                erroinfo = string.Format("因{0},无法完成上传", ex.Message);
                return false;
            }
        }
        private delegate void updateui(long rowCount, int i, ProgressBar PB);
        public static void upui(long rowCount, int i, ProgressBar PB)
        {
            try
            {
                PB.Value = i;
            }
            catch { }
        }
     
     */
