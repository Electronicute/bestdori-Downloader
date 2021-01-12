/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-19
* 引擎版本：	2020.1.17f1c1
* 作用描述：	#
***************************************************/

using System;
using Utils;
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
        private const int showCount = 30;
        private readonly List<DirItem> items = new List<DirItem>(showCount);
        private Action onShowFinish;
        private bool isShowFinish;
        public Button showLiveBtn;
        public Button openLocalBtn;
        public ScrollPage page;
        private List<DirItemData> datas;
        private int perPageCount;
        #endregion

        #region ----公有方法----
        public void InitDirItme()
        {
            GameObject go;
            DirItem item;
            for (int i = 0; i < showCount; i++)
            {
                go = Instantiate(subDirItem, itemParent, false) as GameObject;
                item = go.GetComponent<DirItem>();
                item.Show(false);
                item.Register(OnSelected);
                items.Add(item);
            }
            perPageCount = showCount / 2;
            page.InitData(24, perPageCount, showCount, showCount);
            page.RegisterChanged(OnPageChanged);
        }

        public void RegisterSelect(Action<int> action) => selected += action;

        public void RegisterShowFinish(Action action) => onShowFinish += action;

        public void SetDirName(string name) 
        {
            dirname.text = name;
        }

        public void ShowDirItems(List<DirItemData> dirs)
        {
            datas = dirs;
            isShowFinish = false;
            page.Reset(showCount, datas.Count);
            onShowFinish?.Invoke();
            isShowFinish = true;
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

        public void ShowLocalBtn(bool show) => openLocalBtn.gameObject.SetActive(show);
        #endregion

        #region ----私有方法----
        private void OnSelected(int index)
        {
            if (isShowFinish)
            {
                selected?.Invoke(index);
            }
        }

        private bool OnPageChanged(int page)
        {
            if (page == 0)
            {
                ShowPage(page);
                return true;
            }
            if (page * perPageCount >= datas.Count)
            {
                return false;
            }
            ShowPage(page);
            return true;
        }

        private void ShowPage(int page)
        {
            int startIndex = page * perPageCount;
            DirItem item;
            for (int i = 0; i < showCount; i++)
            {
                item = items[i];
                if (datas.Count > startIndex)
                {
                    item.SetData(datas[startIndex]);
                    item.Show(true);
                }
                else
                {
                    item.Show(false);
                }
                startIndex++;
            }
        }
        #endregion
    }
}