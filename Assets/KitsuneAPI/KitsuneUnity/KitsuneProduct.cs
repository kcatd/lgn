using System;
using System.Collections.Generic;
using KitsuneCore.Entity;
using KitsuneCore.Entity.Components;
using KitsuneCore.Game;
using KitsuneCore.Services.Monetization.Products;
using UnityEngine;

namespace KitsuneAPI.KitsuneUnity
{
	/// <summary>
	/// Represents a product that can be purchased with real money
	/// A Unity ScriptableObject Wrapper for the Kitsune ProductEntity
	/// </summary>
	[Serializable]
	public class KitsuneProduct : UnityKitsuneEntity
	{
		[SerializeField] 
		private string _description;
		/// <inheritdoc cref="ProductEntity.Description"/>
		public string Description => Entity.GetComponent<ProductComponent>().Description;
		
		[SerializeField] 
		private string _billingDescription;
		/// <inheritdoc cref="ProductEntity.BillingDescription"/>
		public string BillingDescription => Entity.GetComponent<ProductComponent>().BillingDescription;
		
		[SerializeField] 
		private long _price;
		/// <inheritdoc cref="ProductEntity.Price"/>
		public long Price => Entity.GetComponent<ProductComponent>().Price;

		/// <inheritdoc cref="ProductEntity.DisplayPrice"/>
		public string DisplayPrice => Entity.GetComponent<ProductComponent>().DisplayPrice;
		
		[SerializeField] 
		private KitsuneReward _rewardToPurchase;
		public KitsuneReward RewardToPurchase => _rewardToPurchase;
		
		private EntityId _purchasedEntityId;
		/// <inheritdoc cref="ProductEntity.RewardId"/>
		public EntityId RewardId => Entity.GetComponent<ProductComponent>().RewardId;
		
		[SerializeField] 
		private List<string> _appStoreSKUs;
		/// <inheritdoc cref="ProductEntity.AppStoreSKUs"/>
		public List<string> AppStoreSKUs => Entity.GetComponent<ProductComponent>().AppStoreSKUs;
		
		public override KitsuneEntity Entity 
		{ 
			get
			{
				if (_entity == null)
				{
					_entity = new ProductEntity();
					_entity.AddComponent(new ProductComponent());
					_entity.AddComponent(new VersionComponent());
				}
					
				_entity.Id = Id;
				_entity.VersionId = VersionId;
				_entity.GameId = KitsuneManager.GameSettings.GameId;
				_entity.ReleaseVersion = _releaseVersion;
				_entity.Name = Name;
				_entity.GetComponent<ProductComponent>().Description = _description;
				_entity.GetComponent<ProductComponent>().BillingDescription = _billingDescription;
				_entity.GetComponent<ProductComponent>().Price = _price;
				
				_purchasedEntityId = _rewardToPurchase.Id;
				_entity.GetComponent<ProductComponent>().RewardId = _purchasedEntityId;
				_entity.GetComponent<ProductComponent>().AppStoreSKUs = _appStoreSKUs;
				_entity.GetComponent<VersionComponent>().GameVersionId = GameVersionId.Mk(_releaseVersionId);
	
				return _entity;
			} 
		}
	}
}