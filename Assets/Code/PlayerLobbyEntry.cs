using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using KitsuneCore.Services.Players;

public class PlayerLobbyEntry : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI    playerNameText;
    [SerializeField] Button             joinGameBtn;
    [SerializeField] Button             spectateGameBtn;
    [SerializeField] Button             kickGameBtn;
    [SerializeField] GameObject         isHostIndicator;
    [SerializeField] GameObject         isSpectatorIndicator;

    PlayerLobbyList                     parentList;
    PlayerId                            playerId;
    int                                 playerTeamId = 0;
    bool                                localPlayer = false;
    bool                                updateFlag = false;

    public int                          PlayerID { get { return playerId; } }
    public int                          TeamID { get { return playerTeamId; } }
    public bool                         IsLocalPlayer { get { return localPlayer; } }

    // Start is called before the first frame update
    void Start()
    {
        PlayerTeamBtn[] teamBtns = GetComponentsInChildren<PlayerTeamBtn>(true);
        foreach (var btn in teamBtns)
        {
            btn.GetComponent<Toggle>().onValueChanged.AddListener(OnEntryUpdated);
        }

        joinGameBtn.onClick.AddListener(OnJoinBtn);
        spectateGameBtn.onClick.AddListener(OnSpectateBtn);
        //kickGameBtn.onClick.AddListener(OnKickBtn);
    }

    // Update is called once per frame
    void Update()
    {
        if (updateFlag)
        {
            updateFlag = false;
            parentList.SetListUpdate();
        }
    }

    void    OnEntryUpdated(bool isOn)
    {
        updateFlag = true;
    }

    void    OnSpectateBtn()
    {
        if (playerTeamId >= 0)
        {
            updateFlag = false;
            playerTeamId = -1;

            spectateGameBtn.interactable = false;
            parentList.SetListUpdate();
        }
    }

    void    OnJoinBtn()
    {
        if (playerTeamId < 0)
        {
            updateFlag = false;
            playerTeamId = 0;

            joinGameBtn.interactable = false;
            parentList.SetListUpdate();
        }
    }

    void    OnKickBtn()
    {
    }

    public void InitPlayer(PlayerId id, string playerName, int teamID, bool isHost, bool isLocalPlayer, PlayerLobbyList listObj)
    {
        PlayerTeamBtn[] teamBtns = GetComponentsInChildren<PlayerTeamBtn>(true);
        foreach (var btn in teamBtns)
        {
            btn.InitState(listObj.Game.TeamColorTable);
        }
        UpdatePlayer(id, playerName, teamID, isHost, isLocalPlayer, listObj);
    }

    public void UpdatePlayer(PlayerId id, string playerName, int teamID, bool isHost, bool isLocalPlayer, PlayerLobbyList listObj)
    {
        PlayerTeamBtn[] teamBtns = GetComponentsInChildren<PlayerTeamBtn>(true);

        parentList = listObj;
        playerId = id;
        playerTeamId = teamID;
        localPlayer = isLocalPlayer;

        playerNameText.text = playerName;
        //playerNameText.color = parentList.Game.TeamColorTable.GetTeamColor(playerTeamId);

        foreach (var btn in teamBtns)
        {
            btn.IsToggled = teamID == btn.TeamID;
            btn.CanInteract = isLocalPlayer;
        }

        joinGameBtn.interactable = true;
        spectateGameBtn.interactable = true;

        joinGameBtn.gameObject.SetActive(isLocalPlayer && (teamID < 0));
        spectateGameBtn.gameObject.SetActive(isLocalPlayer && !isHost && (teamID >= 0));
        isHostIndicator.SetActive(isHost);
        isSpectatorIndicator.SetActive(!isLocalPlayer && (teamID < 0));
    }

    public int  GetSelectedTeamID()
    {
        PlayerTeamBtn[] teamBtns = GetComponentsInChildren<PlayerTeamBtn>(true);
        foreach (var btn in teamBtns)
        {
            if (btn.IsToggled)
            {
                return btn.TeamID;
            }
        }
        return 0;
    }

    public int  GetSortIdx(PlayerLobbyEntry rightEntry)
    {
        if (IsLocalPlayer)
            return -9999;
        else if (rightEntry.IsLocalPlayer)
            return 9999;

        if (playerTeamId != rightEntry.playerTeamId)
        {
            if (playerTeamId < 0)
                return 9999;
            else if (rightEntry.playerTeamId < 0)
                return 9999;
        }

        return string.Compare(playerNameText.text, rightEntry.playerNameText.text);
    }
}