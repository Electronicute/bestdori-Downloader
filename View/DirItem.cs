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
using Utils;

namespace Live2DCharacter
{
    public class DirItem : MonoBehaviour, IPointerClickHandler
    {
        #region ----字段----
        private int index;
        public Text text;
        public Image loaded;
        private Action<int> selected;
        private static Color Gray = new Color(0.4f, 0.4f, 0.4f, 1);
        private static Color Green = Color.green;
        #endregion

        #region ----MonoBehaviour----
        public void OnPointerClick(PointerEventData eventData)
        {
            selected?.Invoke(index);
        }
        #endregion

        #region ----公有方法----
        public void SetData(DirItemData data)
        {
            index = data.Index;
            text.text = data.Name;
            SetState(data.State);
        }

        public string GetText() => text.text;

        public void SetState(NodeDataState state)
        {
            loaded.gameObject.SetActive(false);
            if (state == NodeDataState.Downloaded)
            {
                loaded.gameObject.SetActive(true);
                loaded.color = Green;
            }else if (state == NodeDataState.Loading)
            {
                loaded.gameObject.SetActive(true);
                loaded.color = Gray;
            }
        }

        public void Show(bool show) => gameObject.SetActive(show);

        public void Register(Action<int> onSelect)
        {
            selected += onSelect;
        }
        #endregion
    }
}