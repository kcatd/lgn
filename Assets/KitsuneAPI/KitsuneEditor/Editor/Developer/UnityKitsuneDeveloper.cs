using System;
using System.Collections.Generic;
using System.IO;
using KitsuneAPI.KitsuneEditor.Editor.Game;
using KitsuneAPI.KitsuneUnity;
using KitsuneCore;
using KitsuneCore.Broadcast;
using KitsuneCore.Developer;
using KitsuneCore.Entity;
using KitsuneCore.Game;
using KitsuneCore.Services.Achievements;
using KitsuneCore.Services.Authentication;
using KitsuneCore.Services.CustomData;
using KitsuneCore.Services.Inventory;
using KitsuneCore.Services.Monetization.Products;
using KitsuneCore.Services.Monetization.Rewards;
using KitsuneCore.Services.Monetization.VirtualCurrency;
using KitsuneCore.Services.Players;
using UnityEditor;
using UnityEngine;

namespace KitsuneAPI.KitsuneEditor.Editor.Developer
{
	/// <summary>
	/// Handles Kitsune Developer operations such as item and currency management
	/// </summary>
	public class UnityKitsuneDeveloper
	{
		public const string DEV_SETTINGS_FILE_NAME = "DeveloperSettings";
		public const string TMP_ASSET_NAME = "tmp.asset";

		/// <value>
		/// Serialized game settings
		/// </value>
		public static UnityDeveloperSettings DeveloperSettings => AssetDatabase.LoadAssetAtPath<UnityDeveloperSettings>(EditorPrefs.GetString(EEditorPrefs.BASE_DEVELOPER_PATH) + "/" + DEV_SETTINGS_FILE_NAME + ".asset");

		/// <summary>
		/// Register as a new Kitsune Publisher
		/// </summary>
		/// <param name="publisher"></param>
		/// <param name="password"></param>
		/// <param name="onSuccess"></param>
		public static void RegisterPublisher(KitsunePublisher publisher, string password, Action onSuccess)
		{
			Kitsune.Developer.Subscribe<KitsuneEvent.ON_ERROR>(OnError);
			Kitsune.Developer.Subscribe<DeveloperEvent.ON_PUBLISHER_REGISTERED>(OnRegisteredPublisher);
			Kitsune.Developer.RegisterPublisher(publisher, password);
			
			void OnRegisteredPublisher(KitsunePublisher newPublisher)
			{
				DeveloperSettings.Publisher = newPublisher;
				DeveloperSettings.IsRegistered = true;
				EditorUtility.SetDirty(DeveloperSettings);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				onSuccess.Invoke();				
				Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_PUBLISHER_REGISTERED>(OnRegisteredPublisher);
				Kitsune.Developer.Unsubscribe<KitsuneEvent.ON_ERROR>(OnError);
			}

			void OnError(string errorMessage)
			{
				Kitsune.Developer.Unsubscribe<KitsuneEvent.ON_ERROR>(OnError);
				Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_PUBLISHER_REGISTERED>(OnRegisteredPublisher);
			}
		}
		
		public static void PublisherLogin(string email, string password, Action<string> onSuccess)
		{
			Kitsune.Developer.Subscribe<KitsuneEvent.ON_ERROR>(OnError);
			Kitsune.Developer.Subscribe<DeveloperEvent.ON_AUTHENTICATED>(OnPublisherLogin);
			Kitsune.Developer.Login(email, password);
			
			void OnPublisherLogin(PlayerId publisherId, SessionId sessionId, string publisherName)
			{
				if (!DeveloperSettings.IsRegistered)
				{
					DeveloperSettings.Publisher.Email = email;
					DeveloperSettings.Publisher.Name = publisherName;
					DeveloperSettings.IsRegistered = true;
				}
				EditorUtility.SetDirty(DeveloperSettings);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				onSuccess.Invoke(publisherName);				
				Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_AUTHENTICATED>(OnPublisherLogin);
				Kitsune.Developer.Unsubscribe<KitsuneEvent.ON_ERROR>(OnError); // TODO - remove once GetGameData is hooked up

				// Kitsune.Developer.Subscribe<DeveloperEvent.ON_GET_GAME_DATA>(OnGetGameData);
				// Kitsune.Developer.GetGameData();
			}

			void OnGetGameData(string gameJson)
			{
				Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_GET_GAME_DATA>(OnGetGameData);
				Kitsune.Developer.Unsubscribe<KitsuneEvent.ON_ERROR>(OnError);
			}

			void OnError(string errorMessage)
			{
				Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_AUTHENTICATED>(OnPublisherLogin);
				Kitsune.Developer.Unsubscribe<KitsuneEvent.ON_ERROR>(OnError);
			}
		}
		
		/// <summary>
		/// Update your publisher info
		/// </summary>
		/// <param name="publisher"></param>
		/// <param name="onSuccess"></param>
		public static void ManagePublisher(KitsunePublisher publisher, Action onSuccess)
		{
			Kitsune.Developer.Subscribe<KitsuneEvent.ON_ERROR>(OnError);
			Kitsune.Developer.Subscribe<DeveloperEvent.ON_PUBLISHER_UPDATED>(OnPublisherUpdated);
			Kitsune.Developer.ManagePublisher(publisher);
			
			void OnPublisherUpdated(KitsunePublisher updatedPublisher)
			{
				DeveloperSettings.Publisher = publisher;
				onSuccess.Invoke();
				Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_PUBLISHER_UPDATED>(OnPublisherUpdated);
			}
			
			void OnError(string errorMessage)
			{
				Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_PUBLISHER_UPDATED>(OnPublisherUpdated);
			}
		}

		/// <summary>
		/// Creates a new game.
		/// </summary>
		/// <param name="gameTitle"></param>
		/// <param name="onSuccess"></param>
		public static void CreateGame(string gameTitle, Action onSuccess)
		{
			Kitsune.Developer.Subscribe<KitsuneEvent.ON_ERROR>(OnError);
			Kitsune.Developer.Subscribe<DeveloperEvent.ON_GAME_CREATED>(OnGameCreated);
			Kitsune.Developer.CreateGame(gameTitle);
			
			void OnGameCreated(GameVersion gameVersion, GameAPIKey apiKey, string secretKey)
			{
				KitsuneManager.GameSettings.GameApiKey = apiKey;
				KitsuneManager.GameSettings.GameId = apiKey.GameId;
				KitsuneManager.GameSettings.SetGameVersion(gameVersion.Version);
				KitsuneManager.GameSettings.SetUpdateRequired(false);
				KitsuneManager.GameSettings.SetGameVersionId(gameVersion.GameVersionId);
				DeveloperSettings.GameSecretKey = secretKey;
				EditorUtility.SetDirty(DeveloperSettings); // TODO - needed any more?
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				onSuccess.Invoke();				
				Kitsune.Developer.Unsubscribe<KitsuneEvent.ON_ERROR>(OnError);
				Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_GAME_CREATED>(OnGameCreated);
			}
			
			void OnError(string errorMessage)
			{
				Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_GAME_CREATED>(OnGameCreated);
				Kitsune.Developer.Unsubscribe<KitsuneEvent.ON_ERROR>(OnError);
			}
		}
		
		/// <summary>
		/// Update game metadata.
		/// </summary>
		/// <remarks>Update the game name</remarks>
		/// <param name="gameSettings"></param>
		/// <param name="onSuccess"></param>
		public static void ManageGame(IGameSettings gameSettings, Action onSuccess)
		{
			Kitsune.Developer.Subscribe<DeveloperEvent.ON_GAME_UPDATED>(OnGameUpdated);
			Kitsune.Developer.ManageGame(gameSettings);
			
			void OnGameUpdated()
			{
				EditorUtility.SetDirty(KitsuneManager.GameSettings);
				AssetDatabase.SaveAssets();
				onSuccess.Invoke();
				Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_GAME_UPDATED>(OnGameUpdated);
			}
		}
		
		/// <summary>
		/// Update game metadata.
		/// </summary>
		/// <remarks>Update the game version</remarks>
		/// <param name="gameVersion"></param>
		/// <param name="onSuccess"></param>
		public static void ManageGameVersion(GameVersion gameVersion, Action onSuccess)
		{
			Kitsune.Developer.Subscribe<DeveloperEvent.ON_GAME_VERSION_UPDATED>(OnGameVersionUpdated);
			Kitsune.Developer.ManageGameVersion(gameVersion);
			
			void OnGameVersionUpdated(GameVersionId gameVersionId)
			{
				KitsuneManager.GameSettings.SetGameVersionId(gameVersionId);
				EditorUtility.SetDirty(KitsuneManager.GameSettings);
				AssetDatabase.SaveAssets();
				PlayerSettings.bundleVersion = PlayerSettings.macOS.buildNumber = KitsuneManager.GameSettings.GameVersion.Version;
				onSuccess.Invoke();
				Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_GAME_VERSION_UPDATED>(OnGameVersionUpdated);
			}
		}
		
		/// <summary>
		/// Gets all the current versions of the game.
		/// </summary>
		/// <param name="onSuccess"></param>
		public static void GetGameVersions(Action<List<GameVersionInfo>> onSuccess)
		{
			Kitsune.Developer.Subscribe<DeveloperEvent.ON_GET_GAME_VERSIONS>(OnGetGameVersions);
			Kitsune.Developer.GetGameVersions();
			
			void OnGetGameVersions(List<GameVersionInfo> gameVersions)
			{
				onSuccess.Invoke(gameVersions);
				Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_GET_GAME_VERSIONS>(OnGetGameVersions);
			}
		}
		
		/// <summary>
		/// Gets all the current entities for the current game.
		/// </summary>
		/// <param name="onSuccess"></param>
		public static void GetGameEntities(Action<List<EntityInfo>> onSuccess)
		{
			Kitsune.Developer.Subscribe<DeveloperEvent.ON_GET_GAME_ENTITIES>(OnGetGameEntities);
			Kitsune.Developer.GetGameEntities();
			
			void OnGetGameEntities(List<EntityInfo> entities)
			{
				onSuccess.Invoke(entities);
				Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_GET_GAME_ENTITIES>(OnGetGameEntities);
			}
		}
		
		/// <summary>
		/// Update game metadata.
		/// </summary>
		/// <remarks>Update the game version</remarks>
		/// <param name="gameVersion"></param>
		/// <param name="release"></param>
		/// <param name="onSuccess"></param>
		public static void ReleaseGameVersion(GameVersion gameVersion, bool release, Action onSuccess)
		{
			Kitsune.Developer.Subscribe<DeveloperEvent.ON_GAME_VERSION_RELEASED>(OnGameVersionReleased);
			Kitsune.Developer.ReleaseGameVersion(gameVersion, release);
			
			void OnGameVersionReleased(GameVersionId gameVersionId, string releaseVersion)
			{
				KitsuneManager.GameSettings.SetGameVersionId(gameVersionId);
				KitsuneManager.GameSettings.ReleasedGameVersionId = gameVersionId;
				KitsuneManager.GameSettings.ReleasedVersion = releaseVersion;
				EditorUtility.SetDirty(KitsuneManager.GameSettings);
				AssetDatabase.SaveAssets();
				onSuccess.Invoke();
				Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_GAME_VERSION_RELEASED>(OnGameVersionReleased);
			}
		}

		public static void ManageAPIKey(GameAPIKey gameApiKey, Action onSuccess)
		{
			Kitsune.Developer.Subscribe<DeveloperEvent.ON_API_KEY_RECEIVED>(OnAPIKeyReceieved);
			Kitsune.Developer.ManageAPIKey(gameApiKey);
			
			void OnAPIKeyReceieved(IGameSettings gameSettings)
			{
				KitsuneManager.GameSettings.GameId = gameSettings.GameId;
				KitsuneManager.GameSettings.GameApiKey = gameSettings.GameApiKey;
				onSuccess.Invoke();
				Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_API_KEY_RECEIVED>(OnAPIKeyReceieved);
			}
		}

		public static void ManageGameLevelData(KitsuneGameLevelXpData levelXpData, Action onSuccess)
		{
			Kitsune.Developer.Subscribe<DeveloperEvent.ON_GAME_LEVEL_XP_UPDATED>(OnGameLevelXpUpdated);
			Kitsune.Developer.ManageGameLevelXp(levelXpData);
			
			void OnGameLevelXpUpdated(IGameLevelXpData gameLevelXpData)
			{
				onSuccess.Invoke();
				Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_GAME_LEVEL_XP_UPDATED>(OnGameLevelXpUpdated);
			}
		}
		
		public static void ManageEntity<T>(T entity, EEntityOperation op, Action callback) where T : UnityKitsuneEntity
		{
			Action manageAction = null;
			Action OnSuccess = () =>
			{
				RenameAsset(entity);
				
				callback.Invoke();
			};

			Type entityType = typeof(T);
			if (entityType == typeof(KitsuneAchievement))
			{
				manageAction = () => ManageAchievement(op, entity as KitsuneAchievement, OnSuccess);
			}
			else if (entityType == typeof(KitsuneCurrency))
			{
				manageAction = () => ManageCurrency(op, entity as KitsuneCurrency, OnSuccess);
			}
			if (entityType == typeof(KitsuneCustomData))
			{
				manageAction = () => ManageCustomData(op, entity as KitsuneCustomData, OnSuccess);
			}
			if (entityType == typeof(KitsuneItem))
			{
				manageAction = () => ManageItem(op, entity as KitsuneItem, OnSuccess);
			}
			if (entityType == typeof(KitsuneProduct))
			{
				manageAction = () => ManageProduct(op, entity as KitsuneProduct, OnSuccess);
			}
			if (entityType == typeof(KitsuneReward))
			{
				manageAction = () => ManageReward(op, entity as KitsuneReward, OnSuccess);
			}

			manageAction?.Invoke();
		}

		private static void RenameAsset<T>(T entity, int renameAttempt = 0) where T : UnityKitsuneEntity
		{
			string tmpPath = GetAssetDirectoryPath<T>() + "/" + TMP_ASSET_NAME;
			string entityName = entity.Name;
			string newPath =  GetAssetDirectoryPath<T>() + "/" + entityName + ".asset";

			if (File.Exists(newPath))
			{
				renameAttempt++;
				string newAssetName = entityName + "_" + renameAttempt;
				newPath = GetAssetDirectoryPath<T>() + "/" + newAssetName + ".asset";
				if (File.Exists(newPath))
				{
					RenameAsset(entity, renameAttempt);
					return;
				}
				
				entityName = newAssetName;
			}
			
			AssetDatabase.RenameAsset(tmpPath, entityName);
			AssetDatabase.SaveAssets();
		}

		/// <summary>
		/// Deletes the entity from the server ONLY if it has not been released.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="onSuccess"></param>
		public static void DeleteEntity(KitsuneEntity entity, Action onSuccess)
		{
			// ensure the game settings and secret key are set
			KitsuneFacade.SetDeveloperSettings(DeveloperSettings);
			KitsuneFacade.SetGameSettings(KitsuneManager.GameSettings);

			Kitsune.Developer.Subscribe<DeveloperEvent.ON_ENTITY_DELETED>(OnEntityDeleted);
			Kitsune.Developer.DeleteEntity(entity);
			
			void OnEntityDeleted(EntityId id)
			{
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				onSuccess.Invoke();
				Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_ENTITY_DELETED>(OnEntityDeleted);
			}
		}
		
		public static string GetAssetDirectoryPath<T>()
		{
			string directoryPath = EditorPrefs.GetString(EEditorPrefs.BASE_PATH) + "/" + GetEntityShortName<T>();

			return directoryPath;
		}

		public static string GetEntityShortName<T>()
		{
			string entityType = typeof(T).Name;
			return entityType.Substring(7);
		}

		/// <summary>
		/// Create - Creates a new <see cref="CurrencyEntity"/> on the server and returns an EntityId
		/// Update - Updates the <see cref="CurrencyEntity"/> on the server
		/// Delete - Deletes the <see cref="CurrencyEntity"/> from the server
		/// </summary>
		/// <param name="operation"></param>
		/// <param name="currency"></param>
		/// <param name="onSuccess"></param>
		private static void ManageCurrency(EEntityOperation operation, KitsuneCurrency currency, Action onSuccess)
		{
			if (operation == EEntityOperation.Delete)
			{
				DeleteEntity(currency.Entity, onSuccess);
			}
			else
			{
				// ensure the game settings and secret key are set
				KitsuneFacade.SetDeveloperSettings(DeveloperSettings);
				KitsuneFacade.SetGameSettings(KitsuneManager.GameSettings);
				
				Kitsune.Developer.Subscribe<DeveloperEvent.ON_CURRENCY_UPDATED>(OnCurrencyUpdated);
				Kitsune.Developer.ManageCurrency(currency.Entity);
				
				void OnCurrencyUpdated(CurrencyEntity updatedEntity)
				{
					if (operation == EEntityOperation.Create)
					{
						currency.Id = updatedEntity.Id;
					}
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
					onSuccess.Invoke();
					Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_CURRENCY_UPDATED>(OnCurrencyUpdated);
				}
			}
		}
		
		/// <summary>
		/// Create - Creates a new <see cref="CustomDataEntity"/> on the server and returns an EntityId
		/// Update - Updates the <see cref="CustomDataEntity"/> on the server
		/// Delete - Deletes the <see cref="CustomDataEntity"/> from the server
		/// </summary>
		/// <param name="operation"></param>
		/// <param name="customData"></param>
		/// <param name="onSuccess"></param>
		private static void ManageCustomData(EEntityOperation operation, KitsuneCustomData customData, Action onSuccess)
		{
			if (operation == EEntityOperation.Delete)
			{
				DeleteEntity(customData.Entity, onSuccess);
			}
			else
			{
				// ensure the game settings and secret key are set
				KitsuneFacade.SetDeveloperSettings(DeveloperSettings);
				KitsuneFacade.SetGameSettings(KitsuneManager.GameSettings);
				
				Kitsune.Developer.Subscribe<DeveloperEvent.ON_CUSTOM_DATA_UPDATED>(OnCustomDataUpdated);
				Kitsune.Developer.ManageCustomData(customData.Entity);
				
				void OnCustomDataUpdated(CustomDataEntity updatedEntity)
				{
					if (operation == EEntityOperation.Create)
					{
						customData.Id = updatedEntity.Id;
					}
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
					onSuccess.Invoke();
					Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_CUSTOM_DATA_UPDATED>(OnCustomDataUpdated);
				}
			}
		}
		
		/// <summary>
		/// Create - Creates a new <see cref="RewardEntity"/> on the server and returns an EntityId
		/// Update - Updates the <see cref="RewardEntity"/> on the server
		/// Delete - Deletes the <see cref="RewardEntity"/> from the server
		/// </summary>
		/// <param name="operation"></param>
		/// <param name="reward"></param>
		/// <param name="onSuccess"></param>
		private static void ManageReward(EEntityOperation operation, KitsuneReward reward, Action onSuccess)
		{
			if (operation == EEntityOperation.Delete)
			{
				DeleteEntity(reward.Entity, onSuccess);
			}
			else
			{
				// ensure the game settings and secret key are set
				KitsuneFacade.SetDeveloperSettings(DeveloperSettings);
				KitsuneFacade.SetGameSettings(KitsuneManager.GameSettings);

				Kitsune.Developer.Subscribe<DeveloperEvent.ON_REWARD_UPDATED>(OnRewardUpdated);
				Kitsune.Developer.ManageReward(reward.Entity);
				
				void OnRewardUpdated(RewardEntity updatedEntity)
				{
					if (operation == EEntityOperation.Create)
					{
						reward.Id = updatedEntity.Id;
					}
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
					onSuccess.Invoke();
					Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_REWARD_UPDATED>(OnRewardUpdated);
				}
			}
		}

		/// <summary>
		/// Create - Creates a new <see cref="AchievementEntity"/> on the server and returns an EntityId
		/// Update - Updates the <see cref="AchievementEntity"/> on the server
		/// Delete - Deletes the <see cref="AchievementEntity"/> from the server
		/// </summary>
		/// <param name="operation"></param>
		/// <param name="achievement"></param>
		/// <param name="onSuccess"></param>
		private static void ManageAchievement(EEntityOperation operation, KitsuneAchievement achievement, Action onSuccess)
		{
			if (operation == EEntityOperation.Delete)
			{
				DeleteEntity(achievement.Entity, onSuccess);
			}
			else
			{
				// ensure the game settings and secret key are set
				KitsuneFacade.SetDeveloperSettings(DeveloperSettings);
				KitsuneFacade.SetGameSettings(KitsuneManager.GameSettings);

				Kitsune.Developer.Subscribe<DeveloperEvent.ON_ACHIEVEMENT_UPDATED>(OnAchievementUpdated);
				Kitsune.Developer.ManageAchievement(achievement.Entity);
				
				void OnAchievementUpdated(AchievementEntity updatedEntity)
				{
					if (operation == EEntityOperation.Create)
					{
						achievement.Id = updatedEntity.Id;
					}
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
					onSuccess.Invoke();
					Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_ACHIEVEMENT_UPDATED>(OnAchievementUpdated);
				}
			}
		}
		
		/// <summary>
		/// Create - Creates a new <see cref="ProductEntity"/> on the server and returns an EntityId
		/// Update - Updates the <see cref="ProductEntity"/> on the server
		/// Delete - Deletes the <see cref="ProductEntity"/> from the server
		/// </summary>
		/// <param name="operation"></param>
		/// <param name="product"></param>
		/// <param name="onSuccess"></param>
		private static void ManageProduct(EEntityOperation operation, KitsuneProduct product, Action onSuccess)
		{
			if (operation == EEntityOperation.Delete)
			{
				DeleteEntity(product.Entity, onSuccess);
			}
			else
			{
				// ensure the game settings and secret key are set
				KitsuneFacade.SetDeveloperSettings(DeveloperSettings);
				KitsuneFacade.SetGameSettings(KitsuneManager.GameSettings);

				Kitsune.Developer.Subscribe<DeveloperEvent.ON_PRODUCT_UPDATED>(OnProductUpdated);
				Kitsune.Developer.ManageProduct(product.Entity);
				
				void OnProductUpdated(ProductEntity updatedEntity)
				{
					if (operation == EEntityOperation.Create)
					{
						product.Id = updatedEntity.Id;
					}
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
					onSuccess.Invoke();
					Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_PRODUCT_UPDATED>(OnProductUpdated);
				}
			}
		}

		/// <summary>
		/// Create - Creates a new <see cref="ItemEntity"/> on the server and returns an EntityId
		/// Update - Updates the <see cref="ItemEntity"/> on the server
		/// Delete - Deletes the <see cref="ItemEntity"/> from the server, but not instances of the item (TODO - flag them for removal if you want to remove them from players inventory?)
		/// </summary>
		/// <param name="operation"></param>
		/// <param name="item"></param>
		/// <param name="onSuccess"></param>
		private static void ManageItem(EEntityOperation operation, KitsuneItem item, Action onSuccess)
		{
			if (operation == EEntityOperation.Delete)
			{
				DeleteEntity(item.Entity, onSuccess);
			}
			else
			{
				// ensure the game settings and secret key are set
				KitsuneFacade.SetDeveloperSettings(DeveloperSettings);
				KitsuneFacade.SetGameSettings(KitsuneManager.GameSettings);

				Kitsune.Developer.Subscribe<DeveloperEvent.ON_ITEM_UPDATED>(OnItemUpdated);
				Kitsune.Developer.ManageItem(item.Entity);
				
				void OnItemUpdated(ItemEntity updatedEntity)
				{
					if (operation == EEntityOperation.Create)
					{
						item.Id = updatedEntity.Id;
					}
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
					onSuccess.Invoke();
					Kitsune.Developer.Unsubscribe<DeveloperEvent.ON_ITEM_UPDATED>(OnItemUpdated);
				}
			}
		}
	}
}