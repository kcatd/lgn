using KitsuneAPI.KitsuneUnity;
using UnityEditor;
using UnityEngine.UIElements;

namespace KitsuneAPI.KitsuneEditor.Editor.Currency
{
	[CustomEditor(typeof(KitsuneCurrencyList))]
	public class KitsuneCurrencyEditor : KitsuneBaseEntityListInspector<KitsuneCurrency, KitsuneCurrencyList>
	{
		public override VisualElement CreateInspectorGUI()
		{
			_description = "Create and manage virtual currencies for your game.";

			CreateRootElement();
			
			return _rootElement;
		}

		protected override bool ValidateDependencyRequirements()
		{
			// no dependencies
			return true;
		}
	}
}