using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameTimeBtn : MonoBehaviour
{
    [SerializeField] int    timeInSeconds = 60;
    Toggle                  tog;
    TextMeshProUGUI         label;
    Color                   textColor;
    bool                    initButton = true;

    public int              GameTime { get { return timeInSeconds; } }

    public bool             IsToggled { get { return tog.isOn; } set { tog.isOn = value; } }

    // Start is called before the first frame update
    void Start()
    {
        InitButton();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnToggle(bool isOn)
    {
        label.color = new Color(textColor.r, textColor.g, textColor.b, isOn ? textColor.a : textColor.a * 0.33f);
    }

    public void InitButton()
    {
        if (initButton)
        {
            initButton = false;

            tog = GetComponent<Toggle>();
            label = GetComponentInChildren<TextMeshProUGUI>();
            textColor = label.color;

            tog.onValueChanged.AddListener(OnToggle);
            OnToggle(tog.isOn);
        }
    }
}
