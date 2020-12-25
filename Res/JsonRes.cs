/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-18
* 作用描述：	#
***************************************************/

using System;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine;

namespace Live2DCharacter
{
    public class JsonRes : IRes
    {
        #region ----字段----
        private string url;
        private UnityWebRequest request;
        private string json;
        private Action<TreeNode<NodeData>, string> OnCompleted;
        private TreeNode<NodeData> node;
        #endregion

        #region ----属性----
        string IRes.Url => url;
        bool IRes.NeedDownload => false;
        public string Json => json;
        #endregion

        #region ----构造方法----
        public JsonRes(TreeNode<NodeData> node, string url, Action<TreeNode<NodeData>, string> action)
        {
            this.node = node;
            this.url = url;
            if (action != null)
            {
                OnCompleted += action;
            }
        }
        #endregion

        #region ----实现IRes----
        IEnumerator IRequestTask.SendRequest(Action<IRes> finish)
        {
            using (request = new UnityWebRequest(url))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    AssetDownloader.Instance.Debug($"↑↑失败 {request.error}", ColorView.Red);
                    finish(this);
                    OnCompleted?.Invoke(node,null);
                    request.Dispose();
                    yield break;
                }

                yield return new WaitForEndOfFrame();
                json = DownloadHandlerBuffer.GetContent(request);
                finish(this);
                OnCompleted?.Invoke(node, Json);
                request.Dispose();
            }
        }

        IEnumerator IWriteTask.Write(Action finish)
        {
            finish?.Invoke();
            yield break;
        }

        void IRes.Release()
        {
            url = null;
            request = null;
            json = null;
            OnCompleted = null;
            node = null;
        }
        #endregion
    }
}