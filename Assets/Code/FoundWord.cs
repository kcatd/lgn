using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using KitsuneCore.Services.Players;

public class WordPlayerScore
{
    public readonly PlayerId player;
    public int score;

    public WordPlayerScore(PlayerId id, int val)
    {
        player = id;
        score = val;
    }
}

public class FoundWord : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI    text;
    [SerializeField] Image              textBg;
    [SerializeField] float              unrevealedAlpha;

    string  foundWord = "";
    WordPlayerScore playerScore;
    Color textColor;
    bool hasStarted = false;
    bool isRevealed = false;

    List<WordPlayerScore> finders = new List<WordPlayerScore>();

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

    public void SetFoundWord(string strWord, PlayerId id, Color c, int score)
    {
        foundWord = strWord;
        playerScore = new WordPlayerScore(id, score);

        //textColor = c;
        textColor = text.color;
        text.text = strWord;
        text.color = new Color(textColor.r, textColor.g, textColor.b, textColor.a * unrevealedAlpha);
        //textBg.color = new Color(c.r, c.g, c.b, unrevealedAlpha);
    }

    public void SetFoundPlayer(PlayerId id, Color c, int score)
    {
        if ((null != playerScore) && (playerScore.player != id))
        {
            WordPlayerScore tmp = playerScore;
            playerScore = null;
            AddFinder(tmp.player, tmp.score);
        }
        playerScore = new WordPlayerScore(id, score);

        text.color = new Color(textColor.r, textColor.g, textColor.b, textColor.a * unrevealedAlpha);
        //textBg.color = new Color(c.r, c.g, c.b, unrevealedAlpha);
    }

    public void SetScore(PlayerId id, int score, bool createNew = false)
    {
        if (playerScore.player == id)
        {
            playerScore.score = score;
        }
        else
        {
            WordPlayerScore wps = finders.Find(x => x.player == id);
            if (null != wps)
            {
                wps.score = score;
            }
            else if (createNew)
            {
                AddFinder(id, score);
            }
        }
    }

    public int GetScore(PlayerId id, int defaultValue = 0)
    {
        if (playerScore.player == id)
        {
            return playerScore.score;
        }
        else
        {
            WordPlayerScore score = finders.Find(x => x.player == id);
            if (null != score)
            {
                return score.score;
            }
        }
        return defaultValue;
    }

    public void AddFinder(PlayerId id, int score)
    {
        if (id != playerScore.player)
        {
            WordPlayerScore exist = finders.Find(x => x.player == id);
            if (null == exist)
            {
                finders.Add(new WordPlayerScore(id, score));
            }
            else
            {
                exist.score = score;
            }
        }
    }

    public bool IsFoundPlayer(PlayerId id, bool checkOwnerOnly = true)
    {
        if (id == playerScore.player)
        {
            return true;
        }
        else if (!checkOwnerOnly)
        {
            return finders.Exists(x => x.player == id);
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
            text.color = textColor;
            //textBg.color = textColor;
        }
    }
}
