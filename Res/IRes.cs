/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-18
* 作用描述：	#下载的资源接口
***************************************************/

namespace Live2DCharacter
{
	public interface IRes : IRequestTask, IWriteTask
	{
		string Url { get; }

		void Release();

		bool NeedDownload { get; }
	}
}