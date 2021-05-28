using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using KitsuneCore.Services.Players;

public class SummaryPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI        summaryTotalScore;
    [SerializeField] TextMeshProUGUI        summaryLongestWord;
    [SerializeField] TextMeshProUGUI        summaryTotalWordsFound;
    [SerializeField] SummaryWordList        summaryWordList;
    [SerializeField] ScoreSummaryList       summaryPlayerScores;
    [SerializeField] ScoreSummaryList       summaryTeamScores;
    [SerializeField] Button                 summaryDoneButton;

    GobbleGame                              game;
    PlayerId                                player;

    // Start is called before the first frame update
    void Start()
    {
        summaryDoneButton.onClick.AddListener(OnSummaryDoneBtn);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void    OnSummaryDoneBtn()
    {
        gameObject.SetActive(false);
        game.InitializeLobby();
    }

    public void InitSummary(GobbleGame setGame, PlayerId setPlayer)
    {
        List<string> foundWords = new List<string>();

        game = setGame;
        player = setPlayer;

        game.WordList.GetFoundWordList(player, ref foundWords);
        if (foundWords.Count > 1)
        {
            foundWords.Sort((x, y) => x.Length == y.Length ? string.Compare(x, y, true) : y.Length - x.Length);
        }

        summaryTotalScore.text = game.ScoreBoard.GetScore(player).ToString("#,#");
        
        if (foundWords.Count > 0)
        {
            summaryLongestWord.text = foundWords[0].Length.ToString();
            summaryTotalWordsFound.text = foundWords.Count.ToString();
            summaryWordList.PopulateList(foundWords);
        }
        else
        {
            summaryLongestWord.text = "0";
            summaryTotalWordsFound.text = "0";
            summaryWordList.ClearList();
        }

        summaryPlayerScores.ClearAllEntries();
        foreach (var p in game.Players)
        {
            summaryPlayerScores.AddEntry(p.name, p.value, game.TeamColorTable.GetTeamColor(game.GetPlayerTeam(p.id)));
        }
        summaryPlayerScores.SortEntries();

        summaryTeamScores.ClearAllEntries();
        foreach (var t in game.Teams)
        {
            summaryTeamScores.AddEntry(string.Format("Team {0}", t.id), game.GetTeamScore(t.id), game.TeamColorTable.GetTeamColor(t.id));
        }
        summaryTeamScores.SortEntries();
    }
}
