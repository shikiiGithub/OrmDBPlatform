using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace dotNetLab.Common
{
    public class FileSystem
    {
        public static void CopyDirectory(string srcPath, string destPath)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(srcPath);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //获取目录下（不包含子目录）的文件和子目录
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)     //判断是否文件夹
                    {
                        if (!Directory.Exists(destPath + "\\" + i.Name))
                        {
                            Directory.CreateDirectory(destPath + "\\" + i.Name);   //目标目录下不存在此文件夹即创建子文件夹
                        }
                        CopyDirectory(i.FullName, destPath + "\\" + i.Name);    //递归调用复制子文件夹
                    }
                    else
                    {
                        File.Copy(i.FullName, destPath + "\\" + i.Name, true);      //不是文件夹即复制文件，true表示可以覆盖同名文件
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public static string ComputeFileSHA1(string FileName)
        {
            try
            {
                byte[] hr;
                using (SHA1Managed Hash = new SHA1Managed()) // 创建Hash算法对象
                {
                    using (FileStream fs = new FileStream(FileName, FileMode.Open))
                    // 创建文件流对象
                    {
                        hr = Hash.ComputeHash(fs); // 计算
                    }
                }
                return BitConverter.ToString(hr).Replace("-", ""); // 转化为十六进制字符串 
            }
            catch (IOException)
            {
                return "Error:访问文件时出现异常";
            }
        }
        public static void RenameFile(String strSrc, string strDst)
        {
            File.Move(strSrc, strDst);
        }
        public static void BatchRenameFileByRemovePrefix(string FolderPath, string PrefixFileName, string extName = "*.*")
        {
            String[] FileNames = Directory.GetFiles(FolderPath, extName);

            foreach (var item in FileNames)
            {
                RenameFile(item, item.Replace(PrefixFileName, ""));
            }
        }
        public static void BatchRenameFileByUserDefineRule(string FolderPath, Func<String, String> UserDefineRule, string extName = "*.*")
        {
            String[] FileNames = Directory.GetFiles(FolderPath, extName);

            foreach (String item in FileNames)
            {
                RenameFile(item, UserDefineRule(item));
            }
        }

        public static void BatchRenameFileExtension(string FolderPath, string NewextName, string OldextName = "*.*")
        {
            String[] FileNames = Directory.GetFiles(FolderPath, OldextName);

            foreach (var item in FileNames)
            {

                RenameFile(item, Path.GetFileNameWithoutExtension(item) + NewextName);
            }
        }
        public static bool DeleteDir(string dirPath)
        {
            try
            {

                //去除文件夹和子文件的只读属性
                //去除文件夹的只读属性
                System.IO.DirectoryInfo fileInfo = new DirectoryInfo(dirPath);
                fileInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;

                //去除文件的只读属性
                System.IO.File.SetAttributes(dirPath, System.IO.FileAttributes.Normal);

                //判断文件夹是否还存在
                if (Directory.Exists(dirPath))
                {

                    foreach (string f in Directory.GetFileSystemEntries(dirPath))
                    {

                        if (File.Exists(f))
                        {
                            //如果有子文件删除文件
                            File.Delete(f);
                            Console.WriteLine(f);
                        }
                        else
                        {
                            //循环递归删除子文件夹
                            DeleteDir(f);
                        }

                    }

                    //删除空文件夹

                    Directory.Delete(dirPath);

                }
                return true;

            }
            catch (Exception ex) // 异常处理
            {
                return false;
            }

        }

    }

   
}
