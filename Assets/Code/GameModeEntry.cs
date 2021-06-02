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

    public GameModeOption OptionType    { get {return optionType; } }
    public bool IsToggled               { get { return optionToggle.isOn; } set { optionToggle.SetIsOnWithoutNotify(value); } }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
