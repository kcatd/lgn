using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using KitsuneCore.Services.Players;

public class FoundWordList : MonoBehaviour
{
    [SerializeField] FoundWord      foundWordPrefab;
    [SerializeField] GameDictionary gameDictionary;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int  AddWord(string strWord, PlayerId ownerID, ref FoundWord outWord)
    {
        if (!string.IsNullOrEmpty(strWord))
        {
            FoundWord wordExists = WordExists(strWord);
            int baseScoreVal = GetWordScore(strWord);
            int actualScoreVal = 0;

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
                    actualScoreVal = Mathf.CeilToInt(0.5f * (float)baseScoreVal);
                }
            }
            else if (gameDictionary.ValidateWord(strWord))
            {
                FoundWord newWord = Instantiate<FoundWord>(foundWordPrefab, transform);
                newWord.SetFoundWord(strWord, ownerID);
                outWord = newWord;
                actualScoreVal = baseScoreVal;
            }
            return actualScoreVal;
        }
        return 0;
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

    public int GetWordScore(string strWord)
    {
        // maybe each letter has a different value?
        if (!string.IsNullOrEmpty(strWord))
        {
            return strWord.Length;
        }
        return 0;
    }

    public void ClearWords()
    {
        FoundWord[] Words = GetComponentsInChildren<FoundWord>();

        foreach (FoundWord w in Words)
        {
            Destroy(w.gameObject);
        }
    }

    public void UpdateFoundWords(PlayerId id, string foundWordSet)
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
                        word.SetFoundPlayer(id);
                    else if (!isOwner && word.IsFoundPlayer(id))
                        word.SetFoundPlayer(new PlayerId(0));
                }
                else
                {
                    if (isOwner)
                    {
                        FoundWord newWord = Instantiate<FoundWord>(foundWordPrefab, transform);
                        newWord.SetFoundWord(wordInfo[0], id);
                    }
                    else
                    {
                        FoundWord newWord = Instantiate<FoundWord>(foundWordPrefab, transform);
                        newWord.SetFoundWord(wordInfo[0], new PlayerId(0));
                        newWord.SetRevealed();
                    }
                }
            }
        }
    }
}
