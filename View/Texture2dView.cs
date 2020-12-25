/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-25
* 引擎版本：	2020.1.17f1c1
* 作用描述：	#
***************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Live2DCharacter
{
    public class Texture2dView : MonoBehaviour
    {
        #region ----字段----
        public RawImage image;
        public Button closeBtn;
        #endregion

        #region ----公有方法----
        public void Show(Texture2D texture)
        {
            gameObject.SetActive(true);
            image.texture = texture;
            if (texture.width > Screen.width || texture.height > Screen.height)
            {
                float tRatio = texture.width / (float)texture.height;
                float sRatio = Screen.width / (float)Screen.height;
                RectTransform rt = image.GetComponent<RectTransform>();
                if (tRatio > sRatio)
                {
                    rt.sizeDelta = new Vector2(Screen.width, Screen.width / tRatio);
                }
                else
                {
                    rt.sizeDelta = new Vector2(Screen.height * tRatio, Screen.height);
                }
                return;
            }
            image.SetNativeSize();
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
        #endregion
    }
}