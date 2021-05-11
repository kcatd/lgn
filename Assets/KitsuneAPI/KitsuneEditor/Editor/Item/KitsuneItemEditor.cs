using System.Collections.Generic;
using System.IO;
using KitsuneAPI.KitsuneEditor.Editor.Currency;
using KitsuneAPI.KitsuneUnity;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace KitsuneAPI.KitsuneEditor.Editor.Item
{
	[CustomEditor(typeof(KitsuneItemList))]
	public class KitsuneItemEditor : KitsuneBaseEntityListInspector<KitsuneItem, KitsuneItemList>
	{
		private PopupField<string> _categoryPopup;
		private string _categoryFilter = "All";
		
		public override VisualElement CreateInspectorGUI()
		{
			_description = "Create and manage virtual items for your game.";
			_dependencyRequirementText = "You must create a Virtual Currency before creating an Item";

			CreateRootElement();
			
			ItemCategoryList categoryList = GetOrCreateItemCategoryAsset();
			
			VisualElement categoryListElement = _rootElement.Query<VisualElement>("CategoryListHeader");
			categoryList.categories.Sort();
			_categoryPopup = new PopupField<string>(categoryList.categories, categoryList.categories[0]);
			categoryListElement.Add(_categoryPopup);
			_categoryPopup.RegisterValueChangedCallback(FilterItemsByCategory);
			_categoryPopup.AddToClassList("category-list-items");
			
			return _rootElement;
		}

		private void FilterItemsByCategory(ChangeEvent<string> categoryFilterEvent)
		{
			_categoryFilter = categoryFilterEvent.newValue;
			UQueryBuilder<VisualElement> items = _rootElement.Query<VisualElement>(className: "entity-list-item-container");
			items.ForEach(FilterItem);
		}

		private void FilterItem(VisualElement container)
		{
			if (_categoryFilter == "All")
			{
				container.RemoveFromClassList("invisible");
			}
			else
			{
				string category = container.Q<TextElement>("CategoryId").text;
				if (category == _categoryFilter)
				{
					container.RemoveFromClassList("invisible");
				}
				else
				{
					container.AddToClassList("invisible");
				}
			}
		}

		protected override VisualElement AddEntityToList(KitsuneItem entity)
		{
			VisualElement listItem = base.AddEntityToList(entity);
			VisualElement item = listItem.Q<VisualElement>(className: "entity-list-item-container");
			FilterItem(item);
		
			return listItem;
		}

		public static ItemCategoryList GetOrCreateItemCategoryAsset()
		{
			string listPath = "Assets/KitsuneAPI/KitsuneEditor/Editor/Item/ItemCategoryList.asset";
			if (File.Exists(listPath))
			{
				return AssetDatabase.LoadAssetAtPath<ItemCategoryList>(listPath);
			}
			ItemCategoryList categoryList = CreateInstance<ItemCategoryList>();
			categoryList.categories = new List<string>();
			categoryList.categories.Add("All");
			
			string parentPath = Path.GetDirectoryName(listPath);
			if (!Directory.Exists(parentPath))
			{
				Directory.CreateDirectory(parentPath);
				AssetDatabase.ImportAsset(parentPath);
			}

			AssetDatabase.CreateAsset(categoryList, listPath);
			AssetDatabase.SaveAssets();
				
			return categoryList;
		}

		protected override bool ValidateDependencyRequirements()
		{
			KitsuneCurrencyList currencies = KitsuneEditorWindow.GetOrCreateListAsset<KitsuneCurrency, KitsuneCurrencyList>();
			if (currencies.list.Count > 0)
			{
				return true;
			}

			return false;
		}
	}
}