/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-18
* 作用描述：	#下载的资源
***************************************************/

using System;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Collections;

namespace Live2DCharacter
{
	public class AssetRes : IRes
	{
		#region ----字段----
		private string url;
		private UnityWebRequest request;
		private string path;
		private Action<bool> OnCompleted;
		#endregion

		#region ----属性----
		string IRes.Url => url;
		bool IRes.NeedDownload => true;
		#endregion

		#region ----构造方法----
		public AssetRes(string url, string path, Action<bool> action)
        {
			this.url = url;
			this.path = path;
            if (action != null)
            {
				OnCompleted += action;
			}
		}
		#endregion

		#region ----实现IRes----
		IEnumerator IRequestTask.SendRequest(Action<IRes> finish)
		{
			using (request = new UnityWebRequest(url))
			{
				request.downloadHandler = new DownloadHandlerBuffer();
				yield return request.SendWebRequest();

				if (request.result != UnityWebRequest.Result.Success)
				{
					AssetDownloader.Instance.Debug($"↑↑失败:{request.error}", ColorView.Red);
					request.Dispose();
					request = null;
					finish(this);
					yield break;
				}

				yield return new WaitForEndOfFrame();
				finish(this);
			}
        }

        IEnumerator IWriteTask.Write(Action finish)
        {
            if (request == null || request.downloadHandler == null)
            {
				OnCompleted?.Invoke(false);
				finish?.Invoke();
				yield break;
            }
			var handler = request.downloadHandler as DownloadHandlerBuffer;
			FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
			fs.Write(handler.data, 0, handler.data.Length);
			yield return new WaitForEndOfFrame();
			fs.Flush();
			fs.Close();
			fs.Dispose();
			request.Dispose();
			finish?.Invoke();
			OnCompleted?.Invoke(false);
		}

		void IRes.Release()
        {
			url = null;
			path = null;
			request = null;
        }
        #endregion
    }
}