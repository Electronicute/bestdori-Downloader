/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-31
* 作用描述：	#抽象基类池
***************************************************/

using System.Collections.Generic;

namespace Utils
{
    public abstract class Pool<T> : IPool<T>
    {
        #region ----字段----
        protected int maxCount;         //池子的容量
        protected IFactory<T> factory;  //对象工厂
        protected Stack<T> stack;       //对象栈
        #endregion

        #region ----实现IPool----
        public virtual T Get()
        {
            if (stack.Count == 0)
            {
                return factory.Create();
            }

            return stack.Pop();
        }

        public abstract bool Recycle(T obj);
        #endregion

    }
}