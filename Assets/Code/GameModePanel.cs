using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using KitsuneCore.Services.Players;

public class GameModePanel : MonoBehaviour
{
    [SerializeField] GobbleGame         game;
    [SerializeField] PlayerLobbyList    playerList;
    [SerializeField] Button             startGameBtn;
    [SerializeField] Selectable[]       hostControls;

    List<GameTimeBtn>               gameTimeGroup;
    List<GameWordLengthBtn>         gameWordLengthGroup;
    List<BoardSizeBtn>              boardSizeGroup;
    List<GameModeEntry>             optionsGroup;

    bool                            initPanel = true;
    bool                            settingsChangedFlag = false;

    // Start is called before the first frame update
    void Start()
    {
        InitPanel();
    }

    // Update is called once per frame
    void Update()
    {
        if (settingsChangedFlag)
        {
            settingsChangedFlag = false;
            game.RefreshGameState();
        }
    }


    void OnStartGameBtn()
    {
        GameModeSettings settings = new GameModeSettings();
        GetGameSettings(ref settings);
        game.ResetGame(settings);
    }

    void OnGameSettingsChanged()
    {
        settingsChangedFlag = true;
    }
    void OnGameSettingsChanged(bool isOn)
    {
        settingsChangedFlag = true;
    }
    void OnGameSettingsChanged(int value)
    {
        settingsChangedFlag = true;
    }

    public void GetGameSettings(ref GameModeSettings settings)
    {
        InitPanel();

        foreach (var btn in gameTimeGroup)
        {
            if (btn.IsToggled)
            {
                settings.gameTime = btn.GameTime;
                break;
            }
        }

        foreach (GameWordLengthBtn btn in gameWordLengthGroup)
        {
            if (btn.IsToggled)
            {
                settings.minWordLen = btn.WordLength;
                break;
            }
        }

        foreach (var btn in boardSizeGroup)
        {
            if (btn.IsToggled)
            {
                settings.boardSize.x = btn.Width;
                settings.boardSize.y = btn.Height;
                break;
            }
        }

        foreach (var btn in optionsGroup)
        {
            switch (btn.OptionType)
            {
                case GameModeOption.EnableDoubleLetterTiles:
                    settings.enableDoubleLetterScore = btn.IsToggled;
                    break;

                case GameModeOption.EnableTripleLetterTiles:
                    settings.enableTripleLetterScore = btn.IsToggled;
                    break;

                case GameModeOption.EnableDoubleWordTiles:
                    settings.enableDoubleWordScore = btn.IsToggled;
                    break;

                case GameModeOption.EnableTripleWordTiles:
                    settings.enableTripleWordScore = btn.IsToggled;
                    break;
            }
        }
    }

    public void SetGameSettings(GameModeSettings settings, bool clearPlayerList)
    {
        InitPanel();

        foreach (var btn in gameTimeGroup)
        {
            if (btn.GameTime == settings.gameTime)
            {
                btn.IsToggled = true;
                break;
            }
        }

        foreach (var btn in gameWordLengthGroup)
        {
            if (btn.WordLength == settings.minWordLen)
            {
                btn.IsToggled = true;
                break;
            }
        }

        foreach (var btn in boardSizeGroup)
        {
            if ((btn.Width == settings.boardSize.x) && (btn.Height == settings.boardSize.y))
            {
                btn.IsToggled = true;
                break;
            }
        }

        foreach (var btn in optionsGroup)
        {
            switch (btn.OptionType)
            {
                case GameModeOption.EnableDoubleLetterTiles:
                    btn.IsToggled = settings.enableDoubleLetterScore;
                    break;

                case GameModeOption.EnableTripleLetterTiles:
                    btn.IsToggled = settings.enableTripleLetterScore;
                    break;

                case GameModeOption.EnableDoubleWordTiles:
                    btn.IsToggled = settings.enableDoubleWordScore;
                    break;

                case GameModeOption.EnableTripleWordTiles:
                    btn.IsToggled = settings.enableTripleWordScore;
                    break;
            }
        }

        if (clearPlayerList)
        {
            playerList.ClearList();
        }
    }

    public void SetupPlayerControllables(bool isHost)
    {
        InitPanel();
        foreach (var obj in hostControls)
        {
            obj.interactable = isHost;
        }
    }

    public void UpdatePlayer(PlayerId id, string playerName, int teamID, bool isHost, bool isLocalPlayer)
    {
        InitPanel();
        playerList.UpdatePlayer(id, playerName, teamID, isHost, isLocalPlayer);
    }

    private void    InitPanel()
    {
        if (initPanel)
        {
            initPanel = false;

            gameTimeGroup = new List<GameTimeBtn>();
            GetComponentsInChildren<GameTimeBtn>(gameTimeGroup);
            foreach(var btn in gameTimeGroup)
            {
                btn.InitButton();
            }

            gameWordLengthGroup = new List<GameWordLengthBtn>();
            GetComponentsInChildren<GameWordLengthBtn>(gameWordLengthGroup);
            foreach (var btn in gameWordLengthGroup)
            {
                btn.InitButton();
            }

            boardSizeGroup = new List<BoardSizeBtn>();
            GetComponentsInChildren<BoardSizeBtn>(boardSizeGroup);
            foreach(var btn in boardSizeGroup)
            {
                btn.InitButton();
            }

            optionsGroup = new List<GameModeEntry>();
            GetComponentsInChildren<GameModeEntry>(optionsGroup);
            foreach (var btn in optionsGroup)
            {
                btn.InitButton();
            }

            startGameBtn.onClick.AddListener(OnStartGameBtn);

            Toggle[] allToggles = GetComponentsInChildren<Toggle>(true);
            foreach (var tog in allToggles)
            {
                tog.onValueChanged.AddListener(OnGameSettingsChanged);
            }
        }
    }
}