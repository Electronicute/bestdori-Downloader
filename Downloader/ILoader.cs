/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2021-01-01
* 作用描述：	#加载器
***************************************************/

using System;
namespace Utils
{
	public interface ILoader : IPoolable, IRecyclable
	{
		string Name { get; set; }

		float Progress();

		void Add(string[] urls, ResType resType, Action<bool, IRes> listener, bool autoStart = false);

		void RegisterEvent(Action<string> onAllCompleted);

		void StartAll();

		void StopAll();

		void ReleaseAll();
	}
}