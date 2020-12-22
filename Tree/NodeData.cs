/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-20
* 作用描述：	#
***************************************************/

namespace Live2DCharacter
{
	public class NodeData
	{
		#region ----字段----
		private string dName;
		#endregion

		#region ----属性----
		public string Name => dName;
		public bool IsLeaf { get; set; }

        public bool IsDownloaded { get; set; }
		#endregion

		#region ----构造方法----
		public NodeData(string name, bool isLeaf = false, bool isDownloaded = false)
        {
			this.dName = name;
			IsLeaf = isLeaf;
            IsDownloaded = isDownloaded;
        }
        #endregion

        #region ----公有方法----
        public override bool Equals(object data)
        {
            return ToString() == data.ToString();
        }

        public override string ToString()
        {
            return dName;
        }
        #endregion
    }
}