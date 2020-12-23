/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-18
* 作用描述：	#树
***************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Live2DCharacter
{
	public class GenericTree<T>
	{
		#region ----字段----
		private TreeNode<T> head;
		#endregion

		#region ----属性----
		public TreeNode<T> Head => head;
		#endregion

		#region ----构造方法----
		public GenericTree(TreeNode<T> head)
        {
			this.head = head;
        }
		#endregion

		#region ----公有方法----
		/// <summary>
		/// 查找
		/// </summary>
		public TreeNode<T> FindNode(T data)
        {
			TreeNode<T> result = null;
			FindNode(data, head, out result);

			return result;
        }

		/// <summary>
		/// 遍历
		/// </summary>
		public void Traverse(Action<TreeNode<T>> action)
        {
			Traverse(head, action);
        }
		#endregion

		#region ----静态方法----
		/// <summary>
		/// 获取指定节点下的所有叶子节点
		/// </summary>
		public static List<TreeNode<T>> GetLeafs(TreeNode<T> node)
		{
			List<TreeNode<T>> leafs = new List<TreeNode<T>>();
			FindLeafs(node, leafs);

			return leafs;
		}

		/// <summary>
		/// 遍历查找匹配Data的子节点
		/// </summary>
		public static bool FindNode(T data, TreeNode<T> node, out TreeNode<T> result)
        {
            if (node.Data.Equals(data))
            {
				result = node;
				return true;
            }
            else if (node.Childs != null)
            {
				foreach (var n in node.Childs)
				{
					if (FindNode(data, n, out result))
					{
						return true;
					}
				}				
			}

			result = null;
			return false;
        }

		/// <summary>
		/// 遍历执行Action
		/// </summary>
		public static void Traverse(TreeNode<T> node, Action<TreeNode<T>> action)
        {
            if (node != null)
            {
				action(node);
            }
            if (node.Childs != null)
            {
                foreach (var n in node.Childs)
                {
					Traverse(n, action);
                }
            }
        }

		private static void FindLeafs(TreeNode<T> node, List<TreeNode<T>> leafs)
        {
			if (node.Childs == null)
			{
				leafs.Add(node);
				return;
			}
			foreach (var n in node.Childs)
			{
				FindLeafs(n, leafs);
			}
		}
		#endregion

	}
}