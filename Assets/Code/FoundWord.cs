using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using KitsuneCore.Services.Players;

public class FoundWord : MonoBehaviour
{
    [SerializeField] float  unrevealedAlpha;

    string  foundWord = "";
    PlayerId ownerID;
    Color textColor;
    bool hasStarted = false;
    bool isRevealed = false;

    public bool HasStarted  { get { return hasStarted; } }
    public bool Revealed { get { return isRevealed; } }

    // Start is called before the first frame update
    void Start()
    {
        hasStarted = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetFoundWord(string strWord, PlayerId id)
    {
        foundWord = strWord;
        ownerID = id;

        TextMeshProUGUI foundWordText = GetComponent<TextMeshProUGUI>();
        if (null != foundWordText)
        {
            textColor = foundWordText.color;
            foundWordText.text = strWord;
            foundWordText.color = new Color(textColor.r, textColor.g, textColor.b, unrevealedAlpha);
        }
    }

    public void SetFoundPlayer(PlayerId id)
    {
        ownerID = id;
    }

    public bool IsFoundPlayer(PlayerId id)
    {
        return id == ownerID;
    }

    public bool IsFoundWord(string strWord)
    {
        return 0 == string.Compare(strWord, foundWord, true);
    }

    public void SetRevealed()
    {
        if (!isRevealed)
        {
            isRevealed = true;

            TextMeshProUGUI foundWordText = GetComponent<TextMeshProUGUI>();
            if ((null != foundWordText) && (null != textColor))
            {
                foundWordText.color = textColor;
            }
        }
    }
}
