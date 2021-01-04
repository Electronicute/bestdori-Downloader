/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-31
* 作用描述：	#请求接口
***************************************************/

using System;
using System.Collections;

namespace Utils
{
	public interface IRequestTask
	{
		IEnumerator SendRequest(Action onFinish);

		void StopRequest();
	}
}