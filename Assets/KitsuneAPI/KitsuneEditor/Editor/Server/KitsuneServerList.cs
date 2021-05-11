using System.Collections.Generic;
using KitsuneAPI.KitsuneUnity;
using UnityEngine;

namespace KitsuneAPI.KitsuneEditor.Editor.Server
{
	public class KitsuneServerList : ScriptableObject
	{
		public List<KitsuneServerSettings> Servers;
	}
}