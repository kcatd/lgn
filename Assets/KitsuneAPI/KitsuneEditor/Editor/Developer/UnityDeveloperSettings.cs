using System;
using KitsuneCore.Developer;
using UnityEngine;

namespace KitsuneAPI.KitsuneEditor.Editor.Developer
{
	[Serializable]
	public class UnityDeveloperSettings : ScriptableObject, IDeveloperSettings
	{
		[SerializeField] private string _secretKey;
		/// <value>
		/// Developer Secret Key used to provision game entities. This is unique to each game.
		/// IMPORTANT! DO NOT Publish This With Your Game!
		/// </value>
		public string GameSecretKey
		{
			get => _secretKey;
			set => _secretKey = value;
		}

		/// <value>
		/// Company Name registered to this API Key
		/// </value>
		[SerializeField] private string _companyName;

		/// <value>
		/// Email registered to this API Key
		/// </value>
		[SerializeField] private string _email;

		/// <value>
		/// Publisher password. This should be cleared after login
		/// </value>
		public string password;

		[SerializeField]
		private bool _isRegistered;
		public bool IsRegistered
		{
			get => _isRegistered;
			set => _isRegistered = value;
		}

		private KitsunePublisher _gamePublisher;
		public KitsunePublisher Publisher
		{
			get
			{
				if (_gamePublisher == null)
				{
					_gamePublisher = new KitsunePublisher();
				}

				_gamePublisher.Name = _companyName;
				_gamePublisher.Email = _email;

				return _gamePublisher;
			}
			set
			{
				_gamePublisher = value;
				
				_companyName = _gamePublisher.Name;
				_email = _gamePublisher.Email;
			}
		}
	}
}