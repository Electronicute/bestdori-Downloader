/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-16
* 作用描述：	#
***************************************************/

using System.Text;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Live2DCharacter.JSON;
using LitJson;

namespace Live2DCharacter
{
	public class Live2dCtl
	{
		#region ----字段----
		Live2dView view;
		public const string MocSuffix = ".moc";
		public const string AniSuffix = ".json";
		public const string PngSuffix = ".png";
		private string currLive;
		#endregion

		#region ----构造方法----
		public Live2dCtl()
        {
			GameObject go = GameObject.FindWithTag("Live2dView");
			view = go.GetComponent<Live2dView>();
			view.returnBtn.onClick.AddListener(OnClickReturn);
        }
		#endregion

		#region ----公有方法----
		public bool IsLive2dFolder(string folder, out string[] fileNames)
        {
			fileNames = new string[3];
			
			if (Directory.Exists(folder))
            {
				HashSet<string> suffixs = new HashSet<string>() { ".moc", ".json", ".png" };
				string ext;
                foreach (var fn in Directory.GetFiles(folder))
                {
					ext = Path.GetExtension(fn);
                    if (suffixs.Contains(ext))
					{
						fileNames[GetSuffixIndex(ext)] = fn;
						suffixs.Remove(ext);
						if (suffixs.Count == 0)
						{
							//Debug.Log($"{folder} IsLive2dFolder : true");
							return true;
						}
					}
				}
			}
			//Debug.Log($"{folder} IsLive2dFolder : false");
			return false;
        }

		public void ShowLive2dByFolder(string folder)
        {
            if (folder == currLive)
            {
				view.enabled = true;
				view.ShowReturnBtn(true);
				return;
            }
			string[] paths;
            if (IsLive2dFolder(folder, out paths))
            {
				currLive = folder;
				ShowLive2dByPaths(paths, folder);
            }
        }

		public void CheckToShowLive()
        {
			view.ShowReturnBtn(false);
        }

		public void ShowLive2dByPaths(string[] paths, string folder)
        {
			string[] fds = Directory.GetFiles(folder);
			string fileName;
			int[] indexs = new int[3];
            for (int i = 0; i < paths.Length; i++)
            {
				fileName = DownloadCtl.GetFileNameByPath(paths[i]);
                for(int j = 0; j < fds.Length; j++)
                {
					fds[j] = DownloadCtl.GetFileNameByPath(fds[j]);
					if (fds[j] == fileName)
                    {
						indexs[i] = j;
						break;
                    }
                }
            }
			view.SetFiles(fds, indexs[0], indexs[1], indexs[2]);
			LocalAssetLoader.Instance.LoadLive2d(paths, ShowLive2d);
		}
		#endregion

		#region ----私有方法----
		private string GetFile(string folder, string suffix)
        {
            if (Directory.Exists(folder))
            {
				foreach (var fn in Directory.GetFiles(folder))
				{
					if (Path.GetExtension(fn) == suffix)
					{
						return fn;
					}
				}
			}

			return null;
        }

		private int GetSuffixIndex(string suffix)
        {
            switch (suffix)
            {
				case PngSuffix:
					return 2;
				case AniSuffix:
					return 1;
				case MocSuffix:
					return 0;
            }

			return -1;
        }

		private void ShowLive2d(byte[] moc, byte[] motion, Texture2D texture)
        {
			view.InitModel(moc, motion, new Texture2D[]{ texture});
			view.enabled = true;
			view.ShowReturnBtn(true);
        }

		private void OnClickReturn()
        {
			WindowAnimation.Instance.HideLiveWindow(() => view.enabled = false);
			WindowAnimation.Instance.ShowDownloadWindow(null);
        }
		#endregion

	}
}