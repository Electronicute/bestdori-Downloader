/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2021-01-01
* 作用描述：	#
***************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Utils
{
    public class ResDownloader : ILoader
    {
        #region ----字段----
        protected readonly List<IRes> resList = new List<IRes>();
        #endregion

        #region ----实现IPoolable----
        public bool IsRecycled { get; set; }

        public void OnRecycled()
        {
            ReleaseAll();
            DownloadManager.Instance.RemoveUnUseRes();
        }
        #endregion

        #region ----实现ILoader----
        public string Name { get; set; }

        public float Progress
        {
            get
            {
                float pg = 0;
                foreach (var res in resList)
                {
                    pg += res.Progress;
                }

                return pg / resList.Count;
            }
        }

        public void Add(string[] urls, ResType resType, Action<bool, IRes> listener)
        {
            IRes res;
            foreach (var url in urls)
            {
                res = DownloadManager.Instance.GetRes(url, resType, true);
                res.AddRef();
                resList.Add(res);
                if (res.State == ResState.Waiting)
                {
                    res.RegisterEvent(listener);
                    res.Request();
                }
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
                res.ReduceRef();
            }
            Recycle();
            DownloadManager.Instance.RemoveUnUseRes();
        }
        #endregion

        #region ----实现IRecyclable----
        public void Recycle()
        {
            ObjectPool<ResDownloader>.Instance.Recycle(this);
        }
        #endregion

    }
}