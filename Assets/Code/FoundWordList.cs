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

    public FoundWordResult  AddWord(string strWord, PlayerId ownerID, Color c, ref FoundWord outWord)
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
                newWord.SetFoundWord(strWord, ownerID, c);
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

    public void UpdateFoundWords(PlayerId id, string foundWordSet, Color c)
    {
        if (!string.IsNullOrEmpty(foundWordSet))
        {
            string[] tokens = foundWordSet.Split(',');
            foreach (var token in tokens)
            {
                string[] wordInfo = token.Split('[');
                bool isOwner = (wordInfo.Length > 1) && ('1' == wordInfo[1][0]);

                FoundWord word = WordExists(wordInfo[0]);
                if (null != word)
                {
                    if (isOwner && !word.IsFoundPlayer(id))
                        word.SetFoundPlayer(id, c);
                    else if (!isOwner && word.IsFoundPlayer(id))
                        word.SetFoundPlayer(new PlayerId(0), c);
                }
                else
                {
                    if (isOwner)
                    {
                        FoundWord newWord = Instantiate<FoundWord>(foundWordPrefab, transform);
                        newWord.SetFoundWord(wordInfo[0], id, c);
                    }
                    else
                    {
                        FoundWord newWord = Instantiate<FoundWord>(foundWordPrefab, transform);
                        newWord.SetFoundWord(wordInfo[0], new PlayerId(0), c);
                        newWord.SetRevealed();
                    }
                }
            }
        }
    }

    public void GetFoundWordList(PlayerId id, ref List<string> output)
    {
        FoundWord[] Words = GetComponentsInChildren<FoundWord>();

        foreach (FoundWord w in Words)
        {
            if (w.IsFoundPlayer(id))
            {
                output.Add(w.Word);
            }
        }
    }
}
