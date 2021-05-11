using System.Collections.Generic;
using System.IO;
using KitsuneAPI.KitsuneEditor.Editor.Achievement;
using KitsuneAPI.KitsuneEditor.Editor.Currency;
using KitsuneAPI.KitsuneEditor.Editor.CustomData;
using KitsuneAPI.KitsuneEditor.Editor.Developer;
using KitsuneAPI.KitsuneEditor.Editor.Game;
using KitsuneAPI.KitsuneEditor.Editor.Item;
using KitsuneAPI.KitsuneEditor.Editor.Product;
using KitsuneAPI.KitsuneEditor.Editor.Reward;
using KitsuneAPI.KitsuneEditor.Editor.Server;
using KitsuneAPI.KitsuneEditor.Editor.UI.UIElements;
using KitsuneAPI.KitsuneUnity;
using KitsuneCommon.Debug;
using KitsuneCore;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitsuneAPI.KitsuneEditor.Editor
{
	[InitializeOnLoad]
	public class KitsuneEditorWindow : EditorWindow
	{
		static KitsuneEditorWindow()
		{
			#if UNITY_2018_1_OR_NEWER
			EditorApplication.projectChanged += CreateSettings;
			EditorApplication.hierarchyChanged += CreateSettings;
			#else
			EditorApplication.projectWindowChanged += CreateSettings;
			EditorApplication.hierarchyWindowChanged += CreateSettings;
			#endif
		}
		
		public static Color DEFAULT_COLOR = Color.black;
		
		[MenuItem("Window/Kitsune %K", false, 99)]
		protected static void MenuItemKitsune()
		{
			// Opens the window, otherwise focuses it if it’s already open.
			var window = GetWindow<KitsuneEditorWindow>();

			// Adds a title to the window.
			window.titleContent = new GUIContent("Kitsune Social Gaming API");

			// Sets a minimum size to the window.
			window.minSize = new Vector2(1200, 830);
		}
		
		private VisualElement _rootElement;
		private VisualTreeAsset _visualTree;
		private Label _statusLabel;
		private PopupField<KitsuneServerSettings> _serverSelectionPopup;
		private PopupField<string> _helpPopup;
		private InspectorElement _publisherInspector;
		private InspectorElement _gameInspector;
		private VisualElement _entityListElement;
		private VisualElement _homePage;
		
		public void OnEnable()
		{
			_rootElement = rootVisualElement;
			minSize = new Vector2(800, 600);
			_visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
				EditorPrefs.GetString(EEditorPrefs.BASE_EDITOR_PATH) + "/UI/UXML/KitsuneEditorWindow.uxml");
			
			StyleSheet styleSheet =
				AssetDatabase.LoadAssetAtPath<StyleSheet>(
					EditorPrefs.GetString(EEditorPrefs.BASE_EDITOR_PATH) + "/UI/USS/KitsuneEditorWindow.uss");
			_rootElement.styleSheets.Add(styleSheet);

			_visualTree.CloneTree(_rootElement);

			_entityListElement = _rootElement.Q<VisualElement>("EntityList");
			_homePage = _rootElement.Q<VisualElement>("HomePage");
			
			_statusLabel = _rootElement.Query<Label>("StatusBar");
			_rootElement.RegisterCallback<ChangeEvent<StatusMessage>>(OnStatusUpdate);

			if (!KitsuneManager.GameSettings)
				CreateSettings();
			
			SetLogoButton();
			SetupServerList();
			SetupHelp();
			SetDeveloperSettings();
			if (UnityKitsuneDeveloper.DeveloperSettings.IsRegistered)
			{
				SetGameSettings();
			}

			OnGameSettingsUpdated(null);
		}

		private void OnDeveloperUpdated(ChangeEvent<UnityDeveloperSettings> changeEvent)
		{
			if (!changeEvent.newValue.IsRegistered)
			{
				_rootElement.Q<InspectorElement>("GameSettings").AddToClassList("invisible");
			}
			else
			{
				_rootElement.Q<InspectorElement>("GameSettings").RemoveFromClassList("invisible");
				SetGameSettings();
			}
		}

		private void SetupMenuButton(Button button)
		{
			Image icon = button.Q<Image>(className: "entity-button-icon");
			string iconPath = EditorPrefs.GetString(EEditorPrefs.BASE_EDITOR_PATH) + "/Images/Icons/" +
			                  button.parent.name + "_icon.png";
			Texture2D iconAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
			icon.image = iconAsset;

			Label label = button.Q<Label>(className: "entity-button-label");
			label.text = button.parent.name;

			button.clickable.clicked += () => BindEntityInspector(button.parent.name);
		}

		private void SetLogoButton()
		{
			Button logoButton = _rootElement.Q<Button>("LogoButton");
			logoButton.clickable.clicked += () =>
			{
				_homePage.RemoveFromClassList("invisible");
				_entityListElement.AddToClassList("invisible");
			};
			Image logo = _rootElement.Query<Image>("Logo");
			string logoPath = EditorGUIUtility.isProSkin
				? "/Images/logo_white.png"
				: "/Images/logo_black.png";
			logo.image = AssetDatabase.LoadAssetAtPath<Texture2D>(
				EditorPrefs.GetString(EEditorPrefs.BASE_EDITOR_PATH) + logoPath);
		}

		private void SetDeveloperSettings()
		{
			_publisherInspector = _rootElement.Query<InspectorElement>("Publisher");

			SerializedObject settings = new SerializedObject(UnityKitsuneDeveloper.DeveloperSettings);
			_publisherInspector.Bind(settings);
			
			_rootElement.RegisterCallback<ChangeEvent<UnityDeveloperSettings>>(OnDeveloperUpdated);
		}

		private void SetGameSettings()
		{
			_gameInspector = _rootElement.Query<InspectorElement>("GameSettings");

			SerializedObject gameSettings = new SerializedObject(KitsuneManager.GameSettings);
			_gameInspector.Bind(gameSettings);
			
			_rootElement.RegisterCallback<ChangeEvent<KitsuneGameSettings>>(OnGameSettingsUpdated);
		}

		private void OnGameSettingsUpdated(ChangeEvent<KitsuneGameSettings> changeEvent)
		{
			VisualElement menu = _rootElement.Query<VisualElement>("EntityMenuButtons");
			if (KitsuneManager.GameSettings.GameApiKey.APIKeyId != 0)
			{
				menu.RemoveFromClassList("invisible");
				UQueryBuilder<Button> buttons = menu.Query<Button>();
				buttons.ForEach(SetupMenuButton);
			}
			else
			{
				menu.AddToClassList("invisible");
			}
		}


		private void OnStatusUpdate(ChangeEvent<StatusMessage> changeEvent)
		{
			SetStatus(changeEvent.newValue.Status, changeEvent.newValue.Type);
		}
		
		private void SetStatus(string statusMessage, StatusMessage.EStatusType type)
		{
			switch (type)
			{
				case StatusMessage.EStatusType.Info:
					_statusLabel.RemoveFromClassList("warn-message");
					_statusLabel.RemoveFromClassList("error-message");
					_statusLabel.RemoveFromClassList("success-message");
					break;
				case StatusMessage.EStatusType.Warn:
					_statusLabel.AddToClassList("warn-message");
					_statusLabel.RemoveFromClassList("error-message");
					_statusLabel.RemoveFromClassList("success-message");
					break;
				case StatusMessage.EStatusType.Error:
					_statusLabel.AddToClassList("error-message");
					_statusLabel.RemoveFromClassList("warn-message");
					_statusLabel.RemoveFromClassList("success-message");
					break;
				case StatusMessage.EStatusType.Success:
					_statusLabel.AddToClassList("success-message");
					_statusLabel.RemoveFromClassList("error-message");
					_statusLabel.RemoveFromClassList("warn-message");
					break;
			}
			_statusLabel.text = statusMessage;
		}
		
		private void SetupServerList()
		{
			KitsuneServerList list;
			string listPath = "Assets/KitsuneAPI/Resources/KitsuneServerList.asset";
			if (File.Exists(listPath))
			{
				list = AssetDatabase.LoadAssetAtPath<KitsuneServerList>(listPath);
			}
			else
			{
				list = CreateInstance<KitsuneServerList>();
				string parentPath = Path.GetDirectoryName(listPath);
				if (!Directory.Exists(parentPath))
				{
					Directory.CreateDirectory(parentPath);
					AssetDatabase.ImportAsset(parentPath);
				}

				AssetDatabase.CreateAsset(list, listPath);
				AssetDatabase.SaveAssets();
			}

			if (list.Servers == null)
			{
				list.Servers = new List<KitsuneServerSettings>();
			}
			
			SerializedObject serializedObject = new SerializedObject(KitsuneManager.GameSettings);
			KitsuneServerSettings settings = (KitsuneServerSettings)serializedObject.FindProperty("_serverSettings").objectReferenceValue;
			int index = 0;
			if (settings != null)
			{
				index = list.Servers.IndexOf(settings);
			}

			VisualElement serverContainer = _rootElement.Query<VisualElement>("ServerList");
			_serverSelectionPopup =
				new PopupField<KitsuneServerSettings>(list.Servers, list.Servers[index]);
			serverContainer.Add(_serverSelectionPopup);
			
			if (KitsuneManager.GameSettings.ServerSettings == null)
				OnServerSelected(null);

			_serverSelectionPopup.RegisterCallback<ChangeEvent<KitsuneServerSettings>>(OnServerSelected);
			_serverSelectionPopup.AddToClassList("server-list-items");
			Kitsune.Developer.SetGameSettings(KitsuneManager.GameSettings);
		}
		
		private void SetupHelp()
		{
			VisualElement helpContainer = _rootElement.Query<VisualElement>("HelpList");
			List<string> helpOptions = new List<string>
			{
				"Help...",
				"API Reference", 
				"Kitsune Manual"
			};
			_helpPopup =
				new PopupField<string>(helpOptions, helpOptions[0]);
			helpContainer.Add(_helpPopup);
			_helpPopup.RegisterCallback<ChangeEvent<string>>(OnHelpSelected);
			_helpPopup.AddToClassList("help-list-items");
		}

		private void OnHelpSelected(ChangeEvent<string> changeEvent)
		{
			if (_helpPopup.index != 0)
			{
				string url;
				if (_helpPopup.value == "API Documentation")
				{
					url = "https://flowplay.github.io/kitsune-csharp/";
				}
				else
				{
					url = "https://flowplay.github.io/kitsune-csharp/api/KitsuneAPI.Kitsune.html";
				}
				Application.OpenURL(url);
				_helpPopup.SetValueWithoutNotify("Help...");
			}
		}
		
		private void OnServerSelected(ChangeEvent<KitsuneServerSettings> changeEvent)
		{
			SerializedObject serializedObject = new SerializedObject(KitsuneManager.GameSettings);
			serializedObject.FindProperty("_serverSettings").objectReferenceValue = _serverSelectionPopup.value;
			serializedObject.ApplyModifiedProperties();
			AssetDatabase.SaveAssets();
			Kitsune.Developer.SetGameSettings(KitsuneManager.GameSettings);
		}

		private void BindEntityInspector(string buttonName)
		{
			_homePage.AddToClassList("invisible");
			_entityListElement.RemoveFromClassList("invisible");
			
			SerializedObject serializedObject = null;
			while (_entityListElement.childCount > 0)
			{
				_entityListElement.RemoveAt(0);
			}
			
			switch (buttonName)
			{
				case "Achievements":
					KitsuneAchievementList achievementList = GetOrCreateListAsset<KitsuneAchievement, KitsuneAchievementList>();
					serializedObject = new SerializedObject(achievementList);
					break;
				case "Currency":
					KitsuneCurrencyList currencyList = GetOrCreateListAsset<KitsuneCurrency, KitsuneCurrencyList>();
					serializedObject = new SerializedObject(currencyList);
					break;
				case "Custom Data":
					KitsuneCustomDataList customDataList = GetOrCreateListAsset<KitsuneCustomData, KitsuneCustomDataList>();
					serializedObject = new SerializedObject(customDataList);
					break;
				case "Items":
					KitsuneItemList itemList = GetOrCreateListAsset<KitsuneItem, KitsuneItemList>();
					serializedObject = new SerializedObject(itemList);
					break;
				case "IAPs":
					KitsuneProductList productList = GetOrCreateListAsset<KitsuneProduct, KitsuneProductList>();
					serializedObject = new SerializedObject(productList);
					break;
				case "Levels and XP":
					KitsuneGameLevelXpData gameLevelXpData = LevelXpInspector.GetOrCreateLevelXpData();
					serializedObject = new SerializedObject(gameLevelXpData);
					break;
				case "Rewards":
					KitsuneRewardList rewardList = GetOrCreateListAsset<KitsuneReward, KitsuneRewardList>();
					serializedObject = new SerializedObject(rewardList);
					break;
			}

			if (serializedObject != null)
			{
				InspectorElement listInspector = new InspectorElement();
				listInspector.Bind(serializedObject);
				_entityListElement.Add(listInspector);
			}
		}
		
		public static U GetOrCreateListAsset<T, U>() where T : UnityKitsuneEntity where U : KitsuneBaseEntityList<T>
		{
			string entityType = typeof(T).Name;
			string shortName = entityType.Substring(7);
			string listPath = "Assets/KitsuneAPI/KitsuneEditor/Editor/" + shortName + "/" + entityType + "List.asset";
			if (File.Exists(listPath))
			{
				return AssetDatabase.LoadAssetAtPath<U>(listPath);
			}
			U list = CreateInstance<U>();
			string parentPath = Path.GetDirectoryName(listPath);
			if (!Directory.Exists(parentPath))
			{
				Directory.CreateDirectory(parentPath);
				AssetDatabase.ImportAsset(parentPath);
			}

			AssetDatabase.CreateAsset(list, listPath);
			AssetDatabase.SaveAssets();
			
			return list;
		}

		private static void CreateSettings()
		{
			KitsuneManager.GameSettings = Resources.Load<KitsuneGameSettings>(KitsuneManager.GAME_SETTINGS_FILE_NAME);
			EditorPrefs.SetString(EEditorPrefs.BASE_DEVELOPER_PATH, "Assets/KitsuneAPI/KitsuneEditor/Editor/Developer");
			EditorPrefs.SetString(EEditorPrefs.BASE_EDITOR_PATH, "Assets/KitsuneAPI/KitsuneEditor/Editor");
			EditorPrefs.SetString(EEditorPrefs.BASE_PATH, "Assets/KitsuneAPI/KitsuneData");
			
			if (KitsuneManager.GameSettings != null)
			{
				Kitsune.Developer.SetGameSettings(KitsuneManager.GameSettings);
			}
			else
			{
				Kitsune.Authentication.Logout();
				
				string gameSettingsPath = "Assets/KitsuneAPI/Resources/" + KitsuneManager.GAME_SETTINGS_FILE_NAME + ".asset";
				string settingsPath = Path.GetDirectoryName(gameSettingsPath);
				if (!Directory.Exists(settingsPath))
				{
					Directory.CreateDirectory(settingsPath);
					AssetDatabase.ImportAsset(settingsPath);
				}

				KitsuneGameSettings gameSettings = CreateInstance<KitsuneGameSettings>();
				if (gameSettings != null)
				{
					AssetDatabase.CreateAsset(gameSettings, gameSettingsPath);
				}
				else
				{
					Output.Debug("KitsuneEditorWindow","Failed creating game settings file. CreateInstance(\"KitsuneGameSettings\") returned null.");
				}
			}

			if (UnityKitsuneDeveloper.DeveloperSettings != null)
			{
				KitsuneFacade.SetDeveloperSettings(UnityKitsuneDeveloper.DeveloperSettings);
			}
			else
			{
				Kitsune.Authentication.Logout();
				
				string devSettingsPath = EditorPrefs.GetString(EEditorPrefs.BASE_DEVELOPER_PATH) + "/" + UnityKitsuneDeveloper.DEV_SETTINGS_FILE_NAME + ".asset";
				string settingsPath = Path.GetDirectoryName(devSettingsPath);
				if (!Directory.Exists(settingsPath))
				{
					Directory.CreateDirectory(settingsPath);
					AssetDatabase.ImportAsset(settingsPath);
				}

				UnityDeveloperSettings developerSettings = CreateInstance<UnityDeveloperSettings>();
				if (developerSettings != null)
				{
					AssetDatabase.CreateAsset(developerSettings, devSettingsPath);
				}
				else
				{
					Output.Error("KitsuneEditorWindow","Failed creating developer settings file. CreateInstance(\"KitsuneDeveloperSettings\") returned null.");
				}
			}
		}
	}
}