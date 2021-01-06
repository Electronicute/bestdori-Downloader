/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2021-01-02
* 作用描述：	#
***************************************************/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Utils
{
    public class JsonRes : Res
    {
        #region ----字段----
        protected UnityWebRequest request;
        #endregion

        #region ----实现Res----
        protected override float CalculateProgress()
        {
            if (request == null)
            {
                return 0;
            }

            return request.downloadProgress;
        }

        public override void Request()
        {
            DownloadManager.Instance.AddToDownload(this);
        }

        public override void Recycle()
        {
            ObjectPool<JsonRes>.Instance.Recycle(this);
        }

        public override IEnumerator SendRequest(Action onFinish)
        {
            if (url == null)
            {
                onFinish();
                yield break;
            }
            State = ResState.Loading;
            request = new UnityWebRequest(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            if (state == ResState.Cancel || request == null)
            {
                yield break;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                onFinish();
                OnLoadFaild();
                request.Dispose();
                request = null;
                errorCount--;
                yield break;
            }

            datas = request.downloadHandler.data;
            State = ResState.Completed;
            onFinish();
            OnReleaseRes();
            if (request != null)
            {
                request.Dispose();
                request = null;
            }
        }

        public override void OnRecycled()
        {
            base.OnRecycled();
            if (request != null)
            {
                request.Dispose();
                request = null;
            }
        }
        #endregion
    }
}