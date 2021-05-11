using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using KitsuneCore.Services.Players;

public class ScoreFXEvent
{
    public FoundWord srcObj;
    public PlayerScore destObj;
    public PlayerId ownerID;
    public int addScore;
}

public class GobbleGame : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] DiceBoardGrid  diceBoard;
    [SerializeField] FoundWordList  wordList;
    [SerializeField] GameScoreBoard scoreBoard;
    [SerializeField] LineRenderer   dragPath;
    [SerializeField] MouseFX        mouseDragFX;

    [Header("UI")]
    [SerializeField] GameObject     gameCanvas;
    [SerializeField] GameObject     loginPanel;

    [Header("Prefabs")]
    [SerializeField] ScoreFX        prefabScoreFX;
    [SerializeField] FoundWordFX    prefabFoundWordFX;

    [Header("Properties")]
    [SerializeField] string[]       diceList;

    List<dice>                      trackingSet = new List<dice>();
    private bool                    isTracking = false;

    List<ScoreFXEvent>              scoreFXEvents = new List<ScoreFXEvent>();

    GobbleClient                    client;
    bool                            isGameStarted = false;

    public GameScoreBoard ScoreBoard    { get { return scoreBoard; } }
    public bool IsGameStarted           { get { return isGameStarted; } }
    public bool IsHost                  { get { return client.IsHostPlayer; } }

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
                    d.SetHighlight(true);
                    trackingSet.Add(d);

                    if (tryCenteredLine)
                    {
                        localPos.x = d.transform.position.x;
                        localPos.y = d.transform.position.y;
                    }
                    dragPath.positionCount = dragPath.positionCount + 1;
                    dragPath.SetPosition(dragPath.positionCount - 1, localPos);
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
            string test = "";
            foreach (dice d in trackingSet)
            {
                test += d.FaceValue;
            }

            if (!string.IsNullOrEmpty(test))
            {
                Debug.Log(string.Format("Tracked dice result: {0}", test));

                PlayerId localPlayerID = client.MyPlayerID;
                FoundWord srcObj = null;
                int scoreVal = wordList.AddWord(test.ToLower(), localPlayerID, ref srcObj);

                if (scoreVal > 0)
                {
                    // yay! -- fx and stuff
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

                    client.DoAddFoundWord(test.ToLower());
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

    public void InitGame()
    {
        diceBoard.gameObject.SetActive(false);
        loginPanel.gameObject.SetActive(true);
    }

    public void ResetGame()
    {
        if (client.IsHostPlayer)
        {
            InitializeBoard();
            client.DoStartGame(diceBoard.BoardLayout);
        }
    }

    public void StartGame()
    {
        isGameStarted = true;
    }

    public void EndGame()
    {
        isGameStarted = false;
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

    public void InitializeBoard(bool createEmptyBoard = false)
    {
        ClearTracking();

        if (createEmptyBoard)
        {
            diceBoard.InitializeDefault();
            isGameStarted = false;
        }
        else
        {
            diceBoard.InitializeDiceBoard(diceList);
        }
        wordList.ClearWords();

        scoreBoard.ClearScoreBoard();

        PlayerId localPlayer = client.MyPlayerID;

        if (null != localPlayer)
        {
            scoreBoard.AddPlayer(client.MyPlayerName, localPlayer);
        }
    }

    public void UpdateGameState(string boardLayout)
    {
        diceBoard.UpdateBoardLayout(boardLayout);
    }

    public void UpdatePlayerState(int playerID, string playerName, int playerScore, string foundWordSet)
    {
        PlayerId id = new PlayerId(playerID);
        wordList.UpdateFoundWords(id, foundWordSet);
        scoreBoard.UpdatePlayer(this, id, playerName, playerScore);
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
        isTracking = false;
    }
}
