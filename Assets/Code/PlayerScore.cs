using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using KitsuneCore.Services.Players;

public class PlayerScore : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI    scoreName;
    [SerializeField] TextMeshProUGUI    scoreText;
    [SerializeField] TextMeshProUGUI    updateScoreText;


    PlayAnimation   animCtrl;
    string          playerName = "";
    PlayerId        playerID;
    int             playerScore = 0;
    bool            isLocalPlayer = false;

    public string   PlayerName { get { return playerName; } }
    public PlayerId PlayerID { get { return playerID; } }
    public int      Score { get { return playerScore; } }
    public bool     IsLocal { get { return isLocalPlayer; } }

    // Start is called before the first frame update
    void Start()
    {
        animCtrl = GetComponent<PlayAnimation>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitScore(string name, PlayerId id, bool isLocal)
    {
        playerName = name;
        playerID = id;
        playerScore = 0;
        isLocalPlayer = isLocal;

        scoreName.text = name;
        scoreText.text = "0";
        updateScoreText.text = "0";
        updateScoreText.gameObject.SetActive(false);
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
        if (!absoluteVal)
        {
            updateScoreText.text = setValue.ToString();
            if (animCtrl) animCtrl.Play("ScoreAdded");
        }
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
        scoreText.color = c;
        updateScoreText.color = c;
    }
}
