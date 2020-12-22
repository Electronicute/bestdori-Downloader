/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-18
* 作用描述：	#下载器
***************************************************/

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace Live2DCharacter
{
	public class AssetDownloader : MonoBehaviour
	{
		#region ----Mono单例----
		private static AssetDownloader instance;
		private static readonly object lockOjb = new object();

		public static AssetDownloader Instance
        {
            get
            {
                lock (lockOjb)
                {
					if (instance == null)
                    {
						GameObject go = new GameObject("DownLoader");
						DontDestroyOnLoad(go);
						instance = go.AddComponent<AssetDownloader>();
                    }

					return instance;
                }
            }
        }
        #endregion

        #region ----字段----
        public const int MaxDownLoadTrd					= 4;								//最大请求线程数
        private int curTrd                              = 0;
		private readonly LinkedList<IRequestTask> reqs	= new LinkedList<IRequestTask>();	//等待请求
		private readonly LinkedList<IWriteTask> writes	= new LinkedList<IWriteTask>();		//等待写入
		private readonly Dictionary<string, IRes> res	= new Dictionary<string, IRes>();	//所有资源
        private Action<string, ColorView> debugger;
        private Action<bool> allCompleted;
        private float newReqCount = 0;
        private float newWriteCount = 0;
        private int completedReqCount = 0;
        private int completedWriteCount = 0;
        private readonly float[] progress = new float[2];
		#endregion

		#region ----公有方法----
		public void AddToRequest(string url, Action<bool> onCompleted)
        {
            string fileName = DownloadCtl.GetFileNameByPath(url);
            if (!res.ContainsKey(url))
            {
                CreateDirIfNotFound(url, false);
                AssetRes assetRes = new AssetRes(GetAssetUrl(url), GetPath(url), onCompleted);
				res.Add(url, assetRes);
				reqs.AddLast(assetRes);
                Debug($"↓↓{fileName}");
                TryStartRequest();
            }
            else
            {
				Debug($"↓↓失败:{fileName}已经在列表！", ColorView.Red);
            }
        }

        public void RegisterDebug(Action<string, ColorView> action)
        {
            if (action != null)
            {
                debugger += action;
            }
        }

        public void Debug(string msg, ColorView color = ColorView.Default)
        {
            debugger?.Invoke(msg, color);
        }

        public void RequestJson(string url, Action<string, string> onCompleted)
        {
            string fileName = DownloadCtl.GetFileNameByPath(url);
            newReqCount++;
            if (!res.ContainsKey(url))
            {
                JsonRes json = new JsonRes(GetInfoUrl(url), onCompleted);
                res.Add(url, json);
                reqs.AddLast(json);
                Debug($"↑↑{fileName}");
                TryStartRequest();
            }
            else
            {
                Debug($"↑↑失败:{fileName}已经在列表！", ColorView.Red);
            }
        }

        public void RequestLive2d(string[] paths, Action<Live2dRes> onCompleted)
        {
            Live2dRes live = new Live2dRes(paths, onCompleted);
            reqs.AddLast(live);
            Debug("加载Live2D！");
            TryStartRequest();
        }

        public void RequestAssets(string[] urls, Action<bool> onCompleted)
        {
            foreach (string url in urls)
            {
                AddToRequest(url, null);
            }
            allCompleted += onCompleted;
            newReqCount += urls.Length;
            newWriteCount += urls.Length;
            Debug($"新增{urls.Length}个下载");
        }

        public void ClearCompletedCount()
        {
            newReqCount = 0;
            newWriteCount = 0;
            completedReqCount = 0;
            completedWriteCount = 0;
        }

        public float[] Progress()
        {
            if (newReqCount == 0)
            {
                return null;
            }
            progress[0] = completedReqCount / newReqCount;
            if (newWriteCount == 0)
                progress[1] = 0;
            else
                progress[1] = completedWriteCount / newWriteCount;

            return progress;
        }

		#endregion

		#region ----私有方法----
		private void TryStartRequest()
        {
            if (curTrd < MaxDownLoadTrd)
            {
                if (reqs.Count > 0)
                {
                    IRequestTask task = reqs.First.Value;
                    reqs.RemoveFirst();
                    curTrd++;
                    StartCoroutine(task.SendRequest(OnRequestFinish));
                    return;
                }
                if (curTrd == 0)
                {
                    Debug("所有资源↑↑完毕！", ColorView.Green);
                }
            }
        }

		private void OnRequestFinish(IRes res)
        {
            curTrd--;
            ++completedReqCount;
			TryStartRequest();
            if (res.NeedDownload)
            {
                writes.AddLast(res);
                TryWrite();
            }
        }

		private void TryWrite()
        {
            if (writes.Count > 0)
            {
				IWriteTask task = writes.First.Value;
				writes.RemoveFirst();
                StartCoroutine(task.Write(OnWriteFinish));
                return;
			}
            if (curTrd == 0 && reqs.Count == 0)
            {
                Debug("所有资源↓↓完毕！", ColorView.Green);
                allCompleted?.Invoke(true);
            }
        }

		private void OnWriteFinish()
        {
            ++completedWriteCount;
			TryWrite();
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            if (reqs.Count > 0)
            {
				Debug($"↓↓中断，剩余数量:{reqs.Count}", ColorView.Red);
            }
            foreach (var r in res)
            {
                r.Value.Release();
            }
            reqs.Clear();
            writes.Clear();
            res.Clear();
            instance = null;
        }

        private string GetInfoUrl(string url)
        {
            return DownloadCtl.InfoUrl + url;
        }

        private string GetAssetUrl(string url)
        {
            return DownloadCtl.AssetUrl + url;
        }

        private string GetPath(string url)
        {
            return DownloadCtl.localPath + '/' + url;
        }

        private void CreateDirIfNotFound(string url, bool isFolder = false)
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
        #endregion

    }
}