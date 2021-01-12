/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2021-01-12
* 引擎版本：	2020.1.17f1c1
* 作用描述：	#确认框
***************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Live2DCharacter
{
    public class ConfirmView : MonoBehaviour
    {
        #region ----字段----
        [SerializeField] private Text title;
        [SerializeField] private Text content;
        [SerializeField] private Text yesLabel;
        [SerializeField] private Text noLabel;
        //[SerializeField] private Button yesBtn;
        //[SerializeField] private Button noBtn;

        private Action yesAction;
        #endregion

        #region ----公有方法----
        public void Show(string title, string content, string yesStr, string noStr, Action yesAction)
        {
            gameObject.SetActive(true);
            this.title.text = title;
            this.content.text = content;
            this.yesLabel.text = yesStr;
            this.noLabel.text = noStr;
            this.yesAction += yesAction;
        }

        public void OnClickYes()
        {
            this.yesAction?.Invoke();
            OnClickNo();
        }

        public void OnClickNo()
        {
            Show(false);
            ResetContent();
        }
        #endregion

        #region ----私有方法----
        private void ResetContent()
        {
            this.title.text = "";
            this.content.text = "";
            this.yesLabel.text = "";
            this.noLabel.text = "";
            this.yesAction = null;
        }

        private void Show(bool display)
        {
            gameObject.SetActive(display);
        }
        #endregion
    }
}