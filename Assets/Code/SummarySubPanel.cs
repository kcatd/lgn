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

    [Header("UI Groups")]
    [SerializeField] GameObject             fadeOverlay;
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
    [SerializeField] Button                 mainButton;
    [SerializeField] Button                 playButton;
    [SerializeField] Button                 quitButton;

    SummaryGroup                            parentSummary = null;
    bool                                    isForeground = false;
    bool                                    isLocalOrHost = false;

    public SummaryPaneType                  PaneType { get { return summaryPaneType; } }
    public bool                             IsForeground { get { return isForeground; } }

    // Start is called before the first frame update
    void Start()
    {
        mainButton.onClick.AddListener(OnMainButton);
        playButton.onClick.AddListener(OnPlayButton);
        quitButton.onClick.AddListener(OnQuitButton);

        resultPane.SetActive(SummaryPaneType.GameResults != summaryPaneType);
        scorePane.SetActive(SummaryPaneType.GameResults == summaryPaneType);

        TextMeshProUGUI mainText = mainButton.GetComponentInChildren<TextMeshProUGUI>();
        if (null != mainText)
        {
            switch (summaryPaneType)
            {
                case SummaryPaneType.GameResults:
                    mainText.text = "Game";
                    break;
                case SummaryPaneType.PlayerResults:
                    mainText.text = "Player";
                    break;
                case SummaryPaneType.TeamResults:
                    mainText.text = "Team";
                    break;
            }
        }
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

    public void SetMainSummaryPanel(bool isOn, SummaryGroup parent)
    {
        isForeground = isOn;
        parentSummary = parent;
        UpdateButtonStates();
    }

    public void SetLocalControl(bool isOn)
    {
        isLocalOrHost = isOn;
        UpdateButtonStates();
    }

    public void ClearPanel()
    {
        // anything else need to be cleared?
        pointsDiceBlock.ClearWordBlock();
        wordsDiceBlock.ClearWordBlock();
    }

    void    OnMainButton()
    {
        if (!isForeground && (null != parentSummary))
            parentSummary.MakeForeground(this);
    }

    void    OnPlayButton()
    {
        if (null != parentSummary)
            parentSummary.EndSummary(); // TODO: distinguish between play and quit
    }

    void    OnQuitButton()
    {
        if (null != parentSummary)
            parentSummary.EndSummary(); // TODO: distinguish between play and quit
    }

    void    UpdateButtonStates()
    {
        fadeOverlay.SetActive(!isForeground);
        mainButton.gameObject.SetActive(!isForeground);
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
        scoreValueText.text = playerScore.ToString("#,#");

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
        scoreValueText.text = teamScore.ToString("#,#");

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
