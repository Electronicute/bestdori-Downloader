/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-31
* 作用描述：	#线程安全的单例泛型对象池
***************************************************/

using System.Collections.Generic;

namespace Utils
{
    public class ObjectPool<T> : Pool<T> where T : IPoolable, IRecyclable, new()
    {
        #region ----单例----
        private static ObjectPool<T> instance;
        private static readonly object lockOjb = new object();

        public static ObjectPool<T> Instance
        {
            get
            {
                lock (lockOjb)
                {
                    if (instance == null)
                    {
                        instance = new ObjectPool<T>();
                    }

                    return instance;
                }
            }
        }
        #endregion

        #region ----构造方法----
        private ObjectPool()
        {
            factory = new DefaultFactory<T>();
            maxCount = 10;                      //初始化数量：10
            stack = new Stack<T>(maxCount);           
            for (int i = 0; i < maxCount; i++)
            {
                Recycle(factory.Create());
            }
        }
        #endregion

        #region ----实现IPool----
        //从池子里取对象
        public override T Get()
        {
            T obj = base.Get();
            obj.IsRecycled = false;

            return obj;
        }

        //回收对象
        public override bool Recycle(T obj)
        {
            if (obj == null || obj.IsRecycled)
            {
                return false;                   //null或已经回收的，回收失败
            }

            if (stack.Count == maxCount)
            {
                obj.OnRecycled();               //池子满了，丢弃，回收失败
                return false;
            }

            obj.IsRecycled = true;              //回收成功
            obj.OnRecycled();
            stack.Push(obj);

            return true;
        }
        #endregion

    }
}