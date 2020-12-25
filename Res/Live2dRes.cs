/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-20
* 作用描述：	#
***************************************************/

using System;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Collections;

namespace Live2DCharacter
{
    public class Live2dRes : IRes
    {
		#region ----字段----
		private UnityWebRequest request;
		private string[] paths;
		private Action<Live2dRes> OnCompleted;
		private byte[] mocDatas;
		private byte[] aniDatas;
		private Texture2D texture;
		#endregion

		#region ----属性----
		string IRes.Url => null;
		bool IRes.NeedDownload => false;

		public byte[] Moc => mocDatas;
		public byte[] Motion => aniDatas;
		public Texture2D Texture => texture;
		#endregion

		#region ----构造方法----
		public Live2dRes(string[] paths, Action<Live2dRes> onCompleted)
		{
			this.paths = paths;
            if (onCompleted != null)
            {
				OnCompleted += onCompleted;
            }
		}
		#endregion

		#region ----实现IRes----
		IEnumerator IRequestTask.SendRequest(Action<IRes> finish)
		{
			//moc
			FileStream fs = new FileStream(paths[0], FileMode.Open, FileAccess.Read);
			byte[] buffer = new byte[fs.Length];
			fs.Read(buffer, 0, buffer.Length);
			fs.Close();
			fs.Dispose();
			mocDatas = buffer;

			//animation
			fs = new FileStream(paths[1], FileMode.Open);
			buffer = new byte[fs.Length];
			fs.Read(buffer, 0, buffer.Length);
			fs.Close();
			fs.Dispose();
			aniDatas = buffer;

			//texture
			request = UnityWebRequestTexture.GetTexture("file:///" + paths[2]);
			yield return request.SendWebRequest();
			if (request.result != UnityWebRequest.Result.Success)
			{
				DoError(request);
				finish?.Invoke(null);
				OnCompleted?.Invoke(null);
				yield break;
			}
			texture = DownloadHandlerTexture.GetContent(request);

			yield return new WaitForEndOfFrame();
			finish?.Invoke(this);
			OnCompleted?.Invoke(this);
			request.Dispose();
		}

		IEnumerator IWriteTask.Write(Action finish)
		{
			finish?.Invoke();
			yield return null;
		}

		void IRes.Release()
		{
			mocDatas = null;
			aniDatas = null;
			texture = null;
			paths = null;
			request = null;
		}
		#endregion

		#region ----私有方法----
		private void DoError(UnityWebRequest req)
        {
			AssetDownloader.Instance.Debug($"加载Live失败:{request.error}", ColorView.Red);
			request.Dispose();
			request = null;
			OnCompleted?.Invoke(null);
		}
		#endregion
	}
}