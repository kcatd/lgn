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

public class GobbleMsgTag
{
	public const MessageTag StartGame = MessageTag.kTagMessageCustomGame1;	// c-> s
	public const MessageTag AddWord = MessageTag.kTagMessageCustomGame2;	// c->s
	public const MessageTag GameState = MessageTag.kTagMessageCustomGame3;  // c->s + s->c
	public const MessageTag PlayerTeam = MessageTag.kTagMessageCustomGame4;  // c->s + s->c
}
public class GobbleTag
{
	public const Tag GameID = Tag.kTagCustomGame1;
	public const Tag PlayersInGame = Tag.kTagCustomGame2;
	public const Tag HostPlayer = Tag.kTagCustomGame3;
	public const Tag GameBoard = Tag.kTagCustomGame4;
	public const Tag PlayerScore = Tag.kTagCustomGame5;
	public const Tag PlayerWords = Tag.kTagCustomGame6;
	public const Tag PlayerTeamID = Tag.kTagCustomGame7;
}
public class GobbleJoinConfig : IJoinGameConfig
{
	public Place Place => Place.Mk(PlaceId.Mk(184), "lgngobble");
	public void AddJoinDetails(MessageBody messageBody)
	{
		// no join details
	}
}

public class UserLoginInfo
{
	public readonly string userName;
	public readonly string accountName;
	public readonly string accountPassword;

	public UserLoginInfo(string strName, string strAcct, string strPwd)
    {
		userName = strName;
		accountName = strAcct;
		accountPassword = strPwd;
    }
}

public class GobbleClient : MonoBehaviour, IGameServerSubscriber, IPlacesSubscriber
{
	[SerializeField] LoginPanel	loginPanel;

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

	private bool                        HasQueuedMessages => _queue.Count > 0;
    private ConcurrentQueue<Message>    _queue = new ConcurrentQueue<Message>();

	private PlayerId					hostPlayerID = new PlayerId(0);
	private PlayerId					myPlayerID = new PlayerId(0);
	private string						myPlayerName = "player";
	private UserLoginInfo				pendingLoginInfo;
	private int							gameID = 0;
	private int							myTeamID = 0;

	public string						MyPlayerName { get { return myPlayerName; } }
	public PlayerId						MyPlayerID { get { return myPlayerID; } }
	public PlayerId						HostPlayerID { get { return hostPlayerID; } }
	public int							MyTeamID { get { return myTeamID; } }
	public bool							IsHostPlayer { get { return (hostPlayerID > 0) && (hostPlayerID == myPlayerID); } }
	public bool							IsSpectator { get { return myTeamID < 0; } }

	// Start is called before the first frame update
	void Start()
    {
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

    // Update is called once per frame
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
        Kitsune.GameServer.Subscribe(GobbleMsgTag.GameState);
	}

    private void OnPlayerCreated(PlayerId newPlayerId)
    {
        StatusMessage("Account Created with PlayerId=" + newPlayerId);

        Thread.Sleep(1500);

        StatusMessage("Logging In...");

		if (null != pendingLoginInfo)
		{
			DoLogin(pendingLoginInfo.userName, pendingLoginInfo.accountName, pendingLoginInfo.accountPassword);
		}
		else
		{
			DoLogin("guest", "guest@boomzap.com", "guest");
		}
    }

	private void OnAuthenticated(PlayerId playerId, SessionId sessionId)
	{
		myPlayerID = playerId;

		if (null != pendingLoginInfo)
		{
			myPlayerName = pendingLoginInfo.userName;
			pendingLoginInfo = null;
		}
		StatusMessage("Authenticated. PlayerId=" + playerId + " SessionId=" + sessionId);
	}

	private void OnConnected()
	{
		StatusMessage("Connected");
		loginPanel.LoginState = LoginStateID.LoggedIn;

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

		// Join the game
		IJoinGameConfig joinGameConfig = new GobbleJoinConfig();
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
		Output.Debug(this, "OnGetPlayer");
		/*
		_playerNameText.text = player.Name;
		_playerLevel.text = "Level: " + player.Level;
		_playerIdText.text = "PlayerId: " + player.PlayerId;
		_playerEmailText.text = "Email: " + player.Email;
		*/
	}

	private void OnGetCurrencies(List<CurrencyEntity> currencies)
	{
		Output.Debug(this, "OnGetCurrencies");
		/*
		TextMeshProUGUI currencyLabels = Instantiate(_textFieldPrefab, _currencyContainer.transform);
		currencyLabels.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 100);
		currencyLabels.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -27);

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
			addCurrencyButton.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 30);
			addCurrencyButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(i * 160, -80);
			addCurrencyButton.GetComponentInChildren<TextMeshProUGUI>().text = "Add " + currencies[i].Name;
			addCurrencyButton.onClick.AddListener(() =>
			{
				EntityId id = currencies[index].Id;
				OnAddCurrency(id);
			});
		}
		*/
	}

	private void OnAddCurrency(EntityId currencyId)
	{
		Output.Debug(this, "OnAddCurrency");
		/*
		Kitsune.CurrentPlayer.AdjustBalance(currencyId, 50);
		int adjustedBalance = int.Parse(_currencyBalances[currencyId].text);
		adjustedBalance += 50;
		_currencyBalances[currencyId].text = adjustedBalance.ToString();
		*/
	}

	private void OnPlayerBalances(Dictionary<EntityId, CurrencyBalanceComponent> balances)
	{
		Output.Debug(this, "OnPlayerBalances");
		/*
		foreach (KeyValuePair<EntityId, CurrencyBalanceComponent> balance in balances)
		{
			_currencyBalances[balance.Key].text = balance.Value.Balance.ToString();
		}
		*/
	}

	private void OnGetItems(List<ItemEntity> items)
	{
		Output.Debug(this, "OnGetItems");
		/*
		TextMeshProUGUI itemLabels = Instantiate(_textFieldPrefab, _inventoryContainer.transform);
		itemLabels.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 100);
		itemLabels.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -27);

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
			buyItemButton.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 20);
			buyItemButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(itemQuantity.rectTransform.anchoredPosition.x + 40, itemQuantity.rectTransform.anchoredPosition.y);
			buyItemButton.GetComponentInChildren<TextMeshProUGUI>().text = "Buy - " + items[i].Cost + " " + _currencyCache[items[i].CurrencyId].Name;
			buyItemButton.onClick.AddListener(() =>
			{
				OnBuyClicked(items[index]);
			});
		}
		*/
	}

	private void OnBuyClicked(ItemEntity item)
	{
		Output.Debug(this, "OnBuyClicked");
		/*
		// On success, buys the game item and adds it to the players inventory
		Kitsune.Inventory.BuyGameItem(item.Id, 1, false);
		*/
	}

	private void OnBuyGameItem(ItemEntity item)
	{
		Output.Debug(this, "OnBuyGameItem");
		/*
		long adjustedBalance = int.Parse(_currencyBalances[item.CurrencyId].text);
		adjustedBalance -= item.Cost;
		_currencyBalances[item.CurrencyId].text = adjustedBalance.ToString();

		int adjustedQty = int.Parse(_itemQuantities[item.Id].text);
		adjustedQty += 1;
		_itemQuantities[item.Id].text = adjustedQty.ToString();

		if (item.Consumable && !_consumeButtonIds.Contains(item.Id))
		{
			Button consumeItemButton = Instantiate(_buttonPrefab, _inventoryContainer.transform);
			consumeItemButton.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 30);
			consumeItemButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(_consumeButtonIds.Count * 190, -50);
			consumeItemButton.GetComponentInChildren<TextMeshProUGUI>().text = item.Name;
			consumeItemButton.onClick.AddListener(() =>
			{
				OnConsumeItem(item.Id, item.InventoryItemId);
			});

			_consumeButtonIds.Add(item.Id);
		}
		*/
	}

	private void OnGetPlayerInventory(Dictionary<InventoryItemId, ItemEntity> inventory)
	{
		Output.Debug(this, "OnGetPlayerInventory");
		/*
		foreach (KeyValuePair<InventoryItemId, ItemEntity> itemKV in inventory)
		{
			InventoryItemId inventoryId = itemKV.Key;
			EntityId itemId = itemKV.Value.Id;

			_itemQuantities[itemKV.Value.Id].text = itemKV.Value.Quantity.ToString();

			if (itemKV.Value.Consumable && !_consumeButtonIds.Contains(itemId))
			{
				Button consumeItemButton = Instantiate(_buttonPrefab, _inventoryContainer.transform);
				consumeItemButton.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 30);
				consumeItemButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(_consumeButtonIds.Count * 190, -50);
				consumeItemButton.GetComponentInChildren<TextMeshProUGUI>().text = itemKV.Value.Name;
				consumeItemButton.onClick.AddListener(() =>
				{
					OnConsumeItem(itemId, inventoryId);
				});

				_consumeButtonIds.Add(itemId);
			}
		}
		*/
	}

	private void OnConsumeItem(EntityId itemId, InventoryItemId id)
	{
		Output.Debug(this, "OnConsumeItem");
		/*
		Kitsune.CurrentPlayer.ConsumeItem(id, 1);

		int adjustedQty = int.Parse(_itemQuantities[itemId].text);
		adjustedQty -= adjustedQty > 0 ? 1 : 0;
		_itemQuantities[itemId].text = adjustedQty.ToString();
		*/
	}

	private void OnGetProducts(List<ProductEntity> products)
	{
		Output.Debug(this, "OnGetProducts");
		/*
		TextMeshProUGUI productLabels = Instantiate(_textFieldPrefab, _storeContainer.transform);
		productLabels.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 100);
		productLabels.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -27);

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
		*/
	}

	private void OnPurchaseProduct(ProductEntity product)
	{
		Output.Debug(this, "OnPurchaseProduct");
		/*
		Kitsune.Monetization.Subscribe<MonetizationEvent.ON_PURCHASE_COMPLETED>(OnProductPurchaseCompleted);
		Kitsune.Monetization.PurchaseProduct(product, null);
		*/
	}

	private void OnProductPurchaseCompleted(ProductEntity purchasedProduct)
	{
		Output.Debug(this, "OnProductPurchaseCompleted");
		/*
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
		*/
	}

	private void OnGetPlayerProfile(PlayerProfile profile)
	{
		Output.Debug(this, "OnGetPlayerProfile");
	}

	private void OnDisconnected()
	{
		StatusMessage("Disconnected");

		_currencyBalances.Clear();
		_itemQuantities.Clear();
		_itemCache.Clear();
		_productCache.Clear();
		_currencyCache.Clear();
		_consumeButtonIds.Clear();

		loginPanel.LoginState = LoginStateID.LoggedOut;
	}

	private void OnRoomChat(ChatMessage chatMessage)
	{
		Output.Debug(this, "OnRoomChat");
		/*
		_chatBox.text += "\n" + Kitsune.Players.GetPlayerFromCache(chatMessage.FromPlayer).Name + ": " + chatMessage.Text;
		*/
	}

	public void OnMessageReceived(Message message)
	{
		Output.Debug(this, "OnMessageReceived");
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
			case GobbleMsgTag.GameState: HandleGameStateUpdate(msg); break;
		}
	}

	public void OnError(string message)
	{
		OnErrorEvent(message);
	}

	private void OnErrorEvent(string errorMessage)
	{
		StatusMessage(errorMessage, "red");
		loginPanel.LoginState = LoginStateID.LoggedOut;
	}

	private void StatusMessage(string message, string color = "black")
	{
		/*
		_statusMessageText.gameObject.SetActive(true);
		_statusMessageText.text = "<color=" + color + ">" + message + "</color>";
		*/
		Output.Debug(this, string.Format("StatusMessage: {0}", message));
	}

	public void OnRoomJoined(List<KitsunePlayer> players)
	{
		StatusMessage("Room Joined");

		foreach (var p in players)
		{
			Debug.Log(p.Name);
		}
	}

	public void OnLeftRoom(string roomName)
	{
		StatusMessage("Left Room");
	}

	public void OnPlayersUpdated(List<KitsunePlayer> players)
	{
		Debug.Log("Updated Players List");

		foreach (var p in players)
        {
			Debug.Log(p.Name);
        }
	}

	private void HandleGameStateUpdate(Message msg)
	{
		Output.Debug(this, "HandleGameStateUpdate");
		GobbleGame game = GetComponent<GobbleGame>();

		int curGameID = msg.GetInteger(GobbleTag.GameID);
		int hostID = msg.GetInteger(GobbleTag.HostPlayer);
		string boardLayout = msg.GetString(GobbleTag.GameBoard);
		bool beginNewGame = false;

		//Debug.LogError(string.Format("update state: {0}", boardLayout));
		if (gameID != curGameID)
		{
			beginNewGame = true;
			game.ClearBoard();
		}
		gameID = curGameID;
		hostPlayerID = new PlayerId(hostID);

		game.UpdateGameState(boardLayout);

		MessageBody playersBody = msg.GetChildByTag(GobbleTag.PlayersInGame);
		if ((null != playersBody) && playersBody.HasChildren)
		{
			List<MessageBody> players = playersBody.GetChildrenByTag(Tag.kTagUser);
			foreach (var player in players)
			{
				int playerID = player.GetInteger(Tag.kDBUserId);
				string playerName = player.GetString(Tag.kDBName);
				int curScore = player.GetInteger(GobbleTag.PlayerScore);
				int teamID = player.GetInteger(GobbleTag.PlayerTeamID);
				string foundWords = player.GetString(GobbleTag.PlayerWords);

				if (playerID == myPlayerID)
					myTeamID = teamID;

				game.UpdatePlayerState(playerID, playerName, curScore, teamID, foundWords);
			}
		}

		if ((gameID > 0) && !game.IsGameStarted)
		{
			if (beginNewGame)
			{
				//game.InitializeBoard();
			}
			game.StartGame();
		}
		else if ((0 == gameID) && game.IsGameStarted)
		{
			game.EndGame();
		}

		/*
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
		*/
	}

	private void OnStartGame()
	{
		Output.Debug(this, "OnStartGame");
		// send game message with no data
		Kitsune.GameServer.Send(GameMsgTag.StartGame);
	}

	private void OnAccumulateValue()
	{
		Output.Debug(this, "OnAccumulateValue");
		/*
		if (string.IsNullOrEmpty(_accumulatorValueInputTF.text))
		{
			StatusMessage("Accumulator value can't be empty");
		}

		// send game message with data
		Message message = Message.Mk(GameMsgTag.AddValue);
		message.AddInteger(GameTag.Accumulator, int.Parse(_accumulatorValueInputTF.text));
		Kitsune.GameServer.Send(message);
		*/
	}

	public void	DoLogin(string userName, string accountName, string accountPassword, bool createNewUser = false)
	{
		UserLoginInfo loginInfo = new UserLoginInfo(userName, accountName, accountPassword);

		if (createNewUser)
		{
			StatusMessage("Creating...");

			Kitsune.Players.CreatePlayer(loginInfo.userName, loginInfo.accountName, loginInfo.accountPassword);
			//Kitsune.Players.CreateGuest();
			//loginInfo = null;
		}
		else
		{
			StatusMessage("Connecting...");

			Kitsune.Authentication.Login(loginInfo.accountName, loginInfo.accountPassword);
		}

		loginPanel.LoginState = LoginStateID.LoggingIn;
		pendingLoginInfo = loginInfo;
	}

	public void DoLogout()
	{
		loginPanel.LoginState = LoginStateID.LoggedOut;
		Kitsune.Authentication.Logout();
	}

	public void DoStartGame(string boardLayout)
	{
		Message message = Message.Mk(GobbleMsgTag.StartGame);
		message.AddString(GobbleTag.GameBoard, boardLayout);
		Kitsune.GameServer.Send(message);
	}

	public void DoEndGame()
	{
		if (gameID > 0)
		{
			Message message = Message.Mk(GobbleMsgTag.StartGame);
			message.AddString(GobbleTag.GameBoard, "nil");
			message.AddInteger(GobbleTag.GameID, gameID);
			Kitsune.GameServer.Send(message);
		}
	}

	public void DoAddFoundWord(string foundWordStr, int scoreVal)
	{
		Message message = Message.Mk(GobbleMsgTag.AddWord);
		message.AddInteger(GobbleTag.GameID, gameID);
		message.AddString(GobbleTag.PlayerWords, foundWordStr);
		message.AddInteger(GobbleTag.PlayerScore, scoreVal);
		Kitsune.GameServer.Send(message);
	}

	public void DoUpdateGameState(string boardLayout)
    {
		Message message = Message.Mk(GobbleMsgTag.GameState);
		message.AddString(GobbleTag.GameBoard, boardLayout);
		Kitsune.GameServer.Send(message);
    }

	public void DoUpdatePlayerTeam(int teamID)
    {
		Message message = Message.Mk(GobbleMsgTag.PlayerTeam);
		message.AddInteger(GobbleTag.PlayerTeamID, teamID);
		Kitsune.GameServer.Send(message);
    }
}
