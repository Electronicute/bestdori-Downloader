/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-31
* 作用描述：	#下载管理器
***************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
	public class DownloadManager : MonoBehaviour
	{
        #region ----Mono单例----
        private static DownloadManager instance;
        private static readonly object lockOjb = new object();

        public static DownloadManager Instance
        {
            get
            {
                lock (lockOjb)
                {
                    if (instance == null)
                    {
                        GameObject go = new GameObject("DonwloadManager");
                        DontDestroyOnLoad(go);
                        instance = go.AddComponent<DownloadManager>();
                    }

                    return instance;
                }
            }
        }
        #endregion

        #region ----字段----
        public const int MaxDownLoadTrd                         = 4;								//最大请求线程数
        private int CurrTrd                                     = 0;                                //当前线程数
        private readonly GQueue<IRequestTask> Requests           = new GQueue<IRequestTask>();       //等待请求
        private readonly GQueue<IRequestTask> Retrys             = new GQueue<IRequestTask>();       //等待重试
        private readonly Dictionary<string, IRes> ResDic        = new Dictionary<string, IRes>();	//所有资源
        private readonly Dictionary<string, ILoader> Loaders    = new Dictionary<string, ILoader>();//加载器
        #endregion

        #region ----公有方法----
        public ILoader GetDownloader(string loaderName, string[] urls, ResType resType, Action<bool, IRes> listener = null)
        {
            ILoader loader = GetLoader(loaderName);
            if (loader == null)
            {
                loader = ObjectPool<ResDownloader>.Instance.Get();
                loader.Name = loaderName;
                Loaders.Add(loaderName, loader);
                loader.Add(urls, resType, listener, false);
            }
            else
            {
                loader.Add(urls, resType, listener, true);
            }

            return loader;
        }

        public IRes GetRes(string url, string loaderName, ResType resType, bool create = true)
        {
            IRes res = GetRes(url);
            if (res != null)
            {
                return res;
            }
            if (!create)
            {
                return null;
            }
            res = ResFactory.Create(url, loaderName, resType);
            if (res != null)
            {
                ResDic.Add(url, res);
            }

            return res;
        }

        public void AddToDownload(IRequestTask task)
        {
            if (task == null)
            {
                return;
            }
            Requests.Enqueue(task);
            TryStartTask();
        }

        public void AddToRetry(IRequestTask task)
        {
            if (task == null)
            {
                return;
            }
            if (task.ErrorCount <= 0)
            {
                task.State = ResState.Completed;
                return;
            }
            Retrys.Enqueue(task);
            TryStartTask();
        }

        public void RemoveUnUseRes()
        {
            List<string> dirtys = new List<string>();
            foreach (var res in ResDic)
            {
                if (res.Value.RefCount <= 0)
                {
                    res.Value.Release();
                    dirtys.Add(res.Key);
                    res.Value.Recycle();
                }
            }
            dirtys.ForEach((d) => ResDic.Remove(d));
        }

        public void StopRequest(IRequestTask task)
        {
            if (task == null)
            {
                return;
            }
            if (task.Coroutine != null && task.State == ResState.Loading)
            {
                CurrTrd--;
                StopCoroutine(task.Coroutine);
            }
        }

        public void RemoveLoader(string loaderName)
        {
            if (Loaders.ContainsKey(loaderName))
            {
                Loaders.Remove(loaderName);
            }
            Requests.Remove((t) => ((IRes)t).LoaderName == loaderName);
            Retrys.Remove((t) => ((IRes)t).LoaderName == loaderName);
            int count = MaxDownLoadTrd - CurrTrd;
            while (count > 0)
            {
                TryStartTask();
                count--;
            }
        }
        #endregion

        #region ----私有方法----
        private IRes GetRes(string url)
        {
            IRes res = null;
            ResDic.TryGetValue(url, out res);

            return res;
        }

        private ILoader GetLoader(string name)
        {
            ILoader loader = null;
            Loaders.TryGetValue(name, out loader);

            return loader;
        }

        private void TryStartTask()
        {
            if (CurrTrd < MaxDownLoadTrd)
            {
                IRequestTask task;
                if (Retrys.Count > 0)
                {
                    task = GetNotEmptyTask(Retrys);
                    if (task != null)
                    {
                        CurrTrd++;
                        task.Coroutine = StartCoroutine(task.SendRequest(OnRequestFinish));
                        return;
                    }
                }
                if (Requests.Count > 0)
                {
                    task = GetNotEmptyTask(Requests);
                    if (task != null)
                    {
                        CurrTrd++;
                        task.Coroutine = StartCoroutine(task.SendRequest(OnRequestFinish));
                        return;
                    }
                }
            }
        }

        private void OnRequestFinish()
        {
            CurrTrd--;
            TryStartTask();
        }

        private IRequestTask GetNotEmptyTask(GQueue<IRequestTask> queue)
        {
            IRequestTask task = queue.Dequeue();
            if (task == null)
            {
                return null;
            }
            if (task.State == ResState.Cancel && queue.Count > 0)
            {
                return GetNotEmptyTask(queue);
            }
            return task;
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            foreach (var r in ResDic)
            {
                r.Value.Release();
            }
            Requests.Clear();
            Retrys.Clear();
            ResDic.Clear();
            instance = null;
        }
        #endregion
    }
}