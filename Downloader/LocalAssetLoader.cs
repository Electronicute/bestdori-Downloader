/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2021-01-04
* 引擎版本：	2020.1.17f1c1
* 作用描述：	#
***************************************************/

using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Live2DCharacter
{
    public class LocalAssetLoader : MonoBehaviour
    {
        #region ----单例----
        private static LocalAssetLoader instance;
        private static readonly object lockOjb = new object();

        public static LocalAssetLoader Instance
        {
            get
            {
                lock (lockOjb)
                {
                    if (instance == null)
                    {
                        GameObject go = new GameObject("LocalAssetLoader");
                        DontDestroyOnLoad(go);
                        instance = go.AddComponent<LocalAssetLoader>();
                    }

                    return instance;
                }
            }
        }
        #endregion

        #region ----属性----

        #endregion

        #region ----MonoBehaviour----
        void Start()
        {
            
	    }

	    void Update()
        {
		    
	    }
        #endregion

        #region ----公有方法----
        public void LoadAudio(string path, string name, Action<AudioClip, string> onCompleted)
        {
            StartCoroutine(LoadAudioAsset(path, name, onCompleted));
        }

        public void LoadTexture(string path, Action<Texture2D> onCompleted)
        {
            StartCoroutine(LoadTextureAsset(path, onCompleted));
        }

        public void LoadLive2d(string[] paths, Action<byte[], byte[], Texture2D> onCompleted)
        {
            StartCoroutine(LoadLive2dAsset(paths, onCompleted));
        }
        #endregion

        #region ----私有方法----
        private IEnumerator LoadAudioAsset(string path, string name, Action<AudioClip, string> onCompleted)
        {
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file:///" + path, AudioType.MPEG);
            yield return request.SendWebRequest();

            onCompleted?.Invoke(DownloadHandlerAudioClip.GetContent(request), name);
            request.Dispose();
        }

        private IEnumerator LoadTextureAsset(string path, Action<Texture2D> onCompleted)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture("file:///" + path);
            yield return request.SendWebRequest();

            onCompleted?.Invoke(DownloadHandlerTexture.GetContent(request));
            request.Dispose();
        }

        private IEnumerator LoadLive2dAsset(string[] paths, Action<byte[], byte[], Texture2D> onCompleted)
        {
            FileStream fs = new FileStream(paths[0], FileMode.Open, FileAccess.Read);
            byte[] bufferMoc = new byte[fs.Length];
            fs.Read(bufferMoc, 0, bufferMoc.Length);
            fs.Close();
            fs.Dispose();

            //animation
            fs = new FileStream(paths[1], FileMode.Open);
            byte[] bufferMotion = new byte[fs.Length];
            fs.Read(bufferMotion, 0, bufferMotion.Length);
            fs.Close();
            fs.Dispose();

            //texture
            UnityWebRequest request = UnityWebRequestTexture.GetTexture("file:///" + paths[2]);
            yield return request.SendWebRequest();

            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            onCompleted?.Invoke(bufferMoc, bufferMotion, texture);
            request.Dispose();
        }
        #endregion
    }
}