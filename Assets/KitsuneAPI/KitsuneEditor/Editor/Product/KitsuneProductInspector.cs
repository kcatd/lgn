using KitsuneAPI.KitsuneUnity;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace KitsuneAPI.KitsuneEditor.Editor.Product
{
	[CustomEditor(typeof(KitsuneProduct))]
	public class KitsuneProductInspector : KitsuneBaseEntityInspector<KitsuneProduct>
	{
		public override VisualElement CreateInspectorGUI()
		{
			CreateRootElement();

			_rootElement.Q<ObjectField>("Reward").objectType = typeof(KitsuneReward);

			return _rootElement;
		}
	}
}