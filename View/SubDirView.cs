/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-19
* 引擎版本：	2020.1.17f1c1
* 作用描述：	#
***************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Live2DCharacter
{
    public class SubDirView : MonoBehaviour
    {
        #region ----字段----
        public GameObject subDirItem;
        public Transform itemParent;
        public Text dirname;
        private Action<int> selected;
        private readonly DirItem[] items = new DirItem[100];
        private Action onShowFinish;
        private bool isShowFinish;
        public Button showLiveBtn;
        public Button nextBtn;
        public Button preBtn;
        public Text pageLabel;
        #endregion

        #region ----MonoBehaviour----
        void Awake()
        {
	    }
        #endregion

        #region ----公有方法----
        public void InitDirItme(int count)
        {
            DirItem item;
            GameObject go;
            for (int i = 0; i < count; i++)
            {
                go = Instantiate(subDirItem, itemParent, false) as GameObject;
                item = go.GetComponent<DirItem>();
                items[i] = item;
                item.Show(false);
                item.Register(OnSelected);
            }
        }

        public void RegisterSelect(Action<int> action) => selected += action;

        public void RegisterShowFinish(Action action) => onShowFinish += action;

        public void SetDirName(string name) => dirname.text = name;

        public void ShowDirItems(List<string> dirs, List<NodeDataState> states, int currPage, int maxPage)
        {
            isShowFinish = false;
            ShowDirNormal(dirs, states, currPage, maxPage);
        }

        public void UpdateItem(string name, NodeDataState state)
        {
            foreach (var im in items)
            {
                if (im.GetText() == name)
                {
                    im.SetState(state);
                    break;
                }
            }
        }

        public void Show(bool show) => gameObject.SetActive(show);

        public void ShowLiveBtn(bool show) => showLiveBtn.gameObject.SetActive(show);
        #endregion

        #region ----私有方法----
        private void OnSelected(int index)
        {
            if (isShowFinish)
            {
                selected?.Invoke(index);
            }
        }

        void ShowDirNormal(List<string> dirs, List<NodeDataState> states, int currPage, int maxPage)
        {
            DirItem item;
            for (int i = 0; i < items.Length; i++)
            {
                item = items[i];
                if (dirs != null && dirs.Count > i)
                {
                    item.SetIndex(i);
                    item.SetText(dirs[i]);
                    item.SetState(states[i]);
                    item.Show(true);
                }
                else
                {
                    item.Show(false);
                }
            }
            onShowFinish?.Invoke();
            isShowFinish = true;
            ShowPage(dirs != null && dirs.Count > 0);
            preBtn.interactable = (currPage > 1);
            nextBtn.interactable = (currPage < maxPage);
            pageLabel.text = currPage + "/" + maxPage;
        }

        void ShowPage(bool show)
        {
            nextBtn.gameObject.SetActive(show);
            preBtn.gameObject.SetActive(show);
            pageLabel.gameObject.SetActive(show);
        }
        #endregion
    }
}