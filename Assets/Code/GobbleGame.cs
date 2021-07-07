using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using KitsuneCore.Services.Players;

[System.Serializable]
public struct DiceScoreEntry
{
    public int score;
    public string characters;
}

public class PlayerScoreEntry
{
    public readonly PlayerId id;
    public string name;
    public int value;

    public PlayerScoreEntry(PlayerId pid, string nameStr, int score = 0)
    {
        id = pid;
        name = nameStr;
        value = score;
    }
}

public class PlayerTeamEntry
{
    public readonly int id;
    public List<PlayerId> players;

    public PlayerTeamEntry(int tid)
    {
        id = tid;
        players = new List<PlayerId>();
    }
}

public class ScoreFXEvent
{
    public FoundWord srcObj;
    public PlayerScore destObj;
    public PlayerId ownerID;
}

public class GameModeSettings
{
    public Vector2Int   boardSize = new Vector2Int(5, 5);
    public int          gameTime = 1;
    public int          minWordLen = 2;
    public bool         allowBacktrack = false;
    public bool         enableDoubleLetterScore = true;
    public bool         enableTripleLetterScore = true;
    public bool         enableDoubleWordScore = true;
    public bool         enableTripleWordScore = true;
}

public class GobbleGame : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] DiceBoardGrid  diceBoard;
    [SerializeField] FoundWordList  wordList;
    [SerializeField] GameScoreBoard scoreBoard;
    [SerializeField] LineRenderer   dragPath;

    [Header("UI Panels")]
    [SerializeField] GameObject     gameCanvas;
    [SerializeField] GameObject     loginPanel;
    [SerializeField] GameObject     gameBoardPanel;
    [SerializeField] GameModePanel  gameModePanel;
    [SerializeField] SummaryPanel   summaryPanel;
    [SerializeField] SummaryGroup   summaryGroupPanels;

    [Header("UI Elements")]
    [SerializeField] BackgroundImage    backgroundImage;
    [SerializeField] TextMeshProUGUI    currentWordText;
    [SerializeField] TextMeshProUGUI    gameTimerText;

    [Header("Prefabs")]
    [SerializeField] ScoreFX        prefabScoreFX;
    [SerializeField] FoundWordFX    prefabFoundWordFX;

    [Header("Properties")]
    [SerializeField] string[]       diceList;
    [SerializeField] DiceScoreEntry[] scoreList;

    List<Dice>                      trackingSet = new List<Dice>();
    private string                  curTrackingWord = "";
    private bool                    isTracking = false;

    List<PlayerScoreEntry>          playerScoreList = new List<PlayerScoreEntry>();
    List<PlayerTeamEntry>           playerTeamList = new List<PlayerTeamEntry>();
    List<ScoreFXEvent>              scoreFXEvents = new List<ScoreFXEvent>();

    GobbleClient                    client;
    GameConstants                   gameConstants;
    TeamColors                      teamColorTable;
    GameModeSettings                curGameModeSettings;
    float                           gameTime = 0.0f;
    bool                            isGameStarted = false;
    bool                            isOfflineMode = false;

    //Vector2Int                      lastScreenRes = new Vector2Int(0, 0);

    public DiceBoardGrid DiceBoard      { get { return diceBoard; } }
    public GameScoreBoard ScoreBoard    { get { return scoreBoard; } }
    public FoundWordList WordList       { get { return wordList; } }
    public List<PlayerScoreEntry> Players { get { return playerScoreList; } }
    public List<PlayerTeamEntry> Teams  { get { return playerTeamList; } }
    public TeamColors TeamColorTable    { get { return teamColorTable; } }
    public GameConstants Constants      { get { return gameConstants; } }
    public bool IsGameStarted           { get { return isGameStarted; } }
    public bool IsHost                  { get { return client.IsHostPlayer; } }
    public bool IsOfflineMode           { get { return isOfflineMode; } }

    // Start is called before the first frame update
    void Start()
    {
        Application.runInBackground = true;
        client = GetComponent<GobbleClient>();
        gameConstants = GetComponent<GameConstants>();
        teamColorTable = GetComponent<TeamColors>();
        InitGame();
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_STANDALONE
        /*
        if ((Screen.width != lastScreenRes.x) || (Screen.height != lastScreenRes.y))
        {
            if (FullScreenMode.Windowed == Screen.fullScreenMode)
            {
                int newWidth = Screen.width;
                int newHeight = Screen.height;

                if (newWidth != lastScreenRes.x)
                {
                    newHeight = newWidth * 3 / 4;
                    Screen.SetResolution(newWidth, newHeight, FullScreenMode.Windowed);
                }
                else
                {
                    newWidth = newHeight * 4 / 3;
                    Screen.SetResolution(newWidth, newHeight, FullScreenMode.Windowed);
                }

                lastScreenRes.x = newWidth;
                lastScreenRes.y = newHeight;
            }
        }
        */
#endif //UNITY_STANDALONE

        if (scoreFXEvents.Count > 0)
        {
            ScoreFXEvent e = scoreFXEvents[0];
            //if (e.srcObj.HasStarted)
            if (e.srcObj.Revealed)
            {
                ScoreFX newFX = Instantiate<ScoreFX>(prefabScoreFX, gameCanvas.transform);
                newFX.InitScoreFX(e.ownerID, e.srcObj, e.destObj, this);
                scoreFXEvents.RemoveAt(0);
            }
        }

        if (Input.GetMouseButton(0))
        {
            bool tryCenteredLine = true;

            if (isTracking)
            {
                Vector3 mousePos = Input.mousePosition;
                Vector3 localPos = Camera.main.ScreenToWorldPoint(mousePos);
                Dice d = diceBoard.GetDice(mousePos);

                if ((null != d) && ((trackingSet.Count < 1) || trackingSet[trackingSet.Count - 1].IsAdjacent(d)))
                {
                    if (((null != curGameModeSettings) && curGameModeSettings.allowBacktrack) || !trackingSet.Exists(x => x == d))
                    {
                        d.SetHighlight(true);
                        trackingSet.Add(d);

                        curTrackingWord += d.FaceValue;
                        RefreshTrackingWord();

                        if (tryCenteredLine)
                        {
                            localPos.x = d.transform.position.x;
                            localPos.y = d.transform.position.y;
                        }
                        dragPath.positionCount = dragPath.positionCount + 1;
                        dragPath.SetPosition(dragPath.positionCount - 1, localPos);
                    }
                }
            }
            else if (isGameStarted)
            {
                Vector3 mousePos = Input.mousePosition;
                Vector3 localPos = Camera.main.ScreenToWorldPoint(mousePos);
                Dice d = diceBoard.GetDice(mousePos);

                if (null != d)
                {
                    StartTracking();
                    d.SetHighlight(true);
                    trackingSet.Add(d);

                    curTrackingWord = d.FaceValue;
                    RefreshTrackingWord();

                    if (tryCenteredLine)
                    {
                        localPos.x = d.transform.position.x;
                        localPos.y = d.transform.position.y;
                    }
                    dragPath.positionCount = dragPath.positionCount + 1;
                    dragPath.SetPosition(dragPath.positionCount - 1, localPos);

                    FXController.instance.StartDragging(FXController.instance.MousePos());
                    //mouseDragFX.Trigger();
                }
            }
        }
        else if (isTracking)
        {
            if (ValidateTrackingWord())
            {
                Debug.Log(string.Format("Tracked dice result: {0}", curTrackingWord));

                PlayerId localPlayerID = null;
                FoundWord srcObj = null;
                int teamID = 0;

                if (isOfflineMode)
                {
                    localPlayerID = new PlayerId(0);
                }
                else
                {
                    localPlayerID = client.MyPlayerID;
                    teamID = client.MyTeamID;
                }

                FoundWordResult wordResult = wordList.AddWord(curTrackingWord.ToLower(), localPlayerID, teamColorTable.GetTeamColor(teamID, false), ref trackingSet, ref srcObj);
                if (FoundWordResult.no != wordResult)
                {
                    // yay! -- fx and stuff
                    int fullScoreVal = GetWordScore(curTrackingWord, trackingSet);
                    int scoreVal = fullScoreVal;
                    List<int> diceIdxSet = new List<int>();

                    foreach (var d in trackingSet)
                        diceIdxSet.Add(d.Idx);
                    
                    if (FoundWordResult.partial == wordResult)
                    {
                        scoreVal = Mathf.CeilToInt(0.5f * (float)scoreVal);
                    }
                    srcObj.SetScore(localPlayerID, scoreVal, diceIdxSet);
                    
                    PlayerScore destObj = scoreBoard.GetPlayer(localPlayerID);
                    if (null != destObj)
                    {
                        /*
                        ScoreFXEvent e = new ScoreFXEvent();
                        e.ownerID = localPlayerID;
                        e.srcObj = srcObj;
                        e.destObj = destObj;

                        scoreFXEvents.Add(e);

                        for (int i = 0; i < trackingSet.Count; ++i)
                        {
                            FoundWordFX wordFX = Instantiate<FoundWordFX>(prefabFoundWordFX, gameCanvas.transform);
                            wordFX.InitFoundWordFX(trackingSet[i], srcObj, i);
                        }*/
                        foreach (var t in trackingSet) t.Pulse();

                        // temporarily fix the scoreboard and scoring - at least until the proper fx is decided and put in
                        srcObj.SetRevealed();
                        scoreBoard.AddScore(localPlayerID, scoreVal);

                        // how successful was it?
                        FXController.instance.StopDragging(FXController.instance.MousePos(), 1);
                    }

                    if (isOfflineMode)
                    {
                        UpdatePlayerScore(localPlayerID, "Player", scoreVal, false);
                    }
                    else
                    {
                        if (!client.IsSpectator)
                            client.DoAddFoundWord(curTrackingWord.ToLower(), fullScoreVal, trackingSet);
                    }
                }
                else
                {
                    // nay! -- fx and stuff
                    FXController.instance.StopDragging(FXController.instance.MousePos(), -1);
                }
            }
            ClearTracking();

            //mouseDragFX.Release();
        }
    }

    private void FixedUpdate()
    {
        string timeStr = "";

        if (IsGameStarted && (gameTime > 0.0f))
        {
            int secs = 0;
            bool localGameOver = false;

            gameTime -= Time.deltaTime;

            if (gameTime < 0.0f)
            {
                gameTime = 0.0f;
                localGameOver = true;
            }

            secs = Mathf.CeilToInt(gameTime);
            timeStr = string.Format("{0}:{1:00}", secs / 60, secs % 60);

            if (localGameOver)
            {
                EndGame();

                if (!isOfflineMode && client.IsHostPlayer)
                    client.DoEndGame();
            }
        }
        gameTimerText.text = timeStr;
    }

    public void InitGame()
    {
        gameBoardPanel.gameObject.SetActive(false);
        loginPanel.gameObject.SetActive(true);
    }

    public void ResetGame(GameModeSettings settings = null)
    {
        curGameModeSettings = settings;

        if (isOfflineMode)
        {
            InitializeBoard();
            StartGame();
        }
        else if (client.IsHostPlayer)
        {
            InitializeBoard();
            client.DoStartGame(GetGameState());
        }
    }

    public void RefreshGameState()
    {
        if (!isOfflineMode && client.IsHostPlayer)
        {
            if (null == curGameModeSettings)
                curGameModeSettings = new GameModeSettings();

            gameModePanel.GetGameSettings(ref curGameModeSettings);
            client.DoUpdateGameState(GetGameState());
        }
    }

    public void StartGame()
    {
        backgroundImage.Randomize();
        gameModePanel.GetComponent<PlayAnimation>().Play("GameSettingsExit", ()=>gameModePanel.gameObject.SetActive(false));

        summaryGroupPanels.EndIfActive();

        gameBoardPanel.gameObject.SetActive(true);
        gameBoardPanel.GetComponent<PlayAnimation>().Play("GameBoardEnter");

        isGameStarted = true;

        if (null != curGameModeSettings)
            gameTime = curGameModeSettings.gameTime;
    }

    public void RestartGame()
    {
        if (null != curGameModeSettings)
        {
            GameModeSettings tmp = curGameModeSettings;
            ResetGame(tmp);
        }
    }

    public void EndGame()
    {
        isGameStarted = false;
        ClearTracking();
        FXController.instance.EndGame();
        InitializeSummary();
    }

    public void ForceEndGame()
    {
        gameTime = 0.0f;
        EndGame();

        if (!isOfflineMode && client.IsHostPlayer)
        {
            client.DoEndGame();
        }
    }

    public void StartLogin(string userName, string accountName, string accountPwd, bool createNewUser)
    {
        client.DoLogin(userName, accountName, accountPwd, createNewUser);
    }

    public void LoginDone()
    {
        loginPanel.gameObject.SetActive(false);
        gameBoardPanel.gameObject.SetActive(true);
        InitializeBoard(true);
    }

    public void StartOffline()
    {
        isOfflineMode = true;
        loginPanel.gameObject.SetActive(false);
        gameBoardPanel.gameObject.SetActive(false);
        InitializeBoard(true);
    }

    public void InitializeLobby()
    {
        bool updateServer = false;

        gameBoardPanel.gameObject.SetActive(false);

        /*
        if (summaryPanel.gameObject.activeInHierarchy)
            summaryPanel.GetComponent<PlayAnimation>().Play("SummaryExit", ()=>summaryPanel.gameObject.SetActive(false));
        */
        summaryGroupPanels.EndIfActive();

        gameModePanel.gameObject.SetActive(true);
        gameModePanel.GetComponent<PlayAnimation>().Play("GameSettingsEnter");

        if (isOfflineMode && (null == curGameModeSettings))
        {
            curGameModeSettings = new GameModeSettings();
        }

        if (null != curGameModeSettings)
        {
            gameModePanel.SetGameSettings(curGameModeSettings, false);
            updateServer = !isOfflineMode && client.IsHostPlayer;
        }

        if (isOfflineMode)
        {
            gameModePanel.SetupPlayerControllables(true);
        }
        else
        {
            gameModePanel.SetupPlayerControllables(client.IsHostPlayer);

            if (updateServer)
                client.DoUpdateGameState(GetGameState());
        }
    }

    public void ReturnToLobby()
    {
        if (!gameModePanel.gameObject.activeSelf)
        {
            InitializeLobby();
        }
    }

    public void InitializeBoard(bool createEmptyBoard = false)
    {
        ClearBoard();

        if (createEmptyBoard)
        {
            isGameStarted = false;
            InitializeLobby();
        }
        else
        {
            if (null != curGameModeSettings)
            {
                diceBoard.SetBoardSize(curGameModeSettings.boardSize.x, curGameModeSettings.boardSize.y);
                gameTime = (float)curGameModeSettings.gameTime;
            }
            gameBoardPanel.gameObject.SetActive(true);
            diceBoard.InitializeDiceBoard(diceList, this, curGameModeSettings);
        }
    }

    public void InitializeSummary()
    {

        backgroundImage.Randomize();
        
        gameBoardPanel.GetComponent<PlayAnimation>().Play("GameBoardExit", ()=>gameBoardPanel.gameObject.SetActive(false));

        /*
        summaryPanel.gameObject.SetActive(true);
        summaryPanel.GetComponent<PlayAnimation>().Play("SummaryEnter");

        if (isOfflineMode)
        {
            summaryPanel.InitSummary(this, new PlayerId(0));
        }
        else
        {
            summaryPanel.InitSummary(this, client.MyPlayerID);
        }
        */
        if (isOfflineMode)
        {
            summaryGroupPanels.StartSummary(this, new PlayerId(0), true);
        }
        else
        {
            summaryGroupPanels.StartSummary(this, client.MyPlayerID, client.IsHostPlayer);
        }
    }

    public void ClearBoard()
    {
        playerScoreList.Clear();
        playerTeamList.Clear();

        ClearFX();
        ClearTracking();

        FXController.instance.EndGame();
        wordList.ClearWords();
        ScoreBoard.ClearScoreBoard();

        if (isOfflineMode)
        {
            scoreBoard.AddPlayer("Player", new PlayerId(0), new Color(1.0f, 1.0f, 1.0f), true);
            playerScoreList.Add(new PlayerScoreEntry(new PlayerId(0), "Player"));
        }
        else
        {
            PlayerId localPlayer = client.MyPlayerID;
            if (null != localPlayer)
            {
                scoreBoard.AddPlayer(client.MyPlayerName, localPlayer, new Color(1.0f, 1.0f, 1.0f), true);
            }
        }
    }

    public string GetGameState()
    {
        // game board layout + state format:
        //
        // board layout | width x height | min word length | cur time / max time | options
        //
        string result = diceBoard.BoardLayout;

        if (string.IsNullOrEmpty(result))
        {
            result = "nil";
        }
        if (null != curGameModeSettings)
        {
            string optionsStr = "";
            int optionsCount = 0;

            if (curGameModeSettings.enableDoubleLetterScore)
            {
                if (optionsCount++ > 0)
                    optionsStr += "+dl";
                else
                    optionsStr = "dl";
            }

            if (curGameModeSettings.enableTripleLetterScore)
            {
                if (optionsCount++ > 0)
                    optionsStr += "+tl";
                else
                    optionsStr = "tl";
            }

            if (curGameModeSettings.enableDoubleWordScore)
            {
                if (optionsCount++ > 0)
                    optionsStr += "+dw";
                else
                    optionsStr = "dw";
            }

            if (curGameModeSettings.enableTripleWordScore)
            {
                if (optionsCount++ > 0)
                    optionsStr += "+tw";
                else
                    optionsStr = "tw";
            }

            if (optionsCount > 0)
            {
                result += string.Format("|{0}x{1}|{2}|{3}/{4}|{5}", curGameModeSettings.boardSize.x, curGameModeSettings.boardSize.y, curGameModeSettings.minWordLen, client.IsHostPlayer ? gameTime : -1.0f, curGameModeSettings.gameTime, optionsStr);
            }
            else
            {
                result += string.Format("|{0}x{1}|{2}|{3}/{4}", curGameModeSettings.boardSize.x, curGameModeSettings.boardSize.y, curGameModeSettings.minWordLen, client.IsHostPlayer ? gameTime : -1.0f, curGameModeSettings.gameTime);
            }
        }
        return result;
    }

    public void UpdateGameState(string data)
    {
        string[] tokens = data.Split('|');

        if (tokens.Length > 0)
        {
            string boardLayout = diceBoard.BoardLayout;
            bool initSettings = false;

            if (null == curGameModeSettings)
            {
                curGameModeSettings = new GameModeSettings();
                initSettings = !isOfflineMode && client.IsHostPlayer;
            }

            if (tokens.Length > 0)
            {
                boardLayout = tokens[0];
            }
            if (tokens.Length > 1)
            {
                string[] sizeStr = tokens[1].Split('x');
                if (2 == sizeStr.Length)
                {
                    curGameModeSettings.boardSize.x = int.Parse(sizeStr[0]);
                    curGameModeSettings.boardSize.y = int.Parse(sizeStr[1]);
                }
                else
                {
                    Debug.LogWarning(string.Format("Invalid board size parameter: {0}", tokens[1]));
                }

                if (tokens.Length > 2)
                {
                    curGameModeSettings.minWordLen = int.Parse(tokens[2]);
                }
                if (tokens.Length > 3)
                {
                    string[] timeStr = tokens[3].Split('/');
                    if (2 == timeStr.Length)
                    {
                        float updateTime = float.Parse(timeStr[0]);
                        curGameModeSettings.gameTime = int.Parse(timeStr[1]);
                    }
                    else
                    {
                        Debug.LogWarning(string.Format("Invalid game time parameter: {0}", tokens[3]));
                    }
                }
                if (tokens.Length > 4)
                {
                    string[] optionToks = tokens[4].Split('+');
                    curGameModeSettings.enableDoubleLetterScore = System.Array.Exists<string>(optionToks, x => 0 == string.Compare("dl", x));
                    curGameModeSettings.enableTripleLetterScore = System.Array.Exists<string>(optionToks, x => 0 == string.Compare("tl", x));
                    curGameModeSettings.enableDoubleWordScore = System.Array.Exists<string>(optionToks, x => 0 == string.Compare("dw", x));
                    curGameModeSettings.enableTripleWordScore = System.Array.Exists<string>(optionToks, x => 0 == string.Compare("tw", x));
                }
            }

            diceBoard.SetBoardSize(curGameModeSettings.boardSize.x, curGameModeSettings.boardSize.y);
            diceBoard.UpdateBoardLayout(boardLayout, this);
            gameModePanel.SetGameSettings(curGameModeSettings, true);
            gameModePanel.SetupPlayerControllables(client.IsHostPlayer);

            if (initSettings)
            {
                client.DoUpdateGameState(GetGameState());
            }
        }
    }

    public void UpdatePlayerState(int playerID, string playerName, int playerScore, int teamID, List<FoundWordNetData> foundWordSet)
    {
        PlayerId id = new PlayerId(playerID);
        UpdatePlayerScore(id, playerName, playerScore);
        UpdatePlayerTeam(id, teamID);

        gameModePanel.UpdatePlayer(id, playerName, teamID, !isOfflineMode && (id == client.HostPlayerID), (!isOfflineMode && id == client.MyPlayerID));
        wordList.UpdateFoundWords(id, foundWordSet, teamColorTable.GetTeamColor(teamID, false));
        scoreBoard.UpdatePlayer(this, id, playerName, playerScore, teamID, client.MyPlayerID == playerID);
    }

    public void UpdatePlayerTeam(int teamID)
    {
        client.DoUpdatePlayerTeam(teamID);
    }

    public void UpdatePlayerScore(PlayerId playerID, string nameStr, int playerScore, bool isAbsolute = true)
    {
        PlayerScoreEntry score = playerScoreList.Find(x => x.id == playerID);
        if (null == score)
        {
            score = new PlayerScoreEntry(playerID, nameStr, playerScore);
            playerScoreList.Add(score);
        }
        else
        {
            if (isAbsolute)
                score.value = playerScore;
            else
                score.value += playerScore;
        }
    }

    public void UpdatePlayerTeam(PlayerId playerID, int teamID)
    {
        if (teamID >= 0)
        {
            PlayerTeamEntry team = playerTeamList.Find(x => x.id == teamID);
            if (null == team)
            {
                team = new PlayerTeamEntry(teamID);
                team.players.Add(playerID);
                playerTeamList.Add(team);
            }
            else
            {
                if (!team.players.Contains(playerID))
                    team.players.Add(playerID);
            }
        }
    }

    public int  GetWordScore(string theWord, List<Dice> diceSet = null)
    {
        int result = 0;
        int wordMultiplier = 1;

        if (!string.IsNullOrEmpty(theWord))
        {
            string testWord = theWord.ToLower();
            int wordCount = testWord.Length;
            int diceCount = 0;

            if (null != diceSet)
                diceCount = diceSet.Count;

            for (int i = 0; i < wordCount; ++i)
            {
                char ch = testWord[i];
                int score = 1;
                int letterMultiplier = 1;

                if (i < diceCount)
                {
                    letterMultiplier = diceSet[i].GetLetterMultiplier();

                    if (gameConstants.UseDnDStyleMultipliers)
                        wordMultiplier += diceSet[i].GetWordMultiplier() - 1;
                    else
                        wordMultiplier *= diceSet[i].GetWordMultiplier();
                }

                foreach (var entry in scoreList)
                {
                    if (entry.characters.IndexOf(ch) >= 0)
                    {
                        score = entry.score;
                        break;
                    }
                }

                result += letterMultiplier * score;
            }
        }
        return wordMultiplier * result;
    }

    public int  GetPendingScore(int playerID, ref List<ScoreFXEvent> pendingSet, ref List<ScoreFX> executingSet)
    {
        ScoreFX[] fxSet = GetComponentsInChildren<ScoreFX>();
        PlayerId srcID = new PlayerId(playerID);
        int pendingScoreVal = 0;

        foreach (var score in scoreFXEvents)
        {
            if (playerID == score.ownerID)
            {
                pendingSet.Add(score);
                pendingScoreVal += score.srcObj.GetScore(srcID);
            }
        }

        foreach (var fx in fxSet)
        {
            if (playerID == fx.PlayerID)
            {
                executingSet.Add(fx);
                pendingScoreVal += fx.Score;
            }
        }
        return pendingScoreVal;
    }

    public string   GetPlayerName(PlayerId id)
    {
        PlayerScoreEntry score = playerScoreList.Find(x => x.id == id);
        if (null != score)
        {
            return score.name;
        }
        return "";
    }

    public int  GetPlayerScore(PlayerId id)
    {
        PlayerScoreEntry score = playerScoreList.Find(x => x.id == id);
        if (null != score)
        {
            return score.value;
        }
        return 0;
    }

    public int  GetPlayerTeam(PlayerId id)
    {
        foreach (var team in playerTeamList)
        {
            if (team.players.Exists(x => x == id))
            {
                return team.id;
            }
        }
        return 0;
    }

    public int  GetTeamScore(int teamID)
    {
        PlayerTeamEntry team = playerTeamList.Find(x => x.id == teamID);
        if (null != team)
        {
            int teamScore = 0;

            foreach (var player in team.players)
            {
                teamScore += GetPlayerScore(player);
            }
            return teamScore;
        }
        return 0;
    }

    public bool IsTeamGame()
    {
        return playerTeamList.Count > 1;
    }

    private void StartTracking()
    {
        ClearTracking(false);
        diceBoard.SetDiceCollision(DiceCollisionType.Secondary);
        isTracking = true;
    }

    private void RefreshTrackingWord()
    {
        currentWordText.text = curTrackingWord;

        if (ValidateTrackingWord())
        {
            currentWordText.color = new Color(0.0f, 1.0f, 0.0f);
        }
        else
        {
            currentWordText.color = new Color(1.0f, 0.0f, 0.0f);
        }
    }

    private bool ValidateTrackingWord()
    {
        int minWordLength = 2;

        if (null != curGameModeSettings)
        {
            minWordLength = curGameModeSettings.minWordLen;
        }
        return curTrackingWord.Length >= minWordLength;
    }

    private void ClearTracking(bool resetDiceCollisionType = true)
    {
        foreach (Dice d in trackingSet)
        {
            d.SetHighlight(false);
        }
        
        if (null != dragPath)
        {
            dragPath.positionCount = 0;
        }
        
        if (resetDiceCollisionType)
        {
            diceBoard.SetDiceCollision(DiceCollisionType.Primary);
        }

        trackingSet.Clear();
        curTrackingWord = "";
        isTracking = false;

        RefreshTrackingWord();
    }

    private void ClearFX()
    {
        FoundWordFX[] wordFX = gameCanvas.GetComponentsInChildren<FoundWordFX>();
        foreach (var fx in wordFX)
        {
            Destroy(fx.gameObject);
        }

        ScoreFX[] allFx = gameCanvas.GetComponentsInChildren<ScoreFX>();
        foreach (var fx in allFx)
        {
            Destroy(fx.gameObject);
        }

        scoreFXEvents.Clear();
    }
}
