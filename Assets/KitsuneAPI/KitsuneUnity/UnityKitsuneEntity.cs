using System;
using System.Collections.Generic;
using System.Text;
using KitsuneCommon.Debug;
using KitsuneCore.Entity;
using KitsuneCore.Entity.Components;
using KitsuneCore.Game;
using Newtonsoft.Json;
using UnityEngine;

namespace KitsuneAPI.KitsuneUnity
{
	/// <summary>
	/// A Serializable container for data to create strongly typed objects from
	/// server messages.
	/// A Unity ScriptableObject Wrapper for the Kitsune KitsuneEntity
	/// </summary>
	[Serializable]
	public abstract class UnityKitsuneEntity : ScriptableObject, IKitsuneEntity
	{
		/// <value>
		/// Prefab for this KitsuneEntity
		/// </value>
		[SerializeField] 
		private GameObject _prefab;
		public GameObject Prefab
		{
			get => _prefab;
			set => _prefab = value;
		}
		
		/// <value>
		/// Holds reference to the native <see cref="KitsuneEntity"/>
		/// </value>
		protected KitsuneEntity _entity;
		
		private Dictionary<Type, IKitsuneComponent> _components;
		public Dictionary<Type, IKitsuneComponent> Components => _components;

		// Support for populating from JSON
		[JsonProperty(PropertyName="entity_id")]
		[SerializeField] 
		private int _id;
		/// <value>
		/// Every KitsuneEntity has a unique ID  
		/// </value>
		public EntityId Id 
		{
			get { return EntityId.Mk(_id); }
			set { _id = value; }
		}
		
		// Support for populating from JSON
		[JsonProperty(PropertyName="entity_version")]
		[SerializeField] 
		private int _versionId = EntityVersionId.Mk(1);
		/// <value>
		/// Every KitsuneEntity has an EntityVersionId  
		/// </value>
		public EntityVersionId VersionId 
		{
			get { return EntityVersionId.Mk(_versionId); }
			set { _versionId = value; }
		}
		
		// Support for populating from JSON
		[JsonProperty(PropertyName="game_id")]
		[SerializeField] 
		private GameId _gameId = GameId.Zero;
		/// <value>
		/// Every KitsuneEntity contains a reference to the game id it is associated with  
		/// </value>
		public GameId GameId 
		{
			get { return _gameId; }
			set { _gameId = value; }
		}
		
		// Support for populating from JSON
		[JsonProperty(PropertyName="name")]
		[SerializeField]
		private string _name = "New Entity";
		/// <value>
		/// Every entity has a name for internal or display purposes
		/// </value>
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}
		
		/// <value>
		/// True if this Entity contains custom data
		/// </value>
		public bool HasCustomData => HasComponent<CustomDataComponent>();
		
		/// <value>
		/// Optional custom data serialized JSON string
		/// </value>
		public string CustomData => HasCustomData ?
			GetComponent<CustomDataComponent>().SerializedData :
			"";

		/// <value>
		/// Returns a typed custom data object
		/// </value>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetCustomDataAs<T>()
		{
			if (!HasCustomData)
			{
				return default;
			}
			
			return GetComponent<CustomDataComponent>().DeserializeObject<T>();
		}
		
		[SerializeField] 
		protected string _releaseVersion;
		/// <value>
		/// Release version string this entity will be released on
		/// </value>
		public string ReleaseVersion
		{
			get => _releaseVersion;
			set => _releaseVersion = value;
		}
		
		[SerializeField] 
		protected int _releaseVersionId;
		/// <value>
		/// Release <see cref="ReleaseVersionId"/> this entity will be released on
		/// </value>
		public GameVersionId ReleaseVersionId => Entity.GetComponent<VersionComponent>().GameVersionId;

		public IKitsuneEntity AddComponent(IKitsuneComponent component)
		{
			Type componentType = component.GetType();
			if (Components.ContainsKey(componentType))
			{
				Output.Warn(this,"Entity already has component of type" + componentType.Name + " - Removing existing component");

				Components.Remove(componentType);
			}
			
			Components.Add(component.GetType(), component);

			return this;
		}

		public IKitsuneEntity RemoveComponent(IKitsuneComponent component)
		{
			Components.Remove(component.GetType());

			return this;
		}
		
		public string ToJson()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("{");
			sb.Append(" \"Id\" : " + Id);
			sb.Append(" \"GameId\" : " + GameId);
			sb.Append(" \"Name\" : " + Name);
			sb.Append(" \"Components\" : ");
			sb.Append("{[");
			foreach (KeyValuePair<Type, IKitsuneComponent> entry in Components)
			{
				sb.Append(entry.Value.ToJson());
			}
			sb.Append(" ]}");
			sb.Append(" }");
			
			return sb.ToString();
		}

		public bool HasComponent<T>() 
		{
			return _components.ContainsKey(typeof(T));
		}

		public bool HasComponent(Type type)
		{
			return _components.ContainsKey(type);
		}

		public T GetComponent<T>()
		{
			return (T)Components[typeof(T)];
		}
		
		public abstract KitsuneEntity Entity { get; }
	}
}