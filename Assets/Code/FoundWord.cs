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

    List<PlayerId> finders = new List<PlayerId>();

    public string Word { get { return foundWord; } }
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

    public void SetFoundWord(string strWord, PlayerId id, Color c)
    {
        foundWord = strWord;
        ownerID = id;

        TextMeshProUGUI foundWordText = GetComponent<TextMeshProUGUI>();
        if (null != foundWordText)
        {
            textColor = c;
            foundWordText.text = strWord;
            foundWordText.color = new Color(c.r, c.g, c.b, unrevealedAlpha);
        }
    }

    public void SetFoundPlayer(PlayerId id, Color c)
    {
        ownerID = id;

        TextMeshProUGUI foundWordText = GetComponent<TextMeshProUGUI>();
        if (null != foundWordText)
        {
            foundWordText.color = new Color(c.r, c.g, c.b, unrevealedAlpha);
        }
    }

    public void AddFinder(PlayerId id)
    {
        if ((id != ownerID) && !finders.Exists(x => x == id))
            finders.Add(id);
    }

    public bool IsFoundPlayer(PlayerId id, bool checkOwnerOnly = true)
    {
        if (id == ownerID)
        {
            return true;
        }
        else if (!checkOwnerOnly)
        {
            return finders.Exists(x => x == id);
        }
        return false;
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
