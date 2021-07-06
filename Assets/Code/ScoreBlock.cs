using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using KitsuneCore.Services.Players;

public class ScoreBlock : MonoBehaviour
{
    [SerializeField] SummaryResultType  scoreType;

    [SerializeField] Image              teamIcon;
    [SerializeField] TextMeshProUGUI    nameTitleText;
    [SerializeField] TextMeshProUGUI    scoreValueText;
    [SerializeField] TextMeshProUGUI    scoreValueTitle;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetupScoreBlock(PlayerId id, int resultVal, int totalVal, GobbleGame game)
    {
        int teamID = game.GetPlayerTeam(id);

        teamIcon.color = game.TeamColorTable.GetTeamColor(teamID, false);
        nameTitleText.text = game.GetPlayerName(id);

        switch (scoreType)
        {
            case SummaryResultType.Score:
                scoreValueText.text = resultVal.ToString();
                scoreValueTitle.text = "points";
                break;

            case SummaryResultType.Words:
                scoreValueText.text = string.Format("{0}/{1}", resultVal, totalVal);
                scoreValueTitle.text = "words";
                break;
        }
    }
}
