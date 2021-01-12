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
using Utils;

namespace Live2DCharacter
{
	public class DownloadCtl
	{
		#region ----字段----
		private DownloadView view;
		private SubDirView dirView;
		private ProgressController progressCtl;
		private Live2dCtl l2dCtl;
		private AudioController audioCtl;
		private Texture2dView tView;
		private SettingView sView;
		private ConfirmView cView;

		private GameObject mask;

		private static string infoUrl;
		public static string InfoUrl
        {
            get
            {
                if (infoUrl == null)
                {
					infoUrl = _infoUrl + CountryConfig[Country] + "/assets/";
                }

				return infoUrl;
            }
        }

		private static string assetUrl;
		public static string AssetUrl
        {
            get
            {
                if (assetUrl == null)
                {
					assetUrl = _assetUrl + CountryConfig[Country] + "/";

				}

				return assetUrl;
            }
        }

		private static string localPath;
		public static string LocalPath
        {
            get
            {
                if (localPath == null)
                {
					localPath = _localPath + $"\\{CountryConfig[Country]}";
                }

				return localPath;
            }
        }

		private const string _infoUrl = @"https://bestdori.com/api/explorer/";
		private const string _assetUrl = @"https://bestdori.com/assets/";
		public const string ResSuffix = "_rip";
		private static string _localPath = null;
		private const string localPathKey = "Live2dDownloadPath";

		public static int Country = -1;
		private const string CountryKey = "Country";
		private static readonly string[] CountryConfig = new string[5] { "jp", "cn", "en", "kr", "tw"};
		private int countryTemp = 0;

		private const string infoName = "_info";
		private const string suffix = ".json";

		private GenericTree<NodeData> jsonTree;
		private TreeNode<NodeData> currNode;
		#endregion

		#region ----构造方法----
		public DownloadCtl()
        {
			l2dCtl = new Live2dCtl();
			GameObject go = GameObject.Instantiate(Resources.Load("Panel/DownloadView")) as GameObject;
			go.transform.SetParent(GameObject.FindGameObjectWithTag("DownloadPanel").transform, false);
			view = go.GetComponent<DownloadView>();

			//view.pathBtn.onClick.AddListener(OnClickPathBtn);
			view.dlBtn.onClick.AddListener(OnClickDlBtn);
			view.rtBtn.onClick.AddListener(OnClickRtBtn);
			view.stBtn.onClick.AddListener(OnClickSettingBtn);
			//view.changePathBtn.onClick.AddListener(OnClickChangePath);

			go = GameObject.Instantiate(Resources.Load("Panel/SubDirView")) as GameObject;
			go.transform.SetParent(GameObject.FindGameObjectWithTag("DownloadPanel").transform, false);
			dirView = go.GetComponent<SubDirView>();

			dirView.InitDirItme();
			dirView.showLiveBtn.onClick.AddListener(OnClickShowLive);
			dirView.openLocalBtn.onClick.AddListener(OnClickPathBtn);
			dirView.RegisterSelect(OnSelected);
			dirView.RegisterShowFinish(OnShowFinish);
			dirView.Show(false);
			dirView.ShowLiveBtn(false);

			progressCtl = new ProgressController();

			view.ShowDlBtn(false);
			view.ShowReturnBtn(false);

			audioCtl = new AudioController();

			go = GameObject.Instantiate(Resources.Load("Prefabs/Texture2dView")) as GameObject;
			go.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
			tView = go.GetComponent<Texture2dView>();
			tView.closeBtn.onClick.AddListener(OnClickCloseTexture);
			tView.Close();

			sView = GameObject.FindGameObjectWithTag("SettingView").GetComponent<SettingView>();
			sView.RegisterEvent(OnSelectCountry);
			sView.openPathBtn.onClick.AddListener(OnClickChangePath);
			sView.submitBtn.onClick.AddListener(OnClickSubmitBtn);

			cView = GameObject.FindGameObjectWithTag("ConfirmView").GetComponent<ConfirmView>();
			cView.OnClickNo();

			mask = GameObject.FindGameObjectWithTag("Mask");
			mask.SetActive(false);

			_localPath = PlayerPrefs.GetString(localPathKey, @"D:\Download");
            if (!string.IsNullOrWhiteSpace(_localPath))
            {
				sView.SetDownloadPath(_localPath);
			}
			int ct = PlayerPrefs.GetInt(CountryKey, -1);
            if (ct == -1)
            {
				mask.SetActive(false);
				sView.Show(true);
				sView.SetIndex(0);
            }
            else
            {
				mask.SetActive(true);
				Country = ct;
				RequestInfoJson();
			}
		}
		#endregion

		#region ----与Downloader交互----
		public void RequestInfoJson()
		{
			//TextAsset text = Resources.Load<TextAsset>("Json/" + infoName);
			//ParseInfoJson(text.text);
			infoUrl = null;
			assetUrl = null;
			localPath = null;
			//UnityEngine.Debug.Log(InfoUrl + infoName + suffix);
			DownloadManager.Instance.GetDownloader(infoName, new string[] { InfoUrl + infoName + suffix }, ResType.Json, ParseInfoJson).StartAll();
		}

		public void LoadTree(TreeNode<NodeData> node)
		{
			if (node.Data.State == NodeDataState.Unload)
			{
				List<TreeNode<NodeData>> loads = new List<TreeNode<NodeData>>();
				TraverseSelect(node, loads);
				GenericTree<NodeData>.Traverse(node, (nd) => nd.Data.State = NodeDataState.Loading);

				string[] urls = new string[loads.Count];
				for (int i = 0; i < loads.Count; i++)
				{
					urls[i] = LoadUrlHelper.GetJsonUrl(GetPathWithoutRoot(loads[i]) + suffix);
				}
				ILoader loader = DownloadManager.Instance.GetDownloader(node.Data.Name, urls, ResType.Json, ParseLoadJson);
				loader.RegisterEvent(progressCtl.LoaderFinish);
				if (loads.Count > 0)
				{
					progressCtl.AddLoader(loader);
					Debug($"新增{loads.Count}个请求");
				}
			}
		}

		public void LoadAssets(string[] urls, string loaderName)
        {
			ILoader loader = DownloadManager.Instance.GetDownloader(loaderName, urls, ResType.DownloadRes, OnResCompleted);
			loader.RegisterEvent(progressCtl.LoaderFinish);
			loader.RegisterEvent((str) => ShowNodeDirItems());
			progressCtl.AddLoader(loader);
			Debug($"新增{urls.Length}个下载");
		}

		private void OnResCompleted(bool result, IRes res)
        {
			TreeNode<NodeData> node = FindNode(LoadUrlHelper.AssetUrl2NodePath(res.Url), jsonTree.Head);
            if (result)
            {
				node.Data.State = NodeDataState.Downloaded;
            }
            else
            {
				node.Data.State = NodeDataState.Unload;
            }
			dirView.UpdateItem(node.Data.Name, node.Data.State);
        }
        #endregion

        #region ----ParseJsonData----
        private void ParseInfoJson(bool result, IRes res)
		{
			//UnityEngine.Debug.Log($"{InfoUrl} -> {result}");
			if (result)
			{
				string str = Encoding.UTF8.GetString(res.Datas);
				JsonData jsonData = JsonParser.GetData(str);
				currNode = new TreeNode<NodeData>(new NodeData(infoName));
				jsonTree = new GenericTree<NodeData>(currNode);
				TraverseCreate(jsonData, currNode);
				Check(currNode);
				view.ShowJsonTree(true);
				ShowNodeDirItems();
				sView.Show(false);
				mask.SetActive(false);
				WindowAnimation.Instance.ShowDownloadWindow(null);
			}
		}

		private void ParseLoadJson(bool result, IRes res)
        {
			string path = LoadUrlHelper.JsonUrl2NodePath(res.Url);
			TreeNode<NodeData> node = FindNode(path, jsonTree.Head);
			if (!result)
            {
				node.Data.State = NodeDataState.Unload;
				return;
            }
            if (res.Datas == null)
            {
				return;
            }
			string str = Encoding.UTF8.GetString(res.Datas);
            if (str != null)
            {
				path = path + ResSuffix;
				LoadUrlHelper.CreateDirIfNotFound(path, true);

				JsonData datas = JsonParser.GetData(str);
				node.Data.State = NodeDataState.Downloaded;
				TraverseCreate(datas, node, true, NodeDataState.Loading);
				//Check(jsonTree.Head);

				List<TreeNode<NodeData>> childs = node.Childs;

				if (childs != null && childs.Count > 0)
				{
					string[] urls = new string[childs.Count];
					
					for (int i = 0; i < childs.Count; i++)
					{
						urls[i] = LoadUrlHelper.GetAssetUrl(path + '/' + childs[i].Data.Name);
					}
					LoadAssets(urls, node.Data.Name + ResSuffix);
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
            if (node.Data.State != NodeDataState.Unload || node.Childs == null)
            {
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
			return LocalPath + "/" + GetPathWithoutRoot(node) + ResSuffix;
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

		private void OnClickSettingBtn()
        {
            if (progressCtl.IsLoading())
            {
				cView.Show("警告", "资源正在下载中，更改设置有可能会导致下载目录不正确，确定继续前往设置界面？", "确定", "取消", ToSetting);
            }
            else
            {
				ToSetting();
			}
        }

		private void ToSetting()
        {
			//progressCtl.StopAll();
			WindowAnimation.Instance.HideDownloadWindow(null);
			sView.Show(true);
			sView.SetIndex(Country);
		}

		private void OnClickPathBtn()
		{
			string path = LocalPath + "/" + GetPathWithoutRoot(currNode);
            if (Directory.Exists(path))
            {
				System.Diagnostics.Process.Start("explorer.exe", path.Replace("/", "\\"));
				return;
			}
			path = path + ResSuffix;
			if (Directory.Exists(path))
			{
				System.Diagnostics.Process.Start("explorer.exe", path.Replace("/", "\\"));
				return;
			}

		}

		private bool HasPath(TreeNode<NodeData> node)
        {
			string path = LocalPath + "/" + GetPathWithoutRoot(currNode);
            if (Directory.Exists(path) || Directory.Exists(path + ResSuffix))
            {
				return true;
            }

			return false;
		}

		private void OnClickDlBtn()
		{
            if (string.IsNullOrWhiteSpace(_localPath))
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
				sView.SetDownloadPath(localPath);
			}
        }
		#endregion

		#region ----与SubDirView交互----
		private void ShowNodeDirItems()
		{
			dirView.Show(true);
			view.dlBtn.interactable = false;
			view.rtBtn.interactable = false;
			dirView.SetDirName(currNode.Data.Name);
			//CheckCurrNodePath();
			if (currNode.Childs != null)
			{
				List<DirItemData> datas = new List<DirItemData>(currNode.Childs.Count);
				//Math.Min(100, currNode.Childs.Count);
				for (int i = 0; i < currNode.Childs.Count; i++)
				{
					datas.Add(new DirItemData(currNode.Childs[i].Data.Name, currNode.Childs[i].Data.State, i));
				}
				dirView.ShowDirItems(datas);
			}
			else
			{
				dirView.ShowDirItems(new List<DirItemData>());
			}
			view.ShowReturnBtn(currNode.Parent != null);

			view.ShowDlBtn(currNode.Data.State == NodeDataState.Unload && currNode.Parent != null);
			view.SetTreeNode(GetPath(currNode));
			dirView.ShowLiveBtn(IsLiveFolder());
			dirView.ShowLocalBtn(HasPath(currNode));
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
					LocalAssetLoader.Instance.LoadTexture(GetLocalResPath(currNode) + "/" + temp.Data.Name, OnLoadTexture);
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

		private bool MatchAudio(string name)
        {
			return name.Contains(".mp3");
        }

		private bool MatchTexture(string name)
        {
			return name.Contains(".jpg") || name.Contains(".png");
        }
        #endregion

        #region ----与Setting交互----
		private void OnClickSubmitBtn()
        {
			sView.ShowBtns(false);
            if (countryTemp != Country)
            {
				mask.SetActive(true);
				Country = countryTemp;
				PlayerPrefs.SetInt(CountryKey, Country);
				RequestInfoJson();
				return;
			}
			sView.Show(false);
			WindowAnimation.Instance.ShowDownloadWindow(null);
        }

		private void OnSelectCountry(int index)
        {
			countryTemp = index;
        }
        #endregion
    }
}