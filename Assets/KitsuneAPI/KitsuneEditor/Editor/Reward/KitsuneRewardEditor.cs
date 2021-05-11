using KitsuneAPI.KitsuneEditor.Editor.Currency;
using KitsuneAPI.KitsuneEditor.Editor.Item;
using KitsuneAPI.KitsuneUnity;
using UnityEditor;
using UnityEngine.UIElements;

namespace KitsuneAPI.KitsuneEditor.Editor.Reward
{
	[CustomEditor(typeof(KitsuneRewardList))]
	public class KitsuneRewardEditor : KitsuneBaseEntityListInspector<KitsuneReward, KitsuneRewardList>
	{
		public override VisualElement CreateInspectorGUI()
		{
			_description = "Create and manage Rewards for your game. Rewards are added to Products and Achievements.";
			_dependencyRequirementText = "You must create a either a Virtual Currency or Item before creating Rewards.";

			CreateRootElement(false);
			
			return _rootElement;
		}
		
		protected override bool ValidateDependencyRequirements()
		{
			KitsuneCurrencyList currencies = KitsuneEditorWindow.GetOrCreateListAsset<KitsuneCurrency, KitsuneCurrencyList>();
			KitsuneItemList items = KitsuneEditorWindow.GetOrCreateListAsset<KitsuneItem, KitsuneItemList>();
			if (currencies.list.Count > 0 || items.list.Count > 0)
			{
				return true;
			}

			return false;
		}
	}
}