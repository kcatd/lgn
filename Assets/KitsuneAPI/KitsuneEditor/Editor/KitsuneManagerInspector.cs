using KitsuneAPI.KitsuneUnity;
using UnityEditor;

namespace KitsuneAPI.KitsuneEditor.Editor
{
	[CustomEditor(typeof(KitsuneManager))]
	public class KitsuneManagerInspector : UnityEditor.Editor
	{
		private UnityEditor.Editor _gameSettingsInspector;
		
		public override void OnInspectorGUI()
		{
			EditorWindow.GetWindow(typeof(KitsuneEditorWindow));
		}
	}
}