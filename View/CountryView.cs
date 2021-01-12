/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2021-01-11
* 作用描述：	#
***************************************************/

using System;
using UnityEngine;

namespace Live2DCharacter
{
	public class CountryView : MonoBehaviour
	{
		#region ----字段----
		[SerializeField] private int index;
        private Action<int> onSelected;
        #endregion

        #region ----公有方法----
        public void Selected()
        {
            onSelected?.Invoke(index);
        }

        public void RegisterEvent(Action<int> action)
        {
            onSelected += action;
        }
        #endregion
    }
}