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
        private Vector2 pos;
        private Action onShowFinish;
        private bool isShowFinish;
        public Button showLiveBtn;
        public Button nextBtn;
        public Button preBtn;
        public Text pageLabel;
        private bool isLeaf;
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

        public void ShowDirItems(List<string> dirs, List<NodeDataState> states, int currPage, int maxPage, bool isLeaf = false, bool ani = true)
        {
            this.isLeaf = isLeaf;
            isShowFinish = false;
            //if (ani)
            //{
            //    StopAllCoroutines();
            //    StartCoroutine(ShowDir(dirs, states, currPage, maxPage, isLeaf));
            //}
            //else
            {
                ShowDirNormal(dirs, states, currPage, maxPage, isLeaf);
            }
            
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

        IEnumerator ShowDir(List<string> dirs, List<NodeDataState> states, int currPage, int maxPage, bool isLeaf = false)
        {
            DirItem item;
            
            float time = 0.1f / dirs.Count;
            time = Math.Min(time, 0.03f);
            ShowPage(dirs != null && dirs.Count > 0);
            preBtn.interactable = (currPage > 1);
            nextBtn.interactable = (currPage < maxPage);
            pageLabel.text = currPage + "/" + maxPage;
            for (int i = 0; i < items.Length; i++)
            {
                item = items[i];
                if (dirs.Count > i)
                {
                    item.SetIndex(i);
                    item.SetText(dirs[i]);
                    item.SetState(states[i]);
                    item.Show(true);
                    yield return new WaitForSeconds(time);
                }
                else
                {
                    item.Show(false);
                }
            }
            yield return new WaitForEndOfFrame();
            onShowFinish?.Invoke();
            isShowFinish = true;
        }

        void ShowDirNormal(List<string> dirs, List<NodeDataState> states, int currPage, int maxPage, bool isLeaf = false)
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