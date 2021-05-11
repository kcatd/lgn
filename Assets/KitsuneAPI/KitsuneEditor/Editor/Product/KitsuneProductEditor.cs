using KitsuneAPI.KitsuneEditor.Editor.Reward;
using KitsuneAPI.KitsuneUnity;
using UnityEditor;
using UnityEngine.UIElements;

namespace KitsuneAPI.KitsuneEditor.Editor.Product
{
	[CustomEditor(typeof(KitsuneProductList))]
	public class KitsuneProductEditor : KitsuneBaseEntityListInspector<KitsuneProduct, KitsuneProductList>
	{
		public override VisualElement CreateInspectorGUI()
		{
			_description = "Create and manage IAP's for your game.";
			_dependencyRequirementText = "You must create a Reward before creating Products.";

			CreateRootElement();
			
			return _rootElement;
		}

		protected override VisualElement AddEntityToList(KitsuneProduct entity)
		{
			VisualElement listItem = base.AddEntityToList(entity);
			listItem.Q<TextElement>("Price").text = (entity.Price / 100m).ToString("$#0.00") ;
			listItem.Q<TextElement>("Reward").text = entity.RewardToPurchase.Name;
			
			return listItem;
		}

		protected override void UpdateNonBindableData(KitsuneProduct entity, VisualElement listItem)
		{
			listItem.Q<TextElement>("Price").text = (entity.Price / 100m).ToString("$#0.00") ;
			listItem.Q<TextElement>("Reward").text = entity.RewardToPurchase.Name;
		}

		protected override bool ValidateDependencyRequirements()
		{
			KitsuneRewardList rewards = KitsuneEditorWindow.GetOrCreateListAsset<KitsuneReward, KitsuneRewardList>();
			if (rewards.list.Count > 0)
			{
				return true;
			}

			return false;
		}
	}
}