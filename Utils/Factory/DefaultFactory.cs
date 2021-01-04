/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-31
* 作用描述：	#用默认构造函数创建对象的工厂
***************************************************/

namespace Utils
{
	public class DefaultFactory<T> : IFactory<T> where T : new()
	{
		public T Create()
        {
			return new T();
        }
	}
}