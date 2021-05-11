using KitsuneAPI.KitsuneUnity;
using UnityEditor;
using UnityEngine.UIElements;

namespace KitsuneAPI.KitsuneEditor.Editor.Currency
{
	[CustomEditor(typeof(KitsuneCurrency))]
	public class KitsuneCurrencyInspector : KitsuneBaseEntityInspector<KitsuneCurrency>
	{
		public override VisualElement CreateInspectorGUI()
		{
			CreateRootElement();

			return _rootElement;
		}
	}
}