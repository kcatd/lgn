using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Demo.GameServer;
using KitsuneAPI;
using KitsuneCommon.Debug;
using KitsuneCommon.GameServer;
using KitsuneCommon.Net.Messages;
using KitsuneCore.Broadcast;
using KitsuneCore.Entity;
using KitsuneCore.Entity.Components;
using KitsuneCore.Services.Authentication;
using KitsuneCore.Services.Chat;
using KitsuneCore.Services.GameServer;
using KitsuneCore.Services.Inventory;
using KitsuneCore.Services.Monetization;
using KitsuneCore.Services.Monetization.Products;
using KitsuneCore.Services.Monetization.Rewards;
using KitsuneCore.Services.Monetization.VirtualCurrency;
using KitsuneCore.Services.Places;
using KitsuneCore.Services.Players;
using KitsuneCore.Services.Players.Profile;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
	public class KitsuneDemo : MonoBehaviour, IGameServerSubscriber, IPlacesSubscriber
	{
		// global
		[SerializeField] private TextMeshProUGUI _statusMessageText;
		
		// screens
		[SerializeField] private GameObject _loginScreen;
		[SerializeField] private GameObject _loggedInScreen;
		
		// login
		[SerializeField] private GameObject _loginGroup;
		[SerializeField] private Button _loginButton;
		[SerializeField] private Button _createButton;
		[SerializeField] private Button _logoutButton;
		[SerializeField] private TMP_InputField _loginPlayername;
		[SerializeField] private TMP_InputField _loginEmail;
		[SerializeField] private TMP_InputField _loginPassword;
		
		// player info
		[SerializeField] private TextMeshProUGUI _playerNameText;
		[SerializeField] private TextMeshProUGUI _playerLevel;
		[SerializeField] private TextMeshProUGUI _playerIdText;
		[SerializeField] private TextMeshProUGUI _playerEmailText;

		// currencies
		[SerializeField] private GameObject _currencyContainer;
		private Dictionary<long, TextMeshProUGUI> _currencyBalances = new Dictionary<long, TextMeshProUGUI>(); 
		private Dictionary<long, CurrencyEntity> _currencyCache = new Dictionary<long, CurrencyEntity>();

		// inventory
		[SerializeField] private GameObject _inventoryContainer;
		private Dictionary<long, TextMeshProUGUI> _itemQuantities = new Dictionary<long, TextMeshProUGUI>();
		private List<long> _consumeButtonIds = new List<long>();
		private Dictionary<long, ItemEntity> _itemCache = new Dictionary<long, ItemEntity>();
		
		// products
		[SerializeField] private GameObject _storeContainer;
		private Dictionary<long, ProductEntity> _productCache = new Dictionary<long, ProductEntity>();

		// rewards
		private Dictionary<long, RewardEntity> _rewardCache = new Dictionary<long, RewardEntity>();
		
		// chat
		[SerializeField] private TextMeshProUGUI _chatBox;
		[SerializeField] private TMP_InputField _chatInputField;
		[SerializeField] private Button _chatSendButton;

		// ui prefabs
		[SerializeField] private Button _buttonPrefab;
		[SerializeField] private TextMeshProUGUI _textFieldPrefab;
		[SerializeField] private ProductPanel _productPanelPrefab;
		
		// game server
		[SerializeField] private Button _startGameButton;
		[SerializeField] private Button _sendAccumulatorValueButton;
		[SerializeField] private TMP_InputField _accumulatorValueInputTF;
		[SerializeField] private TextMeshProUGUI _accumulatorValueTotalTF;
		[SerializeField] private TextMeshProUGUI _activePlayerMessageTF;
		[SerializeField] private TextMeshProUGUI _playerListTF;
		[SerializeField] private GameObject _gameContainer;
		private bool HasQueuedMessages => _queue.Count > 0;
		private ConcurrentQueue<Message> _queue = new ConcurrentQueue<Message>();

		void Start()
		{
			_loginButton.onClick.AddListener(OnLoginButton);
			_createButton.onClick.AddListener(OnCreateButton);
			_logoutButton.onClick.AddListener(OnLogoutButton);
			_chatSendButton.onClick.AddListener(OnChatSendButton);
			_startGameButton.onClick.AddListener(OnStartGame);
			_sendAccumulatorValueButton.onClick.AddListener(OnAccumulateValue);
			
			// subscribe to kitsune events
			Kitsune.Players.Subscribe<PlayerEvent.ON_PLAYER_CREATED>(OnPlayerCreated);
			Kitsune.Players.Subscribe<KitsuneEvent.ON_ERROR>(OnErrorEvent);
			Kitsune.Authentication.Subscribe<AuthenticationEvent.ON_AUTHENTICATED>(OnAuthenticated);
			Kitsune.Authentication.Subscribe<AuthenticationEvent.ON_CONNECTED>(OnConnected);
			Kitsune.Authentication.Subscribe<KitsuneEvent.ON_ERROR>(OnError);
			Kitsune.Inventory.Subscribe<KitsuneEvent.ON_ERROR>(OnError);
			Kitsune.Authentication.Subscribe<AuthenticationEvent.ON_DISCONNECTED>(OnDisconnected);
			Kitsune.Chat.Subscribe<ChatEvent.ON_ROOM_CHAT>(OnRoomChat);

			// or register a service subscriber
			Kitsune.Places.RegisterTarget(this);
			
			RegisterGameServer();
		}

		void Update()
		{
			// process game server messages on the main thread
			if (Kitsune.Connected && HasQueuedMessages)
			{
				DequeueMessage();
			}
		}

		private void RegisterGameServer()
		{
			Kitsune.GameServer.RegisterTarget(this);
			Kitsune.GameServer.Subscribe(GameMsgTag.GameState);
		}

		private void OnLoginButton()
		{
			LoginEnabled(false);
			
			StatusMessage("Connecting...");

			Kitsune.Authentication.Login(_loginEmail.text, _loginPassword.text);
		}

		private void OnCreateButton()
		{
			LoginEnabled(false);
			
			Kitsune.Players.CreatePlayer(_loginPlayername.text, _loginEmail.text, _loginPassword.text);
		}
		
		private void OnPlayerCreated(PlayerId newPlayerId)
		{
			StatusMessage("Account Created with PlayerId=" + newPlayerId);
			
			Thread.Sleep(1500);
			
			StatusMessage("Logging In...");
			
			Kitsune.Authentication.Login(_loginEmail.text, _loginPassword.text);
		}

		private void OnLogoutButton()
		{
			_logoutButton.interactable = false;
			Kitsune.Authentication.Logout();
		}

		private void OnAuthenticated(PlayerId playerId, SessionId sessionId)
		{
			StatusMessage("Authenticated. PlayerId=" + playerId + " SessionId=" + sessionId);
		}

		private void OnConnected()
		{
			StatusMessage("Connected");
			_loginScreen.SetActive(false);
			_loggedInScreen.SetActive(true);
			
			Kitsune.Players.Subscribe<PlayerEvent.ON_GET_PLAYER>(OnGetPlayer);
			Kitsune.Players.GetPlayer(Kitsune.MyPlayerId);

			Kitsune.Monetization.Subscribe<MonetizationEvent.ON_GET_CURRENCIES>(OnGetCurrencies);
			Kitsune.Monetization.GetCurrencies();

			Kitsune.Players.Subscribe<PlayerEvent.ON_GET_PLAYER_BALANCES>(OnPlayerBalances);
			Kitsune.Players.GetPlayerBalances();
			
			Kitsune.Inventory.Subscribe<InventoryEvent.ON_BUY_GAME_ITEM>(OnBuyGameItem);
			Kitsune.Inventory.Subscribe<InventoryEvent.ON_GET_ITEMS>(OnGetItems);
			
			Kitsune.Monetization.Subscribe<MonetizationEvent.ON_GET_PRODUCTS>(OnGetProducts);
			Kitsune.Monetization.Subscribe<MonetizationEvent.ON_GET_REWARDS>(OnGetRewards);
			Kitsune.Monetization.GetRewards();
			Kitsune.Monetization.GetProducts();
			
			// Getting player inventory loads game items
			Kitsune.Players.Subscribe<PlayerEvent.ON_GET_PLAYER_INVENTORY>(OnGetPlayerInventory);
			Kitsune.Players.GetPlayerInventory();

			// Join the Accumulator game
			IJoinGameConfig joinGameConfig = new AccumulatorJoinConfig();
			Kitsune.GameServer.JoinGame(joinGameConfig);
		}
		
		private void OnGetRewards(List<RewardEntity> rewards)
		{
			foreach (RewardEntity reward in rewards)
			{
				_rewardCache[reward.Id] = reward;
			}
		}

		private void OnGetPlayer(KitsunePlayer player)
		{
			_playerNameText.text = player.Name;
			_playerLevel.text = "Level: " + player.Level;
			_playerIdText.text = "PlayerId: " + player.PlayerId;
			_playerEmailText.text = "Email: " + player.Email;
		}
		
		private void OnGetCurrencies(List<CurrencyEntity> currencies)
		{
			Output.Debug(this, "OnGetCurrencies");
			TextMeshProUGUI currencyLabels = Instantiate(_textFieldPrefab, _currencyContainer.transform);
			currencyLabels.GetComponent<RectTransform>().sizeDelta = new Vector2( 180, 100);
			currencyLabels.GetComponent<RectTransform>().anchoredPosition = new Vector2( 0, -27);
			
			for (int i = 0; i < currencies.Count; ++i)
			{
				int index = i;

				_currencyCache.Add(currencies[i].Id, currencies[i]);
				currencyLabels.text += currencies[i].Name + ":\n";
				
				TextMeshProUGUI currencyBalanceTF = Instantiate(_textFieldPrefab, _currencyContainer.transform);
				currencyBalanceTF.text = "0";
				currencyBalanceTF.GetComponent<RectTransform>().anchoredPosition = new Vector2(100, -27 + (i * -20));
				_currencyBalances.Add(currencies[i].Id, currencyBalanceTF);
				
				Button addCurrencyButton = Instantiate(_buttonPrefab, _currencyContainer.transform);
				addCurrencyButton.GetComponent<RectTransform>().sizeDelta = new Vector2( 150, 30);
				addCurrencyButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(i * 160, -80);
				addCurrencyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Add " + currencies[i].Name;
				addCurrencyButton.onClick.AddListener(() =>
				{
					EntityId id = currencies[index].Id;
					OnAddCurrency(id);
				});
			}
		}

		private void OnAddCurrency(EntityId currencyId)
		{
			Kitsune.CurrentPlayer.AdjustBalance(currencyId, 50);
			int adjustedBalance = int.Parse(_currencyBalances[currencyId].text);
			adjustedBalance += 50;
			_currencyBalances[currencyId].text = adjustedBalance.ToString();
		}

		private void OnPlayerBalances(Dictionary<EntityId, CurrencyBalanceComponent> balances)
		{
			Output.Debug(this, "OnPlayerBalances");

			foreach (KeyValuePair<EntityId,CurrencyBalanceComponent> balance in balances)
			{
				_currencyBalances[balance.Key].text = balance.Value.Balance.ToString();
			}
		}
		
		private void OnGetItems(List<ItemEntity> items)
		{
			Output.Debug(this, "OnGetItems");
			TextMeshProUGUI itemLabels = Instantiate(_textFieldPrefab, _inventoryContainer.transform);
			itemLabels.GetComponent<RectTransform>().sizeDelta = new Vector2( 180, 100);
			itemLabels.GetComponent<RectTransform>().anchoredPosition = new Vector2( 0, -27);
			
			for (int i = 0; i < items.Count; ++i)
			{
				int index = i;

				_itemCache.Add(items[i].Id, items[i]);

				itemLabels.text += items[i].Name + ":\n";
				
				TextMeshProUGUI itemQuantity = Instantiate(_textFieldPrefab, _inventoryContainer.transform);
				itemQuantity.text = "0";
				itemQuantity.GetComponent<RectTransform>().anchoredPosition = new Vector2(itemLabels.rectTransform.sizeDelta.x + 10, -27 + (i * -20));
				_itemQuantities[items[i].Id] = itemQuantity;
				
				Button buyItemButton = Instantiate(_buttonPrefab, _inventoryContainer.transform);
				buyItemButton.GetComponent<RectTransform>().sizeDelta = new Vector2( 180, 20);
				buyItemButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(itemQuantity.rectTransform.anchoredPosition.x + 40, itemQuantity.rectTransform.anchoredPosition.y);
				buyItemButton.GetComponentInChildren<TextMeshProUGUI>().text = "Buy - " + items[i].Cost + " " + _currencyCache[items[i].CurrencyId].Name;
				buyItemButton.onClick.AddListener(() =>
				{
					OnBuyClicked(items[index]);
				});
			}
		}
		
		private void OnBuyClicked(ItemEntity item)
		{
			// On success, buys the game item and adds it to the players inventory
			Kitsune.Inventory.BuyGameItem(item.Id, 1, false);
		}

		private void OnBuyGameItem(ItemEntity item)
		{
			long adjustedBalance = int.Parse(_currencyBalances[item.CurrencyId].text);
			adjustedBalance -= item.Cost;
			_currencyBalances[item.CurrencyId].text = adjustedBalance.ToString();

			int adjustedQty = int.Parse(_itemQuantities[item.Id].text);
			adjustedQty += 1;
			_itemQuantities[item.Id].text = adjustedQty.ToString();

			if (item.Consumable && !_consumeButtonIds.Contains(item.Id))
			{
				Button consumeItemButton = Instantiate(_buttonPrefab, _inventoryContainer.transform);
				consumeItemButton.GetComponent<RectTransform>().sizeDelta = new Vector2( 180, 30);
				consumeItemButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(_consumeButtonIds.Count * 190, -50);
				consumeItemButton.GetComponentInChildren<TextMeshProUGUI>().text = item.Name;
				consumeItemButton.onClick.AddListener(() =>
				{
					OnConsumeItem(item.Id, item.InventoryItemId);
				});				
					
				_consumeButtonIds.Add(item.Id);
			}
		}

		private void OnGetPlayerInventory(Dictionary<InventoryItemId, ItemEntity> inventory)
		{
			Output.Debug(this, "OnGetPlayerInventory");

			foreach (KeyValuePair<InventoryItemId,ItemEntity> itemKV in inventory)
			{
				InventoryItemId inventoryId = itemKV.Key;
				EntityId itemId = itemKV.Value.Id;
				
				_itemQuantities[itemKV.Value.Id].text = itemKV.Value.Quantity.ToString();

				if (itemKV.Value.Consumable && !_consumeButtonIds.Contains(itemId))
				{
					Button consumeItemButton = Instantiate(_buttonPrefab, _inventoryContainer.transform);
					consumeItemButton.GetComponent<RectTransform>().sizeDelta = new Vector2( 180, 30);
					consumeItemButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(_consumeButtonIds.Count * 190, -50);
					consumeItemButton.GetComponentInChildren<TextMeshProUGUI>().text = itemKV.Value.Name;
					consumeItemButton.onClick.AddListener(() =>
					{
						OnConsumeItem(itemId, inventoryId);
					});				
					
					_consumeButtonIds.Add(itemId);
				}
			}
		}
		
		private void OnConsumeItem(EntityId itemId, InventoryItemId id)
		{
			Kitsune.CurrentPlayer.ConsumeItem(id, 1);
			
			int adjustedQty = int.Parse(_itemQuantities[itemId].text);
			adjustedQty -= adjustedQty > 0 ? 1 : 0;
			_itemQuantities[itemId].text = adjustedQty.ToString();
		}

		private void OnGetProducts(List<ProductEntity> products)
		{
			Output.Debug(this, "OnGetProducts");

			TextMeshProUGUI productLabels = Instantiate(_textFieldPrefab, _storeContainer.transform);
			productLabels.GetComponent<RectTransform>().sizeDelta = new Vector2( 180, 100);
			productLabels.GetComponent<RectTransform>().anchoredPosition = new Vector2( 0, -27);
			
			for (int i = 0; i < products.Count; ++i)
			{
				int index = i;

				_productCache.Add(products[i].Id, products[i]);
				ProductPanel productPanel = Instantiate(_productPanelPrefab, _storeContainer.transform);
				productPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(i * (productPanel.GetComponent<RectTransform>().sizeDelta.x + 15), -30);
				productPanel.Product = products[i];
				productPanel.BuyButton.onClick.AddListener(() => { OnPurchaseProduct(products[index]); });
			}
			
			Output.Debug(this, "Products Cached");
		}

		private void OnPurchaseProduct(ProductEntity product)
		{
			Kitsune.Monetization.Subscribe<MonetizationEvent.ON_PURCHASE_COMPLETED>(OnProductPurchaseCompleted);
			Kitsune.Monetization.PurchaseProduct(product, null);
		}

		private void OnProductPurchaseCompleted(ProductEntity purchasedProduct)
		{
			Kitsune.Monetization.Unsubscribe<MonetizationEvent.ON_PURCHASE_COMPLETED>(OnProductPurchaseCompleted);

			if (_rewardCache != null && _rewardCache.ContainsKey(purchasedProduct.Id))
			{
				RewardEntity reward = _rewardCache[purchasedProduct.Id];
				foreach (RewardPartComponent rewardPart in reward.Rewards)
				{
					if (_itemQuantities.ContainsKey(rewardPart.part_entity_id))
					{
						_itemQuantities[rewardPart.part_entity_id].text += rewardPart.count;
					}
					else if (_currencyBalances.ContainsKey(rewardPart.part_entity_id))
					{
						_currencyBalances[rewardPart.part_entity_id].text += rewardPart.count;
					}
				}
			}
			
			Output.Debug(this, "Product Purchased!" + purchasedProduct);
		}
		
		private void OnGetPlayerProfile(PlayerProfile profile)
		{
			Output.Debug(this, "OnGetPlayerProfile");
		}
		
		private void OnDisconnected()
		{
			StatusMessage("Disconnected");
			_logoutButton.interactable = true;
			_loginScreen.SetActive(true);
			_loggedInScreen.SetActive(false);
			
			_currencyBalances.Clear();
			_itemQuantities.Clear();
			_itemCache.Clear();
			_productCache.Clear();
			_currencyCache.Clear();
			_consumeButtonIds.Clear();

			LoginEnabled(true);
		}
		
		private void OnChatSendButton()
		{
			if (!string.IsNullOrEmpty(_chatInputField.text))
			{
				Kitsune.Chat.SendRoomChat(_chatInputField.text);
				_chatInputField.text = "";
			}
		}

		private void OnRoomChat(ChatMessage chatMessage)
		{
			_chatBox.text += "\n" + Kitsune.Players.GetPlayerFromCache(chatMessage.FromPlayer).Name + ": " + chatMessage.Text;
		}
		
		private void LoginEnabled(bool value)
		{
			_loginButton.interactable = value;
			_loginPlayername.interactable = value;
			_loginEmail.interactable = value;
			_loginPassword.interactable = value;
			_createButton.interactable = value;
		}

		public void OnMessageReceived(Message message)
		{
			_queue.Enqueue(message);
		}

		private void DequeueMessage()
		{
			if (_queue.TryDequeue(out Message msg))
			{
				ProcessMessage(msg);
			}
		}

		private void ProcessMessage(Message msg)
		{
			switch (msg.Tag)
			{
				case GameMsgTag.GameState: HandleGameStateUpdate(msg); break;
			}
		}

		public void OnError(string message)
		{
			OnErrorEvent(message);
		}

		private void OnErrorEvent(string errorMessage)
		{
			StatusMessage(errorMessage, "red");
			LoginEnabled(true);
		}
		
		private void StatusMessage(string message, string color = "black")
		{
			_statusMessageText.gameObject.SetActive(true);
			_statusMessageText.text = "<color=" + color +">" + message+ "</color>";
		}

		public void OnRoomJoined(List<KitsunePlayer> players)
		{
			StatusMessage("Room Joined");
		}

		public void OnLeftRoom(string roomName)
		{
			StatusMessage("Left Room");
		}

		public void OnPlayersUpdated(List<KitsunePlayer> players)
		{
			Debug.Log("Updated Players List");
		}
		
		private void HandleGameStateUpdate(Message msg)
		{
			GameState gameState = new GameState();
			gameState.ParseMessage(msg.Body);
			
			_startGameButton.gameObject.SetActive(!gameState.GameStarted && gameState.HostId == Kitsune.MyPlayerId);
			_startGameButton.interactable = gameState.Players.Count > 1;
			_gameContainer.SetActive(gameState.GameStarted);

			StringBuilder sb = new StringBuilder();
			foreach (PlayerState playersValue in gameState.Players.Values)
			{
				sb.Append(playersValue.Name);
				sb.Append("\n");
			}

			_playerListTF.text = sb.ToString();
			_accumulatorValueInputTF.gameObject.SetActive(gameState.ActivePlayerId == Kitsune.MyPlayerId);
			_sendAccumulatorValueButton.gameObject.SetActive(gameState.ActivePlayerId == Kitsune.MyPlayerId);
			_accumulatorValueTotalTF.text = gameState.Accumulator.ToString();
			if (gameState.GameStarted)
			{
				string activePlayerMessage = gameState.ActivePlayerId == Kitsune.MyPlayerId
					? "Your Turn"
					: "Waiting for " + gameState.Players[gameState.ActivePlayerId].Name;
				_activePlayerMessageTF.text = activePlayerMessage;
			}
			else
			{
				_activePlayerMessageTF.text = "";
			}
		}

		private void OnStartGame()
		{
			// send game message with no data
			Kitsune.GameServer.Send(GameMsgTag.StartGame);
		}

		private void OnAccumulateValue()
		{
			if (string.IsNullOrEmpty(_accumulatorValueInputTF.text))
			{
				StatusMessage("Accumulator value can't be empty");
			}
			
			// send game message with data
			Message message = Message.Mk(GameMsgTag.AddValue);
			message.AddInteger(GameTag.Accumulator, int.Parse(_accumulatorValueInputTF.text));
			Kitsune.GameServer.Send(message);
		}
	}
}