using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using KitsuneCore.Services.Players;

public class GameScoreBoard : MonoBehaviour
{
    [SerializeField] PlayerScore    scorePrefab;

    List<PlayerScore>   activePlayers = new List<PlayerScore>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void    ClearScoreBoard()
    {
        foreach (var p in activePlayers)
        {
            Destroy(p.gameObject);
        }
        activePlayers.Clear();
    }

    public void    AddPlayer(string name, PlayerId id)
    {
        if (null == GetPlayer(id))
        {
            PlayerScore newScore = Instantiate<PlayerScore>(scorePrefab, transform);
            newScore.InitScore(name, id);
            activePlayers.Add(newScore);
        }
    }

    public void AddScore(PlayerId id, int scoreVal, bool setAbsolute = false)
    {
        PlayerScore ps = GetPlayer(id);
        if (null != ps)
        {
            ps.AddScore(scoreVal, setAbsolute);
        }
    }

    public PlayerScore GetPlayer(PlayerId id)
    {
        foreach (var p in activePlayers)
        {
            if (p.PlayerID == id)
                return p;
        }
        return null;
    }

    public void UpdatePlayer(GobbleGame game, PlayerId id, string playerName, int playerScore)
    {
        List<ScoreFXEvent> pendingSet = new List<ScoreFXEvent>();
        List<ScoreFX> execSet = new List<ScoreFX>();
        int updateScoreVal = playerScore;

        PlayerScore player = GetPlayer(id);
        int pendingScoreVal = game.GetPendingScore(id, ref pendingSet, ref execSet);

        if (pendingScoreVal > 0)
        {
            int toDisplay = updateScoreVal - pendingScoreVal;
            int currentDisplayScore = 0;

            if (null != player)
            {
                currentDisplayScore = player.Score;
            }

            if (toDisplay >= currentDisplayScore)
            {
                updateScoreVal = toDisplay;
            }
            else
            {
                // TODO: uh... somehow make corrections/alterations to the pending fx?
                updateScoreVal = toDisplay;
            }
        }

        if (null == player)
        {
            AddPlayer(playerName, id);
            AddScore(id, updateScoreVal, true);
        }
        else
        {
            player.UpdateName(playerName);
            player.AddScore(updateScoreVal, true);
        }
    }
}
