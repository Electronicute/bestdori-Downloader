/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2021-01-11
* 引擎版本：	2020.1.17f1c1
* 作用描述：	#设置界面
***************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Live2DCharacter
{
    public class SettingView : MonoBehaviour
    {
        #region ----字段----
        private static readonly Vector2 TargetShow = Vector2.zero;
        private static readonly Vector2 TargetHide = new Vector2(1280, 0); 
        private Vector2 target;
        private Vector2 flagTarget;

        public Button submitBtn;
        public RectTransform trans;
        public CountryView[] Countrys;
        public RectTransform flag;
        public Text pathLabel;
        public Button openPathBtn;

        private Action<int> onSelected;
        private Coroutine coroutine;
        #endregion

        #region ----Mono----
        private void Awake()
        {
            for (int i = 0; i < Countrys.Length; i++)
            {
                Countrys[i].RegisterEvent(OnSelected);
            }
        }
        #endregion

        #region ----公有方法----
        public void Show(bool display)
        {
            if (!display)
            {
                target = TargetHide;
                StartCoroutine(Show(() => { ShowBtns(false); }));

            }
            else
            {
                target = TargetShow;
                StartCoroutine(Show(() => ShowBtns(true)));

            }
        }

        public void ShowBtns(bool display) => submitBtn.gameObject.SetActive(display);

        public void SetDownloadPath(string path) => pathLabel.text = path;

        public void RegisterEvent(Action<int> action) => onSelected += action;

        public void SetIndex(int index)
        {
            OnSelected(index);
        }
        #endregion

        #region ----私有方法----
        IEnumerator Show(Action onFinish = null)
        {
            float speed = 0.3f;
            while (Vector2.Distance(trans.anchoredPosition, target) > 1)
            {
                trans.anchoredPosition = Vector2.Lerp(trans.anchoredPosition, target, speed);
                yield return new WaitForSeconds(0.02f);
            }
            trans.anchoredPosition = target;
            yield return new WaitForSeconds(0.05f);

            onFinish?.Invoke();
        }

        private void OnSelected(int index)
        {
            flagTarget = new Vector2(Countrys[index].GetComponent<RectTransform>().anchoredPosition.x, 0);
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            coroutine = StartCoroutine(MoveFlag());
            onSelected?.Invoke(index);
        }

        IEnumerator MoveFlag()
        {
            float factor = 0.1f;
            float speed = 0;
            float drag = 0.75f;
            float acce = (flagTarget.x - flag.anchoredPosition.x) * factor;
            while (Mathf.Abs(acce) > 0.1f || Mathf.Abs(speed) > 0.1f)
            {
                flag.anchoredPosition = new Vector2(flag.anchoredPosition.x + speed, 0);
                acce = (flagTarget.x - flag.anchoredPosition.x) * factor;
                speed += acce;
                speed *= drag;
                yield return new WaitForSeconds(0.02f);
            }
            flag.anchoredPosition = flagTarget;
        }
        #endregion
    }
}