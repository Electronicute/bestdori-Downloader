/**************************************************
* 创建作者：	咕咕咕
* 创建时间：	2020-12-17
* 作用描述：	#JSON解析器
***************************************************/

using UnityEngine;
using LitJson;

namespace Live2DCharacter.JSON
{
	public class JsonParser
	{
		#region ----公有方法----
		public static JsonData GetData(string jsonStr)
        {
			return JsonMapper.ToObject(jsonStr);
        }

		public static string GetJson(object obj)
        {
			return JsonMapper.ToJson(obj);
        }

		
		#endregion
	}
}