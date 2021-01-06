/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2021-01-02
* 作用描述：	#
***************************************************/

using System;
using System.Text;
using System.IO;
using Live2DCharacter;

namespace Utils
{
	public class LoadUrlHelper
	{
		public static string GetJsonUrl(string nodePath)
        {
            return DownloadCtl.InfoUrl + nodePath;
        }

		public static string GetAssetUrl(string nodePath)
        {
            return DownloadCtl.AssetUrl + nodePath;
        }

		public static string GetLocalPath(string nodePath)
        {
            return DownloadCtl.localPath + '/' + nodePath;
        }

		public static string StandardLocalPath(string path)
        {
			return path.Replace("/", "\\");
        }

		public static string Url2LocalPath(string assetUrl)
        {
			return GetLocalPath(assetUrl.Substring(DownloadCtl.AssetUrl.Length));
        }

        public static string AssetUrl2NodePath(string assetUrl)
        {
            return assetUrl.Substring(DownloadCtl.AssetUrl.Length).Replace(DownloadCtl.ResSuffix, "");
        }

        public static string JsonUrl2NodePath(string jsonUrl)
        {
            string temp = jsonUrl.Substring(DownloadCtl.InfoUrl.Length);
            return temp.Substring(0, temp.Length - 5);
        }

		public static void CreateDirIfNotFound(string url, bool isFolder = false)
        {
            string[] path = url.Split('/');
            int lastIndex = path.Length - 1;
            if (!isFolder)
            {
                lastIndex--;
            }
            StringBuilder sb = new StringBuilder(DownloadCtl.localPath);
            string curPath;
            for (int i = 0; i <= lastIndex; i++)
            {
                sb.Append("/" + path[i]);
                curPath = sb.ToString();
                if (!Directory.Exists(curPath))
                {
                    Directory.CreateDirectory(curPath);
                }
            }
        }
    }
}