using System.IO;
using KitsuneAPI.KitsuneEditor.Editor.Developer;
using KitsuneAPI.KitsuneEditor.Editor.UI.UIElements;
using KitsuneCommon.Debug;
using KitsuneCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace KitsuneAPI.KitsuneEditor.Editor.Game
{
	/// <summary>
	/// Level data isn't an Entity, this is a custom non-entity inspector
	/// </summary>
	[CustomEditor(typeof(KitsuneGameLevelXpData))]
	public class LevelXpInspector : KitsuneBaseInspector
	{
		protected Button _saveButton;
		private Foldout _jsonFoldout;
		private TextField _jsonTF;

		public override VisualElement CreateInspectorGUI()
		{
			VisualTreeAsset levelXpTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
				EditorPrefs.GetString(EEditorPrefs.BASE_EDITOR_PATH) + "/Game/UXML/LevelXpInspector.uxml");

			_rootElement = levelXpTree.CloneTree();

			if (_saveButton == null)
			{
				_saveButton = _rootElement.Q<Button>("Save");
				_saveButton.clickable.clicked += OnSave;
			}

			_jsonFoldout = _rootElement.Q<Foldout>("JSONFoldout");
			
			_jsonTF = _rootElement.Q<TextField>("JSON");
			_jsonTF.isDelayed = true;
			_jsonTF.RegisterValueChangedCallback(ValidateJSON);

			Toggle toggle = _rootElement.Q<Toggle>("AutoScale");
			toggle.value = serializedObject.FindProperty("_autoScale").boolValue;
			EnableAutoScale(toggle.value);
			toggle.RegisterValueChangedCallback(evt =>
			{
				serializedObject.FindProperty("_autoScale").boolValue = evt.newValue;
				serializedObject.UpdateIfRequiredOrScript();
				serializedObject.ApplyModifiedProperties();
				EnableAutoScale(evt.newValue);
			});
			return _rootElement;
		}

		private void EnableAutoScale(bool value)
		{
			if (value)
			{
				_rootElement.Q<VisualElement>("ManualScaleContainer").AddToClassList("invisible");
				_rootElement.Q<VisualElement>("AutoScaleContainer").RemoveFromClassList("invisible");
				_jsonTF.isReadOnly = true;
			}
			else
			{
				_rootElement.Q<VisualElement>("ManualScaleContainer").RemoveFromClassList("invisible");
				_rootElement.Q<VisualElement>("AutoScaleContainer").AddToClassList("invisible");
				_jsonTF.isReadOnly = false;
				_jsonFoldout.value = true;
			}
		}

		private void ValidateJSON(ChangeEvent<string> changeEvent)
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(changeEvent.newValue))
				{
					JsonConvert.DeserializeObject<JObject>(changeEvent.newValue);
					_saveButton.SetEnabled(true);
					ClearStatus();
				}
			}
			catch (JsonException e)
			{
				SetStatus("Invalid JSON", StatusMessage.EStatusType.Error);
				_saveButton.SetEnabled(false);
			}
		}

		private void OnSave()
		{
			if (!KitsuneFacade.Authenticated &&
			    string.IsNullOrEmpty(UnityKitsuneDeveloper.DeveloperSettings.GameSecretKey))
			{
				SetStatus("Login or DevKey Required", StatusMessage.EStatusType.Error);
				return;
			}

			KitsuneGameLevelXpData levelXpData = serializedObject.targetObject as KitsuneGameLevelXpData;
			
			if (string.IsNullOrEmpty(levelXpData.GameVersion))
			{
				SetStatus("Game Version is Required", StatusMessage.EStatusType.Error);
				return;
			}

			UnityKitsuneDeveloper.ManageGameLevelData(levelXpData, OnSuccess);
		}

		private void OnSuccess()
		{
			serializedObject.ApplyModifiedProperties();
			AssetDatabase.SaveAssets();
			SetStatus("Level Xp Data Saved!", StatusMessage.EStatusType.Success);
		}
		
		public static KitsuneGameLevelXpData GetOrCreateLevelXpData()
		{
			string directoryPath = "Assets/KitsuneAPI/KitsuneEditor/Editor/Game/";
			string assetPath = directoryPath + "gameLevelXpData.asset";

			KitsuneGameLevelXpData levelXpData = AssetDatabase.LoadAssetAtPath<KitsuneGameLevelXpData>(assetPath);
			if (levelXpData != null)
			{
				return levelXpData;
			}

			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
				AssetDatabase.ImportAsset(directoryPath);
			}

			levelXpData = CreateInstance<KitsuneGameLevelXpData>();
			if (levelXpData != null)
			{
				AssetDatabase.CreateAsset(levelXpData, assetPath);
				AssetDatabase.SaveAssets();
			}
			else
			{
				Output.Debug("KitsuneEditor","Failed creating GameLevelXpData. CreateInstance(\"GameLevelXPData\") returned null.");
			}
			
			return levelXpData;
		}
		
		private bool DeleteTempAsset()
		{
			string assetPath = "Assets/KitsuneAPI/KitsuneData/Game/gameLevelXpData.asset";
			if (File.Exists(assetPath))
			{
				return AssetDatabase.DeleteAsset(assetPath);
			}

			return false;
		}
	}
}