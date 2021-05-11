using System.Collections.Generic;
using System.IO;
using KitsuneAPI.KitsuneEditor.Editor.Developer;
using KitsuneAPI.KitsuneEditor.Editor.UI.UIElements;
using KitsuneAPI.KitsuneUnity;
using KitsuneCore;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace KitsuneAPI.KitsuneEditor.Editor
{
	public abstract class KitsuneBaseEntityListInspector<T, U> : KitsuneBaseInspector where T : UnityKitsuneEntity where U : KitsuneBaseEntityList<T>
	{
		protected string _description = "";
		protected string _dependencyRequirementText = "";
		protected Button _addEntityButton;
		protected VisualTreeAsset _listItemTemplate;
		protected VisualTreeAsset _listItemTree;
		protected VisualElement _listItemsElement;
		protected KitsuneBaseEntityList<T> _entityList;
		protected string _entityShortName;
		private bool _dependencyRequirementsMet;
		private bool _addPreview;

		private List<UnityEditor.Editor> _prefabPreviewEditors;
		
		protected void CreateRootElement(bool addPreview = true)
		{
			_addPreview = addPreview;

			_dependencyRequirementsMet = ValidateDependencyRequirements();

			_entityShortName = UnityKitsuneDeveloper.GetEntityShortName<T>();
			
			VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
				EditorPrefs.GetString(EEditorPrefs.BASE_EDITOR_PATH) + "/" + _entityShortName + "/UXML/" + _entityShortName + "List.uxml");
			_rootElement = visualTree.CloneTree();
			
			VisualTreeAsset listHeaderTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
				EditorPrefs.GetString(EEditorPrefs.BASE_EDITOR_PATH) + "/UI/UXML/Common/EntityListHeaderTemplate.uxml");
			
			VisualTreeAsset listHeaderTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
				EditorPrefs.GetString(EEditorPrefs.BASE_EDITOR_PATH) + "/" + _entityShortName + "/UXML/" + _entityShortName + "ListHeader.uxml");
			
			_listItemTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
				EditorPrefs.GetString(EEditorPrefs.BASE_EDITOR_PATH) + "/UI/UXML/Common/EntityListItemTemplate.uxml");
			
			_listItemTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
				EditorPrefs.GetString(EEditorPrefs.BASE_EDITOR_PATH) + "/" + _entityShortName + "/UXML/" + _entityShortName + "ListItem.uxml");

			if (!_dependencyRequirementsMet)
			{
				UnityEngine.UIElements.Image warnIcon = _rootElement.Q<UnityEngine.UIElements.Image>("WarnIcon");
				string iconPath = EditorPrefs.GetString(EEditorPrefs.BASE_EDITOR_PATH) + "/Images/Icons/warn_icon.png";
				Texture2D iconAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
				warnIcon.image = iconAsset;
				
				TextElement dependencyTF = _rootElement.Q<TextElement>("DependencyRequirements");
				dependencyTF.text = _dependencyRequirementText;
			}
			else
			{
				_rootElement.Q<VisualElement>("DependenciesContainer").RemoveFromHierarchy();
			}
			
			_listItemsElement = _rootElement.Q<VisualElement>(className: "entity-list");
			
			// add the entity data cols to the header
			VisualElement listHeader = listHeaderTemplate.CloneTree();
			VisualElement listHeaderData = listHeaderTree.CloneTree();
			listHeaderData.AddToClassList("row");
			listHeaderData.AddToClassList("entity-list-item");
			listHeaderData.AddToClassList("list-header");
			listHeader.Q<VisualElement>("EntityHeaderData").Add(listHeaderData);

			if (!addPreview)
			{
				listHeader.Q<VisualElement>("Preview").RemoveFromHierarchy();
			}
			
			_listItemsElement.Add(listHeader);
			
			_entityList = serializedObject.targetObject as U;

			if (_entityList.list == null)
			{
				_entityList.list = new List<T>();
			}

			// TODO - remove when complete
			for (int i = _entityList.list.Count - 1; i >= 0; --i)
			{
				if (_entityList.list[i] == null)
				{
					_entityList.list.RemoveAt(i);
				}
			}
			
			EditorUtility.SetDirty(_entityList);

			for (int i = 0; i < _entityList.list.Count; ++i)
			{
				AddEntityToList(_entityList.list[i]);
			}
			
			SetupHeader();
		}
		
		protected virtual VisualElement AddEntityToList(T entity)
		{
			VisualElement listItem = _listItemTemplate.CloneTree();
			VisualElement listItemData = _listItemTree.CloneTree();
			listItemData.AddToClassList("row");
			listItemData.AddToClassList("entity-list-item");
			listItem.Q<VisualElement>("EntityData").Add(listItemData);
				
			SerializedObject serializedEntity = new SerializedObject(entity);
			listItem.Bind(serializedEntity);

			if (!_addPreview)
			{
				listItem.Q<VisualElement>("Preview").RemoveFromHierarchy();
			}
			else if (_addPreview && entity.Prefab)
			{
				string assetPath = AssetDatabase.GetAssetPath(entity.Prefab);
				GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
					
				if (asset.GetComponent<Image>() != null)
				{
					// UI Assets
					Texture uiIconTexture = asset.GetComponent<Image>().mainTexture;
					UnityEngine.UIElements.Image uiIcon = new UnityEngine.UIElements.Image();
					uiIcon.image = uiIconTexture;
					listItem.Q<VisualElement>("Preview").Add(uiIcon);
				}
				else
				{
					IMGUIContainer prefabIcon = new IMGUIContainer(() =>
					{
						UnityEditor.Editor editor = GetPreviewEditor(asset);
						editor.OnPreviewGUI(GUILayoutUtility.GetRect(45, 45), null);
					});
					prefabIcon.focusable = false;
					listItem.Q<VisualElement>("Preview").Add(prefabIcon);
				}
			}

			listItem.Q<Button>("Edit").clickable.clicked += () => EditEntity(serializedEntity, listItem);
			listItem.Q<Button>("Delete").clickable.clicked += () => DeleteEntity(serializedEntity, listItem);
			_listItemsElement.Add(listItem);

			return listItem;
		}

		protected virtual void UpdateNonBindableData(T entity, VisualElement listItem)
		{
			// override to update any custom data that isn't bindable 
		}
		
		private void SetupHeader()
		{
			string title = _rootElement.Q("EntityListHeader").parent.name;
			
			_addEntityButton = _rootElement.Q<Button>("AddEntityButton");
			_addEntityButton.tooltip = "Add " + title;
			_addEntityButton.clickable.clicked += () => OnAddEntityClicked();
			
			_addEntityButton.SetEnabled(_dependencyRequirementsMet);

			_rootElement.Q<TextElement>("ListTitle").text = title;
			_rootElement.Q<TextElement>("ListDescription").text = _description;
		}
		
		protected virtual T OnAddEntityClicked()
		{
			_addEntityButton.SetEnabled(false);
			
			T newEntity = CreateEntityAsset();
			VisualElement inspectorContainer = _rootElement.Q<VisualElement>("NewEntityInspector");
			InspectorElement inspectorElement = new InspectorElement();
			inspectorElement.AddToClassList("entity-inspector");
			inspectorElement.Bind(new SerializedObject(newEntity));
			inspectorElement.Q<Label>("Context Title").text = "New " + _entityShortName;
			inspectorContainer.Add(inspectorElement);
			inspectorElement.RegisterCallback<ChangeEvent<T>>(OnAddEntity);

			return newEntity;
		}
		
		protected void EditEntity(SerializedObject serializedEntity, VisualElement listItem)
		{
			SetEditDeleteEnabled(listItem, false);
			
			VisualElement inspectorContainer = listItem.Q<VisualElement>("EditEntityInspector");
			InspectorElement inspectorElement = new InspectorElement();
			inspectorElement.AddToClassList("entity-inspector");
			inspectorElement.Bind(serializedEntity);
			inspectorElement.Q<Label>("Context Title").text = "Edit " + _entityShortName;
			inspectorContainer.Add(inspectorElement);
			inspectorElement.RegisterCallback<ChangeEvent<T>>(changeEvent =>
			{
				if (changeEvent.newValue == null)
				{
					RemoveEditInspector(listItem);
			
					SetEditDeleteEnabled(listItem, true);
				}
				else
				{
					OnEditEntity(changeEvent.newValue, listItem);
				}
			});
		}
		
		protected void DeleteEntity(SerializedObject serializedObject, VisualElement listItem)
		{
			// TODO - add confirmation
			return;
			
			if (!KitsuneFacade.Authenticated &&
			    string.IsNullOrEmpty(UnityKitsuneDeveloper.DeveloperSettings.GameSecretKey))
			{
				SetStatus("Login or DevKey Required", StatusMessage.EStatusType.Error);
				return;
			}
			
			T entity = serializedObject.targetObject as T;
			UnityKitsuneDeveloper.DeleteEntity(entity.Entity, () =>
			{
				_entityList.list.Remove(entity);
				
				EditorUtility.SetDirty(_entityList);

				AssetDatabase.DeleteAsset(UnityKitsuneDeveloper.GetAssetDirectoryPath<T>() + "/" + entity.Name + ".asset");

				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				
				_listItemsElement.Remove(listItem);
				
				SetStatus("Currency Deleted", StatusMessage.EStatusType.Warn);
			});
		}
		
		private void OnAddEntity(ChangeEvent<T> changeEvent)
		{
			_addEntityButton.SetEnabled(true);
			
			if (changeEvent.newValue == null)
			{
				RemoveAddInspector();
			}
			else
			{
				T newEntity = changeEvent.newValue;
				_entityList.list.Add(newEntity);

				AddEntityToList(newEntity);

				EditorUtility.SetDirty(_entityList);

				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();

				RemoveAddInspector();
				
				SetStatus("Save Successful", StatusMessage.EStatusType.Success);
			}
		}
		
		private void OnEditEntity(T entity, VisualElement listItem)
		{
			if (!KitsuneFacade.Authenticated &&
			    string.IsNullOrEmpty(UnityKitsuneDeveloper.DeveloperSettings.GameSecretKey))
			{
				SetStatus("Login or DevKey Required", StatusMessage.EStatusType.Error);
				return;
			}

			UpdateNonBindableData(entity, listItem);

			int index = _entityList.list.IndexOf(entity);
			_entityList.list[index] = entity;
			EditorUtility.SetDirty(_entityList);
			
			RemoveEditInspector(listItem);

			SetEditDeleteEnabled(listItem, true);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		private void SetEditDeleteEnabled(VisualElement listItem, bool value)
		{
			listItem.Q<Button>("Edit").SetEnabled(value);
			listItem.Q<Button>("Delete").SetEnabled(value);
		}
		
		private void RemoveAddInspector()
		{
			InspectorElement elementToRemove = _rootElement.Q<InspectorElement>();
			elementToRemove?.UnregisterCallback<ChangeEvent<T>>(OnAddEntity);
			elementToRemove?.RemoveFromHierarchy();
		}
		
		private void RemoveEditInspector(VisualElement listItem)
		{
			InspectorElement elementToRemove = listItem.Q<InspectorElement>();
			elementToRemove?.RemoveFromHierarchy();
		}
		
		protected T CreateEntityAsset()
		{
			T entity = CreateInstance<T>();

			string directoryPath = UnityKitsuneDeveloper.GetAssetDirectoryPath<T>();
			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
				AssetDatabase.ImportAsset(directoryPath);
			}
			
			string tmpAssetPath = directoryPath + "/" + UnityKitsuneDeveloper.TMP_ASSET_NAME;
			if (File.Exists(tmpAssetPath))
			{
				return AssetDatabase.LoadAssetAtPath<T>(tmpAssetPath);
			}
			
			AssetDatabase.CreateAsset(entity, tmpAssetPath);
			AssetDatabase.SaveAssets();

			return entity;
		}

		protected abstract bool ValidateDependencyRequirements();
		
		private UnityEditor.Editor GetPreviewEditor(GameObject asset)
		{
			if (_prefabPreviewEditors == null)
			{
				_prefabPreviewEditors = new List<UnityEditor.Editor>();
			}

			//Check if there's already a preview
			foreach(UnityEditor.Editor editor in _prefabPreviewEditors)
			{
				if((GameObject)editor.target == asset)
				{
					return editor;
				}
			}

			UnityEditor.Editor newEditor = CreateEditor(asset);
			_prefabPreviewEditors.Add(newEditor);
			return newEditor;

		}
	}
}