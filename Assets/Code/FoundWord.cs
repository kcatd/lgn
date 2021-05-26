using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using KitsuneCore.Services.Players;

public class FoundWord : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI    text;
    [SerializeField] Image              textBg;
    [SerializeField] float              unrevealedAlpha;

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

        textColor = c;
        text.text = strWord;
        textBg.color = new Color(c.r, c.g, c.b, unrevealedAlpha);
    }

    public void SetFoundPlayer(PlayerId id, Color c)
    {
        ownerID = id;
        textBg.color = new Color(c.r, c.g, c.b, unrevealedAlpha);
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
            textBg.color = textColor;
        }
    }
}
