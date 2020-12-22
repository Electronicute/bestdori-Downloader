/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-22
* 引擎版本：	2020.1.17f1c1
* 作用描述：	#
***************************************************/

using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Live2DCharacter
{
    public class ProgressView : MonoBehaviour
    {
        #region ----字段----
        public Slider reqSlider;
        public Slider writeSlider;
        public Text reqProgress;
        public Text writeProgress;
        private Func<float[]> onProgress;
        private float reqP;
        private float wrP;
        #endregion

        #region ----MonoBehaviour----
        void Update()
        {
            if (onProgress == null)
            {
                StopAllCoroutines();
                StartCoroutine(Hide());
                return;
            }
            float[] progress = onProgress.Invoke();
            reqP = progress[0] > 1 ? 1 : progress[0];
            wrP = progress[1] > 1 ? 1 : progress[1];
            reqProgress.text = string.Format("{0:F2}%", reqP * 100);
            reqSlider.value = progress[0];
            writeProgress.text = string.Format("{0:F2}%", wrP * 100);
            writeSlider.value = progress[1];

            if (progress[0] >= 1 && progress[1] >= 1)
            {
                StopAllCoroutines();
                StartCoroutine(Hide());
            }
        }
        #endregion

        #region ----公有方法----
        public void Show(bool show)
        {
            if (show)
            {
                StopAllCoroutines();
            }
            gameObject.SetActive(show);
            enabled = show;
        }

        public void Register(Func<float[]> func)
        {
            if (func != null)
            {
                onProgress += func;
            }
        }
        #endregion

        #region ----私有方法----
        IEnumerator Hide()
        {
            enabled = false;
            yield return new WaitForSeconds(3);
            gameObject.SetActive(false);
        }
        #endregion
    }
}