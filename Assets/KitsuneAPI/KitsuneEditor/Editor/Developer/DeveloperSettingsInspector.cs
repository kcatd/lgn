using KitsuneAPI.KitsuneEditor.Editor.UI.UIElements;
using KitsuneAPI.KitsuneUnity;
using KitsuneCore;
using KitsuneCore.Broadcast;
using KitsuneCore.Developer;
using KitsuneCore.Game;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KitsuneAPI.KitsuneEditor.Editor.Developer
{
	[CustomEditor(typeof(UnityDeveloperSettings))]
	public class DeveloperSettingsInspector : KitsuneBaseInspector
	{	
		private Button _registerButton;
		private Button _loginButton;
		private Button _logoutButton;
		private Button _cancelButton;
		private Button _saveButton;
		private Button _newButton;

		public override VisualElement CreateInspectorGUI()
		{
			Kitsune.Developer.Subscribe<KitsuneEvent.ON_ERROR>(OnError);
			Kitsune.Developer.Subscribe<DeveloperEvent.ON_API_KEY_RECEIVED>(OnAPIKeyReceived);
			
			VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(EditorPrefs.GetString(EEditorPrefs.BASE_EDITOR_PATH) + "/UI/UXML/Publisher/Publisher.uxml");
			_rootElement = visualTree.CloneTree();

			if (KitsuneFacade.Authenticated)
			{
				SetContext(EPublisherContext.Authenticated);
			}
			else if (UnityKitsuneDeveloper.DeveloperSettings.IsRegistered)
			{
				SetContext(EPublisherContext.Registered);
			}
			else if (!UnityKitsuneDeveloper.DeveloperSettings.IsRegistered)
			{
				SetContext(EPublisherContext.Unregistered);
			}
			
			UQueryBuilder<Button> settingsButtons =_rootElement.Query<Button>(className: "publisher-settings-button");
			settingsButtons.ForEach(SetupSettingsButtons);

			return _rootElement;
		}
		
		private void SetupSettingsButtons(Button button)
		{
			Image icon = button.Q<Image>(className: "publisher-settings-icon");
			string iconPath = EditorPrefs.GetString(EEditorPrefs.BASE_EDITOR_PATH) + "/Images/Icons/settings_icon.png";
			Texture2D iconAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
			icon.image = iconAsset;
			
			button.tooltip = button.parent.name;
			button.clickable.clicked += OnSettingsClicked;
		}

		private void OnSettingsClicked()
		{
			SetContext(EPublisherContext.Edit);
		}

		private void SetContext(EPublisherContext publisherContext)
		{
			switch (publisherContext)
			{
				case EPublisherContext.Authenticated:
					_rootElement.Q<VisualElement>("Unregistered").AddToClassList("invisible");
					_rootElement.Q<VisualElement>("Registered").AddToClassList("invisible");
					_rootElement.Q<VisualElement>("Edit").AddToClassList("invisible");
					
					_rootElement.Q<VisualElement>("Authenticated").RemoveFromClassList("invisible");

					if (_logoutButton == null)
					{
						_logoutButton = _rootElement.Q<Button>("Logout");
						_logoutButton.clickable.clicked += OnLogoutClicked;
					}
					break;
				case EPublisherContext.Registered:
					// if we have a secret key we don't need to login
					// TODO - always use session id for now until I can verify the game settings are getting saved
					// if (!string.IsNullOrEmpty(UnityKitsuneDeveloper.DeveloperSettings.GameSecretKey))
					// {
					// 	SetContext(EPublisherContext.Authenticated);
					// 	return;
					// }
					_rootElement.Q<VisualElement>("Unregistered").AddToClassList("invisible");
					_rootElement.Q<VisualElement>("Edit").AddToClassList("invisible");
					_rootElement.Q<VisualElement>("Authenticated").AddToClassList("invisible");
					// TODO add secret key foldout
					_rootElement.Q<VisualElement>("Registered").RemoveFromClassList("invisible");

					if (_loginButton == null)
					{
						_loginButton = _rootElement.Q<Button>("Login");
						_loginButton.clickable.clicked += OnLoginClicked;
					}
					break;
				case EPublisherContext.Unregistered:
					_rootElement.Q<VisualElement>("Edit").AddToClassList("invisible");
					_rootElement.Q<VisualElement>("Authenticated").AddToClassList("invisible");
					_rootElement.Q<VisualElement>("Registered").AddToClassList("invisible");
					
					_rootElement.Q<VisualElement>("Unregistered").RemoveFromClassList("invisible");
					
					if (_registerButton == null)
					{
						_registerButton = _rootElement.Q<Button>("Register");
						_registerButton.clickable.clicked += OnRegisterClicked;
					}
					break;
				case EPublisherContext.Edit:
					_rootElement.Q<VisualElement>("Authenticated").AddToClassList("invisible");
					_rootElement.Q<VisualElement>("Registered").AddToClassList("invisible");
					_rootElement.Q<VisualElement>("Unregistered").AddToClassList("invisible");

					_rootElement.Q<VisualElement>("Edit").RemoveFromClassList("invisible");

					if (_cancelButton == null)
					{
						_cancelButton = _rootElement.Q<Button>("Cancel");
						_cancelButton.clickable.clicked += OnCancel;
					}
					
					if (_saveButton == null)
					{
						_saveButton = _rootElement.Q<Button>("Save");
						_saveButton.clickable.clicked += OnSave;
					}
					
					if (_newButton == null)
					{
						_newButton = _rootElement.Q<Button>("New");
						_newButton.clickable.clicked += OnNew;
					}
					break;
			}
		}

		private void OnCancel()
		{
			SetContext(EPublisherContext.Authenticated);
		}

		private void OnSave()
		{
			UnityKitsuneDeveloper.DeveloperSettings.Publisher.Name = serializedObject.FindProperty("_companyName").stringValue;
			UnityKitsuneDeveloper.DeveloperSettings.Publisher.Email = serializedObject.FindProperty("_email").stringValue;
			
			KitsuneFacade.SetDeveloperSettings(UnityKitsuneDeveloper.DeveloperSettings);

			AssetDatabase.SaveAssets();

			UnityKitsuneDeveloper.ManagePublisher(UnityKitsuneDeveloper.DeveloperSettings.Publisher, () =>
				{
					SetContext(EPublisherContext.Authenticated);
				});
		}

		private void OnNew()
		{
			UnityKitsuneDeveloper.DeveloperSettings.Publisher.Name = "";
			UnityKitsuneDeveloper.DeveloperSettings.password = "";
			UnityKitsuneDeveloper.DeveloperSettings.GameSecretKey = "";
			UnityKitsuneDeveloper.DeveloperSettings.IsRegistered = false;
			KitsuneFacade.SetDeveloperSettings(UnityKitsuneDeveloper.DeveloperSettings);

			KitsuneManager.GameSettings.Title = "";
			KitsuneManager.GameSettings.GameId = GameId.Zero;
			KitsuneManager.GameSettings.GameVersion = null;
			KitsuneManager.GameSettings.GameApiKey = null;
			KitsuneFacade.SetGameSettings(KitsuneManager.GameSettings);
			
			AssetDatabase.SaveAssets();
			
			DispatchEvent(UnityKitsuneDeveloper.DeveloperSettings);
			
			SetContext(EPublisherContext.Unregistered);
		}

		private void OnRegisterClicked()
		{
			_rootElement.SetEnabled(false);

			UnityKitsuneDeveloper.DeveloperSettings.Publisher.Name = serializedObject.FindProperty("_companyName").stringValue;
			UnityKitsuneDeveloper.DeveloperSettings.Publisher.Email = serializedObject.FindProperty("_email").stringValue;
			
			KitsuneFacade.SetDeveloperSettings(UnityKitsuneDeveloper.DeveloperSettings);
			
			AssetDatabase.SaveAssets();

			SetStatus("Registering...");
			
			TextField password = _rootElement.Query<TextField>("Password");
			UnityKitsuneDeveloper.RegisterPublisher(UnityKitsuneDeveloper.DeveloperSettings.Publisher, password.value, () =>
			{
				SetStatus("Registration Successful");

				SetContext(EPublisherContext.Authenticated);
				
				_rootElement.SetEnabled(true);
				
				DispatchEvent(UnityKitsuneDeveloper.DeveloperSettings);
			});
		}

		private void OnLoginClicked()
		{
			_rootElement.SetEnabled(false);

			SetStatus("Logging in...");

			TextField password = _rootElement.Query<TextField>("Password");
			UnityKitsuneDeveloper.PublisherLogin(serializedObject.FindProperty("_email").stringValue, password.value, publisherName=>
			{
				SetStatus("Login Succesful");

				SetContext(EPublisherContext.Authenticated);
				_rootElement.SetEnabled(true);
			});
		}

		private void OnLogoutClicked()
		{
			Kitsune.Authentication.Logout();
			SetContext(EPublisherContext.Registered);
			
			SetStatus("Logged Out");
		}

		private void OnSuccessMessage(string successMessage)
		{
			SetStatus(successMessage, StatusMessage.EStatusType.Success);
		}
		
		private void OnError(string errorMessage)
		{
			SetStatus(errorMessage, StatusMessage.EStatusType.Error);
		}

		private void OnAPIKeyReceived(IGameSettings gameSettings)
		{
			KitsuneManager.GameSettings.GameApiKey = gameSettings.GameApiKey;
			EditorUtility.SetDirty(KitsuneManager.GameSettings);
		}
	}

	public enum EPublisherContext
	{
		Unregistered,
		Registered,
		Authenticated,
		Edit
	}
}