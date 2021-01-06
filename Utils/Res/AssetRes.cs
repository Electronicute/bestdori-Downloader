/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2021-01-02
* 作用描述：	#
***************************************************/

using System;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine;
using System.IO;

namespace Utils
{
	public class AssetRes : JsonRes
	{
        #region ----实现Res----
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

            var handler = request.downloadHandler as DownloadHandlerBuffer;
            FileStream fs = new FileStream(LoadUrlHelper.Url2LocalPath(url), FileMode.OpenOrCreate);
            fs.Write(handler.data, 0, handler.data.Length);
            yield return new WaitForEndOfFrame();
            fs.Flush();
            fs.Close();
            fs.Dispose();

            State = ResState.Completed;
            onFinish();
            //OnReleaseRes();
            if (request != null)
            {
                request.Dispose();
                request = null;
            }
        }

        public override void Recycle()
        {
            ObjectPool<AssetRes>.Instance.Recycle(this);
        }
        #endregion

    }
}