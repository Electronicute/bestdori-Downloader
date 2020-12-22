/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-18
* 引擎版本：	2020.1.17f1c1
* 作用描述：	#显示Debug
***************************************************/

using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Live2DCharacter
{
    public class DebugView : MonoBehaviour
    {
        #region ----字段----
        public Text text;
        private readonly StringBuilder sb = new StringBuilder();
        private const int maxSize = 100;
        private readonly Queue<int> msgQueue = new Queue<int>(maxSize);
        #endregion

        #region ----公有方法----
        public void Show(string msg, ColorView color = ColorView.Default)
        {
            if (msgQueue.Count == maxSize)
            {
                sb.Remove(0, msgQueue.Dequeue());
            }
            string nmsg = '\n' + AddColor(msg, color);
            msgQueue.Enqueue(nmsg.Length);
            sb.Append(nmsg);
            text.text = sb.ToString();
        }
        #endregion

        #region ----私有方法----
        private string AddColor(string msg, ColorView color)
        {
            switch (color)
            {
                case ColorView.Default:
                    return $"<color=#FF7EF8>{msg}</color>";
                case ColorView.Green:
                    return $"<color=#00FF00>{msg}</color>";
                case ColorView.Red:
                    return $"<color=#FF0000>{msg}</color>";
            }

            return msg;
        }
        #endregion
    }
}
