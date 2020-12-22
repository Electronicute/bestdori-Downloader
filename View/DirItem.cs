/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-19
* 引擎版本：	2020.1.17f1c1
* 作用描述：	#
***************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Live2DCharacter
{
    public class DirItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        #region ----字段----
        private int index;
        public Text text;
        private Action<int> selected;
        private Action<int> hoverd;
        #endregion

        #region ----MonoBehaviour----
        public void OnPointerEnter(PointerEventData eventData)
        {
            hoverd?.Invoke(index);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            selected?.Invoke(index);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hoverd?.Invoke(-1);
        }
        #endregion

        #region ----公有方法----
        public void SetIndex(int index) => this.index = index;

        public void SetText(string str) => text.text = str;

        public void Show(bool show) => gameObject.SetActive(show);

        public void Register(Action<int> onSelect, Action<int> onHover)
        {
            hoverd += onHover;
            selected += onSelect;
        }
        #endregion
    }
}