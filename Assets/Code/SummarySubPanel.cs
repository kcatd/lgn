using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using KitsuneCore.Services.Players;

public enum SummaryResultType
{
    Score,
    Words,
}

public enum SummaryPaneType
{
    GameResults,
    PlayerResults,
    TeamResults,
}

public class SummarySubPanel : MonoBehaviour
{
    [SerializeField] SummaryPaneType        summaryPaneType;
    [SerializeField] TextMeshProUGUI        summaryTitleText;

    [Header("Settings")]
    [SerializeField] float                  backgroundScale = 0.75f;
    [SerializeField] float                  animSpeedFactor = 4.0f;

    [Header("UI Groups")]
    [SerializeField] Image                  fadeOverlay;
    [SerializeField] GameObject             resultPane;
    [SerializeField] GameObject             scorePane;
    [SerializeField] GameObject             diceBlockPane;
    [SerializeField] GameObject             buttonsPane;

    [Header("Result Group")]
    [SerializeField] TextMeshProUGUI        scoreTitleText;
    [SerializeField] TextMeshProUGUI        scoreValueText;
    [SerializeField] TextMeshProUGUI        wordsTitleText;
    [SerializeField] TextMeshProUGUI        wordsValueText;

    [Header("Score Group")]
    [SerializeField] ScoreBlock             pointsScoreBlock;
    [SerializeField] ScoreBlock             wordsScoreBlock;

    [Header("Dice Blocks")]
    [SerializeField] WordBlock              pointsDiceBlock;
    [SerializeField] WordBlock              wordsDiceBlock;

    [Header("Buttons")]
    [SerializeField] Button                 mainButtonLeft;
    [SerializeField] Button                 mainButtonRight;
    [SerializeField] Button                 playButton;
    [SerializeField] Button                 quitButton;

    SummaryGroup                            parentSummary = null;
    Color                                   fadeColor = new Color(1.0f, 1.0f, 1.0f);
    int                                     panePosition = 0;
    bool                                    isForeground = false;
    bool                                    isLocalOrHost = false;

    public SummaryPaneType                  PaneType { get { return summaryPaneType; } }
    public bool                             IsForeground { get { return isForeground; } }

    // Start is called before the first frame update
    void Start()
    {
        mainButtonLeft.onClick.AddListener(OnMainButton);
        mainButtonRight.onClick.AddListener(OnMainButton);
        playButton.onClick.AddListener(OnPlayButton);
        quitButton.onClick.AddListener(OnQuitButton);

        resultPane.SetActive(SummaryPaneType.GameResults != summaryPaneType);
        scorePane.SetActive(SummaryPaneType.GameResults == summaryPaneType);

        TextMeshProUGUI mainTextL = mainButtonLeft.GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI mainTextR = mainButtonRight.GetComponentInChildren<TextMeshProUGUI>();

        switch (summaryPaneType)
        {
            case SummaryPaneType.GameResults:
                summaryTitleText.text = "Game";
                break;
            case SummaryPaneType.PlayerResults:
                summaryTitleText.text = "Player";
                break;
            case SummaryPaneType.TeamResults:
                summaryTitleText.text = "Team";
                break;
        }
        if (null != mainTextL) mainTextL.text = summaryTitleText.text;
        if (null != mainTextR) mainTextR.text = summaryTitleText.text;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetupSummary(GobbleGame game, PlayerId thePlayer)
    {
        switch (summaryPaneType)
        {
            case SummaryPaneType.GameResults:
                SetupGameSummary(game, thePlayer);
                break;
            case SummaryPaneType.PlayerResults:
                SetupPlayerSummary(game, thePlayer);
                break;
            case SummaryPaneType.TeamResults:
                SetupTeamSummary(game, thePlayer);
                break;
        }
    }

    public void SetMainSummaryPanel(bool isOn, SummaryGroup parent, bool initFlag = false)
    {
        isForeground = isOn;
        parentSummary = parent;

        if (initFlag)
        {
            fadeColor = fadeOverlay.color;
            fadeOverlay.gameObject.SetActive(!isForeground);
            gameObject.transform.localScale = new Vector3(isForeground ? 1.0f : backgroundScale, isForeground ? 1.0f : backgroundScale, 1.0f);
        }
        UpdateButtonStates();
    }

    public void SetLocalControl(bool isOn)
    {
        isLocalOrHost = isOn;
        UpdateButtonStates();
    }

    public void SetPanePosition(int dir)
    {
        panePosition = dir;
        UpdateButtonStates();
    }

    public void ClearPanel()
    {
        // anything else need to be cleared?
        pointsDiceBlock.ClearWordBlock();
        wordsDiceBlock.ClearWordBlock();
    }

    public void MoveTo(Transform parent, Vector3 destPos, bool toForeground)
    {
        StartCoroutine(MovePane(parent, destPos, toForeground));
    }
    IEnumerator MovePane(Transform parent, Vector3 destPos, bool toForeground)
    {
        const float moveFPS = 1.0f / 60.0f;

        Vector3 startPos = gameObject.transform.position;
        Vector3 dir = destPos - startPos;
        float prog = 0.0f;
        bool updateParent = true;

        if (toForeground)
        {
            fadeOverlay.gameObject.SetActive(true);
            gameObject.transform.SetParent(parent);
            updateParent = false;
        }

        if (dir.magnitude > float.Epsilon)
        {
            for (; ; )
            {
                //prog += 4.0f * Time.deltaTime;
                prog += animSpeedFactor * moveFPS;
                if (prog < 1.0f)
                {
                    Vector3 scale = gameObject.transform.localScale;
                    Color c = fadeColor;

                    scale.x = backgroundScale + ((1.0f - backgroundScale) * (toForeground ? prog : 1.0f - prog));
                    scale.y = backgroundScale + ((1.0f - backgroundScale) * (toForeground ? prog : 1.0f - prog));
                    c.a = fadeColor.a * (toForeground ? 1.0f - prog : prog);

                    fadeOverlay.color = c;
                    gameObject.transform.position = startPos + (prog * dir);
                    gameObject.transform.localScale = scale;
                    yield return new WaitForSeconds(moveFPS);
                }
                else
                {
                    fadeOverlay.color = fadeColor;
                    gameObject.transform.position = destPos;
                    gameObject.transform.localScale = new Vector3(isForeground ? 1.0f : backgroundScale, isForeground ? 1.0f : backgroundScale, 1.0f);
                    break;
                }
            }
        }

        if (updateParent)
        {
            gameObject.transform.SetParent(parent);
        }
        fadeOverlay.gameObject.SetActive(!toForeground);
        UpdateButtonStates();
        yield return null;
    }

    void    OnMainButton()
    {
        if (!isForeground && (null != parentSummary))
            parentSummary.MakeForeground(this);
    }

    void    OnPlayButton()
    {
        if (null != parentSummary)
            parentSummary.EndSummary(EndSummaryMode.RestartWithCurrentSettings);
    }

    void    OnQuitButton()
    {
        if (null != parentSummary)
            parentSummary.EndSummary(EndSummaryMode.QuitToGameOptions);
    }

    void    UpdateButtonStates()
    {
        //mainButtonLeft.gameObject.SetActive(!isForeground && (panePosition < 0));
        //mainButtonRight.gameObject.SetActive(!isForeground && (panePosition > 0));
        mainButtonLeft.gameObject.SetActive(!isForeground);
        mainButtonRight.gameObject.SetActive(!isForeground);
        playButton.gameObject.SetActive(isForeground);
        quitButton.gameObject.SetActive(isForeground);

        if (isForeground)
        {
            playButton.interactable = isLocalOrHost;
            quitButton.interactable = isLocalOrHost;
        }
    }

    void    SetupGameSummary(GobbleGame game, PlayerId thePlayer)
    {
        PlayerScoreEntry highestScore = null;
        foreach (var score in game.Players)
        {
            if ((null == highestScore) || (score.value > highestScore.value))
                highestScore = score;
        }
        if (null != highestScore)
        {
            pointsScoreBlock.SetupScoreBlock(highestScore.id, highestScore.value, highestScore.value, game);
        }

        PlayerId mostWordsPlayer = thePlayer;
        int mostWordCount = game.WordList.GetMostWordsFound(ref mostWordsPlayer);
        wordsScoreBlock.SetupScoreBlock(mostWordsPlayer, mostWordCount, game.WordList.GetTotalWordsFound(), game);

        string highestWordStr = "";
        WordPlayerScore highestWord = game.WordList.GetHighestScoringWord(ref highestWordStr);
        if (null != highestWord)
        {
            pointsDiceBlock.SetupWordBlock(highestWordStr, highestWord, game);
        }

        string longestWordStr = "";
        WordPlayerScore longestWordInfo = game.WordList.GetLongestWord(ref longestWordStr);
        if (null != longestWordInfo)
        {
            wordsDiceBlock.SetupWordBlock(longestWordStr, longestWordInfo, game);
        }
    }

    void    SetupPlayerSummary(GobbleGame game, PlayerId thePlayer)
    {
        int playerScore = game.GetPlayerScore(thePlayer);
        scoreTitleText.text = "You scored";
        scoreValueText.text = playerScore > 0 ? playerScore.ToString("#,#") : "0";

        List<FoundWordInfo> foundWordList = new List<FoundWordInfo>();
        game.WordList.GetFoundWordList(thePlayer, ref foundWordList);
        wordsTitleText.text = "You found";
        wordsValueText.text = string.Format("{0}/{1}", foundWordList.Count, game.WordList.GetTotalWordsFound());

        if (foundWordList.Count > 0)
        {
            if (foundWordList.Count > 1)
                foundWordList.Sort((x, y) => x.score == y.score ? x.word.Length == y.word.Length ? string.Compare(x.word, y.word, true) : y.word.Length - x.word.Length : y.score.score - x.score.score);

            pointsDiceBlock.SetupWordBlock(foundWordList[0].word, foundWordList[0].score, game);

            if (foundWordList.Count > 1)
                foundWordList.Sort((x, y) => x.word.Length == y.word.Length ? x.score == y.score ? string.Compare(x.word, y.word, true) : y.score.score - x.score.score : y.word.Length - x.word.Length);

            wordsDiceBlock.SetupWordBlock(foundWordList[0].word, foundWordList[0].score, game);
        }
    }

    void    SetupTeamSummary(GobbleGame game, PlayerId thePlayer)
    {
        int teamID = game.GetPlayerTeam(thePlayer);
        int teamScore = game.GetTeamScore(teamID);
        scoreTitleText.text = "Your team scored";
        scoreValueText.text = teamScore > 0 ? teamScore.ToString("#,#") : "0";

        List<PlayerId> teamPlayers = new List<PlayerId>();
        PlayerTeamEntry teamInfo = game.Teams.Find(x => x.id == teamID);
        if (null == teamInfo)
            teamPlayers.Add(thePlayer);
        else
            teamPlayers = teamInfo.players;

        List<FoundWordInfo> foundWordList = new List<FoundWordInfo>();
        game.WordList.GetFoundWordList(teamPlayers, ref foundWordList);
        wordsTitleText.text = "Your team found";
        wordsValueText.text = string.Format("{0}/{1}", foundWordList.Count, game.WordList.GetTotalWordsFound());

        if (foundWordList.Count > 0)
        {
            if (foundWordList.Count > 1)
                foundWordList.Sort((x, y) => x.score == y.score ? x.word.Length == y.word.Length ? string.Compare(x.word, y.word, true) : y.word.Length - x.word.Length : y.score.score - x.score.score);

            pointsDiceBlock.SetupWordBlock(foundWordList[0].word, foundWordList[0].score, game);

            if (foundWordList.Count > 1)
                foundWordList.Sort((x, y) => x.word.Length == y.word.Length ? x.score == y.score ? string.Compare(x.word, y.word, true) : y.score.score - x.score.score : y.word.Length - x.word.Length);

            wordsDiceBlock.SetupWordBlock(foundWordList[0].word, foundWordList[0].score, game);
        }
    }
}
