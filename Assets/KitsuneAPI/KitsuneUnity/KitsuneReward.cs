using System;
using System.Collections.Generic;
using KitsuneCore.Entity;
using KitsuneCore.Entity.Components;
using KitsuneCore.Game;
using KitsuneCore.Services.Monetization.Rewards;
using UnityEngine;

namespace KitsuneAPI.KitsuneUnity
{
	/// <summary>
	/// Represents a reward that can be added to an IAP
	/// A Unity ScriptableObject Wrapper for the Kitsune RewardEntity
	/// </summary>
	[Serializable]
	public class KitsuneReward : UnityKitsuneEntity
	{
		[SerializeField]
		private string _description;
		/// <inheritdoc cref="RewardEntity.Description"/>
		public string Description => Entity.GetComponent<DescriptionComponent>().Value;
		
		[SerializeField] private List<int> _itemQuantities;
		public List<int> ItemQuantities
		{
			get
			{
				if (_itemQuantities == null)
				{
					_itemQuantities = new List<int>();
				}

				return _itemQuantities;
			}
			set => _itemQuantities = value;
		}
		[SerializeField]
		private List<KitsuneItem> _itemRewards;
		public List<KitsuneItem> ItemRewards
		{
			get
			{
				if (_itemRewards == null)
				{
					_itemRewards = new List<KitsuneItem>();
				}

				return _itemRewards;
			}
			set => _itemRewards = value;
		}
		[SerializeField] 
		private List<int> _currencyQuantities;
		public List<int> CurrencyQuantities
		{
			get
			{
				if (_currencyQuantities == null)
				{
					_currencyQuantities = new List<int>();
				}

				return _currencyQuantities;
			}
			set => _currencyQuantities = value;
		}
		[SerializeField] 
		private List<KitsuneCurrency> _currencyRewards;
		public List<KitsuneCurrency> CurrencyRewards
		{
			get
			{
				if (_currencyRewards == null)
				{
					_currencyRewards = new List<KitsuneCurrency>();
				}

				return _currencyRewards;
			}
			set => _currencyRewards = value;
		}
		
		/// <inheritdoc cref="RewardEntity.Rewards"/>
		public List<RewardPartComponent> Rewards => Entity.GetComponent<RewardPartsComponent>().Rewards;

		public override KitsuneEntity Entity 
		{
			get
			{
				if (_entity == null)
				{
					_entity = new RewardEntity();
					_entity.AddComponent(new RewardPartsComponent());
					_entity.AddComponent(new DescriptionComponent());
					_entity.AddComponent(new VersionComponent());
				}
				
				_entity.Id = Id;
				_entity.VersionId = VersionId;
				_entity.GameId = KitsuneManager.GameSettings.GameId;
				_entity.Name = Name;
				_entity.ReleaseVersion = _releaseVersion;

				List<RewardPartComponent> rewardParts = new List<RewardPartComponent>();
				for (int i = 0; i < _itemRewards.Count; ++i)
				{
					// don't add item rewards without quantities
					if (_itemQuantities.Count <= i) break;
					RewardPartComponent rewardPart = new RewardPartComponent
					{
						RewardedEntityId = _itemRewards[i].Id,
						Quantity = _itemQuantities[i],
					};
					rewardParts.Add(rewardPart);
				}
				
				for (int i = 0; i < _currencyRewards.Count; ++i)
				{
					// don't add currency rewards without quantities
					if (_currencyQuantities.Count <= i) break;
					RewardPartComponent rewardPart = new RewardPartComponent
					{
						RewardedEntityId = _currencyRewards[i].Id,
						Quantity = _currencyQuantities[i],
					};
					rewardParts.Add(rewardPart);
				}

				_entity.GetComponent<DescriptionComponent>().Value = _description;
				_entity.GetComponent<RewardPartsComponent>().Rewards = rewardParts;
				_entity.GetComponent<VersionComponent>().GameVersionId = GameVersionId.Mk(_releaseVersionId);
			
				return _entity;
			}
		}
	}
}