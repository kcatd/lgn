using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SummaryWord : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI    wordText;

    public string                       Word { get { return wordText.text; } set { wordText.text = value; } }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
