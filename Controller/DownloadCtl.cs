/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-18
* 作用描述：	#
***************************************************/

using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using Live2DCharacter.JSON;
using System.Linq;
using System.IO;

namespace Live2DCharacter
{
	public class DownloadCtl
	{
		#region ----字段----
		private AssetDownloader downloader;
		private DownloadView view;
		private SubDirView dirView;
		private ProgressView pView;
		private Live2dCtl l2dCtl;

		public const string InfoUrl = @"https://bestdori.com/api/explorer/jp/assets/";
		public const string AssetUrl = @"https://bestdori.com/assets/jp/";
		public const string ResSuffix = "_rip";
		public static string localPath = null;
		private const string localPathKey = "Live2dDownloadPath";
		public const int ShowItemCount = 100;

		private const string infoName = "_info";
		private const string suffix = ".json";

		private GenericTree<NodeData> jsonTree;
		private TreeNode<NodeData> currNode;
		private int currPage = 1;
		private int maxPage = 1;
		#endregion

		#region ----构造方法----
		public DownloadCtl()
        {
			downloader = AssetDownloader.Instance;
			l2dCtl = new Live2dCtl();
			GameObject go = GameObject.Instantiate(Resources.Load("Panel/DownloadView")) as GameObject;
			go.transform.SetParent(GameObject.FindGameObjectWithTag("DownloadPanel").transform, false);
			view = go.GetComponent<DownloadView>();
			downloader.RegisterDebug(Debug);

			view.pathBtn.onClick.AddListener(OnClickPathBtn);
			view.dlBtn.onClick.AddListener(OnClickDlBtn);
			view.rtBtn.onClick.AddListener(OnClickRtBtn);
			view.changePathBtn.onClick.AddListener(OnClickChangePath);

			go = GameObject.Instantiate(Resources.Load("Panel/SubDirView")) as GameObject;
			go.transform.SetParent(GameObject.FindGameObjectWithTag("DownloadPanel").transform, false);
			dirView = go.GetComponent<SubDirView>();

			dirView.InitDirItme(ShowItemCount);
			dirView.showLiveBtn.onClick.AddListener(OnClickShowLive);
			dirView.nextBtn.onClick.AddListener(OnClickNextPage);
			dirView.preBtn.onClick.AddListener(OnClickPrePage);
			dirView.RegisterSelect(OnSelected);
			dirView.RegisterShowFinish(OnShowFinish);
			dirView.Show(false);
			dirView.ShowLiveBtn(false);

			pView = GameObject.FindGameObjectWithTag("ProgressView").GetComponent<ProgressView>();
			pView.Register(downloader.Progress);
			pView.Show(false);

			view.ShowDlBtn(false);
			view.ShowReturnBtn(false);

			Debug(@"/*****************************", ColorView.Green);
			Debug(@"* <color=#FF7EF8>制作： 咕咕咕</color>", ColorView.Green);
			Debug(@"* <color=#FF7EF8>时间： 2020-12-20</color>", ColorView.Green);
			Debug(@"******************************/", ColorView.Green);

			localPath = PlayerPrefs.GetString(localPathKey, @"D:\Download");
            if (!string.IsNullOrWhiteSpace(localPath))
            {
				view.SetDlPath(localPath);
			}
			WindowAnimation.Instance.ShowDownloadWindow(RequestInfoJson);
		}
		#endregion

		#region ----公有方法----
		public void RequestInfoJson()
		{
			TextAsset text = Resources.Load<TextAsset>("Json/" + infoName);
			ParseInfoJson(text.text);
			//GetJson(infoName + suffix, ParseInfoJson);
		}

		public void LoadTree(TreeNode<NodeData> node)
		{
			List<TreeNode<NodeData>> leafs = GenericTree<NodeData>.GetLeafs(node);
			GenericTree<NodeData>.Traverse(node, (nd) => nd.Data.IsDownloaded = true);
			float[] currProgress = downloader.Progress();
            if (currProgress != null && currProgress[0] >= 1)
            {
				downloader.ClearCompletedCount();
            }
			int count = 0;
			foreach (var l in leafs)
			{
                if (!l.Data.IsLeaf)
                {
					count++;
					//Debug(l.ToString(), ColorView.Red);
					GetJson(GetPathWithoutRoot(l) + suffix, ParseLoadJson);
				}
			}
            if (count > 0)
            {
				pView.Show(true);
            }
			Debug($"新增{count}个请求");
		}

		public static string GetFileNameByPath(string path)
		{
			string temp = path.Replace('/', '\\');

			return temp.Substring(temp.LastIndexOf('\\') + 1);
		}

		#region ----与Downloader交互----
		public void LoadAssets(string[] urls, Action<bool> onCompleted)
        {
			downloader.RequestAssets(urls, onCompleted);
        }

		public void GetJson(string url, Action<string, string> onCompleted)
        {
			downloader.RequestJson(url, onCompleted);
        }
        #endregion
        #endregion

        #region ----私有方法----
        #region ----树节点的操作----
        private void ParseInfoJson(string json)
		{
			if (json != null)
			{
				JsonData jsonData = JsonParser.GetData(json);
				currNode = new TreeNode<NodeData>(new NodeData(infoName));
				jsonTree = new GenericTree<NodeData>(currNode);
				TraverseCreate(jsonData, currNode);
				view.ShowJsonTree(true);
				ShowNodeDirItems();
			}
		}

		private void ParseLoadJson(string url, string json)
		{
			if (json != null)
			{
				string path = url.Replace(InfoUrl, "");
				path = path.Substring(0, path.Length - 5);
				TreeNode<NodeData> parent = FindNode(path, jsonTree.Head);
				JsonData datas = JsonParser.GetData(json);
				TraverseCreate(datas, parent, true, true);

				List<TreeNode<NodeData>> childs = parent.Childs;
				
				if (childs != null && childs.Count > 0)
				{
					string[] urls = new string[childs.Count];
					for (int i = 0; i < childs.Count; i++)
					{
						urls[i] = GetPathWithoutRoot(parent) + ResSuffix + '/' + childs[i].Data.Name;
					}
					LoadAssets(urls, ShowNodeDirItems);
				}
			}
		}

		/// <summary>
		/// 遍历JsonData创建子树
		/// </summary>
		private void TraverseCreate(JsonData data, TreeNode<NodeData> parent, bool isLeaf = false, bool isDownloaded = false)
		{
			if (data != null)
			{
				JsonData d;
				TreeNode<NodeData> child;
				if (data.IsObject)
				{
					foreach (string n in data.Keys)
					{
						child = new TreeNode<NodeData>(new NodeData(n, isLeaf, isDownloaded), parent);
						parent.AddChild(child);
						d = data[n];
						if (d.IsObject || d.IsArray)
						{
							TraverseCreate(d, child, isLeaf, isDownloaded);
						}
					}
					return;
				}
				if (data.IsArray)
				{
					for (int i = 0; i < data.Count; i++)
					{
						child = new TreeNode<NodeData>(new NodeData(data[i].ToString(), isLeaf, isDownloaded), parent);
						parent.AddChild(child);
						d = data[i];
						if (d.IsObject || d.IsArray)
						{
							TraverseCreate(d, child, isLeaf, isDownloaded);
						}
					}
				}
			}
		}

		/// <summary>
		/// 获取字符串路径
		/// </summary>
		private string GetPath(TreeNode<NodeData> node)
		{
			List<TreeNode<NodeData>> path = node.GetPath();
			StringBuilder sb = new StringBuilder();
			for (int i = path.Count - 1; i > 0; i--)
			{
				sb.Append(path[i] + "/");
			}
			sb.Append(path[0]);

			return sb.ToString();
		}

		private string GetPathWithoutRoot(TreeNode<NodeData> node)
        {
			List<TreeNode<NodeData>> path = node.GetPath();
			StringBuilder sb = new StringBuilder();
			for (int i = path.Count - 1; i > 0; i--)
			{
				sb.Append(path[i] + "/");
			}
			sb.Append(path[0]);

			return sb.ToString().Replace(infoName + "/", "");
		}

		/// <summary>
		/// 获取路径最后的节点
		/// </summary>
		private TreeNode<NodeData> FindNode(string path, TreeNode<NodeData> node)
		{
			string[] ps = path.Split('/');
			foreach (string str in ps)
			{
				node = node.GetChild(new NodeData(str));
				if (node == null)
				{
					return null;
				}
			}

			return node;
		}

		private void CheckCurrNodePath()
        {
			if (!currNode.Data.IsDownloaded && currNode.Childs == null)
			{
				string path = GetLocalResPath(currNode);
				if (Directory.Exists(path) && Directory.GetFiles(path).Length > 0)
				{
					currNode.Data.IsDownloaded = true;
					string[] fnames = Directory.GetFiles(path);
					string fname;
					TreeNode<NodeData> nd;
					for (int i = 0; i < fnames.Length; i++)
					{
						fname = GetFileNameByPath(fnames[i]);
						nd = new TreeNode<NodeData>(new NodeData(fname, true, true), currNode);
						currNode.AddChild(nd);
					}
				}
			}
		}

		private string GetLocalResPath(TreeNode<NodeData> node)
        {
			return localPath + "/" + GetPathWithoutRoot(node) + ResSuffix;
        }
        #endregion

        #region ----与DownloadView交互----
        private void Debug(string msg, ColorView color = ColorView.Default)
        {
			view.ShowDebug(msg, color);
        }

		private void OnClickPathBtn()
		{
			System.Diagnostics.Process.Start("explorer.exe", localPath.Replace("/", "\\"));
		}

		private void OnClickDlBtn()
		{
            if (string.IsNullOrWhiteSpace(localPath))
            {
				OnClickChangePath();
				return;
            }
			LoadTree(currNode);
		}

		private void OnClickRtBtn()
		{
            if (currNode.Parent != null)
            {
				currNode = currNode.Parent;
				ShowNodeDirItems();
			}
		}

		private void OnClickChangePath()
        {
			string path = WindowsExplorer.GetPathFromWindowsExplorer();
            if (!string.IsNullOrWhiteSpace(path))
            {
				localPath = path;
				PlayerPrefs.SetString(localPathKey, localPath);
				view.SetDlPath(localPath);
			}
        }
		#endregion

		#region ----与SubDirView交互----
		private void ShowNodeDirItems(bool ani = true)
		{
			dirView.Show(true);
			view.dlBtn.interactable = false;
			view.rtBtn.interactable = false;
			CheckCurrNodePath();
			if (currNode.Childs != null)
			{
				List<string> names;
				int startIndex = 0;
				int lastIndex = 0;
				if (currNode.Childs.Count > ShowItemCount)
                {
					maxPage = Mathf.CeilToInt(currNode.Childs.Count / (float)ShowItemCount);
					startIndex = ShowItemCount * (currPage - 1);
                    if (currPage == maxPage)
                    {
						lastIndex = currNode.Childs.Count;
						names = new List<string>(lastIndex - startIndex);
                    }
                    else
                    {
						lastIndex = startIndex + ShowItemCount;
						names = new List<string>(ShowItemCount);
					}
				}
                else
                {
					lastIndex = currNode.Childs.Count;
					names = new List<string>(currNode.Childs.Count);
					currPage = 1;
					maxPage = 1;
				}
				//Math.Min(100, currNode.Childs.Count);
				for (int i = startIndex; i < lastIndex; i++)
				{
					names.Add(currNode.Childs[i].Data.Name);
				}
				dirView.ShowDirItems(names, currPage, maxPage, currNode.Childs[0].Data.IsLeaf, ani);
			}
			else
			{
				currPage = 0;
				maxPage = 0;
				dirView.ShowDirItems(null, currPage, maxPage, true, false);
			}
			dirView.SetDirName(currNode.Data.Name);
			view.ShowReturnBtn(currNode.Parent != null);
            
			view.ShowDlBtn(currNode.Data.IsDownloaded == false && currNode.Parent != null);
			view.SetTreeNode(GetPath(currNode));
			dirView.ShowLiveBtn(IsLiveFolder());
		}

		private bool IsLiveFolder()
        {
			var childs = currNode.Childs;
            if (currNode.Data.IsDownloaded)
            {
				if (l2dCtl.IsLive2dFolder(GetLocalResPath(currNode), out var fs))
				{
					return true;
                }
            }
			return false;
        }

		private void OnSelected(int index)
        {
			var childs = currNode.Childs;
            if (childs != null && childs.Count > index)
            {
				TreeNode<NodeData> temp = childs[index];
                if (!temp.Data.IsLeaf)
                {
					currNode = temp;
					ShowNodeDirItems();
					return;
                }
            }
			//Debug($"没有子目录了~", ColorView.Red);
        }

		private void OnShowFinish()
        {
			view.dlBtn.interactable = true;
			view.rtBtn.interactable = true;
        }

		private void OnClickShowLive()
        {
			WindowAnimation.Instance.HideDownloadWindow(null);
			l2dCtl.CheckToShowLive();
			l2dCtl.ShowLive2dByFolder(GetLocalResPath(currNode));
			WindowAnimation.Instance.ShowLiveWindow(null);
        }

		private void OnClickPrePage()
        {
            if (currPage > 1)
            {
				currPage--;
				ShowNodeDirItems(true);
            }
        }

		private void OnClickNextPage()
        {
            if (currPage < maxPage)
            {
				currPage++;
				ShowNodeDirItems(true);
            }
        }
        #endregion
        #endregion

    }
}