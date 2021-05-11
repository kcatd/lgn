using KitsuneAPI.KitsuneUnity;
using UnityEditor;
using UnityEngine.UIElements;

namespace KitsuneAPI.KitsuneEditor.Editor.CustomData
{
	[CustomEditor(typeof(KitsuneCustomDataList))]
	public class KitsuneCustomDataEditor : KitsuneBaseEntityListInspector<KitsuneCustomData, KitsuneCustomDataList>
	{
		public override VisualElement CreateInspectorGUI()
		{
			_description = "Create and manage custom data for your game and entities.";

			CreateRootElement(false);

			return _rootElement;
		}

		protected override bool ValidateDependencyRequirements()
		{
			// no dependencies
			return true;
		}
	}
}