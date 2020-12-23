/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-18
* 作用描述：	#树节点
***************************************************/

using System.Collections.Generic;

namespace Live2DCharacter
{
    public class TreeNode<T>
    {
        #region ----字段----
        public T Data;
        public TreeNode<T> Parent;
        public List<TreeNode<T>> Childs;
        #endregion

        #region ----构造方法----
        public TreeNode(T data)
        {
            Data = data;
        }

        public TreeNode(T data, TreeNode<T> parent)
        {
            Data = data;
            Parent = parent;
        }
        #endregion

        #region ----公有方法----
        public void AddChild(TreeNode<T> node)
        {
            if (Childs == null)
            {
                Childs = new List<TreeNode<T>>();
            }
            Childs.Add(node);
        }

        public List<TreeNode<T>> GetPath()
        {
            List<TreeNode<T>> path = new List<TreeNode<T>>();
            path.Add(this);
            
            TreeNode<T> curr = Parent;

            while (curr != null)
            {
                path.Add(curr);
                curr = curr.Parent;
            }

            return path;
        }

        public TreeNode<T> FindChild(T t)
        {
            if (Childs != null)
            {
                foreach (var c in Childs)
                {
                    if (c.Data.Equals(t))
                    {
                        return c;
                    }
                }
            }

            return null;
        }

        public override string ToString()
        {
            return Data.ToString();
        }
        #endregion
    }
}