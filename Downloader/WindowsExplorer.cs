/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-20
* 作用描述：	#
***************************************************/

using System;
using System.Collections;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Live2DCharacter
{
	public class WindowsExplorer
	{
        [DllImport("shell32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SHBrowseForFolder([In, Out] OpenDialogDir ofn);
        [DllImport("shell32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern bool SHGetPathFromIDList([In] IntPtr pidl, [In, Out] char[] fileName);
        /// <summary>
        /// 调用WindowsExploer 并返回所选文件夹路径
        /// </summary>
        /// <param name="dialogtitle">打开对话框的标题</param>
        /// <returns>所选文件夹路径</returns>
        public static string GetPathFromWindowsExplorer(string dialogtitle = "请选择下载路径")
        {
            string res;
            OpenDialogDir ofn2 = new OpenDialogDir();
            ofn2.pszDisplayName = new string(new char[4096]); ;     // 存放目录路径缓冲区 
            ofn2.lpszTitle = dialogtitle;// 标题 
            ofn2.ulFlags = 0x00000040; // 新的样式,带编辑框 
            IntPtr pidlPtr = WindowsExplorer.SHBrowseForFolder(ofn2);
            char[] charArray = new char[2000];
            for (int i = 0; i < 2000; i++)
                charArray[i] = '\0';
            WindowsExplorer.SHGetPathFromIDList(pidlPtr, charArray);
            res = new String(charArray);
            res = res.Substring(0, res.IndexOf('\0'));

            return res;
        }

    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class OpenDialogDir
    {
        public IntPtr hwndOwner = IntPtr.Zero;
        public IntPtr pidlRoot = IntPtr.Zero;
        public String pszDisplayName = null;
        public String lpszTitle = null;
        public UInt32 ulFlags = 0;
        public IntPtr lpfn = IntPtr.Zero;
        public IntPtr lParam = IntPtr.Zero;
        public int iImage = 0;
    }
}