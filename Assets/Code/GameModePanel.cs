using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameModePanel : MonoBehaviour
{
    [SerializeField] GobbleGame game;
    [SerializeField] Button startGameBtn;

    List<GameTimeBtn>       gameTimeGroup = new List<GameTimeBtn>();
    GameModeSettings        settings;

    // Start is called before the first frame update
    void Start()
    {
        startGameBtn.onClick.AddListener(OnStartGameBtn);

        GetComponentsInChildren<GameTimeBtn>(gameTimeGroup);
        foreach (var btn in gameTimeGroup)
        {
            btn.GetComponent<Toggle>().onValueChanged.AddListener(delegate { OnGameTimeToggled(btn); } );
            btn.IsToggled = btn.IsDefault;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        settings = new GameModeSettings();

        foreach (var btn in gameTimeGroup)
        {
            if (btn.IsToggled)
                settings.gameTime = btn.GameTime;
        }
    }

    void OnGameTimeToggled(GameTimeBtn btnToggled)
    {
        foreach (var btn in gameTimeGroup)
        {
            if (btn == btnToggled)
            {
                settings.gameTime = btn.GameTime;
            }
            else
            {
                btn.IsToggled = false;
            }
        }
    }

    void OnStartGameBtn()
    {
        Debug.Log(string.Format("Start game time: {0}", settings.gameTime));
        game.ResetGame();
    }
}
