/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2021-01-02
* 作用描述：	#
***************************************************/

namespace Utils
{
	public class ResFactory
	{
		public static IRes Create(string url, ResType type)
        {
            switch (type)
            {
                case ResType.Json:
                    return Res.Get<JsonRes>(url);
                case ResType.DownloadRes:
                    return Res.Get<AssetRes>(url);
            }

            return null;
        }
	}
}