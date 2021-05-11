using KitsuneAPI.KitsuneEditor.Editor.Reward;
using KitsuneAPI.KitsuneUnity;
using UnityEditor;
using UnityEngine.UIElements;

namespace KitsuneAPI.KitsuneEditor.Editor.Achievement
{
	[CustomEditor(typeof(KitsuneAchievementList))]
	public class KitsuneAchievementEditor : KitsuneBaseEntityListInspector<KitsuneAchievement, KitsuneAchievementList>
	{
		public override VisualElement CreateInspectorGUI()
		{
			_description = "Create and manage Achievements for your game.";
			_dependencyRequirementText = "You must create a Reward before creating Achievements.";

			CreateRootElement();
			
			return _rootElement;
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