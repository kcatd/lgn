using KitsuneAPI.KitsuneUnity;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitsuneAPI.KitsuneEditor.Editor.Reward
{
	[CustomEditor(typeof(KitsuneReward))]
	public class KitsuneRewardInspector : KitsuneBaseEntityInspector<KitsuneReward>
	{
		private VisualTreeAsset _rewardPartTree;
		private VisualElement _rewardPartsContainer;
		private KitsuneReward _kitsuneReward;
		private Button _addItemButton;
		private Button _addCurrencyButton;
		
		public override VisualElement CreateInspectorGUI()
		{
			CreateRootElement(false);
		
			_rewardPartTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
				EditorPrefs.GetString(EEditorPrefs.BASE_EDITOR_PATH) + "/" + _entityShortName + "/UXML/" + _entityShortName + "PartTemplate.uxml");

			_rewardPartsContainer = _rootElement.Q<VisualElement>("RewardPartsContainer");
			
			_kitsuneReward = serializedObject.targetObject as KitsuneReward;

			if (_addCurrencyButton == null)
			{
				_addCurrencyButton = _rootElement.Q<Button>("AddCurrencyReward");
				_addCurrencyButton.clickable.clicked += AddCurrencyReward;
			}

			if (_addItemButton == null)
			{
				_addItemButton = _rootElement.Q<Button>("AddItemReward");
				_addItemButton.clickable.clicked += AddItemReward;
			}
			
			UpdateRewards();

			return _rootElement;
		}

		private void AddCurrencyReward()
		{
			_addCurrencyButton.SetEnabled(false);
			
			VisualElement rewardPartElement = _rewardPartTree.CloneTree();
			
			_kitsuneReward.CurrencyQuantities.Add(1);
			IntegerField qtyField = rewardPartElement.Q<IntegerField>("RewardQuantity");
			qtyField.value = 1;
			qtyField.RegisterValueChangedCallback(evt =>
			{
				_kitsuneReward.CurrencyQuantities[_kitsuneReward.CurrencyQuantities.Count - 1] = evt.newValue;
			});
			
			ObjectField currencyField = rewardPartElement.Q<ObjectField>("RewardEntity");
			currencyField.objectType = typeof(KitsuneCurrency);
			currencyField.label = "Currency Reward";
			currencyField.RegisterValueChangedCallback(evt =>
			{
				if (evt.newValue != null)
				{
					_kitsuneReward.CurrencyRewards.Add((KitsuneCurrency)evt.newValue);
					_addCurrencyButton.SetEnabled(true);
				}
			});
			
			Button removeButton = rewardPartElement.Q<Button>("Remove");
			removeButton.clickable.clicked += () =>
			{
				if (currencyField.value != null)
				{
					int rewardPartIndex = _kitsuneReward.CurrencyRewards.IndexOf((KitsuneCurrency) currencyField.value);
					_kitsuneReward.CurrencyQuantities.RemoveAt(rewardPartIndex);
					_kitsuneReward.CurrencyRewards.RemoveAt(rewardPartIndex);
				}
				rewardPartElement.RemoveFromHierarchy();
				_addCurrencyButton.SetEnabled(true);
			};
			
			_rewardPartsContainer.Add(rewardPartElement);
		}

		private void AddItemReward()
		{
			_addItemButton.SetEnabled(false);
			
			VisualElement rewardPartElement = _rewardPartTree.CloneTree();
			
			_kitsuneReward.ItemQuantities.Add(1);
			IntegerField qtyField = rewardPartElement.Q<IntegerField>("RewardQuantity");
			qtyField.value = 1;
			qtyField.RegisterValueChangedCallback(evt =>
			{
				_kitsuneReward.ItemQuantities[_kitsuneReward.ItemQuantities.Count - 1] = evt.newValue;
			});
			
			ObjectField itemField = rewardPartElement.Q<ObjectField>("RewardEntity");
			itemField.objectType = typeof(KitsuneItem);
			itemField.label = "Item Reward";
			itemField.RegisterValueChangedCallback(evt =>
			{
				if (evt.newValue != null)
				{
					_kitsuneReward.ItemRewards.Add((KitsuneItem)evt.newValue);
					_addItemButton.SetEnabled(true);
				}
			});
			
			Button removeButton = rewardPartElement.Q<Button>("Remove");
			removeButton.clickable.clicked += () =>
			{
				if (itemField.value != null)
				{
					int rewardPartIndex = _kitsuneReward.ItemRewards.IndexOf((KitsuneItem) itemField.value);
					_kitsuneReward.ItemQuantities.RemoveAt(rewardPartIndex);
					_kitsuneReward.ItemRewards.RemoveAt(rewardPartIndex);
				}
				rewardPartElement.RemoveFromHierarchy();
				_addItemButton.SetEnabled(true);
			};
			
			_rewardPartsContainer.Add(rewardPartElement);
		}

		private void UpdateRewards()
		{
			foreach (KitsuneCurrency currencyReward in _kitsuneReward.CurrencyRewards)
			{
				AddExistingCurrencyRewardPart(currencyReward);
			}
			
			foreach (KitsuneItem itemReward in _kitsuneReward.ItemRewards)
			{
				AddExistingItemRewardPart(itemReward);
			}
		}

		private void AddExistingItemRewardPart(KitsuneItem itemReward)
		{
			VisualElement rewardPartElement = _rewardPartTree.CloneTree();
			
			ObjectField itemField = rewardPartElement.Q<ObjectField>("RewardEntity");
			itemField.objectType = typeof(KitsuneItem);
			itemField.label = "Item Reward";
			itemField.value = itemReward;

			int qtyIndex = _kitsuneReward.ItemRewards.IndexOf(itemReward);
			IntegerField qtyField = rewardPartElement.Q<IntegerField>("RewardQuantity");
			qtyField.value = _kitsuneReward.ItemQuantities[qtyIndex];
			qtyField.RegisterValueChangedCallback(evt =>
			{
				_kitsuneReward.ItemQuantities[qtyIndex] = evt.newValue;
			});
			
			Button removeButton = rewardPartElement.Q<Button>("Remove");
			removeButton.clickable.clicked += () =>
			{
				_kitsuneReward.ItemRewards.Remove(itemReward);
				_kitsuneReward.ItemQuantities.RemoveAt(qtyIndex);
				rewardPartElement.RemoveFromHierarchy();
			};
			
			_rewardPartsContainer.Add(rewardPartElement);
		}

		private void AddExistingCurrencyRewardPart(KitsuneCurrency currencyReward)
		{
			VisualElement rewardPartElement = _rewardPartTree.CloneTree();
			
			ObjectField itemField = rewardPartElement.Q<ObjectField>("RewardEntity");
			itemField.objectType = typeof(KitsuneCurrency);
			itemField.label = "Currency Reward";
			itemField.value = currencyReward;

			int qtyIndex = _kitsuneReward.CurrencyRewards.IndexOf(currencyReward);
			IntegerField qtyField = rewardPartElement.Q<IntegerField>("RewardQuantity");
			qtyField.value = _kitsuneReward.CurrencyQuantities[qtyIndex];
			qtyField.RegisterValueChangedCallback(evt =>
			{
				_kitsuneReward.CurrencyQuantities[qtyIndex] = evt.newValue;
			});
			
			Button removeButton = rewardPartElement.Q<Button>("Remove");
			removeButton.clickable.clicked += () =>
			{
				_kitsuneReward.CurrencyRewards.Remove(currencyReward);
				_kitsuneReward.CurrencyQuantities.RemoveAt(qtyIndex);
				rewardPartElement.RemoveFromHierarchy();
			};
			
			_rewardPartsContainer.Add(rewardPartElement);
		}

		protected override void OnSave()
		{
			serializedObject.ApplyModifiedProperties();
			serializedObject.Update();
			
			base.OnSave();
		}
	}
}