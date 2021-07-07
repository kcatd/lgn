using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using KitsuneCore.Services.Players;

public enum EndSummaryMode
{
    CloseOnly,
    QuitToGameOptions,
    RestartWithCurrentSettings,
}

public class PanePositionEntry
{
    public Vector3 pos;
    public int dir;
}
public class SummaryGroup : MonoBehaviour
{
    [SerializeField] GameObject         foregroundGroup;
    [SerializeField] GameObject         backgroundGroup;
    [SerializeField] TextMeshProUGUI    titleText;

    List<SummarySubPanel>               panes;
    PanePositionEntry[]                 panePositions;
    bool                                initSummaryGroup = true;

    GobbleGame                          game = null;
    PlayerId                            summaryPlayer = null;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartSummary(GobbleGame theGame, PlayerId setPlayer, bool allowLocalControl)
    {
        InitSummaryGroup();
        game = theGame;
        summaryPlayer = setPlayer;

        // TODO: animate summary?
        gameObject.SetActive(true);

        if (panes.Count > 0)
        {
            foreach (var pane in panes)
            {
                pane.SetupSummary(game, summaryPlayer);
                pane.SetLocalControl(allowLocalControl);
            }

            if (!panes[0].IsForeground)
            {
                MakeForeground(panes[0]);
            }
        }
    }

    public void EndSummary(EndSummaryMode mode)
    {
        // TODO: animate summary?
        foreach (var pane in panes)
            pane.ClearPanel();

        gameObject.SetActive(false);

        if ((null != game) && (EndSummaryMode.CloseOnly != mode))
        {
            switch (mode)
            {
                case EndSummaryMode.RestartWithCurrentSettings:
                    game.RestartGame();
                    break;
                case EndSummaryMode.QuitToGameOptions:
                    game.InitializeLobby();
                    break;
            }
        }
    }

    public void EndIfActive()
    {
        if (gameObject.activeSelf)
        {
            EndSummary(EndSummaryMode.CloseOnly);
        }
    }

    public void MakeForeground(SummarySubPanel pane)
    {
        int idx = -1;
        InitSummaryGroup();

        idx = GetPaneIdx(pane);
        if (-1 != idx)
        {
            int paneCount = panes.Count;
            for (int i = 0; i < paneCount; ++i)
            {
                SummarySubPanel curPane = panes[(i + idx) % paneCount];
                curPane.SetMainSummaryPanel(0 == i, this);
                MovePane(curPane, 0 == i ? foregroundGroup : backgroundGroup, panePositions[i]);
            }
        }
    }

    void    InitSummaryGroup()
    {
        if (initSummaryGroup)
        {
            SummarySubPanel activePane = foregroundGroup.GetComponentInChildren<SummarySubPanel>();
            SummarySubPanel[] inactivePanes = backgroundGroup.GetComponentsInChildren<SummarySubPanel>();
            Vector3 activePos = new Vector3();

            panes = new List<SummarySubPanel>();

            if (null != activePane)
            {
                activePos = activePane.transform.position;
                activePane.SetMainSummaryPanel(true, this);
                panes.Add(activePane);
            }

            foreach (var pane in inactivePanes)
            {
                pane.SetMainSummaryPanel(false, this);
                panes.Add(pane);
            }

            if (panes.Count > 0)
            {
                int idx = 0;
                panePositions = new PanePositionEntry[panes.Count];

                foreach (var pane in panes)
                {
                    Vector3 pos = pane.transform.position;

                    panePositions[idx] = new PanePositionEntry();
                    panePositions[idx].pos = pos;
                    panePositions[idx].dir = 0;

                    if ((null != activePane) && (pane != activePane))
                    {
                        if (pos.x < activePos.x)
                            panePositions[idx].dir = -1;
                        else if (pos.x > activePos.x)
                            panePositions[idx].dir = 1;
                    }

                    pane.SetPanePosition(panePositions[idx].dir);
                    ++idx;
                }
            }

            initSummaryGroup = false;
        }
    }

    void    MovePane(SummarySubPanel pane, GameObject parent, PanePositionEntry pos)
    {
        pane.SetPanePosition(pos.dir);

        // TODO: animate motion?
        pane.transform.SetParent(parent.transform);
        pane.transform.position = pos.pos;
    }

    SummarySubPanel GetPaneByType(SummaryPaneType type)
    {
        foreach (var p in panes)
        {
            if (p.PaneType == type)
                return p;
        }
        return null;
    }
    int     GetPaneIdx(SummaryPaneType type)
    {
        int idx = 0;
        foreach (var p in panes)
        {
            if (p.PaneType == type)
                return idx;

            ++idx;
        }
        return -1;
    }
    int     GetPaneIdx(SummarySubPanel pane)
    {
        int idx = 0;
        foreach (var p in panes)
        {
            if (p == pane)
                return idx;

            ++idx;
        }
        return -1;
    }
}
