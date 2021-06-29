using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameModeOption
{
    EnableDoubleLetterTiles,
    EnableTripleLetterTiles,
    EnableDoubleWordTiles,
    EnableTripleWordTiles,
}

public class GameModeEntry : MonoBehaviour
{
    [SerializeField] GameModeOption     optionType;
    [SerializeField] Toggle             optionToggle;
    [SerializeField] Image              selectedBackground;
    Toggle                              tog;
    public bool                         initButton = true;

    public GameModeOption OptionType    { get {return optionType; } }
    public bool IsToggled               { get { return optionToggle.isOn; } set { optionToggle.SetIsOnWithoutNotify(value); } }

    // Start is called before the first frame update
    void Start()
    {
        InitButton();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void    OnToggle(bool isOn)
    {
        if (null != selectedBackground)
            selectedBackground.gameObject.SetActive(isOn);
    }

    public void InitButton()
    {
        if (initButton)
        {
            initButton = false;

            tog = GetComponentInChildren<Toggle>();
            if (null != tog)
            {
                tog.onValueChanged.AddListener(OnToggle);
                OnToggle(tog.isOn);
            }
        }
    }
}
