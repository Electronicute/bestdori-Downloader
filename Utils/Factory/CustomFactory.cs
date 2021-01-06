/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2021-01-04
* 作用描述：	#自定义工厂
***************************************************/

using System;

namespace Utils
{
    public class CustomFactory<T> : IFactory<T>
    {
        #region ----字段----
        protected Func<T> customFunc;
        #endregion

        #region ----构造方法----
        public CustomFactory(Func<T> func)
        {
            customFunc = func;
        }
        #endregion

        #region ----实现IFactory----
        public T Create()
        {
            return customFunc.Invoke();
        }
        #endregion
    }
}