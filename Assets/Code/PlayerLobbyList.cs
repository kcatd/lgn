using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using KitsuneCore.Services.Players;

public class PlayerLobbyList : MonoBehaviour
{
    [SerializeField] PlayerLobbyEntry   playerEntryPrefab;
    [SerializeField] GobbleGame         game;

    List<PlayerLobbyEntry>              players = new List<PlayerLobbyEntry>();
    bool                                isListUpdated = false;

    public GobbleGame                   Game { get { return game; } }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isListUpdated)
        {
            isListUpdated = false;

            foreach (var player in players)
            {
                if (player.IsLocalPlayer)
                {
                    int teamID = player.TeamID;
                    if (teamID < 0)
                    {
                        game.UpdatePlayerTeam(teamID);
                    }
                    else
                    {
                        game.UpdatePlayerTeam(player.GetSelectedTeamID());
                    }
                    break;
                }
            }
        }
    }

    PlayerLobbyEntry    GetPlayer(PlayerId id)
    {
        foreach (var player in players)
        {
            if (player.PlayerID == id)
            {
                return player;
            }
        }
        return null;
    }

    public void UpdatePlayer(PlayerId id, string playerName, int teamID, bool isHost, bool isLocalPlayer)
    {
        PlayerLobbyEntry player = GetPlayer(id);
        if (null == player)
        {
            player = Instantiate<PlayerLobbyEntry>(playerEntryPrefab, transform);
            players.Add(player);
            player.InitPlayer(id, playerName, teamID, isHost, isLocalPlayer, this);
        }
        else
        {
            player.UpdatePlayer(id, playerName, teamID, isHost, isLocalPlayer, this);
        }
    }

    public void ClearList()
    {
        foreach (var player in players)
        {
            Destroy(player.gameObject);
        }
        players.Clear();
        isListUpdated = false;
    }

    public void SetListUpdate()
    {
        isListUpdated = true;
    }
}
