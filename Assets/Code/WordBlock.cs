using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WordBlock : MonoBehaviour
{
    [SerializeField] SummaryResultType  wordBlockType;

    [SerializeField] DiceBoardGrid      diceBoardGrid;
    [SerializeField] TextMeshProUGUI    titleText;
    [SerializeField] TextMeshProUGUI    mainText;
    [SerializeField] TextMeshProUGUI    descText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetupWordBlock(string wordStr, WordPlayerScore foundWord, GobbleGame game)
    {
        mainText.text = wordStr;

        switch (wordBlockType)
        {
            case SummaryResultType.Score:
                titleText.text = "highest word";
                descText.text = string.Format("{0} points", foundWord.score);
                break;

            case SummaryResultType.Words:
                titleText.text = "longest word";
                descText.text = string.Format("{0} letters", wordStr.Length);
                break;
        }

        diceBoardGrid.InitializeDiceBoard(game.DiceBoard.Width, game.DiceBoard.Height, game.DiceBoard.BoardLayout, foundWord, game);
    }

    public void ClearWordBlock()
    {
        diceBoardGrid.ClearBoard();
    }
}
