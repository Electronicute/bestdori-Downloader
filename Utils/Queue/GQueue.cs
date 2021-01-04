/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-31
* 作用描述：	#循环队列
***************************************************/

using System;

namespace Utils
{
	public class GQueue<T>
	{
		#region ----字段----
		protected int maxCount;
		protected int headIdx;
		protected int tailIdx;
		protected int length;
		protected T[] datas;
		#endregion

		#region ----属性----
		public int Length => length;
		#endregion

		#region ----构造方法----
		public GQueue(int capacity = 10)
        {
			maxCount = capacity;
			datas = new T[maxCount];
        }
		#endregion

		#region ----公有方法----
		public void Enqueue(T item)
        {
			length++;
            if (length > maxCount)
            {
				Dilatation();
            }
			if (tailIdx >= maxCount)
			{
				tailIdx %= maxCount;
			}
			datas[tailIdx] = item;
			tailIdx++;
        }

		public T Dequeue()
        {
            if (length > 0)
            {
				length--;
				if (headIdx >= maxCount)
				{
					headIdx %= maxCount;
				}
				T data = datas[headIdx];
				datas[headIdx++] = default(T);

				return data;
			}

			throw new Exception("队列已经空了！");
		}

		public void Clear()
        {
            for (int i = 0; i < datas.Length; i++)
            {
				datas[i] = default(T);
            }
        }
		#endregion

		#region ----私有方法----
		private void Dilatation()
        {
			maxCount *= 2;
			T[] temp = new T[maxCount];
			datas.CopyTo(temp, 0);
			datas = temp;
        }
		#endregion

	}
}