using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreSummaryEntry : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI    nameText;
    [SerializeField] TextMeshProUGUI    scoreText;

    int         scoreValue = 0;
    public int  Score { get { return scoreValue; } set { scoreValue = value; } }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetupEntry(string name, int score, Color c)
    {
        scoreValue = score;

        nameText.text = name;
        scoreText.text = score.ToString();
        nameText.color = c;
        scoreText.color = c;
    }
}
