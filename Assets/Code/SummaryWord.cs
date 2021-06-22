using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SummaryWord : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI    wordText;

    public string                       Word { get { return wordText.text; } }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSummaryWord(string theWord, bool isPrimary)
    {
        wordText.text = theWord;

        if (!isPrimary)
        {
            Color c = wordText.color;
            c.a *= 0.5f;
            wordText.color = c;
        }
    }
}
