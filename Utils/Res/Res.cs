/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-31
* 作用描述：	#抽象资源
***************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Utils
{
    public abstract class Res : IRes, IPoolable
    {
        #region ----字段----
        protected string url;
        protected byte[] datas;
        protected ResState state        = ResState.Waiting;
        private int refCount;
        private Action<bool, IRes> listener;
        #endregion

        #region ----公有方法----
        public static T Get<T>(string url) where T : Res, new()
        {
            T res = ObjectPool<T>.Instance.Get();
            res.url = url;

            return res;
        }
        #endregion

        #region ----私有方法----
        protected virtual void OnReleaseRes()
        {
            datas = null;
        }

        protected virtual float CalculateProgress()
        {
            return 0;
        }

        protected void OnLoadFaild()
        {
            state = ResState.Error;
            OnResult(false);
        }

        private void OnResult(bool result)
        {
            listener?.Invoke(result, this);
            listener = null;
        }
        #endregion

        #region ----实现IRes----
        string IRes.Url => url;

        byte[] IRes.Datas => datas;

        public ResState State
        {
            get => state;
            set
            {
                state = value;
                if (state == ResState.Completed)
                {
                    OnResult(true);
                }
            }
        }

        public int RefCount => refCount;

        float IRes.Progress
        {
            get
            {
                switch (state)
                {
                    case ResState.Loading:
                        return CalculateProgress();
                    case ResState.Completed:
                        return 1;
                }

                return 0;
            }
        }

        void IRes.AddRef()
        {
            refCount++;
        }

        public virtual void Recycle()
        {
        }

        void IRes.ReduceRef()
        {
            refCount--;
            if (refCount <= 0)
            {
                if (state == ResState.Loading)
                {
                    return;
                }
                OnReleaseRes();
            }
        }

        public bool Release()
        {
            if (state == ResState.Loading)
            {
                return false;
            }
            if (state != ResState.Completed)
            {
                return true;
            }
            OnReleaseRes();
            state = ResState.Waiting;
            listener = null;

            return true;
        }

        void IRes.RegisterEvent(Action<bool, IRes> onFinish)
        {
            if (onFinish == null)
            {
                return;
            }
            if (state == ResState.Completed)
            {
                onFinish(true, this);
                return;
            }
            listener += onFinish;
        }

        public abstract void Request();

        public virtual IEnumerator SendRequest(Action onFinish)
        {
            onFinish();
            yield break;
        }

        public virtual void StopRequest()
        {
            
        }
        #endregion

        #region ----实现IPoolable----
        bool IPoolable.IsRecycled { get; set; }

        public virtual void OnRecycled()
        {
            url = null;
            listener = null;
        }
        #endregion
    }
}