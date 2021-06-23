using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SummaryWordList : MonoBehaviour
{
    [SerializeField] SummaryWord summaryWordPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void    PopulateList(ref List<FoundWordInfo> theList, ref List<string> allWords)
    {
        ClearList();

        foreach (var word in theList)
        {
            SummaryWord newWord = Instantiate<SummaryWord>(summaryWordPrefab, transform);
            newWord.SetSummaryWord(word.word, true);
        }

        foreach (var word in allWords)
        {
            if (!theList.Exists(x => 0 == string.Compare(x.word, word, true)))
            {
                SummaryWord newWord = Instantiate<SummaryWord>(summaryWordPrefab, transform);
                newWord.SetSummaryWord(word, false);
            }
        }
    }

    public void    ClearList()
    {
        SummaryWord[] words = GetComponentsInChildren<SummaryWord>();
        foreach (var word in words)
        {
            Destroy(word.gameObject);
        }
    }
}
