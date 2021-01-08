/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2021-01-08
* 作用描述：	#
***************************************************/

namespace Live2DCharacter
{
	public struct DirItemData
	{
		#region ----字段----
		public string Name;
		public NodeDataState State;
		public int Index;
        #endregion

        #region ----构造方法----
        public DirItemData(string name, NodeDataState state, int index)
        {
            Name = name;
            State = state;
            Index = index;
        }
        #endregion
    }
}