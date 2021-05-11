using KitsuneAPI.KitsuneEditor.Editor.Developer;
using KitsuneAPI.KitsuneUnity;
using KitsuneCore.Developer;
using KitsuneCore.Game;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitsuneAPI.KitsuneEditor.Editor
{
	[CustomEditor(typeof(KitsuneGameSettings))]
	public class GameSettingsInspector : KitsuneBaseInspector
	{
		private Button _settingsButton;
		private Button _versionSyncButton;
		private Button _createGameButton;
		private Button _cancelButton;
		private Button _saveButton;
		private Button _newVersionButton;
		private Button _releaseButton;
		
		public override VisualElement CreateInspectorGUI()
		{
			VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
				EditorPrefs.GetString(EEditorPrefs.BASE_EDITOR_PATH) + "/UI/UXML/Game/Game.uxml");
			_rootElement = visualTree.CloneTree();

			if (_settingsButton == null)
			{
				_settingsButton =_rootElement.Q<Button>(className: "game-settings-button");
				SetupSettingsButton();
			}

			if (KitsuneManager.GameSettings.GameId == 0)
			{
				SetContext(EGameContext.New);
			}
			else
			{
				SetContext(EGameContext.Registered);
			}
			
			return _rootElement;
		}

		private void SetupSettingsButton()
		{
			Image icon = _settingsButton.Q<Image>(className: "game-settings-icon");
			string iconPath = EditorPrefs.GetString(EEditorPrefs.BASE_EDITOR_PATH) + 
			                  "/Images/Icons/settings_icon.png";
			Texture2D iconAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
			icon.image = iconAsset;

			_settingsButton.tooltip = _settingsButton.parent.name;
			_settingsButton.clickable.clicked += OnSettingsClicked;
		}
		
		private void SetupVersionsSyncButton()
		{
			Image icon = _versionSyncButton.Q<Image>(className: "versions-sync-icon");
			string iconPath = EditorPrefs.GetString(EEditorPrefs.BASE_EDITOR_PATH) + 
			                  "/Images/Icons/refresh_icon.png";
			Texture2D iconAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
			icon.image = iconAsset;

			_versionSyncButton.tooltip = _versionSyncButton.name;
			_versionSyncButton.clickable.clicked += OnSyncVersions;
		}
		
		private void SetContext(EGameContext gameContext)
		{
			switch (gameContext)
			{
				case EGameContext.New:
					_rootElement.Q<VisualElement>("RegisteredGame").AddToClassList("invisible");
					_rootElement.Q<VisualElement>("EditGame").AddToClassList("invisible");
					_rootElement.Q<VisualElement>("NewGame").RemoveFromClassList("invisible");

					if (_createGameButton == null)
					{
						_createGameButton = _rootElement.Q<Button>("CreateGame");
						_createGameButton.clickable.clicked += OnCreateGame;
					}
					break;
				case EGameContext.Registered:
					_rootElement.Q<VisualElement>("NewGame").AddToClassList("invisible");
					_rootElement.Q<VisualElement>("EditGame").AddToClassList("invisible");
					_rootElement.Q<VisualElement>("RegisteredGame").RemoveFromClassList("invisible");

					_rootElement.Q<Label>("GameVersion")
						.tooltip = "All Entities created will be created with this version";
					if (_versionSyncButton == null)
					{
						_versionSyncButton =_rootElement.Q<Button>(className: "versions-sync-button");
						SetupVersionsSyncButton();
					}
					break;
				case EGameContext.Edit:
					_rootElement.Q<VisualElement>("NewGame").AddToClassList("invisible");
					_rootElement.Q<VisualElement>("RegisteredGame").AddToClassList("invisible");
					_rootElement.Q<VisualElement>("EditGame").RemoveFromClassList("invisible");

					if (_cancelButton == null)
					{
						_cancelButton = _rootElement.Q<Button>("Cancel");
						_cancelButton.clickable.clicked += OnCancel;
					}
					
					if (_saveButton == null)
					{
						_saveButton = _rootElement.Q<Button>("Save");
						_saveButton.clickable.clicked += OnSave;
					}
					
					if (_newVersionButton == null)
					{
						_newVersionButton = _rootElement.Q<Button>("NewVersion");
						_newVersionButton.clickable.clicked += OnNewVersion;
					}
					
					if (_releaseButton == null)
					{
						_releaseButton = _rootElement.Q<Button>("New");
						_releaseButton.clickable.clicked += OnRelease;
					}
					break;
			}
		}

		private void OnCreateGame()
		{
			_rootElement.SetEnabled(false);
			UnityKitsuneDeveloper.CreateGame(KitsuneManager.GameSettings.Title, () =>
			{
				_rootElement.SetEnabled(true);
				serializedObject.ApplyModifiedProperties();
				SetContext(EGameContext.Registered);
				
				DispatchEvent(KitsuneManager.GameSettings);
			});
		}

		private void OnCancel()
		{
			SetContext(EGameContext.Registered);
		}

		private void OnSave()
		{
			string title = serializedObject.FindProperty("_title").stringValue;
			
			if (KitsuneManager.GameSettings.Title != title)
			{
				UnityKitsuneDeveloper.ManageGame(KitsuneManager.GameSettings, () =>
				{
					SetContext(EGameContext.Registered);
				});
			}
			
			UnityKitsuneDeveloper.ManageGameVersion(KitsuneManager.GameSettings.GameVersion, () =>
			{
				SetContext(EGameContext.Registered);
			});
		}

		private void OnNewVersion()
		{
			KitsuneManager.GameSettings.SetGameVersionId(new GameVersionId());
			EditorUtility.SetDirty(KitsuneManager.GameSettings);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			UnityKitsuneDeveloper.ManageGameVersion(KitsuneManager.GameSettings.GameVersion, () =>
			{
				SetContext(EGameContext.Registered);
			});
		}

		private void OnRelease()
		{
			string title = serializedObject.FindProperty("_title").stringValue;

			if (KitsuneManager.GameSettings.Title != title)
			{
				UnityKitsuneDeveloper.ManageGame(KitsuneManager.GameSettings, () =>
				{
					SetContext(EGameContext.Registered);
				});
			}
			
			UnityKitsuneDeveloper.ReleaseGameVersion(KitsuneManager.GameSettings.GameVersion, true, () =>
			{
				Debug.Log("Release Set");
				SetContext(EGameContext.Registered);
			});
		}

		private void OnSettingsClicked()
		{
			SetContext(EGameContext.Edit);
		}
		
		private void OnSyncVersions()
		{
			UnityKitsuneDeveloper.GetGameVersions(list =>
			{
				int newestRelease = 0;
				int releaseIndex = -1;
				int devVersionId = 0;
				int devVersionIndex = -1;
				for (int i = 0; i < list.Count; ++i)
				{
					if (list[i].ReleaseStatus == EReleaseStatus.Released)
					{
						if (list[i].ReleaseNumber > newestRelease)
						{
							newestRelease = list[i].ReleaseNumber;
							releaseIndex = i;
						}
					}
				}
				
				for (int i = 0; i < list.Count; ++i)
				{
					if (list[i].ReleaseStatus == EReleaseStatus.Unreleased)
					{
						if (list[i].game_version_id > devVersionId)
						{
							devVersionId = list[i].game_version_id;
							devVersionIndex = i;
						}
					}
				}

				if (releaseIndex > -1)
				{
					KitsuneManager.GameSettings.ReleasedVersion = list[releaseIndex].Version;
					KitsuneManager.GameSettings.ReleasedGameVersionId = list[releaseIndex].GameVersionId;
				}

				if (devVersionIndex > -1)
				{
					KitsuneManager.GameSettings.SetGameVersion(list[devVersionIndex].Version);
					KitsuneManager.GameSettings.SetGameVersionId(list[devVersionIndex].GameVersionId);
				}
				else
				{
					KitsuneManager.GameSettings.GameVersion.Version = list[releaseIndex].Version;
					KitsuneManager.GameSettings.GameVersion.GameVersionId = list[releaseIndex].GameVersionId;
				}
				
				EditorUtility.SetDirty(KitsuneManager.GameSettings);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			});
		}
	}
	
	public enum EGameContext
	{
		New,
		Registered,
		Edit
	}
}