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

    List<ScoreFXEvent>              scoreFXEvents = new List<ScoreFXEvent>();

    GobbleClient                    client;
    GameModeSettings                curGameModeSettings;
    float                           gameTime = 0.0f;
    bool                            isGameStarted = false;
    bool                            isOfflineMode = false;

    public GameScoreBoard ScoreBoard    { get { return scoreBoard; } }
    public FoundWordList WordList       { get { return wordList; } }
    public bool IsGameStarted           { get { return isGameStarted; } }
    public bool IsHost                  { get { return client.IsHostPlayer; } }
    public bool IsOfflineMode           { get { return isOfflineMode; } }

    // Start is called before the first frame update
    void Start()
    {
        client = GetComponent<GobbleClient>();
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

                if (isOfflineMode)
                    localPlayerID = new PlayerId(0);
                else
                    localPlayerID = client.MyPlayerID;

                FoundWordResult wordResult = wordList.AddWord(curTrackingWord.ToLower(), localPlayerID, ref srcObj);
                if (FoundWordResult.no != wordResult)
                {
                    // yay! -- fx and stuff
                    int scoreVal = GetWordScore(curTrackingWord);
                    
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

                    if (!isOfflineMode)
                        client.DoAddFoundWord(curTrackingWord.ToLower());
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
            client.DoStartGame(diceBoard.BoardLayout);
        }
    }

    public void StartGame()
    {
        gameModePanel.gameObject.SetActive(false);
        isGameStarted = true;
    }

    public void EndGame()
    {
        isGameStarted = false;
        ClearTracking();
        mouseDragFX.Release();
        InitializeSummary();
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
        ClearFX();
        ClearTracking();

        mouseDragFX.Release();
        wordList.ClearWords();
        ScoreBoard.ClearScoreBoard();

        if (isOfflineMode)
        {
            scoreBoard.AddPlayer("Player", new PlayerId(0));
        }
        else
        {
            PlayerId localPlayer = client.MyPlayerID;
            if (null != localPlayer)
            {
                scoreBoard.AddPlayer(client.MyPlayerName, localPlayer);
            }
        }
    }

    public void UpdateGameState(string boardLayout)
    {
        diceBoard.UpdateBoardLayout(boardLayout, this);
    }

    public void UpdatePlayerState(int playerID, string playerName, int playerScore, string foundWordSet)
    {
        PlayerId id = new PlayerId(playerID);
        wordList.UpdateFoundWords(id, foundWordSet);
        scoreBoard.UpdatePlayer(this, id, playerName, playerScore);
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

    private void StartTracking()
    {
        ClearTracking();
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

    private void ClearTracking()
    {
        foreach (dice d in trackingSet)
        {
            d.SetHighlight(false);
        }
        if (null != dragPath)
        {
            dragPath.positionCount = 0;
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
