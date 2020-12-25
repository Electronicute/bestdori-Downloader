/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-25
* 作用描述：	#
***************************************************/

using System;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine;

namespace Live2DCharacter
{
	public class AudioRes : IRes
	{
		#region ----字段----
		private UnityWebRequest request;
		private string path;
		private string audioName;
		private Action<AudioClip, string> OnCompleted;
		#endregion

		#region ----属性----
		string IRes.Url => null;
		bool IRes.NeedDownload => false;
		#endregion

		#region ----构造方法----
		public AudioRes(string path, string audioName, Action<AudioClip, string> onCompleted)
		{
			this.path = path;
			this.audioName = audioName;
			if (onCompleted != null)
			{
				OnCompleted += onCompleted;
			}
		}
		#endregion

		#region ----实现IRes----
		IEnumerator IRequestTask.SendRequest(Action<IRes> finish)
		{
			request = UnityWebRequestMultimedia.GetAudioClip("file:///" + path, AudioType.MPEG);
			yield return request.SendWebRequest();
			if (request.result != UnityWebRequest.Result.Success)
			{
				AssetDownloader.Instance.Debug($"加载失败:{request.error}", ColorView.Red);
				request.Dispose();
				request = null;
				finish?.Invoke(null);
				OnCompleted?.Invoke(null, null);
				yield break;
			}

			yield return new WaitForEndOfFrame();
			finish?.Invoke(this);
			OnCompleted?.Invoke(DownloadHandlerAudioClip.GetContent(request), audioName);
			request.Dispose();
		}

		IEnumerator IWriteTask.Write(Action finish)
		{
			finish?.Invoke();
			yield return null;
		}

		void IRes.Release()
		{
			path = null;
			request = null;
			audioName = null;
			OnCompleted = null;
		}
		#endregion
	}
}