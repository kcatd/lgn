using System;
using System.Collections.Generic;
using UnityEngine;

namespace KitsuneAPI.KitsuneEditor.Editor
{
	[Serializable]
	public class KitsuneBaseEntityList<T> : ScriptableObject
	{
		public List<T> list;
	}
}