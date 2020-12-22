/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-18
* 作用描述：	#写入接口
***************************************************/

using System;
using System.Collections;

namespace Live2DCharacter
{
	public interface IWriteTask
	{
		IEnumerator Write(Action finish);
	}
}