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
       
    }

   
}
