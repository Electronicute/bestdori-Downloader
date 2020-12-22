/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-18
* 引擎版本：	2020.1.17f1c1
* 作用描述：	#
***************************************************/

using UnityEngine;
using UnityEngine.UI;

namespace Live2DCharacter 
{
    public class DownloadView : MonoBehaviour
    {
        #region ----字段----
        public DebugView debug;
        public Text dlPath;
        public Button changePathBtn;
        public Button pathBtn;
        public GameObject jsonTreeGo;
        public Text jsonTreeNode;
        public Button dlBtn;
        public Button rtBtn;
        #endregion

        #region ----公有方法----
        public void ShowDebug(string msg, ColorView color)
        {
            debug.Show(msg, color);
        }

        public void SetDlPath(string path) => dlPath.text = path;

        public void ShowJsonTree(bool show) => jsonTreeGo.SetActive(show);

        public void SetTreeNode(string nodeStr) => jsonTreeNode.text = nodeStr;

        public void ShowDlBtn(bool show) => dlBtn.gameObject.SetActive(show);

        public void ShowReturnBtn(bool show) => rtBtn.gameObject.SetActive(show);
        #endregion
    }
}
