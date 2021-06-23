using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreSummaryEntry : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI    nameText;
    [SerializeField] TextMeshProUGUI    scoreText;

    int         ownerID = 0;
    int         scoreValue = 0;

    public string OwnerName { get { return nameText.text; } }
    public int  OwnerID { get { return ownerID; } }
    public int  Score { get { return scoreValue; } set { scoreValue = value; } }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetupEntry(string name, int id, int score, Color c)
    {
        ownerID = id;
        scoreValue = score;

        nameText.text = name;
        scoreText.text = score.ToString();
        nameText.color = c;
        scoreText.color = c;
    }
}
