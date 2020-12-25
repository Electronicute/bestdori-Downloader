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
		private AudioController audioCtl;
		private Texture2dView tView;

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

			audioCtl = new AudioController();

			go = GameObject.Instantiate(Resources.Load("Prefabs/Texture2dView")) as GameObject;
			go.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
			tView = go.GetComponent<Texture2dView>();
			tView.closeBtn.onClick.AddListener(OnClickCloseTexture);
			tView.Close();

			Debug("初始化中...", ColorView.Default);

			localPath = PlayerPrefs.GetString(localPathKey, @"D:\Download");
            if (!string.IsNullOrWhiteSpace(localPath))
            {
				view.SetDlPath(localPath);
			}
			RequestInfoJson();
			WindowAnimation.Instance.ShowDownloadWindow(null);
		}
		#endregion

		#region ----与Downloader交互----
		public void RequestInfoJson()
		{
            TextAsset text = Resources.Load<TextAsset>("Json/" + infoName);
            ParseInfoJson(text.text);
            //Debug("请求Json数据中...", ColorView.Default);
			//downloader.RequestJson(null, infoName + suffix, (n, s) => ParseInfoJson(s));
		}

		public void LoadTree(TreeNode<NodeData> node)
		{
			if (node.Data.State == NodeDataState.Unload)
			{
				List<TreeNode<NodeData>> loads = new List<TreeNode<NodeData>>();
				TraverseSelect(node, loads);
				GenericTree<NodeData>.Traverse(node, (nd) => nd.Data.State = NodeDataState.Loading);
				float[] currProgress = downloader.Progress();
				if (currProgress != null && currProgress[0] >= 1)
				{
					downloader.ClearCompletedCount();
				}

				foreach (var l in loads)
				{
					LoadJson(l, ParseLoadJson);
				}
				if (loads.Count > 0)
				{
					pView.Show(true);
					Debug($"新增{loads.Count}个请求");
				}
			}
		}

		public void LoadAssets(string[] urls, TreeNode<NodeData>[] nds, Action<bool> onCompleted)
        {
			downloader.RequestAssets(urls, nds, OnNodeCompleted, onCompleted);
        }

		public void LoadJson(TreeNode<NodeData> node, Action<TreeNode<NodeData>, string> onCompleted)
        {
			string url = GetPathWithoutRoot(node) + suffix;
			downloader.RequestJson(node, url, onCompleted);
        }

		private void OnNodeCompleted(TreeNode<NodeData> node)
        {
			node.Data.State = NodeDataState.Downloaded;
			dirView.UpdateItem(node.Data.Name, node.Data.State);
        }
		#endregion

		#region ----ParseJsonData----
		private void ParseInfoJson(string json)
		{
			if (json != null)
			{
				Debug("/*****************************", ColorView.Green);
				Debug("* <color=#FF7EF8>制作： 咕咕咕</color>", ColorView.Green);
				Debug("* <color=#FF7EF8>时间： 2020-12-20</color>", ColorView.Green);
				Debug("******************************/", ColorView.Green);
				
				JsonData jsonData = JsonParser.GetData(json);
				currNode = new TreeNode<NodeData>(new NodeData(infoName));
				jsonTree = new GenericTree<NodeData>(currNode);
				TraverseCreate(jsonData, currNode);
				Check(currNode);
				view.ShowJsonTree(true);
				ShowNodeDirItems();

				Debug("初始化完毕！", ColorView.Default);
			}
		}

		private void ParseLoadJson(TreeNode<NodeData> node, string json)
		{
			if (json != null)
			{
				JsonData datas = JsonParser.GetData(json);
				node.Data.State = NodeDataState.Downloaded;
				TraverseCreate(datas, node, true, NodeDataState.Loading);
				Check(jsonTree.Head);
				List<TreeNode<NodeData>> childs = node.Childs;
				
				if (childs != null && childs.Count > 0)
				{
					string[] urls = new string[childs.Count];
					TreeNode<NodeData>[] nds = new TreeNode<NodeData>[childs.Count];
					for (int i = 0; i < childs.Count; i++)
					{
						nds[i] = childs[i];
						urls[i] = GetPathWithoutRoot(node) + ResSuffix + '/' + childs[i].Data.Name;
					}
					LoadAssets(urls, nds, ShowNodeDirItems);
				}
				ShowNodeDirItems();
			}
		}

		/// <summary>
		/// 遍历JsonData创建子树
		/// </summary>
		private void TraverseCreate(JsonData data, TreeNode<NodeData> parent, bool isLeaf = false, NodeDataState state = NodeDataState.Unload)
		{
			if (data != null)
			{
				JsonData d;
				TreeNode<NodeData> child;
				if (data.IsObject)
				{
					foreach (string n in data.Keys)
					{
						child = new TreeNode<NodeData>(new NodeData(n, isLeaf, state), parent);
						parent.AddChild(child);
						d = data[n];
						if (d.IsObject || d.IsArray)
						{
							TraverseCreate(d, child, isLeaf, state);
						}
					}
					return;
				}
				if (data.IsArray)
				{
					for (int i = 0; i < data.Count; i++)
					{
						child = new TreeNode<NodeData>(new NodeData(data[i].ToString(), isLeaf, state), parent);
						parent.AddChild(child);
						d = data[i];
						if (d.IsObject || d.IsArray)
						{
							TraverseCreate(d, child, isLeaf, state);
						}
					}
				}
			}
		}

		/// <summary>
		/// 遍历筛选节点下未下载的节点
		/// </summary>
		private void TraverseSelect(TreeNode<NodeData> node, List<TreeNode<NodeData>> outputList)
        {
			if (node.Data.State == NodeDataState.Unload && node.Childs == null && !node.Data.IsLeaf)
			{
				outputList.Add(node);
				return;
			}
			foreach (var n in node.Childs)
			{
				TraverseSelect(n, outputList);
			}
		}

		private bool Check(TreeNode<NodeData> node)
        {
            if (node.Data.State == NodeDataState.Downloaded)
            {
				return true;
            }
            if (node.Childs != null)
            {
				bool dl = true;
                foreach (var child in node.Childs)
                {
                    dl &= Check(child);
                }
                if (dl)
                {
					node.Data.State = NodeDataState.Downloaded;
					return true;
                }
            }

			return CheckNodeDownload(node);
        }

		private bool CheckNodeDownload(TreeNode<NodeData> node)
        {
			if (node.Data.State == NodeDataState.Unload && node.Childs == null)
			{
				string path = GetLocalResPath(node);
				if (Directory.Exists(path) && Directory.GetFiles(path).Length > 0)
				{
					node.Data.State = NodeDataState.Downloaded;
					string[] fnames = Directory.GetFiles(path);
					string fname;
					TreeNode<NodeData> nd;
					for (int i = 0; i < fnames.Length; i++)
					{
						fname = GetFileNameByPath(fnames[i]);
						nd = new TreeNode<NodeData>(new NodeData(fname, true, NodeDataState.Downloaded), node);
						node.AddChild(nd);
					}
					return true;
				}
			}
			return false;
		}
		#endregion

		#region ----节点与路径转化----
		/// <summary>
		/// 文件路径->文件名
		/// </summary>
		public static string GetFileNameByPath(string path)
		{
			string temp = path.Replace('/', '\\');

			return temp.Substring(temp.LastIndexOf('\\') + 1);
		}

		/// <summary>
		/// 节点->路径
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

		/// <summary>
		/// 节点->路径（不带根节点）
		/// </summary>
		private string GetPathWithoutRoot(TreeNode<NodeData> node)
		{
			return GetPath(node).Replace(infoName + "/", "");
		}

		/// <summary>
		/// Json树的叶子节点->本地下载的目录
		/// </summary>
		private string GetLocalResPath(TreeNode<NodeData> node)
		{
			return localPath + "/" + GetPathWithoutRoot(node) + ResSuffix;
		}

		/// <summary>
		/// 节点顺着路径找子节点
		/// </summary>
		private TreeNode<NodeData> FindNode(string path, TreeNode<NodeData> node)
		{
			string[] ps = path.Split('/');
			foreach (string str in ps)
			{
				node = node.FindChild(new NodeData(str));
				if (node == null)
				{
					return null;
				}
			}

			return node;
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
			view.ShowDlBtn(false);
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
			//CheckCurrNodePath();
			if (currNode.Childs != null)
			{
				List<string> names;
				List<NodeDataState> states;
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
						states = new List<NodeDataState>(lastIndex - startIndex);
                    }
                    else
                    {
						lastIndex = startIndex + ShowItemCount;
						names = new List<string>(ShowItemCount);
						states = new List<NodeDataState>(ShowItemCount);
					}
				}
                else
                {
					lastIndex = currNode.Childs.Count;
					names = new List<string>(currNode.Childs.Count);
					states = new List<NodeDataState>(currNode.Childs.Count);
					currPage = 1;
					maxPage = 1;
				}
				//Math.Min(100, currNode.Childs.Count);
				for (int i = startIndex; i < lastIndex; i++)
				{
					names.Add(currNode.Childs[i].Data.Name);
					states.Add(currNode.Childs[i].Data.State);
				}
				dirView.ShowDirItems(names, states, currPage, maxPage, currNode.Childs[0].Data.IsLeaf, ani);
			}
			else
			{
				currPage = 1;
				maxPage = 1;
				dirView.ShowDirItems(null, null, 0, 0, true, false);
			}
			dirView.SetDirName(currNode.Data.Name);
			view.ShowReturnBtn(currNode.Parent != null);
            
			view.ShowDlBtn(currNode.Data.State == NodeDataState.Unload && currNode.Parent != null);
			view.SetTreeNode(GetPath(currNode));
			dirView.ShowLiveBtn(IsLiveFolder());
		}

		private bool IsLiveFolder()
        {
			var childs = currNode.Childs;
            if (currNode.Data.State == NodeDataState.Downloaded)
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
                if (temp.Data.State != NodeDataState.Downloaded)
                {
					return;
                }
                if (MatchAudio(temp.Data.Name))
                {
					audioCtl.Play(GetLocalResPath(currNode), temp.Data.Name);
                }else if (MatchTexture(temp.Data.Name))
                {
					downloader.RequestTexture(GetLocalResPath(currNode) + "/" + temp.Data.Name, OnLoadTexture);
                }
            }
			
        }

		private void OnClickCloseTexture()
        {
			tView.Close();
        }

		private void OnLoadTexture(Texture2D texture)
        {
			tView.Show(texture);
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

		private bool MatchAudio(string name)
        {
			return name.Contains(".mp3");
        }

		private bool MatchTexture(string name)
        {
			return name.Contains(".jpg") || name.Contains(".png");
        }
        #endregion


    }
}