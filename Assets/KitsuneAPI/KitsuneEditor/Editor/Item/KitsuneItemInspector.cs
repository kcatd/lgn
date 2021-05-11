using System.Collections.Generic;
using KitsuneAPI.KitsuneEditor.Editor.UI.UIElements;
using KitsuneAPI.KitsuneUnity;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace KitsuneAPI.KitsuneEditor.Editor.Item
{
	[CustomEditor(typeof(KitsuneItem), true)]
	public class KitsuneItemInspector : KitsuneBaseEntityInspector<KitsuneItem>
	{
		private PopupField<string> _categoryPopup;
		private ItemCategoryList _itemCategoryList;
		private string _previousCategoryValue;

		public override VisualElement CreateInspectorGUI()
		{
			CreateRootElement();

			_itemCategoryList = KitsuneItemEditor.GetOrCreateItemCategoryAsset();

			string selectedCategory = serializedObject.FindProperty("_categoryId").stringValue;

			if (_itemCategoryList.categories.Count > 1)
			{
				VisualElement categoryListElement = _rootElement.Query<VisualElement>("CategoryDropDownContainer");

				List<string> categoryList = new List<string>(_itemCategoryList.categories);
				categoryList.RemoveAt(0);
				categoryList.Sort();
				categoryList.Insert(0, "-");
				int index = categoryList.IndexOf(selectedCategory);
				_categoryPopup = new PopupField<string>(categoryList, categoryList[index >= 0 ? index : 0]);
				categoryListElement.Add(_categoryPopup);
				_categoryPopup.RegisterValueChangedCallback(SetCategory);
				_categoryPopup.AddToClassList("category-list-items");
			}
			
			return _rootElement;
		}

		private void SetCategory(ChangeEvent<string> categoryChangeEvent)
		{
			if (categoryChangeEvent.newValue == "-")
				return;
			
			_previousCategoryValue = categoryChangeEvent.previousValue;
				
			serializedObject.UpdateIfRequiredOrScript();
			serializedObject.FindProperty("_categoryId").stringValue = categoryChangeEvent.newValue;
			serializedObject.ApplyModifiedProperties();
		}

		protected override void OnCancel()
		{
			base.OnCancel();

			if (serializedObject.targetObject && !string.IsNullOrEmpty(_previousCategoryValue))
			{
				serializedObject.FindProperty("_categoryId").stringValue = _previousCategoryValue;
				serializedObject.ApplyModifiedProperties();
			}
			
			if (serializedObject.targetObject && !string.IsNullOrEmpty(_previousReleaseVersion))
			{
				serializedObject.FindProperty("_releaseVersion").stringValue = _previousReleaseVersion;
				serializedObject.ApplyModifiedProperties();
			}
		}

		protected override void OnSave()
		{
			serializedObject.UpdateIfRequiredOrScript();
			serializedObject.ApplyModifiedProperties();

			SerializedProperty categoryId = serializedObject.FindProperty("_categoryId");
			if (string.IsNullOrEmpty(categoryId.stringValue))
			{
				SetStatus("Category is Required!", StatusMessage.EStatusType.Error);
				return;
			}
			
			base.OnSave();
			
			string newCategory = serializedObject.FindProperty("_categoryId").stringValue;
			if (!string.IsNullOrEmpty(newCategory) && !_itemCategoryList.categories.Contains(newCategory))
			{
				_itemCategoryList.categories.Add(newCategory);
				EditorUtility.SetDirty(_itemCategoryList);
			}
		}
	}
}