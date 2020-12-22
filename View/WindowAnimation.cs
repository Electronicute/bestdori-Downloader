/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-20
* 引擎版本：	2020.1.17f1c1
* 作用描述：	#
***************************************************/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Live2DCharacter
{
    public class WindowAnimation : MonoBehaviour
    {
        #region ----字段----
        public RectTransform downloadWindow;
        public RectTransform liveWindow;

        private Coroutine showDl;
        private Coroutine hideDl;
        private Coroutine showLive;
        private Coroutine hideLive;
        #endregion

        #region ----Mono单例----
        private static WindowAnimation instance;
        public static WindowAnimation Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("WindowAnimation");
                    DontDestroyOnLoad(go);
                    instance = go.AddComponent<WindowAnimation>();
                }

                return instance;
            }
        }
        #endregion

        #region ----公有方法----
        public void ShowDownloadWindow(Action finish)
        {
            if (showDl != null)
                StopCoroutine(showDl);
            if (hideDl != null)
                StopCoroutine(hideDl);
            showDl = StartCoroutine(OpenDownloadAni(finish));
        }

        public void HideDownloadWindow(Action finish)
        {
            if (showDl != null)
                StopCoroutine(showDl);
            if (hideDl != null)
                StopCoroutine(hideDl);
            hideDl = StartCoroutine(CloseDownloadAni(finish));
        }

        public void ShowLiveWindow(Action finish)
        {
            if (showLive != null)
                StopCoroutine(showLive);
            if (hideLive != null)
                StopCoroutine(hideLive);
            showLive = StartCoroutine(OpenLiveAni(finish));
        }

        public void HideLiveWindow(Action finish)
        {
            if (showLive != null)
                StopCoroutine(showLive);
            if (hideLive != null)
                StopCoroutine(hideLive);
            hideLive = StartCoroutine(CloseLiveAni(finish));
        }
        #endregion

        #region ----私有方法----
        IEnumerator OpenDownloadAni(Action finish)
        {
            float speed = 0.3f;
            downloadWindow.anchoredPosition = new Vector2(-1280, 0);
            while (Vector2.Distance(downloadWindow.anchoredPosition, Vector2.zero) > 1)
            {
                downloadWindow.anchoredPosition = Vector2.Lerp(downloadWindow.anchoredPosition, Vector2.zero, speed);
                yield return new WaitForSeconds(0.02f);
            }
            downloadWindow.anchoredPosition = Vector2.zero;
            yield return new WaitForSeconds(0.05f);

            finish?.Invoke();
        }

        IEnumerator CloseDownloadAni(Action finish)
        {
            float factor = 0.1f;
            float target = -1280;
            float speed = 0;
            float drag = 0.75f;
            float acce = (target - downloadWindow.anchoredPosition.x) * factor;
            downloadWindow.anchoredPosition = new Vector2(0, 0);
            while (Mathf.Abs(acce) > 0.1f || Mathf.Abs(speed) > 0.1f)
            {
                downloadWindow.anchoredPosition = new Vector2(downloadWindow.anchoredPosition.x + speed, 0);
                acce = (target - downloadWindow.anchoredPosition.x) * factor;
                speed += acce;
                speed *= drag;
                yield return new WaitForSeconds(0.02f);
            }
            downloadWindow.anchoredPosition = new Vector2(-1280, 0);

            finish?.Invoke();
        }

        IEnumerator CloseLiveAni(Action finish)
        {
            float speed = 0.3f;
            Vector2 target = new Vector2(1280, 0);
            liveWindow.anchoredPosition = new Vector2(0, 0);
            while (Vector2.Distance(liveWindow.anchoredPosition, target) > 1)
            {
                liveWindow.anchoredPosition = Vector2.Lerp(liveWindow.anchoredPosition, target, speed);
                yield return new WaitForSeconds(0.02f);
            }
            liveWindow.anchoredPosition = target;

            finish?.Invoke();
        }

        IEnumerator OpenLiveAni(Action finish)
        {
            float factor = 0.1f;
            float target = 0;
            float speed = 0;
            float drag = 0.75f;
            float acce = (target - liveWindow.anchoredPosition.x) * factor;
            liveWindow.anchoredPosition = new Vector2(1280, 0);
            while (Mathf.Abs(acce) > 0.1f || Mathf.Abs(speed) > 0.1f)
            {
                liveWindow.anchoredPosition = new Vector2(liveWindow.anchoredPosition.x + speed, 0);
                acce = (target - liveWindow.anchoredPosition.x) * factor;
                speed += acce;
                speed *= drag;
                yield return new WaitForSeconds(0.02f);
            }
            liveWindow.anchoredPosition = new Vector2(0, 0);

            finish?.Invoke();
        }

        private void Awake()
        {
            instance = this;
        }
        #endregion
    }
}