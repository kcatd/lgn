using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KitsuneAPI.KitsuneUnity
{
	public abstract class KitsuneSingleton<T> : MonoBehaviour
		where T : MonoBehaviour
	{
		public static bool HasInstance { get; private set; }
		
		private static T _instance;
		private static int _instanceId;
		private static readonly object _lockObject = new object();

		public static T Instance
		{
			get
			{
				lock (_lockObject)
				{
					if (HasInstance)
					{
						return _instance;
					}

					_instance = FindFirstInstance();
					if (!_instance)
					{
						Debug.LogWarning( "The instance of singleton component " + typeof(T) + " was requested, but it doesn't appear to exist in the scene." );
						return null;
					}

					HasInstance = true;
					_instanceId = _instance.GetInstanceID();
					return _instance;
				}
			}
		}

		/// <summary>
		/// Returns true if the object is NOT the singleton instance and should exit early from doing any redundant work.
		/// It will also log a warning if called from another instance in the editor during play mode.
		/// </summary>
		protected bool EnforceSingleton
		{
			get
			{
				if (GetInstanceID() == Instance.GetInstanceID())
				{
					return false;
				}

				if (Application.isPlaying)
				{
					enabled = false;
				}

				return true;
			}
		}

		/// <summary>
		/// Returns true if the object is the singleton instance.
		/// </summary>
		protected bool IsTheSingleton
		{
			get
			{
				lock (_lockObject)
				{
					// We compare against the last known instance ID because Unity destroys objects
					// in random order and this may get called during teardown when the instance is
					// already gone.
					return GetInstanceID() == _instanceId;
				}
			}
		}

		/// <summary>
		/// Returns true if the object is not the singleton instance.
		/// </summary>
		protected bool IsNotTheSingleton
		{
			get
			{
				lock (_lockObject)
				{
					// We compare against the last known instance ID because Unity destroys objects
					// in random order and this may get called during teardown when the instance is
					// already gone.
					return GetInstanceID() != _instanceId;
				}
			}
		}

		static T[] FindInstances()
		{
			var objects = FindObjectsOfType<T>();
			Array.Sort( objects, ( a, b ) => a.transform.GetSiblingIndex().CompareTo( b.transform.GetSiblingIndex() ) );
			return objects;
		}


		static T FindFirstInstance()
		{
			var objects = FindInstances();
			return objects.Length > 0 ? objects[0] : null;
		}
		
		protected virtual void Awake()
		{
			if (Application.isPlaying && Instance)
			{
				if (GetInstanceID() != _instanceId)
				{
					#if UNITY_EDITOR
					Debug.LogWarning( "A redundant instance (" + name + ") of singleton " + typeof(T) + " is present in the scene.", this );
					EditorGUIUtility.PingObject( this );
					#endif
					enabled = false;
				}

				// This might be unnecessary, but just to be safe we do it anyway.
				foreach (var redundantInstance in FindInstances().Where( o => o.GetInstanceID() != _instanceId ))
				{
					redundantInstance.enabled = false;
				}
			}
		}
		
		protected virtual void OnDestroy()
		{
			lock (_lockObject)
			{
				if (GetInstanceID() == _instanceId)
				{
					HasInstance = false;
				}
			}
		}
	}
}