/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-31
* 作用描述：	#请求接口
***************************************************/

using System;
using System.Collections;
using UnityEngine;

namespace Utils
{
	public interface IRequestTask
	{
		IEnumerator SendRequest(Action onFinish);

		byte ErrorCount { get; }

		Coroutine Coroutine { get; set; }

		ResState State { get; set; }

		void StopRequest();
	}
}