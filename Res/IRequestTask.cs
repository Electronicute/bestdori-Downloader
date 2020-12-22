/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-18
* 作用描述：	#请求接口
***************************************************/

using System;
using System.Collections;

namespace Live2DCharacter
{
	public interface IRequestTask
	{
		IEnumerator SendRequest(Action<IRes> finish);
	}
}