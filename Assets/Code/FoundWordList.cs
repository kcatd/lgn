using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using KitsuneCore.Services.Players;

public enum FoundWordResult
{
    no,
    partial,
    ok,
}

public class FoundWordInfo
{
    public readonly string word;
    public readonly WordPlayerScore score;
    public FoundWordInfo(string str, WordPlayerScore data)
    {
        word = str;
        score = new WordPlayerScore(data.player, data.score, data.diceSet);
    }
}

public class FoundWordNetData
{
    public readonly string data;
    public readonly List<int> diceIdx;

    public FoundWordNetData(string str, List<int> idx)
    {
        data = str;
        diceIdx = idx;
    }
}

public class FoundWordList : MonoBehaviour
{
    [SerializeField] FoundWord      foundWordPrefab;
    [SerializeField] GameDictionary gameDictionary;
    [SerializeField] bool           autoResizeGrid = true;

    // Start is called before the first frame update
    void Start()
    {
        if (autoResizeGrid)
        {
            RectTransform rt = foundWordPrefab.GetComponent<RectTransform>();
            Rect cellRect = rt.rect;
            GridLayoutGroup grid = GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(cellRect.width, cellRect.height);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public FoundWordResult  AddWord(string strWord, PlayerId ownerID, Color c, ref List<Dice> diceSet, ref FoundWord outWord)
    {
        if (!string.IsNullOrEmpty(strWord))
        {
            FoundWord wordExists = WordExists(strWord);

            if (null != wordExists)
            {
                // someone else got here before us
                if (wordExists.IsFoundPlayer(ownerID))
                {
                    // i found it, no repeats for me!!
                }
                else
                {
                    // we get partial score for this
                    outWord = wordExists;
                    return FoundWordResult.partial;
                }
            }
            else if (gameDictionary.ValidateWord(strWord))
            {
                FoundWord newWord = Instantiate<FoundWord>(foundWordPrefab, transform);
                List<int> diceIdxSet = new List<int>();

                foreach (Dice d in diceSet)
                    diceIdxSet.Add(d.Idx);

                newWord.SetFoundWord(strWord, diceIdxSet, ownerID, c, 0);
                outWord = newWord;
                return FoundWordResult.ok;
            }
        }
        return FoundWordResult.no;
    }

    public FoundWord    WordExists(string strWord)
    {
        if (!string.IsNullOrEmpty(strWord))
        {
            FoundWord[] Words = GetComponentsInChildren<FoundWord>();

            foreach (FoundWord w in Words)
            {
                if (w.IsFoundWord(strWord))
                    return w;
            }
        }
        return null;
    }

    public void ClearWords()
    {
        FoundWord[] Words = GetComponentsInChildren<FoundWord>();

        foreach (FoundWord w in Words)
        {
            Destroy(w.gameObject);
        }
    }

    public void UpdateFoundWords(PlayerId id, List<FoundWordNetData> foundWordSet, Color c)
    {
        foreach (var foundWord in foundWordSet)
        {
            /*
            string debugDice = "";
            foreach (var d in foundWord.diceIdx)
                debugDice += string.Format("+{0}", d);
            Debug.Log("Update found word: " + foundWord.data + debugDice);
            */
            string[] wordInfo = foundWord.data.Split('[');
            int scoreVal = -1;
            bool isOwner = false;

            if ((wordInfo.Length > 1) && (wordInfo[1].Length > 1))
            {
                string tok = wordInfo[1].Substring(0, wordInfo[1].Length - 1);

                if ('!' == tok[tok.Length - 1])
                {
                    isOwner = true;
                    scoreVal = int.Parse(tok.Substring(0, tok.Length - 1));
                }
                else
                {
                    scoreVal = int.Parse(tok);
                }
            }

            FoundWord word = WordExists(wordInfo[0]);
            if (null != word)
            {
                if (scoreVal < 0)
                    scoreVal = word.GetScore(id);

                if (isOwner && !word.IsFoundPlayer(id))
                    word.SetFoundPlayer(id, c, scoreVal, foundWord.diceIdx);
                else if (!isOwner && word.IsFoundPlayer(id))
                    word.SetFoundPlayer(new PlayerId(0), c, scoreVal, foundWord.diceIdx);
            }
            else
            {
                if (scoreVal < 0)
                    scoreVal = 0;

                if (isOwner)
                {
                    FoundWord newWord = Instantiate<FoundWord>(foundWordPrefab, transform);
                    newWord.SetFoundWord(wordInfo[0], foundWord.diceIdx, id, c, scoreVal);
                }
                else
                {
                    FoundWord newWord = Instantiate<FoundWord>(foundWordPrefab, transform);
                    newWord.SetFoundWord(wordInfo[0], foundWord.diceIdx, new PlayerId(0), c, scoreVal);
                    newWord.SetRevealed();
                }
            }
        }
    }

    public void GetAllFoundWords(ref List<string> output)
    {
        FoundWord[] Words = GetComponentsInChildren<FoundWord>();

        foreach (FoundWord w in Words)
        {
            output.Add(w.Word);
        }
    }
    public void GetFoundWordList(PlayerId id, ref List<FoundWordInfo> output)
    {
        FoundWord[] Words = GetComponentsInChildren<FoundWord>();

        foreach (FoundWord w in Words)
        {
            if (w.IsFoundPlayer(id))
            {
                output.Add(new FoundWordInfo(w.Word, w.PlayerScore));
            }
        }
    }
    public void GetFoundWordList(List<PlayerId> players, ref List<FoundWordInfo> output)
    {
        FoundWord[] Words = GetComponentsInChildren<FoundWord>();

        foreach (FoundWord w in Words)
        {
            bool isTeamFind = false;
            foreach (var p in players)
            {
                if (w.IsFoundPlayer(p, false))
                {
                    isTeamFind = true;
                    break;
                }
            }

            if (isTeamFind)
            {
                WordPlayerScore score = w.GetHighestScore(players);
                if (null != score)
                {
                    output.Add(new FoundWordInfo(w.Word, score));
                }
            }
        }
    }

    public int  GetFoundWordScore(PlayerId id, string strWord, int defaultValue)
    {
        FoundWord word = WordExists(strWord);
        if (null != word)
        {
            return word.GetScore(id, defaultValue);
        }
        return defaultValue;
    }

    public WordPlayerScore  GetHighestScoringWord(ref string highestWordStr)
    {
        WordPlayerScore highestWord = null;
        int wordScore = 0;

        FoundWord[] Words = GetComponentsInChildren<FoundWord>();
        highestWordStr = "";

        foreach (FoundWord w in Words)
        {
            WordPlayerScore score = w.GetHighestScore();
            if (score.score > wordScore)
            {
                highestWordStr = w.Word;
                highestWord = score;
                wordScore = score.score;
            }
        }
        return highestWord;
    }
    public WordPlayerScore  GetLongestWord(ref string longestWordStr)
    {
        WordPlayerScore longestWord = null;
        int wordScore = 0;

        FoundWord[] Words = GetComponentsInChildren<FoundWord>();
        longestWordStr = "";

        foreach (FoundWord w in Words)
        {
            if (w.Word.Length >= longestWordStr.Length)
            {
                WordPlayerScore score = w.GetHighestScore();

                if (w.Word.Length > longestWordStr.Length)
                {
                    // longest word takes precedence
                    longestWordStr = w.Word;
                    longestWord = score;
                }
                else if (score.score > wordScore)
                {
                    // score takes precedence when words are same length
                    longestWordStr = w.Word;
                    longestWord = score;
                }
            }
        }
        return longestWord;
    }

    public int  GetMostWordsFound(ref PlayerId id)
    {
        Dictionary<PlayerId, int> resultMap = new Dictionary<PlayerId, int>();
        FoundWord[] Words = GetComponentsInChildren<FoundWord>();
        int highestVal = 0;

        foreach (FoundWord w in Words)
        {
            PlayerId finderId = w.PlayerScore.player;
            if (resultMap.ContainsKey(finderId))
            {
                resultMap[finderId]++;
            }
            else
            {
                resultMap[finderId] = 1;
            }
        }

        if (resultMap.Count > 0)
        {
            foreach(var res in resultMap)
            {
                if (res.Value > highestVal)
                {
                    id = res.Key;
                    highestVal = res.Value;
                }
            }
        }
        return highestVal;
    }

    public int  GetTotalWordsFound()
    {
        FoundWord[] Words = GetComponentsInChildren<FoundWord>();
        return Words.Length;
    }
}
