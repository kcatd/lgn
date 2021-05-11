using System;
using KitsuneCore.Developer;
using KitsuneCore.Game;
using KitsuneCore.Server;
using UnityEngine;

namespace KitsuneAPI.KitsuneUnity
{
	/// <summary>
	/// A ScriptableObject that stores the game information including
	///  - Title
	///  - Version Info
	///  - GameId
	///  - ApiKey
	/// A Unity ScriptableObject Wrapper for the Kitsune GameSettings
	/// </summary>
	[Serializable]
	public class KitsuneGameSettings : ScriptableObject, IGameSettings
	{
		/// <value>
		/// Game Name
		/// </value>
		[SerializeField] private string _title;
		public string Title
		{
			get => _title;
			set => _title = value;
		}

		/// <value>
		/// If 0, adds a new version. Else, updates existing version.
		/// </value>
		[SerializeField] private int _gameVersionId;
		/// <value>
		/// The current version id of the current released game.
		/// </value>
		[SerializeField] private int _releasedGameVersionId;
		public GameVersionId ReleasedGameVersionId
		{
			get => GameVersionId.Mk(_releasedGameVersionId);
			set => _releasedGameVersionId = value;
		}
		/// <value>
		/// Game version string.
		/// </summary>
		[SerializeField] private string _gameVersion;
		/// <value>
		/// The current string version of the current released game.
		/// </value>
		[SerializeField] private string _releasedGameVersion;
		public string ReleasedVersion
		{
			get => _releasedGameVersion;
			set => _releasedGameVersion = value;
		}

		/// <value>
		/// Is an app update required.
		/// </summary>
		[SerializeField] private bool _updateRequired;
		private GameVersion _kitsuneGameVersion;
		public GameVersion GameVersion
		{
			get
			{
				if (_kitsuneGameVersion == null)
				{
					_kitsuneGameVersion = new GameVersion();
				}

				_kitsuneGameVersion.GameId = GameId;
				_kitsuneGameVersion.Version = _gameVersion;
				_kitsuneGameVersion.Version = _gameVersion;
				_kitsuneGameVersion.UpdateRequired = _updateRequired;
				_kitsuneGameVersion.GameVersionId = GameVersionId.Mk(_gameVersionId);
				return _kitsuneGameVersion;
			}
			set 
			{ 
				_kitsuneGameVersion = value;
				if (_kitsuneGameVersion == null)
				{
					_gameVersionId = 0;
					_gameVersion = "";
					_updateRequired = false;
				}
				else
				{
					_gameVersionId = _kitsuneGameVersion.GameVersionId.Value;
					_gameVersion = _kitsuneGameVersion.Version;
					_updateRequired = _kitsuneGameVersion.UpdateRequired;
				}
			} 
		}

		// TODO - BADBAD - version id and version cannot be set by the GameVersion accessor do to 
		// the game settings scriptable object
		public void SetGameVersionId(GameVersionId gameVersionId)
		{
			_gameVersionId = gameVersionId;
			GameVersion.GameVersionId = gameVersionId;
		}

		public void SetGameVersion(string version)
		{
			_gameVersion = version;
			GameVersion.Version = version;
		}
		
		public void SetUpdateRequired(bool value)
		{
			_updateRequired = value;
			GameVersion.UpdateRequired = value;
		}

		/// <value>
		/// Kitsune GameId
		/// </value>
		[HideInInspector]
		[SerializeField] private int _gameId;
		public GameId GameId
		{
			get => GameId.Mk(_gameId);
			set => _gameId = value;
		}

		[SerializeField] private int _apiKeyId;
		/// <value>
		/// Nickname for the API Key
		/// </value>
		[SerializeField] private string _apiKeyNickname = "Default";
		/// <value>
		/// Kitsune Developer API SecretKey
		/// </value>
		[SerializeField] private string _gameApiKey;
		private GameAPIKey _kitsuneGameAPIKey;
		public GameAPIKey GameApiKey
		{
			get
			{
				if (_kitsuneGameAPIKey == null)
				{
					_kitsuneGameAPIKey = new GameAPIKey
					{
						Enabled = true
					};
				}

				_kitsuneGameAPIKey.GameId = GameId.Mk(_gameId);
				_kitsuneGameAPIKey.Name = _apiKeyNickname;
				_kitsuneGameAPIKey.PublicKey = _gameApiKey;
				_kitsuneGameAPIKey.APIKeyId = APIKeyId.Mk(_apiKeyId);

				return _kitsuneGameAPIKey;
			}
			set
			{
				_kitsuneGameAPIKey = value;
				if (_kitsuneGameAPIKey == null)
				{
					_apiKeyNickname = "";
					_gameApiKey = "";
					_gameId = 0;
					_apiKeyId = 0;
				}
				else
				{
					_apiKeyNickname = _kitsuneGameAPIKey.Name;
					_gameApiKey = _kitsuneGameAPIKey.PublicKey;
					_gameId = _kitsuneGameAPIKey.GameId;
					_apiKeyId = _kitsuneGameAPIKey.APIKeyId;
				}
			}
		}

		/// <value>
		/// Kitsune Server To Use (i.e production, staging, dev, etc)
		/// </value>
		[SerializeField] private KitsuneServerSettings _serverSettings;
		public IServerSettings ServerSettings
		{
			get => _serverSettings;
			set => _serverSettings = (KitsuneServerSettings) value;
		}
	}
}