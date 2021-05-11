using System;
using KitsuneCore.Entity;
using KitsuneCore.Entity.Components;
using KitsuneCore.Game;
using KitsuneCore.Services.Monetization.VirtualCurrency;
using UnityEngine;

namespace KitsuneAPI.KitsuneUnity
{
	/// <summary>
	/// Representation of a form of virtual currency
	/// (i.e. Gems, Coins, Diamonds, Gold)
	/// A Unity ScriptableObject Wrapper for the Kitsune CurrencyEntity
	/// </summary>
	[Serializable]
	public class KitsuneCurrency : UnityKitsuneEntity
	{		
		[SerializeField]
		private string _description;
		/// <inheritdoc cref="CurrencyEntity.Description"/>
		public string Description => Entity.GetComponent<DescriptionComponent>().Value;
			
		[SerializeField]
		private ECurrencyType _type = ECurrencyType.SoftCurrency;
		/// <inheritdoc cref="CurrencyEntity.Type"/>
		public ECurrencyType Type => Entity.GetComponent<CurrencyTypeComponent>().Type;
		
		//
		public override KitsuneEntity Entity 
		{
			get
			{
				if (_entity == null)
				{
					_entity = new CurrencyEntity();
					_entity.AddComponent(new CurrencyTypeComponent());
					_entity.AddComponent(new DescriptionComponent());
					_entity.AddComponent(new VersionComponent());
				}
				
				_entity.Id = Id;
				_entity.VersionId = VersionId;
				_entity.GameId = KitsuneManager.GameSettings.GameId;
				_entity.ReleaseVersion = _releaseVersion;
				_entity.Name = Name;
				_entity.GetComponent<CurrencyTypeComponent>().Type = _type;
				_entity.GetComponent<DescriptionComponent>().Value = _description;
				_entity.GetComponent<VersionComponent>().GameVersionId = GameVersionId.Mk(_releaseVersionId);

				return _entity;
			}
		}

		public override string ToString()
		{
			return Name;
		}
	}
}