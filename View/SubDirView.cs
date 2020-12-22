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
        public RectTransform flag;
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
            pos = flag.anchoredPosition;
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
                item.Register(OnSelected, OnHoverd);
            }
        }

        public void RegisterSelect(Action<int> action) => selected += action;

        public void RegisterShowFinish(Action action) => onShowFinish += action;

        public void SetDirName(string name) => dirname.text = name;

        public void ShowDirItems(List<string> dirs, int currPage, int maxPage, bool isLeaf = false, bool ani = true)
        {
            this.isLeaf = isLeaf;
            isShowFinish = false;
            flag.gameObject.SetActive(false);
            if (ani)
            {
                StopAllCoroutines();
                StartCoroutine(ShowDir(dirs, currPage, maxPage, isLeaf));
            }
            else
            {
                ShowDirNormal(dirs, currPage, maxPage, isLeaf);
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

        private void OnHoverd(int index)
        {
            if (index == -1 || !isShowFinish || isLeaf)
            {
                flag.gameObject.SetActive(false);
                return;
            }
            flag.gameObject.SetActive(true);
            flag.anchoredPosition = new Vector2(pos.x, pos.y - 24 * index);
        }

        IEnumerator ShowDir(List<string> dirs, int currPage, int maxPage, bool isLeaf = false)
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
            flag.gameObject.SetActive(!isLeaf && dirs.Count > 0);
        }

        void ShowDirNormal(List<string> dirs, int currPage, int maxPage, bool isLeaf = false)
        {
            DirItem item;
            for (int i = 0; i < items.Length; i++)
            {
                item = items[i];
                if (dirs != null && dirs.Count > i)
                {
                    item.SetIndex(i);
                    item.SetText(dirs[i]);
                    item.Show(true);
                }
                else
                {
                    item.Show(false);
                }
            }
            onShowFinish?.Invoke();
            isShowFinish = true;
            flag.gameObject.SetActive(!isLeaf && dirs.Count > 0);
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