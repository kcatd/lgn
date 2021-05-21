using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using KitsuneCore.Services.Players;

public class PlayerScore : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI    scoreName;
    [SerializeField] TextMeshProUGUI    scoreText;

    string          playerName = "";
    PlayerId        playerID;
    int             playerScore = 0;

    public string   PlayerName { get { return playerName; } }
    public PlayerId PlayerID { get { return playerID; } }
    public int      Score { get { return playerScore; } }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitScore(string name, PlayerId id)
    {
        playerName = name;
        playerID = id;
        playerScore = 0;

        scoreName.text = name;
        scoreText.text = "0";
    }

    public void AddScore(int setValue, bool absoluteVal = false)
    {
        if (absoluteVal)
        {
            playerScore = setValue;
        }
        else
        {
            playerScore += setValue;
        }

        scoreText.text = playerScore.ToString();
    }

    public void UpdateName(string updateName)
    {
        if (!string.IsNullOrEmpty(updateName))
        {
            if (0 != string.Compare(playerName, updateName))
            {
                playerName = updateName;
                scoreName.text = updateName;
            }
        }
    }

    public void UpdateColor(Color c)
    {
        scoreName.color = c;
    }
}
