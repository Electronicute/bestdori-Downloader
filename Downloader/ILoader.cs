/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2021-01-01
* 作用描述：	#加载器
***************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Utils
{
	public interface ILoader : IPoolable, IRecyclable
	{
		string Name { get; set; }

		float Progress { get; }

		void Add(string[] urls, ResType resType, Action<bool, IRes> listener);

		void StopAll();

		void ReleaseAll();
	}
}