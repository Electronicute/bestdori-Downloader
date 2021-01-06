/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2021-01-04
* 作用描述：	#
***************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
	public class MonoObjectPool<T> : Pool<T> where T : MonoBehaviour, IPoolable, IRecyclable
	{
        #region ----单例----
        private static MonoObjectPool<T> instance;
        private static readonly object lockOjb = new object();

        public static MonoObjectPool<T> Instance
        {
            get
            {
                lock (lockOjb)
                {
                    if (instance == null)
                    {
                        instance = new MonoObjectPool<T>();
                    }

                    return instance;
                }
            }
        }
        #endregion

        #region ----构造方法----
        private MonoObjectPool() 
        {
            maxCount = 5;
            stack = new Stack<T>(maxCount);
        }
        #endregion

        #region ----公有方法----
        public void SetFactory(IFactory<T> factory)
        {
            this.factory = factory;
            for (int i = 0; i < maxCount; i++)
            {
                stack.Push(factory.Create());
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