/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-24
* 引擎版本：	2020.1.17f1c1
* 作用描述：	#
***************************************************/

using UnityEngine;

namespace Live2DCharacter
{
    public class SpectrumView : MonoBehaviour
    {
        #region ----字段----
        public Vector2 screenPos;
        public float scale = 1;
        [SerializeField] private Color startColor = Color.cyan;
        [SerializeField] private Color endColor = Color.magenta;

        private Material lineMaterial;
        private float[] spectrumData = new float[1024];
        private Vector3 startPos;
        private float space;
        private int length;
        private Camera cam;
        private float startX;
        private float camHalfView;
        #endregion

        #region ----MonoBehaviour----
        private void Start()
        {
            cam = Camera.main;
            //转换成相对位置
            transform.localScale = Vector3.one;
            transform.rotation = cam.transform.rotation;
            transform.position = cam.transform.position;
            camHalfView = Mathf.Tan(Mathf.Deg2Rad * (cam.fieldOfView / 2)) * 10;

            lineMaterial = new Material(Shader.Find("UI/Unlit/Transparent"));
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
            Show(false);
        }

        private void Update()
        {
            float width = camHalfView * scale;
            float per = width * 2 / Screen.width;
            float offsetX = per * screenPos.x;
            float offsetY = per * screenPos.y;
            length = spectrumData.Length / 12;
            startX = -width / 2;
            space = width / length;
            startPos = new Vector3(startX + offsetX, offsetY, 10);
            AudioListener.GetSpectrumData(spectrumData, 0, FFTWindow.Blackman);
        }

        private void OnRenderObject()
        {
            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(transform.localToWorldMatrix);
            GL.Begin(GL.QUADS);
            float data;
            float data2;
            for (int i = 0; i < length - 1; ++i)
            {
                data = spectrumData[i];
                data2 = spectrumData[i + 1];
                data = data > 0.1f ? 0.1f : data;
                data2 = data2 > 0.1f ? 0.1f : data2;
                GL.Color(startColor);
                GL.Vertex3(startPos.x + i * space, startPos.y, startPos.z);
                GL.Vertex3(startPos.x + (i + 1) * space, startPos.y, startPos.z);
                GL.Color(Color.Lerp(startColor, endColor, (data + data2) / 0.02f));
                GL.Vertex3(startPos.x + (i + 1) * space, startPos.y + data2 * 15, startPos.z);
                GL.Vertex3(startPos.x + i * space, startPos.y + data * 15, startPos.z);
            }
            GL.End();
            GL.PopMatrix();
        }
        #endregion

        #region ----外部接口----
        public void Show(bool show)
        {
            enabled = show;
        }
        #endregion
    }
}