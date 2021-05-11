using System;
using KitsuneCore.Entity;
using KitsuneCore.Entity.Components;
using KitsuneCore.Game;
using KitsuneCore.Services.CustomData;
using UnityEngine;

namespace KitsuneAPI.KitsuneUnity
{
	/// <summary>
	/// Custom Data can be added to a game or added to a parent entity
	/// A Unity ScriptableObject Wrapper for the Kitsune CustomDataEntity
	/// </summary>
	[Serializable]
	public class KitsuneCustomData : UnityKitsuneEntity, IComparable<KitsuneCustomData>
	{
		[HideInInspector]
		[SerializeField]
		private string _json;
		
		[SerializeField] 
		private string _description; 
		/// <value>
		/// Editor only description of the custom data
		/// </value>
		public string Description => _description;
		
		public int CompareTo(KitsuneCustomData other)
		{
			return Id.CompareTo(other.Id);
		}

		[SerializeField] private UnityKitsuneEntity _parentEntity;
		public EntityId ParentId => Entity.GetComponent<CustomDataComponent>().ParentId;
		
		public override KitsuneEntity Entity 
		{
			get
			{
				if (_entity == null)
				{
					_entity = new CustomDataEntity();
					_entity.AddComponent(new CustomDataComponent());
					_entity.AddComponent(new VersionComponent());
				}
				
				_entity.Id = Id;
				_entity.VersionId = VersionId;
				_entity.GameId = KitsuneManager.GameSettings.GameId;
				_entity.ReleaseVersion = _releaseVersion;
				_entity.Name = Name;
				_entity.GetComponent<VersionComponent>().GameVersionId = GameVersionId.Mk(_releaseVersionId);
				_entity.GetComponent<CustomDataComponent>().SerializedData = _json;
				if (_parentEntity)
				{
					_entity.GetComponent<CustomDataComponent>().ParentId = _parentEntity.Id;
				}

				return _entity;
			}
		}
	}
}