using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DictionaryWord
{
    public string word;
}

[System.Serializable]
public class DictionaryWordSet
{
    public DictionaryWord[] words;
}

public class GameWord
{
    public string   word;
    public Hash128  hashID;

    public GameWord(string strWord)
    {
        word = strWord;
        hashID = Hash128.Compute(word.ToUpper());
    }
}

public class GameWordSet
{
    public GameWord name;
    public Dictionary<Hash128, GameWord> words = new Dictionary<Hash128, GameWord>();

    public GameWordSet(GameWord groupName)
    {
        name = groupName;
    }
}

public class GameDictionary : MonoBehaviour
{
    [SerializeField] TextAsset[]        jsonFiles;

    Dictionary<Hash128, GameWordSet>    wordDictionary = new Dictionary<Hash128, GameWordSet>();

    // Start is called before the first frame update
    void Start()
    {
        LoadDictionary();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void    LoadDictionary()
    {
        int FileC = 0;
        int WordC = 0;

        ClearDictionary();

        foreach (TextAsset file in jsonFiles)
        {
            if (string.IsNullOrEmpty(file.text))
                continue;

            string jsonText = "{\"words\":" + file.text + "}";
            DictionaryWordSet wordSet = JsonUtility.FromJson<DictionaryWordSet>(jsonText);

            ++FileC;

            foreach (DictionaryWord w in wordSet.words)
            {
                if (!string.IsNullOrEmpty(w.word))
                {
                    GameWord newWord = new GameWord(w.word);
                    GameWord groupName = new GameWord(w.word.Substring(0, 1));
                    GameWordSet groupKey;

                    if (wordDictionary.TryGetValue(groupName.hashID, out groupKey))
                    {
                        groupKey.words.Add(newWord.hashID, newWord);
                    }
                    else
                    {
                        groupKey = new GameWordSet(groupName);
                        groupKey.words.Add(newWord.hashID, newWord);

                        wordDictionary.Add(groupKey.name.hashID, groupKey);
                    }
                    ++WordC;
                }
            }
        }

        Debug.Log(string.Format("Dictionary initialized! Processed {0} words in {1} files!", WordC, FileC));
    }

    void    ClearDictionary()
    {
        wordDictionary.Clear();
    }

    public bool ValidateWord(string strWord)
    {
        if (!string.IsNullOrEmpty(strWord))
        {
            GameWord groupName = new GameWord(strWord.Substring(0, 1));
            GameWordSet groupKey;

            if (wordDictionary.TryGetValue(groupName.hashID, out groupKey))
            {
                GameWord testWord = new GameWord(strWord);
                return groupKey.words.ContainsKey(testWord.hashID);
            }
        }
        return false;
    }
}
