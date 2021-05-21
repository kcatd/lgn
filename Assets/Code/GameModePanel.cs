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
    [SerializeField] TMP_Dropdown       minWordLengthGroup;
    [SerializeField] Selectable[]       hostControls;

    List<GameTimeBtn>               gameTimeGroup;
    List<BoardSizeBtn>              boardSizeGroup;

    bool                            settingsChangedFlag = false;

    // Start is called before the first frame update
    void Start()
    {
        gameTimeGroup = new List<GameTimeBtn>();
        GetComponentsInChildren<GameTimeBtn>(gameTimeGroup);

        boardSizeGroup = new List<BoardSizeBtn>();
        GetComponentsInChildren<BoardSizeBtn>(boardSizeGroup);

        startGameBtn.onClick.AddListener(OnStartGameBtn);

        minWordLengthGroup.onValueChanged.AddListener(OnGameSettingsChanged);

        Toggle[] allToggles = GetComponentsInChildren<Toggle>(true);
        foreach (var tog in allToggles)
        {
            tog.onValueChanged.AddListener(OnGameSettingsChanged);
        }
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

    private void OnEnable()
    {
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
        foreach (var btn in gameTimeGroup)
        {
            if (btn.IsToggled)
            {
                settings.gameTime = btn.GameTime;
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

        settings.minWordLen = int.Parse(minWordLengthGroup.captionText.text);
    }

    public void SetGameSettings(GameModeSettings settings, bool clearPlayerList)
    {
        foreach (var btn in gameTimeGroup)
        {
            if (btn.GameTime == settings.gameTime)
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

        string minWordLenStr = settings.minWordLen.ToString();
        int optionC = minWordLengthGroup.options.Count;

        for (int i = 0; i < optionC; ++i)
        {
            if (minWordLenStr == minWordLengthGroup.options[i].text)
            {
                minWordLengthGroup.value = i;
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
        foreach (var obj in hostControls)
        {
            obj.interactable = isHost;
        }
    }

    public void UpdatePlayer(PlayerId id, string playerName, int teamID, bool isHost, bool isLocalPlayer)
    {
        playerList.UpdatePlayer(id, playerName, teamID, isHost, isLocalPlayer);
    }
}