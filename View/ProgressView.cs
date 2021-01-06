/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-22
* 引擎版本：	2020.1.17f1c1
* 作用描述：	#
***************************************************/

using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Live2DCharacter
{
    public class ProgressView : MonoBehaviour, IPoolable, IRecyclable
    {
        #region ----字段----
        public Text nameLabel;
        public Slider downloadSlider;
        public Text progressLabel;
        public RectTransform rectTrans;
        public CanvasGroup group;
        public Button cancelBtn;

        private Func<float> onProgress;
        private float lastValue = 0;
        private Vector2 targetPos;
        private const int height = -50;
        #endregion

        #region ----MonoBehaviour----
        void Update()
        {
            if (onProgress == null)
            {
                return;
            }
            float progress = onProgress.Invoke();
            lastValue = Mathf.Lerp(lastValue, progress, 0.7f);
            progressLabel.text = string.Format("{0:F2}%", lastValue * 100);
            downloadSlider.value = lastValue;
        }
        #endregion

        #region ----公有方法----
        public void SetName(string loaderName)
        {
            nameLabel.text = loaderName;
        }

        public void Show(int index)
        {
            StopAllCoroutines();
            cancelBtn.gameObject.SetActive(false);
            gameObject.SetActive(true);
            enabled = true;
            rectTrans.anchoredPosition = new Vector2(200, height * index);
            targetPos = new Vector2(0, height * index);
            group.alpha = 0;
            StartCoroutine(Show());
        }

        public void Stop()
        {
            enabled = false;
        }

        public void Remove(Action finish, float time = 0.7f)
        {
            StopAllCoroutines();
            cancelBtn.gameObject.SetActive(false);
            targetPos.x = 200;
            StartCoroutine(Hide(finish, time));
        }

        public void SetIndex(int index)
        {
            //StopAllCoroutines();
            targetPos.y = height * index;
            StartCoroutine(Move());
        }

        public void Register(Func<float> func)
        {
            if (func != null)
            {
                onProgress += func;
            }
        }
        #endregion

        #region ----私有方法----
        IEnumerator Hide(Action finish, float time)
        {
            if (time > 0)
            {
                yield return new WaitForSeconds(time);
            }
            enabled = false;
            while (targetPos.x - rectTrans.anchoredPosition.x > 0.01f)
            {
                rectTrans.anchoredPosition = Vector2.Lerp(rectTrans.anchoredPosition, targetPos, 0.5f);
                group.alpha = Mathf.Lerp(group.alpha, 0, 0.6f);
                yield return new WaitForSeconds(0.02f);
            }
            finish?.Invoke();
            Recycle();
        }

        IEnumerator Show()
        {
            yield return new WaitForSeconds(0.1f);
            while (rectTrans.anchoredPosition.x - targetPos.x > 0.01f)
            {
                rectTrans.anchoredPosition = Vector2.Lerp(rectTrans.anchoredPosition, targetPos, 0.5f);
                group.alpha = Mathf.Lerp(group.alpha, 1, 0.6f);
                yield return new WaitForSeconds(0.02f);
            }
            yield return new WaitForSeconds(0.02f);
            rectTrans.anchoredPosition = targetPos;
            cancelBtn.gameObject.SetActive(true);
        }

        IEnumerator Move()
        {
            while (targetPos.y - rectTrans.anchoredPosition.y > 0.01f)
            {
                rectTrans.anchoredPosition = Vector2.Lerp(rectTrans.anchoredPosition, targetPos, 0.8f);
                yield return new WaitForSeconds(0.02f);
            }
            rectTrans.anchoredPosition = targetPos;
        }
        #endregion

        #region ----实现IPoolable----
        public bool IsRecycled { get; set; }

        public void OnRecycled()
        {
            enabled = false;
            gameObject.SetActive(false);
            onProgress = null;
            cancelBtn.onClick.RemoveAllListeners();
        }
        #endregion

        #region ----实现IPoolable----
        public void Recycle()
        {
            MonoObjectPool<ProgressView>.Instance.Recycle(this);
        }
        #endregion
    }
}