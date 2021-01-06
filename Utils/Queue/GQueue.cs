/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-31
* 作用描述：	#循环队列
***************************************************/

using System;
using System.Collections.Generic;

namespace Utils
{
	public class GQueue<T>
	{
		#region ----字段----
		protected Queue<T> datas;
		#endregion

		#region ----属性----
		public int Count => datas.Count;
		#endregion

		#region ----构造方法----
		public GQueue(int capacity = 10)
        {
			datas = new Queue<T>(capacity);
        }
		#endregion

		#region ----公有方法----
		public void Enqueue(T item)
        {
			datas.Enqueue(item);
        }

		public T Dequeue()
        {
            if (datas.Count > 0)
            {
				return datas.Dequeue();
			}

			throw new Exception("队列已经空了！");
		}

		public void Remove(Func<T, bool> func)
        {
			Queue<T> temp = new Queue<T>(datas.Count);
			while(datas.Count > 0)
            {
				T d = datas.Dequeue();
                if (!func(d))
                {
					temp.Enqueue(d);
                }
            }
			datas = temp;
        }

		public void Clear()
        {
			datas.Clear();
        }
		#endregion
	}
}