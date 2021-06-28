using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using KitsuneCore.Services.Players;

public class GameScoreBoard : MonoBehaviour
{
    [SerializeField] PlayerScore    scorePrefab;
    [SerializeField] bool           autoResizeGrid = true;

    List<PlayerScore>   activePlayers = new List<PlayerScore>();
    bool                sortFlag = false;

    // Start is called before the first frame update
    void Start()
    {
        if (autoResizeGrid)
        {
            RectTransform rect = scorePrefab.GetComponent<RectTransform>();
            Rect cellRect = rect.rect;
            GridLayoutGroup grid = GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(cellRect.width, cellRect.height);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (sortFlag)
        {
            SortScoreBoard(true);
        }
    }

    public void    ClearScoreBoard()
    {
        foreach (var p in activePlayers)
        {
            Destroy(p.gameObject);
        }
        activePlayers.Clear();
        sortFlag = false;
    }

    public void    AddPlayer(string name, PlayerId id, Color c, bool isLocal)
    {
        if (null == GetPlayer(id))
        {
            PlayerScore newScore = Instantiate<PlayerScore>(scorePrefab, transform);
            newScore.InitScore(name, id, isLocal);
            newScore.UpdateColor(c);
            activePlayers.Add(newScore);
            sortFlag = activePlayers.Count > 1;
        }
    }

    public void AddScore(PlayerId id, int scoreVal, bool setAbsolute = false)
    {
        PlayerScore ps = GetPlayer(id);
        if (null != ps)
        {
            ps.AddScore(scoreVal, setAbsolute);
            sortFlag = activePlayers.Count > 1;
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

    public int  GetScore(PlayerId id)
    {
        PlayerScore score = GetPlayer(id);
        if (null != score)
        {
            return score.Score;
        }
        return 0;
    }

    public void UpdatePlayer(GobbleGame game, PlayerId id, string playerName, int playerScore, int teamID, bool isLocal)
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
            if (teamID >= 0)
            {
                AddPlayer(playerName, id, game.TeamColorTable.GetTeamColor(teamID), isLocal);
                AddScore(id, updateScoreVal, true);
            }
        }
        else
        {
            player.UpdateName(playerName);
            player.UpdateColor(game.TeamColorTable.GetTeamColor(teamID));
            player.AddScore(updateScoreVal, true);
            sortFlag = activePlayers.Count > 1;
        }
    }

    public void SortScoreBoard(bool immediate = false)
    {
        if (immediate)
        {
            int playerCount = activePlayers.Count;
            sortFlag = false;

            if (playerCount > 1)
            {
                activePlayers.Sort((x, y) => x.IsLocal ? -99999 : y.IsLocal ? 99999 : x.Score == y.Score ? string.Compare(x.PlayerName, y.PlayerName, true) : y.Score - x.Score);

                for (int i = 0; i < playerCount; ++i)
                    activePlayers[i].gameObject.transform.SetSiblingIndex(i);
            }
        }
        else if (!sortFlag)
        {
            sortFlag = activePlayers.Count > 1;
        }
    }
}