using System.Collections.Generic;
using System.IO;
using KitsuneAPI.KitsuneEditor.Editor.Developer;
using KitsuneAPI.KitsuneEditor.Editor.UI.UIElements;
using KitsuneAPI.KitsuneUnity;
using KitsuneCore;
using KitsuneCore.Entity;
using KitsuneCore.Game;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitsuneAPI.KitsuneEditor.Editor
{
	public abstract class KitsuneBaseEntityInspector<T> : KitsuneBaseInspector where T : UnityKitsuneEntity
	{
		protected Button _cancelButton;
		protected Button _saveButton;
		protected string _entityShortName;
		protected List<GameVersionInfo> _gameVersions;
		protected List<string> _gameVersionStrings = new List<string>();
		protected PopupField<string> _gameVersionsPopUp;
		protected string _previousReleaseVersion;
		protected EntityInfo _entityInfo;
		protected string _currentReleaseVersion = "";
		protected void CreateRootElement(bool hasPrefab = true)
		{
			_gameVersionStrings.Insert(0, "Unreleased");
			
			UnityKitsuneDeveloper.GetGameEntities(OnGameEntities);
			
			_entityShortName = UnityKitsuneDeveloper.GetEntityShortName<T>();

			VisualTreeAsset entityTemplateTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
				EditorPrefs.GetString(EEditorPrefs.BASE_EDITOR_PATH) + "/UI/UXML/Common/EntityInspectorTemplate.uxml");
			
			VisualTreeAsset entityPropertiesTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
				EditorPrefs.GetString(EEditorPrefs.BASE_EDITOR_PATH) + "/" + _entityShortName + "/UXML/" + _entityShortName + "Inspector.uxml");

			_rootElement = entityTemplateTree.CloneTree();
			_rootElement.Q<VisualElement>("EntityProperties").Add(entityPropertiesTree.CloneTree());
			
			if (hasPrefab)
			{
				_rootElement.Q<ObjectField>("Prefab").objectType = typeof(GameObject);
			}
			else
			{
				_rootElement.Q<VisualElement>("PrefabContainer").RemoveFromHierarchy();
			}

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
			
			_rootElement.RegisterCallback<DetachFromPanelEvent>(OnPanelChanged);
		}

		private void OnGameEntities(List<EntityInfo> entities)
		{
			EntityId currentEntityId = EntityId.Mk(serializedObject.FindProperty("_id").intValue);
			for (int i = 0; i < entities.Count; ++i)
			{
				if (entities[i].EntityId == currentEntityId)
				{
					_entityInfo = entities[i];
					break;
				}
			}

			if (_entityInfo == null)
			{
				serializedObject.FindProperty("_id").intValue = 0;
				serializedObject.ApplyModifiedProperties();
			}

			UnityKitsuneDeveloper.GetGameVersions(OnGetGameVersions);
		}

		protected virtual void OnGetGameVersions(List<GameVersionInfo> gameVersions)
		{
			_gameVersions = gameVersions;
			_gameVersions.Reverse();

			if (_entityInfo == null)
			{
				_currentReleaseVersion = _gameVersions[0].game_version_string;
			}
			else
			{
				for (int i = 0; i < _gameVersions.Count; ++i)
				{
					if (_entityInfo.ReleaseVersion == _gameVersions[i].GameVersionId)
					{
						_currentReleaseVersion = _gameVersions[i].game_version_string;
					}
				}
			}
			
			if (_gameVersions.Count > 0)
			{
				VisualElement releaseVersionElement = _rootElement.Query<VisualElement>("ReleaseVersionDropDownContainer");
               
				for (int i = 0; i < _gameVersions.Count; ++i)
				{
					_gameVersionStrings.Add(_gameVersions[i].game_version_string);
				}
				int index = _entityInfo != null 
					? _gameVersionStrings.IndexOf(_entityInfo.ReleaseVersion) 
					: 0;
				_gameVersionsPopUp = new PopupField<string>(_gameVersionStrings, _gameVersionStrings[index >= 0 ? index : 0]);
				releaseVersionElement.Add(_gameVersionsPopUp);
				_gameVersionsPopUp.RegisterValueChangedCallback(SetReleaseVersion);
				_gameVersionsPopUp.AddToClassList("release-version-list-items");
			}
			
			_rootElement.MarkDirtyRepaint();
		}
		
		private void SetReleaseVersion(ChangeEvent<string> categoryChangeEvent)
		{
			_previousReleaseVersion = categoryChangeEvent.previousValue;

			string version = categoryChangeEvent.newValue;
			if (version == "Unreleased")
			{
				version = "";
			}
			
			serializedObject.UpdateIfRequiredOrScript();
			serializedObject.FindProperty("_releaseVersion").stringValue = version;
			serializedObject.ApplyModifiedProperties();
		}

		private void OnPanelChanged(DetachFromPanelEvent detachFromPanelEvent)
		{
			// OnCancel(); // TODO - why was this here
		}
		
		protected virtual void OnCancel()
		{
			DeleteTempAsset();
			DispatchEvent<T>(default);
		}

		protected virtual void OnSave()
		{
			if (!KitsuneFacade.Authenticated &&
			    string.IsNullOrEmpty(UnityKitsuneDeveloper.DeveloperSettings.GameSecretKey))
			{
				SetStatus("Login or DevKey Required", StatusMessage.EStatusType.Error);
				return;
			}
			
			T entity = serializedObject.targetObject as T;

			if (string.IsNullOrEmpty(entity.Name) ||
			    entity.Name == "New Entity")
			{
				SetStatus(_entityShortName + " Name Required", StatusMessage.EStatusType.Error);
				return;
			}

			EEntityOperation op = entity.Id == 0 ? EEntityOperation.Create : EEntityOperation.Update;
			
			// TODO - temp until we can sync from prod
			// if (op == EEntityOperation.Create && 
			//     File.Exists(UnityKitsuneDeveloper.GetAssetDirectoryPath<T>() + "/" + entity.Name + ".asset"))
			// {
			// 	SetStatus("A " + _entityShortName + " entity with the name \"" + entity.Name + "\" already exists!", StatusMessage.EStatusType.Error);
			// 	return;
			// }

			UnityKitsuneDeveloper.ManageEntity(entity, op, () =>
			{
				serializedObject.ApplyModifiedProperties();

				EditorUtility.SetDirty(entity);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				
				DispatchEvent(entity);
			});
		}
		
		private bool DeleteTempAsset()
		{
			string tmpAssetPath = UnityKitsuneDeveloper.GetAssetDirectoryPath<T>() + "/" + UnityKitsuneDeveloper.TMP_ASSET_NAME;
			if (File.Exists(tmpAssetPath))
			{
				return AssetDatabase.DeleteAsset(tmpAssetPath);
			}

			return false;
		}
	}
}