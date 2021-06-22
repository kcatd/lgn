using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using KitsuneCore.Services.Players;

public class SummaryPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI        summaryWinner;
    [SerializeField] TextMeshProUGUI        summaryTotalScore;
    [SerializeField] TextMeshProUGUI        summaryHighestScoringWord;
    [SerializeField] TextMeshProUGUI        summaryLongestWord;
    [SerializeField] TextMeshProUGUI        summaryTotalWordsFound;
    [SerializeField] SummaryWordList        summaryWordList;
    [SerializeField] ScoreSummaryList       summaryPlayerScores;
    [SerializeField] ScoreSummaryList       summaryTeamScores;
    [SerializeField] Button                 summaryDoneButton;
    [SerializeField] Toggle                 summaryPlayerTabButton;
    [SerializeField] Toggle                 summaryTeamTabButton;
    [SerializeField] GameObject             summaryPlayerTab;
    [SerializeField] GameObject             summaryTeamTab;

    GobbleGame                              game;
    PlayerId                                player;

    bool                                    refreshTabs = false;

    // Start is called before the first frame update
    void Start()
    {
        summaryDoneButton.onClick.AddListener(OnSummaryDoneBtn);

        if (null != summaryPlayerTabButton)
            summaryPlayerTabButton.onValueChanged.AddListener(OnChangeTab);

        if (null != summaryTeamTabButton)
            summaryTeamTabButton.onValueChanged.AddListener(OnChangeTab);
    }

    // Update is called once per frame
    void Update()
    {
        if (refreshTabs)
        {
            refreshTabs = false;
            RefreshTabs();
        }
    }

    void    OnSummaryDoneBtn()
    {
        gameObject.SetActive(false);
        game.InitializeLobby();
    }

    void    OnChangeTab(bool isOn)
    {
        refreshTabs = true;
    }

    void    RefreshTabs()
    {
        if ((null != summaryPlayerTabButton) && summaryPlayerTabButton.isOn)
        {
            if (null != summaryPlayerTab)
                summaryPlayerTab.gameObject.SetActive(true);

            if (null != summaryTeamTab)
                summaryTeamTab.gameObject.SetActive(false);
        }
        else if ((null != summaryTeamTabButton) && summaryTeamTabButton.isOn)
        {
            if (null != summaryPlayerTab)
                summaryPlayerTab.gameObject.SetActive(false);

            if (null != summaryTeamTab)
                summaryTeamTab.gameObject.SetActive(true);
        }
    }
    public void InitSummary(GobbleGame setGame, PlayerId setPlayer)
    {
        List<string> allWords = new List<string>();
        List<FoundWordInfo> foundWords = new List<FoundWordInfo>();
        List<FoundWordInfo> sortedBySize = new List<FoundWordInfo>();
        List<FoundWordInfo> sortedByScore = new List<FoundWordInfo>();
        int totalWordCount = 0;
        int wordCount = 0;
        bool isTeamGame = false;

        game = setGame;
        player = setPlayer;
        refreshTabs = false;

        game.WordList.GetAllFoundWords(ref allWords);
        game.WordList.GetFoundWordList(player, ref foundWords);
        isTeamGame = game.Teams.Count > 1;

        totalWordCount = allWords.Count;
        wordCount = foundWords.Count;

        foreach (var w in foundWords)
        {
            sortedBySize.Add(w);
            sortedByScore.Add(w);
        }

        if (totalWordCount > 1)
        {
            allWords.Sort((x, y) => x.Length == y.Length ? string.Compare(x, y) : y.Length - x.Length);
        }
        if (wordCount > 1)
        {
            sortedBySize.Sort((x, y) => x.word.Length == y.word.Length ? x.score == y.score ? string.Compare(x.word, y.word, true) : y.score - x.score : y.word.Length - x.word.Length);
            sortedByScore.Sort((x, y) => x.score == y.score ? x.word.Length == y.word.Length ? string.Compare(x.word, y.word, true) : y.word.Length - x.word.Length : y.score - x.score);
        }

        if (null != summaryTotalScore)
            summaryTotalScore.text = game.ScoreBoard.GetScore(player).ToString("#,#");

        if (null != summaryTotalWordsFound)
            summaryTotalWordsFound.text = string.Format("{0}/{1}", wordCount, totalWordCount);

        if (null != summaryWordList)
            summaryWordList.PopulateList(ref sortedBySize, ref allWords);

        if (wordCount > 0)
        {
            if (null != summaryHighestScoringWord)
                summaryHighestScoringWord.text = string.Format("{0} ({1})", sortedByScore[0].word, sortedByScore[0].score);

            if (null != summaryLongestWord)
                summaryLongestWord.text = string.Format("{0} ({1})", sortedBySize[0].word, sortedBySize[0].word.Length);
        }
        else
        {
            if (null != summaryHighestScoringWord)
                summaryHighestScoringWord.text = "-";

            if (null != summaryLongestWord)
                summaryLongestWord.text = "-";
        }

        if (null != summaryPlayerScores)
        {
            summaryPlayerScores.ClearAllEntries();
            foreach (var p in game.Players)
            {
                summaryPlayerScores.AddEntry(p.name, p.id, p.value, game.TeamColorTable.GetTeamColor(game.GetPlayerTeam(p.id)));
            }
            summaryPlayerScores.SortEntries();
        }

        if (null != summaryTeamScores)
        {
            summaryTeamScores.ClearAllEntries();
            foreach (var t in game.Teams)
            {
                summaryTeamScores.AddEntry(string.Format("Team {0}", t.id), t.id, game.GetTeamScore(t.id), game.TeamColorTable.GetTeamColor(t.id));
            }
            summaryTeamScores.SortEntries();
        }

        if (isTeamGame)
        {
            if (null != summaryPlayerTabButton)
            {
                summaryPlayerTabButton.gameObject.SetActive(true);
                summaryPlayerTabButton.interactable = true;
                summaryPlayerTabButton.SetIsOnWithoutNotify(false);
            }

            if (null != summaryTeamTabButton)
            {
                summaryTeamTabButton.gameObject.SetActive(true);
                summaryTeamTabButton.interactable = true;
                summaryTeamTabButton.SetIsOnWithoutNotify(true);
            }

            if ((null != summaryWinner) && (null != summaryTeamScores))
            {
                string winnerName = "";
                int winnerID = 0;
                int winnerScore = 0;

                if (summaryTeamScores.GetTopEntry(ref winnerName, ref winnerID, ref winnerScore))
                {
                    summaryWinner.text = winnerName;
                    summaryWinner.color = game.TeamColorTable.GetTeamColor(winnerID);
                }
            }
        }
        else
        {
            if (null != summaryPlayerTabButton)
            {
                summaryPlayerTabButton.gameObject.SetActive(true);
                summaryPlayerTabButton.interactable = false;
                summaryPlayerTabButton.SetIsOnWithoutNotify(true);
            }

            if (null != summaryTeamTabButton)
            {
                summaryTeamTabButton.gameObject.SetActive(false);
                summaryTeamTabButton.interactable = false;
                summaryTeamTabButton.SetIsOnWithoutNotify(false);
            }

            if ((null != summaryWinner) && (null != summaryPlayerScores))
            {
                string winnerName = "";
                int winnerID = 0;
                int winnerScore = 0;

                if (summaryPlayerScores.GetTopEntry(ref winnerName, ref winnerID, ref winnerScore))
                {
                    summaryWinner.text = winnerName;
                    summaryWinner.color = game.TeamColorTable.GetTeamColor(game.GetPlayerTeam(new PlayerId(winnerID)));
                }
            }
        }

        RefreshTabs();
    }
}
