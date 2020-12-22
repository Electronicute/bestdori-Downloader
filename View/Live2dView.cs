/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-20
* 引擎版本：	2020.1.17f1c1
* 作用描述：	#
***************************************************/

using UnityEngine;
using live2d;
using live2d.framework;
using UnityEngine.UI;
using System.Text;

namespace Live2DCharacter
{
    public class Live2dView : MonoBehaviour
    {
        #region ----字段----
        public int FadeOutTime = 2000;                  //模型渲染后置延时
        public int FadeInTime = 2000;                   //模型渲染前置延时
        public bool EyeFlag = true;                     //眼球是否跟随
        public int AnimationIndex;                      //当前动作位置

        public Text fileNameLabel;
        public RectTransform mocFlag;
        public RectTransform motionFlag;
        public RectTransform textureFlag;
        public Button returnBtn;

        private Live2DModelUnity live2DModel;           //模型
        private Matrix4x4 live2DCanvansPos;             //模型绘制矩阵
        private Live2DMotion[] motions;                 //动作
        private MotionQueueManager motionQueueManager;  //动作队列
        private L2DTargetPoint drag;                    //鼠标跟踪输入

        private float ddx;
        private float ddy;
        #endregion

        #region ----MonoBehaviour----
        void Awake()
        {
            Live2D.init();
            motionQueueManager = new MotionQueueManager();
            drag = new L2DTargetPoint();
        }

        void Update()
        {
            live2DModel.setMatrix(transform.localToWorldMatrix * live2DCanvansPos);
            if (EyeFlag)
                DragMove();
            live2DModel.update();
        }

        private void OnRenderObject()
        {
            live2DModel.draw();
        }
        #endregion

        /// <summary>
        /// 外部初始化入口
        /// </summary>
        public void InitModel(byte[] modelDatas, byte[] animationDatas, Texture2D[] textures)
        {
            enabled = true;
            //载入模型
            live2DModel = Live2DModelUnity.loadModel(modelDatas);

            //载入贴图
            Texture2D texture2D = Resources.Load<Texture2D>("");
            live2DModel.setTexture(0, texture2D);
            for (int i = 0; i < textures.Length; i++) { live2DModel.setTexture(i, textures[i]); }
            float modelWidth = live2DModel.getCanvasWidth();
            live2DCanvansPos = Matrix4x4.Ortho(0, modelWidth, modelWidth, 0, -50, 50);

            //载入动作
            motions = new Live2DMotion[1];
            for (int i = 0; i < motions.Length; i++) { motions[i] = Live2DMotion.loadMotion(animationDatas); }
            motions[0].setLoopFadeIn(true);
            motions[0].setFadeOut(FadeOutTime);
            motions[0].setFadeIn(FadeInTime);
            motions[0].setLoop(true);

            //播放第一个动作
            motionQueueManager.startMotion(motions[0]);
        }

        /*-*/
        private void DragMove()
        {
            Vector3 pos = Input.mousePosition;
            /*--pos.x|pox.y--*/

            ddx = pos.x / (Screen.width) * 2 - 1;//绝对坐标变换
            ddy = pos.y / (Screen.height) * 2 - 1;//绝对坐标变换

            if (Input.GetMouseButton(0)) { drag.Set(ddx, ddy); }
            //把屏幕坐标转换成live2D检测的坐标
            else if (Input.GetMouseButtonUp(0)) { drag.Set(0, 0); }
            //点击重设位置
            //参数及时更新，考虑加速度等自然因素,计算坐标，进行逐帧更新
            drag.update();

            if (drag.getX() != 0)
            {
                live2DModel.setParamFloat("PARAM_ANGLE_X", 30 * drag.getX());
                live2DModel.setParamFloat("PARAM_ANGLE_Y", 30 * drag.getY());
                live2DModel.setParamFloat("PARAM_BODY_ANGLE_X", 10 * drag.getX());
                if (EyeFlag == true)
                {
                    //眼睛跟随鼠标移动
                    live2DModel.setParamFloat("PARAM_EYE_BALL_X", drag.getX());
                    live2DModel.setParamFloat("PARAM_EYE_BALL_Y", drag.getY());
                }
                else
                {
                    //眼睛只望向前方
                    live2DModel.setParamFloat("PARAM_EYE_BALL_X", -drag.getX());
                    live2DModel.setParamFloat("PARAM_EYE_BALL_Y", -drag.getY());
                }
            }
        }

        public void ShowReturnBtn(bool show) => returnBtn.gameObject.SetActive(show);

        public void SetFiles(string[] fileNames, int mocIndex, int motionIndex, int textureIndex)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < fileNames.Length - 1; i++)
            {
                sb.Append(fileNames[i] + '\n');
            }
            sb.Append(fileNames[fileNames.Length - 1]);
            fileNameLabel.text = sb.ToString();
            mocFlag.anchoredPosition = new Vector2(mocFlag.anchoredPosition.x, -mocIndex * 20);
            motionFlag.anchoredPosition = new Vector2(motionFlag.anchoredPosition.x, -motionIndex * 20);
            textureFlag.anchoredPosition = new Vector2(textureFlag.anchoredPosition.x, -textureIndex * 20);
        }
    }
}