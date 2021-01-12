/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2021-01-04
* 作用描述：	#
***************************************************/

using System.Collections.Generic;
using Live2DCharacter;
using UnityEngine;

namespace Utils
{
	public class ProgressController
	{
		#region ----字段----
		private Queue<ILoader> loaders;
		private List<LoaderView> views;
		private int showCount = 6;
		#endregion

		#region ----构造方法----
		public ProgressController()
        {
			loaders = new Queue<ILoader>(5);
			views = new List<LoaderView>(5);
			MonoObjectPool<ProgressView>.Instance.SetFactory(new CustomFactory<ProgressView>(CreateView));
        }
		#endregion

		protected class LoaderView
        {
			public ILoader Loader { get;}
			public ProgressView View { get;}

			public LoaderView(ILoader loader, ProgressView view)
            {
				Loader = loader;
				View = view;
            }
        }

		#region ----公有方法----
		public void AddLoader(ILoader loader)
        {
			if (views.Count < showCount)
            {
				ProgressView v = MonoObjectPool<ProgressView>.Instance.Get();
				LoaderView lv = new LoaderView(loader, v);
				v.SetName(loader.Name);
				v.cancelBtn.onClick.AddListener(() => OnClickCancelBtn(loader.Name));
				v.Register(loader.Progress);
				loader.StartAll();
				v.Show(views.Count);
				views.Add(lv);
			}
            else
            {
				loaders.Enqueue(loader);
			}
		}

		public void LoaderFinish(string name)
        {
			LoaderView lv;
            for (int i = views.Count - 1; i >= 0; i--)
            {
				lv = views[i];
                if (lv.Loader.Name == name)
                {
					ProgressView pv = lv.View;
					views.RemoveAt(i);
					pv.Remove(() => { Move(); TryNext(); lv.Loader.Recycle(); });
					break;
				}
            }
        }

		public bool IsLoading()
        {
            if (loaders.Count > 0)
            {
				return true;
            }
            foreach (var v in views)
            {
                if (v.Loader.Progress() < 1)
                {
					return true;
                }
            }
			return false;
        }

		public void StopAll()
        {
            while (loaders.Count > 0)
            {
				loaders.Dequeue().StopAll();
            }
			LoaderView lv;
			ProgressView pv;
			for (int i = views.Count - 1; i >= 0; i--)
			{
				lv = views[i];

				pv = lv.View;
				pv.cancelBtn.gameObject.SetActive(false);
				pv.Stop();
				lv.Loader.StopAll();
				pv.Remove(null, 0);
			}
			views.Clear();
		}
		#endregion

		#region ----私有方法----
		private ProgressView CreateView()
        {
			GameObject go = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/ProgressView"), GameObject.FindGameObjectWithTag("ProgressView").transform);
			ProgressView pv = go.GetComponent<ProgressView>();
			pv.gameObject.SetActive(false);

			return pv;
        }

		private void TryNext()
        {
            if (loaders.Count > 0)
            {
				ProgressView pv = MonoObjectPool<ProgressView>.Instance.Get();
				ILoader loader = loaders.Dequeue();
				pv.SetName(loader.Name);
				pv.cancelBtn.onClick.AddListener(() => OnClickCancelBtn(loader.Name));
				pv.Register(loader.Progress);
				loader.StartAll();
				pv.Show(views.Count);
				LoaderView lv = new LoaderView(loader, pv);
				views.Add(lv);
            }
        }

		private void Move()
        {
			int i = 0;
			foreach (var p in views)
			{
				p.View.SetIndex(i++);
			}
		}

		private void OnClickCancelBtn(string name)
        {
			LoaderView lv;
			for (int i = views.Count - 1; i >= 0; i--)
			{
				lv = views[i];
				if (lv.Loader.Name == name)
				{
					ProgressView pv = lv.View;
					pv.cancelBtn.gameObject.SetActive(false);
					pv.Stop();
					lv.Loader.StopAll();
					views.RemoveAt(i);
					pv.Remove(() => { Move(); TryNext(); }, 0.2f);
					break;
				}
			}
		}
		#endregion

	}
}