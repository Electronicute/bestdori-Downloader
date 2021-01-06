/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-24
* 作用描述：	#
***************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace Live2DCharacter
{
	public class AudioController
	{
		#region ----字段----
		private AudioView view;
		private string curName;
		private SpectrumView sview;
		#endregion

		#region ----构造方法----
		public AudioController()
        {
			GameObject go = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/AudioView"));
			go.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
			view = go.AddComponent<AudioView>();
			view.Init();
			view.RegisterPlayBtn(OnClickPlay);
			view.RegisterStopBtn(OnClickStop);
			view.RegisterCloseBtn(OnClickClose);
			view.RegisterSlider(OnChangePlayTime);

			go = new GameObject("SpectrumView");
			sview = go.AddComponent<SpectrumView>();
			sview.screenPos = new Vector2(0, 24);
        }
		#endregion

		#region ----公有方法----
		public void Play(string path, string name)
        {
			view.Show(true);
			sview.Show(true);
            if (curName == name)
            {
				view.Play(true);
				return;
            }
			curName = name;
			LocalAssetLoader.Instance.LoadAudio(path + "/" + name, name, OnLoadClip);
        }
		#endregion

		#region ----私有方法----
		private void OnLoadClip(AudioClip clip, string name)
        {
            if (clip == null)
            {
				sview.Show(false);
				view.Show(false);
				return;
            }
			view.PlayClip(clip, name);
        }

		private void OnClickPlay()
        {
			sview.Show(true);
			view.Play(true);
        }

		private void OnClickStop()
        {
			sview.Show(false);
			view.Play(false);
        }

		private void OnClickClose()
        {
			view.Play(false);
			view.Show(false);
			sview.Show(false);
        }

		private void OnChangePlayTime(float progress)
        {
			if (progress <= 0)
				progress = 0;
			if (progress >= 1)
				progress = 0.99f;
			view.OnChangePlayTime(progress);
        }
        #endregion

        #region ----静态方法----
		/// <summary>
		/// 秒数 -> 00:00:00
		/// </summary>
		public static string Sec2TimeFormat(int sec)
        {
			StringBuilder timeStr = new StringBuilder();
			int curTime = sec;
			timeStr.Append(string.Format("{0:D2}", curTime / 3600) + ":");
			curTime = curTime % 3600;
			timeStr.Append(string.Format("{0:D2}", curTime / 60) + ":");
			curTime = curTime % 60;
			timeStr.Append(string.Format("{0:D2}", curTime));

			return timeStr.ToString();
        }
        #endregion

       

    }
}