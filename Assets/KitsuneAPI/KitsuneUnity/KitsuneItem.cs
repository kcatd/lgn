using System;
using KitsuneCore.Entity;
using KitsuneCore.Entity.Components;
using KitsuneCore.Game;
using KitsuneCore.Services.Inventory;
using UnityEngine;

namespace KitsuneAPI.KitsuneUnity
{
	/// <summary>
	/// Represents an item that can be purchased, gifted, traded, or won
	/// A Unity ScriptableObject Wrapper for the Kitsune ItemEntity
	/// </summary>
	[Serializable]
	public class KitsuneItem : UnityKitsuneEntity, IComparable<KitsuneItem>
	{
		[HideInInspector]
		[SerializeField] 
		private string _categoryId;
		/// <inheritdoc cref="ItemEntity.CategoryId"/>
		public ItemCategoryId CategoryId
		{
			get => Entity.GetComponent<ItemComponent>().CategoryId;
			set => _categoryId = value;
		}
		
		[SerializeField] 
		private string _description;
		/// <inheritdoc cref="ItemEntity.Description"/>
		public string Description => Entity.GetComponent<ItemComponent>().Description;
		
		[SerializeField] 
		private KitsuneCurrency _currency;
		/// <inheritdoc cref="ItemEntity.CurrencyId"/>
		public EntityId CurrencyId => Entity.GetComponent<CostComponent>().CurrencyId;
		
		[SerializeField] 
		private int _cost;
		/// <inheritdoc cref="ItemEntity.Cost"/>
		public long Cost => Entity.GetComponent<CostComponent>().Cost;

		[SerializeField] 
		private bool _consumable;
		/// <inheritdoc cref="ItemEntity.Consumable"/>
		public bool Consumable => Entity.GetComponent<ConsumableComponent>().Value;
		
		[SerializeField] 
		private bool _stackable;
		/// <inheritdoc cref="ItemEntity.Stackable"/>
		public bool Stackable => Entity.GetComponent<StackableComponent>().Value;
		
		[SerializeField] 
		private bool _tradeable;
		/// <inheritdoc cref="ItemEntity.Tradeable"/>
		public bool Tradeable => Entity.GetComponent<TradeableComponent>().Value;
		
		[SerializeField] 
		private int _levelRequirement;
		/// <inheritdoc cref="ItemEntity.LevelRequirement"/>
		public int LevelRequirement => Entity.GetComponent<LevelRequirementComponent>().Value;

		public int CompareTo(KitsuneItem other)
		{
			return Id.CompareTo(other.Id);
		}
		
		public override KitsuneEntity Entity 
		{
			get
			{
				if (_entity == null)
				{
					_entity = new ItemEntity();
					_entity.AddComponent(new ItemComponent());
					_entity.AddComponent(new CostComponent());
					_entity.AddComponent(new ConsumableComponent());
					_entity.AddComponent(new StackableComponent());
					_entity.AddComponent(new TradeableComponent());
					_entity.AddComponent(new LevelRequirementComponent());
					_entity.AddComponent(new VersionComponent());
				}
				
				_entity.Id = Id;
				_entity.VersionId = VersionId;
				_entity.GameId = KitsuneManager.GameSettings.GameId;
				_entity.Name = Name;
				_entity.ReleaseVersion = _releaseVersion;
				_entity.GetComponent<ItemComponent>().CategoryId = ItemCategoryId.Mk(_categoryId);
				_entity.GetComponent<ItemComponent>().Description = _description;
				_entity.GetComponent<CostComponent>().CurrencyId = _currency?.Id;
				_entity.GetComponent<CostComponent>().Cost = _cost;
				_entity.GetComponent<ConsumableComponent>().Value = _consumable;
				_entity.GetComponent<StackableComponent>().Value = _stackable;
				_entity.GetComponent<TradeableComponent>().Value = _tradeable;
				_entity.GetComponent<VersionComponent>().GameVersionId = GameVersionId.Mk(_releaseVersionId);
				_entity.GetComponent<LevelRequirementComponent>().Value = _levelRequirement;
				
				return _entity;
			}
		}
	}
}