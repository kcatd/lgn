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

    public void    PopulateList(ref List<FoundWordInfo> theList)
    {
        ClearList();

        foreach (var word in theList)
        {
            SummaryWord newWord = Instantiate<SummaryWord>(summaryWordPrefab, transform);
            newWord.Word = word.word;
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
