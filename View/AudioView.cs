/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-24
* 引擎版本：	2020.1.17f1c1
* 作用描述：	#
***************************************************/

using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Live2DCharacter
{
    public class AudioView : MonoBehaviour
    {
        #region ----字段----
        [SerializeField] private AudioSource audioPlayer;
        [SerializeField] private Text lengthLabel;
        [SerializeField] private Text curTimeLabel;
        [SerializeField] private Slider progress;
        [SerializeField] private Text nameLabel;
        [SerializeField] private Button PlayBtn;
        [SerializeField] private Button StopBtn;
        [SerializeField] private Button CloseBtn;
        #endregion

        #region ----MonoBehaviour----
        private void Awake()
        {
            Show(false);
        }


        void Update()
        {
            if (audioPlayer.clip != null)
            {
                progress.value = audioPlayer.time / audioPlayer.clip.length;
                curTimeLabel.text = AudioController.Sec2TimeFormat(Mathf.FloorToInt(audioPlayer.time));
            }
	    }
        #endregion

        #region ----公有方法----
        public void Init()
        {
            audioPlayer = GetComponent<AudioSource>();
            lengthLabel = transform.Find("lengthLabel").GetComponent<Text>();
            curTimeLabel = transform.Find("curTimeLabel").GetComponent<Text>();
            progress = transform.Find("progress").GetComponent<Slider>();
            nameLabel = transform.Find("nameLabel").GetComponent<Text>();
            PlayBtn = transform.Find("PlayBtn").GetComponent<Button>();
            StopBtn = transform.Find("StopBtn").GetComponent<Button>();
            CloseBtn = transform.Find("CloseBtn").GetComponent<Button>();
        }

        public void Show(bool show)
        {
            gameObject.SetActive(show);
        }

        public void PlayClip(AudioClip clip, string clipName)
        {
            audioPlayer.clip = clip;
            nameLabel.text = clipName;
            lengthLabel.text = AudioController.Sec2TimeFormat(Mathf.FloorToInt(clip.length));
            audioPlayer.time = 0;
            PlayBtn.gameObject.SetActive(false);
            StopBtn.gameObject.SetActive(true);
            audioPlayer.Play();
        }

        public void OnChangePlayTime(float progress)
        {
            float time = audioPlayer.clip.length * progress;
            audioPlayer.time = time;
        }

        public void Play(bool play)
        {
            if (play)
            {
                PlayBtn.gameObject.SetActive(false);
                StopBtn.gameObject.SetActive(true);
                audioPlayer.UnPause();
            }
            else
            {
                PlayBtn.gameObject.SetActive(true);
                StopBtn.gameObject.SetActive(false);
                audioPlayer.Pause();
            }
        }

        public void RegisterPlayBtn(UnityAction actoin)
        {
            PlayBtn.onClick.AddListener(actoin);
        }

        public void RegisterStopBtn(UnityAction action)
        {
            StopBtn.onClick.AddListener(action);
        }

        public void RegisterCloseBtn(UnityAction action)
        {
            CloseBtn.onClick.AddListener(action);
        }

        public void RegisterSlider(UnityAction<float> action)
        {
            progress.onValueChanged.AddListener(action);
        }
        #endregion
    }
}