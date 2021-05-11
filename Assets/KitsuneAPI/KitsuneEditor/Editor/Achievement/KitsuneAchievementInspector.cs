using System.Collections.Generic;
using KitsuneAPI.KitsuneEditor.Editor.UI.UIElements;
using KitsuneAPI.KitsuneUnity;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace KitsuneAPI.KitsuneEditor.Editor.Achievement
{
	[CustomEditor(typeof(KitsuneAchievement))]
	public class KitsuneAchievementInspector : KitsuneBaseEntityInspector<KitsuneAchievement>
	{
		private List<string> AllTypes = new List<string>
		{
			"score",
			"level",
			"game_stat",
			"custom",
		};
			
		public override VisualElement CreateInspectorGUI()
		{
			CreateRootElement();
			
			VisualElement typesContainer = _rootElement.Query<VisualElement>("ObjectiveType");
			string selectedType = serializedObject.FindProperty("_objectiveType").stringValue;
			int index = AllTypes.IndexOf(selectedType);
			PopupField<string> typesPopup = new PopupField<string>(AllTypes, AllTypes[index >= 0 ? index : 0]);
			typesPopup.AddToClassList("achievement-inspector--type-popup");
			typesPopup.label = "Objective";
			typesContainer.Add(typesPopup);
			typesPopup.RegisterCallback<ChangeEvent<string>>(OnTypeSelected);

			_rootElement.Q<ObjectField>("Precondition").objectType = typeof(KitsuneAchievement);
			_rootElement.Q<ObjectField>("Reward").objectType = typeof(KitsuneReward);
			
			return _rootElement;
		}

		private void OnTypeSelected(ChangeEvent<string> evt)
		{
			serializedObject.UpdateIfRequiredOrScript();
			serializedObject.FindProperty("_objectiveType").stringValue = evt.newValue;
			serializedObject.ApplyModifiedProperties();
		}

		protected override void OnSave()
		{
			if (_rootElement.Q<ObjectField>("Reward").value == null)
			{
				SetStatus("Reward is Required!", StatusMessage.EStatusType.Error);
				return;
			}
			
			base.OnSave();
		}
	}
}