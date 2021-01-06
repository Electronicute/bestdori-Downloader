/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2021-01-01
* 作用描述：	#
***************************************************/

using System;
using System.Collections.Generic;

namespace Utils
{
    public class ResDownloader : ILoader
    {
        #region ----字段----
        protected readonly List<IRes> resList = new List<IRes>();
        protected int completedCount = 0;
        protected Action<string> onAllCompleted;
        #endregion

        #region ----实现IPoolable----
        public bool IsRecycled { get; set; }

        public void OnRecycled()
        {
            ReleaseAll();
            completedCount = 0;
            onAllCompleted = null;
            resList.Clear();
            DownloadManager.Instance.RemoveLoader(Name);
            DownloadManager.Instance.RemoveUnUseRes();
        }
        #endregion

        #region ----实现ILoader----
        public string Name { get; set; }

        public float Progress()
        {
            if (resList.Count == 0)
            {
                return 1;
            }
            float pg = 0;
            foreach (var res in resList)
            {
                pg += res.Progress;
            }

            return pg / resList.Count;
        }

        public void Add(string[] urls, ResType resType, Action<bool, IRes> listener, bool autoStart = false)
        {
            IRes res;
            foreach (var url in urls)
            {
                res = DownloadManager.Instance.GetRes(url, Name, resType, true);
                res.AddRef();
                resList.Add(res);
                if (res.State == ResState.Waiting)
                {
                    res.RegisterEvent(listener);
                    res.RegisterEvent(OnFinish);
                    if (autoStart)
                    {
                        res.Request();
                    }
                }
            }
        }

        void ILoader.StartAll()
        {
            foreach (var res in resList)
            {
                res.Request();
            }
        }

        void ILoader.RegisterEvent(Action<string> onAllCompleted)
        {
            if (onAllCompleted != null)
            {
                this.onAllCompleted += onAllCompleted;
            }
        }

        public void ReleaseAll()
        {
            foreach (var res in resList)
            {
                res.ReduceRef();
            }
        }

        public void StopAll()
        {
            foreach (var res in resList)
            {
                res.StopRequest();
                if (res.State != ResState.Completed)
                {
                    res.State = ResState.Cancel;
                }
            }
            Recycle();
        }
        #endregion

        #region ----实现IRecyclable----
        public void Recycle()
        {
            ObjectPool<ResDownloader>.Instance.Recycle(this);
        }
        #endregion

        #region ----私有方法----
        private void OnFinish(bool result, IRes res)
        {
            if (result)
            {
                completedCount++;
                if (completedCount >= resList.Count)
                {
                    onAllCompleted?.Invoke(Name);
                }
            }
            else
            {
                if (res.State != ResState.Cancel)
                {
                    DownloadManager.Instance.AddToRetry(res);
                }
            }
        }
        #endregion
    }
}