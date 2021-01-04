/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-31
* 作用描述：	#可被放入对象池的
***************************************************/

namespace Utils
{
	public interface IPoolable
	{
		bool IsRecycled { get; set; }

		void OnRecycled();
	}
}