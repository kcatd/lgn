using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace KitsuneAPI.KitsuneEditor.Editor.Item
{
	public class ItemCategoryList : ScriptableObject
	{ 
		[FormerlySerializedAs("list")] public List<string> categories;
	}
}