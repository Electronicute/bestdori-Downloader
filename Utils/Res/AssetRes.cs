/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2021-01-02
* 作用描述：	#
***************************************************/

using System;
using System.Collections;
using UnityEngine.Networking;

namespace Utils
{
	public class AssetRes : JsonRes
	{
        #region ----实现Res----
        public override IEnumerator SendRequest(Action onFinish)
        {
            request =  new UnityWebRequest(url);
            request.downloadHandler = new DownloadHandlerFile(LoadUrlHelper.Url2LocalPath(url));
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onFinish();
                OnLoadFaild();
                request.Dispose();
                request = null;
                yield break;
            }
            state = ResState.Completed;
            onFinish();
            OnReleaseRes();
            request.Dispose();
            request = null;
        }

        public override void Recycle()
        {
            ObjectPool<AssetRes>.Instance.Recycle(this);
        }
        #endregion

    }
}