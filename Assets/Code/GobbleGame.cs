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
    public int value;

    public PlayerScoreEntry(PlayerId pid, int score = 0)
    {
        id = pid;
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
    public int addScore;
}

public class GameModeSettings
{
    public Vector2Int   boardSize = new Vector2Int(4, 4);
    public int          gameTime = 1;
    public int          minWordLen = 2;
    public bool         allowBacktrack = false;
}

public class GobbleGame : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] DiceBoardGrid  diceBoard;
    [SerializeField] FoundWordList  wordList;
    [SerializeField] GameScoreBoard scoreBoard;
    [SerializeField] LineRenderer   dragPath;
    [SerializeField] MouseFX        mouseDragFX;

    [Header("UI Panels")]
    [SerializeField] GameObject     gameCanvas;
    [SerializeField] GameObject     loginPanel;
    [SerializeField] GameModePanel  gameModePanel;
    [SerializeField] SummaryPanel   summaryPanel;

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

    List<dice>                      trackingSet = new List<dice>();
    private string                  curTrackingWord = "";
    private bool                    isTracking = false;

    List<PlayerScoreEntry>          playerScoreList = new List<PlayerScoreEntry>();
    List<PlayerTeamEntry>           playerTeamList = new List<PlayerTeamEntry>();
    List<ScoreFXEvent>              scoreFXEvents = new List<ScoreFXEvent>();

    GobbleClient                    client;
    TeamColors                      teamColorTable;
    GameModeSettings                curGameModeSettings;
    float                           gameTime = 0.0f;
    bool                            isGameStarted = false;
    bool                            isOfflineMode = false;

    public GameScoreBoard ScoreBoard    { get { return scoreBoard; } }
    public FoundWordList WordList       { get { return wordList; } }
    public TeamColors TeamColorTable    { get { return teamColorTable; } }
    public bool IsGameStarted           { get { return isGameStarted; } }
    public bool IsHost                  { get { return client.IsHostPlayer; } }
    public bool IsOfflineMode           { get { return isOfflineMode; } }

    // Start is called before the first frame update
    void Start()
    {
        client = GetComponent<GobbleClient>();
        teamColorTable = GetComponent<TeamColors>();
        InitGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (scoreFXEvents.Count > 0)
        {
            ScoreFXEvent e = scoreFXEvents[0];
            //if (e.srcObj.HasStarted)
            if (e.srcObj.Revealed)
            {
                ScoreFX newFX = Instantiate<ScoreFX>(prefabScoreFX, gameCanvas.transform);
                newFX.InitScoreFX(e.ownerID, e.addScore, e.srcObj, e.destObj, this);
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
                dice d = diceBoard.GetDice(mousePos);

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
                dice d = diceBoard.GetDice(mousePos);

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

                    mouseDragFX.Trigger();
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

                FoundWordResult wordResult = wordList.AddWord(curTrackingWord.ToLower(), localPlayerID, teamColorTable.GetTeamColor(teamID, false), ref srcObj);
                if (FoundWordResult.no != wordResult)
                {
                    // yay! -- fx and stuff
                    int fullScoreVal = GetWordScore(curTrackingWord);
                    int scoreVal = fullScoreVal;
                    
                    if (FoundWordResult.partial == wordResult)
                    {
                        scoreVal = Mathf.CeilToInt(0.5f * (float)scoreVal);
                    }
                    
                    PlayerScore destObj = scoreBoard.GetPlayer(localPlayerID);
                    if (null != destObj)
                    {
                        ScoreFXEvent e = new ScoreFXEvent();
                        e.ownerID = localPlayerID;
                        e.addScore = scoreVal;
                        e.srcObj = srcObj;
                        e.destObj = destObj;

                        scoreFXEvents.Add(e);

                        for (int i = 0; i < trackingSet.Count; ++i)
                        {
                            FoundWordFX wordFX = Instantiate<FoundWordFX>(prefabFoundWordFX, gameCanvas.transform);
                            wordFX.InitFoundWordFX(trackingSet[i], srcObj, i);
                        }
                    }

                    if (isOfflineMode)
                    {
                        UpdatePlayerScore(localPlayerID, scoreVal, false);
                    }
                    else
                    {
                        if (!client.IsSpectator)
                            client.DoAddFoundWord(curTrackingWord.ToLower(), fullScoreVal);
                    }
                }
                else
                {
                    // nay! -- fx and stuff
                }
            }
            ClearTracking();
            mouseDragFX.Release();
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
        diceBoard.gameObject.SetActive(false);
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
        gameModePanel.gameObject.SetActive(false);
        diceBoard.gameObject.SetActive(true);
        isGameStarted = true;
    }

    public void EndGame()
    {
        isGameStarted = false;
        ClearTracking();
        mouseDragFX.Release();
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
        diceBoard.gameObject.SetActive(true);
        InitializeBoard(true);
    }

    public void StartOffline()
    {
        isOfflineMode = true;
        loginPanel.gameObject.SetActive(false);
        diceBoard.gameObject.SetActive(false);
        InitializeBoard(true);
    }

    public void InitializeLobby()
    {
        diceBoard.gameObject.SetActive(false);
        summaryPanel.gameObject.SetActive(false);
        gameModePanel.gameObject.SetActive(true);

        if (null != curGameModeSettings)
        {
            gameModePanel.SetGameSettings(curGameModeSettings, false);
        }

        if (isOfflineMode)
        {
            gameModePanel.SetupPlayerControllables(true);
        }
        else
        {
            gameModePanel.SetupPlayerControllables(client.IsHostPlayer);
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
            diceBoard.gameObject.SetActive(true);
            diceBoard.InitializeDiceBoard(diceList, this);
        }
    }

    public void InitializeSummary()
    {
        diceBoard.gameObject.SetActive(false);
        summaryPanel.gameObject.SetActive(true);

        if (isOfflineMode)
        {
            summaryPanel.InitSummary(this, new PlayerId(0));
        }
        else
        {
            summaryPanel.InitSummary(this, client.MyPlayerID);
        }
    }

    public void ClearBoard()
    {
        playerScoreList.Clear();
        playerTeamList.Clear();

        ClearFX();
        ClearTracking();

        mouseDragFX.Release();
        wordList.ClearWords();
        ScoreBoard.ClearScoreBoard();

        if (isOfflineMode)
        {
            scoreBoard.AddPlayer("Player", new PlayerId(0), new Color(1.0f, 1.0f, 1.0f));
        }
        else
        {
            PlayerId localPlayer = client.MyPlayerID;
            if (null != localPlayer)
            {
                scoreBoard.AddPlayer(client.MyPlayerName, localPlayer, new Color(1.0f, 1.0f, 1.0f));
            }
        }
    }

    public string GetGameState()
    {
        // game board layout + state format:
        //
        // board layout | width x height | min word length | cur time / max time
        //
        string result = diceBoard.BoardLayout;

        if (string.IsNullOrEmpty(result))
        {
            result = "nil";
        }
        if (null != curGameModeSettings)
        {
            result += string.Format("|{0}x{1}|{2}|{3}/{4}", curGameModeSettings.boardSize.x, curGameModeSettings.boardSize.y, curGameModeSettings.minWordLen, gameTime, curGameModeSettings.gameTime);
        }
        return result;
    }

    public void UpdateGameState(string data)
    {
        string[] tokens = data.Split('|');

        if (tokens.Length > 0)
        {
            string boardLayout = diceBoard.BoardLayout;

            if (null == curGameModeSettings)
                curGameModeSettings = new GameModeSettings();

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

                    // how much leeway do we want to give before "correcting" the time discrepancy?
                }
                else
                {
                    Debug.LogWarning(string.Format("Invalid game time parameter: {0}", tokens[3]));
                }
            }

            diceBoard.SetBoardSize(curGameModeSettings.boardSize.x, curGameModeSettings.boardSize.y);
            diceBoard.UpdateBoardLayout(boardLayout, this);
            gameModePanel.SetGameSettings(curGameModeSettings, true);
            gameModePanel.SetupPlayerControllables(client.IsHostPlayer);
        }
    }

    public void UpdatePlayerState(int playerID, string playerName, int playerScore, int teamID, string foundWordSet)
    {
        PlayerId id = new PlayerId(playerID);
        UpdatePlayerScore(id, playerScore);

        gameModePanel.UpdatePlayer(id, playerName, teamID, !isOfflineMode && (id == client.HostPlayerID), (!isOfflineMode && id == client.MyPlayerID));
        wordList.UpdateFoundWords(id, foundWordSet, teamColorTable.GetTeamColor(teamID, false));
        scoreBoard.UpdatePlayer(this, id, playerName, playerScore, teamID);
    }

    public void UpdatePlayerTeam(int teamID)
    {
        client.DoUpdatePlayerTeam(teamID);
    }

    public void UpdatePlayerScore(PlayerId playerID, int playerScore, bool isAbsolute = true)
    {
        PlayerScoreEntry score = playerScoreList.Find(x => x.id == playerID);
        if (null == score)
        {
            score = new PlayerScoreEntry(playerID, playerScore);
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

    public int  GetWordScore(string theWord)
    {
        int result = 0;

        if (!string.IsNullOrEmpty(theWord))
        {
            string testWord = theWord.ToLower();

            foreach (var ch in testWord)
            {
                int score = 1;

                foreach (var entry in scoreList)
                {
                    if (entry.characters.IndexOf(ch) >= 0)
                    {
                        score = entry.score;
                        break;
                    }
                }
                result += score;
            }
        }
        return result;
    }

    public int  GetPendingScore(int playerID, ref List<ScoreFXEvent> pendingSet, ref List<ScoreFX> executingSet)
    {
        ScoreFX[] fxSet = GetComponentsInChildren<ScoreFX>();
        int pendingScoreVal = 0;

        foreach (var score in scoreFXEvents)
        {
            if (playerID == score.ownerID)
            {
                pendingSet.Add(score);
                pendingScoreVal += score.addScore;
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

    public int  GetPlayerScore(PlayerId id)
    {
        PlayerScoreEntry score = playerScoreList.Find(x => x.id == id);
        if (null != score)
        {
            return score.value;
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
        diceBoard.SetDiceCollision(false);
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
        foreach (dice d in trackingSet)
        {
            d.SetHighlight(false);
        }
        
        if (null != dragPath)
        {
            dragPath.positionCount = 0;
        }
        
        if (resetDiceCollisionType)
        {
            diceBoard.SetDiceCollision(true);
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
