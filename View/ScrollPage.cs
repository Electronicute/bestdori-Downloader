/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2021-01-08
* 作用描述：	#
***************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Live2DCharacter
{
    [RequireComponent(typeof(ScrollRect))]
	public class ScrollPage : MonoBehaviour
	{
        #region ----字段----
        [SerializeField] private RectTransform contentTrans;
        [SerializeField] private RectTransform topTrans;
        [SerializeField] private RectTransform bottomTrans;

        private int size;
        private int count;
        private int perPageCount;
        private int showCount;
        private int perPageSize;
        private int currPage;
        private int tempPage;
        private Func<int, bool> onChanged;
        #endregion

        #region ----Mono----
        private void Awake()
        {
            enabled = false;
        }

        private void Update()
        {
            tempPage = UpdatePage();
            if (tempPage >= 0 && tempPage != currPage)
            {
                if (onChanged != null && onChanged.Invoke(tempPage))
                {
                    ResetPage();
                }
            }
        }
        #endregion

        #region ----公有方法----
        public void InitData(int size, int perPageCount, int showCount, int count)
        {
            this.size = size;
            this.count = count;
            this.showCount = showCount;
            this.perPageCount = perPageCount;
            perPageSize = size * perPageCount;
            currPage = 0;
            enabled = true;
            bottomTrans.SetAsLastSibling();
        }

        public void Reset(int showCount, int count)
        {
            this.count = count;
            this.showCount = showCount;
            bottomTrans.SetAsLastSibling();
            tempPage = 0;
            if (onChanged != null && onChanged.Invoke(0))
            {
                ResetPage();
            }
        }

        public void RegisterChanged(Func<int, bool> action)
        {
            if (action != null)
            {
                onChanged += action;
            }
        }
        #endregion

        #region ----私有方法----
        private int Check(int num)
        {
            if (num < 0)
            {
                return 0;
            }
            return num;
        }

        private void ResetPage()
        {
            currPage = tempPage;
            int topCount = Check(currPage) * perPageCount;
            topTrans.sizeDelta = new Vector2(topTrans.sizeDelta.x, topCount * size);
            bottomTrans.sizeDelta = new Vector2(topTrans.sizeDelta.x, Check(count - topCount - showCount) * size);
        }

        private int UpdatePage()
        {
            return Check(Mathf.CeilToInt((contentTrans.anchoredPosition.y - perPageSize) / perPageSize));
        }
        #endregion

    }
}