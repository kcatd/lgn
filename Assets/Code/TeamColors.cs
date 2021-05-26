using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct TeamColor
{
    public int      teamID;
    public Color    teamColor;
}

public class TeamColors : MonoBehaviour
{
    [SerializeField] TeamColor[]    teamColors;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Color    GetTeamColor(int teamID, bool defaultToWhite = true)
    {
        foreach (var team in teamColors)
        {
            if (teamID == team.teamID)
            {
                return team.teamColor;
            }
        }
        return defaultToWhite ? new Color(1.0f, 1.0f, 1.0f) : new Color(0.0f, 0.0f, 0.0f, 0.0f);
    }

    public int      GetMaxTeams()
    {
        return teamColors.Length;
    }
}
