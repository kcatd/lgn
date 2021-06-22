using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreSummaryList : MonoBehaviour
{
    [SerializeField] ScoreSummaryEntry  prefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddEntry(string name, int id, int score, Color c)
    {
        ScoreSummaryEntry newEntry = Instantiate<ScoreSummaryEntry>(prefab, transform);
        newEntry.SetupEntry(name, id, score, c);
    }

    public void ClearAllEntries()
    {
        ScoreSummaryEntry[] scores = GetComponentsInChildren<ScoreSummaryEntry>();
        foreach (var score in scores)
        {
            Destroy(score.gameObject);
        }
    }

    public void SortEntries()
    {
        List<ScoreSummaryEntry> scores = new List<ScoreSummaryEntry>();
        GetComponentsInChildren<ScoreSummaryEntry>(scores);

        int entryCount = scores.Count;
        if (entryCount > 1)
        {
            scores.Sort((x, y) => y.Score - x.Score);

            for (int i = 0; i < entryCount; ++i)
                scores[i].transform.SetSiblingIndex(i);
        }
    }

    public bool GetTopEntry(ref string outName, ref int outID, ref int outScore)
    {
        ScoreSummaryEntry[] scores = GetComponentsInChildren<ScoreSummaryEntry>();
        if (scores.Length > 0)
        {
            outName = scores[0].OwnerName;
            outID = scores[0].OwnerID;
            outScore = scores[0].Score;
            return true;
        }
        return false;
    }
}
