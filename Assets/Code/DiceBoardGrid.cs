using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceBoardGrid : MonoBehaviour
{
    [SerializeField] dice       dicePrefab;
    [SerializeField] Vector2Int baseBoardSize = new Vector2Int(5, 5);

    GridLayoutGroup             grid;
    Vector2                     baseCellSize;
    Vector2                     baseCellSpacing;

    private List<dice>          diceSet = new List<dice>();
    private bool                diceCollisionFlag = true;

    private Vector2Int          curBoardSize = new Vector2Int(5, 5);
    private float               curGridItemScale = 1.0f;
    private string              curBoardLayout = "";

    public string               BoardLayout { get { return curBoardLayout; } }

    // Start is called before the first frame update
    void Start()
    {
        grid = GetComponent<GridLayoutGroup>();
        baseCellSize = grid.cellSize;
        baseCellSpacing = grid.spacing;

        // test
        // SetBoardSize(6, 6);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void InitializeDefault(GobbleGame game)
    {
        int diceCount = curBoardSize.x * curBoardSize.y;
        int idx = 0;

        ClearBoard();
        ResizeBoard();

        AddDice("L", game); ++idx;
        AddDice("G", game); ++idx;
        AddDice("N", game); ++idx;
        AddDice(".", game); ++idx;
        AddDice(".", game); ++idx;
        AddDice("G", game); ++idx;
        AddDice("O", game); ++idx;
        AddDice("B", game); ++idx;
        AddDice("L", game); ++idx;
        AddDice("E", game); ++idx;

        for (int i = idx; i < diceCount; ++i)
            AddDice(".", game);
    }

    public void InitializeDiceBoard(string[] diceList, GobbleGame game, GameModeSettings settings = null)
    {
        List<string> dicePool = new List<string>();
        List<string> diceUsed = new List<string>();
        Dictionary<int, WordType> specialDiceSet = new Dictionary<int, WordType>();
        int diceCount = curBoardSize.x * curBoardSize.y;

        ClearBoard();
        ResizeBoard();

        if (null != settings)
        {
            SpecialTilesInfo specials = game.Constants.GetSpecialTilesInfo(settings.boardSize.x, settings.boardSize.y);

            if (null != specials)
            {
                if (settings.enableDoubleLetterScore)
                {
                    for (int i = 0; i < specials.doubleLetterTiles; ++i)
                    {
                        int idx = Random.Range(0, diceCount);

                        if (specialDiceSet.ContainsKey(idx))
                            continue;

                        specialDiceSet[idx] = WordType.DoubleLetterScore;
                    }
                }
                if (settings.enableTripleLetterScore)
                {
                    for (int i = 0; i < specials.tripleLetterTiles; ++i)
                    {
                        int idx = Random.Range(0, diceCount);

                        if (specialDiceSet.ContainsKey(idx))
                            continue;

                        specialDiceSet[idx] = WordType.TripleLetterScore;
                    }
                }
                if (settings.enableDoubleWordScore)
                {
                    for (int i = 0; i < specials.doubleWordTiles; ++i)
                    {
                        int idx = Random.Range(0, diceCount);

                        if (specialDiceSet.ContainsKey(idx))
                            continue;

                        specialDiceSet[idx] = WordType.DoubleWordScore;
                    }
                }
                if (settings.enableTripleWordScore)
                {
                    for (int i = 0; i < specials.tripleWordTiles; ++i)
                    {
                        int idx = Random.Range(0, diceCount);

                        if (specialDiceSet.ContainsKey(idx))
                            continue;

                        specialDiceSet[idx] = WordType.TripleWordScore;
                    }
                }
            }
        }

        foreach (string d in diceList)
        {
            dicePool.Add(d);
        }

        for (int i = 0; i < diceCount; ++i)
        {
            int idx = Random.Range(0, dicePool.Count);
            string diceValue = dicePool[idx];

            WordType diceType = WordType.Normal;
            if (specialDiceSet.ContainsKey(i))
                diceType = specialDiceSet[i];

            AddDice(diceValue, game, diceType);

            diceUsed.Add(diceValue);
            dicePool.RemoveAt(idx);

            if (dicePool.Count < 1)
            {
                foreach (string d in diceUsed)
                {
                    dicePool.Add(d);
                }
                diceUsed.Clear();
            }
        }
    }

    public void SetBoardSize(int x, int y)
    {
        curBoardSize.x = x;
        curBoardSize.y = y;

        curGridItemScale = (float)baseBoardSize.x / (float)curBoardSize.x;
    }

    public void UpdateBoardLayout(string data, GobbleGame game)
    {
        if (!string.IsNullOrEmpty(data) && (0 != string.Compare(data, curBoardLayout, true)))
        {
            ClearBoard();
            ResizeBoard();

            if ("nil" != data)
            {
                string[] tokens = data.Split(',');
                foreach (string t in tokens)
                {
                    string[] toks = t.Split(':');

                    if (toks.Length > 1)
                    {
                        WordType type = (WordType)int.Parse(toks[1]);
                        AddDice(toks[0], game, type, false);
                    }
                    else
                    {
                        AddDice(t, game, WordType.Normal, false);
                    }
                }
            }
        }
    }

    public void ClearBoard()
    {
        curBoardLayout = "";

        foreach (dice obj in diceSet)
        {
            Destroy(obj.gameObject);
        }
        diceSet.Clear();
        diceCollisionFlag = true;
    }

    public dice GetDice(Vector3 hitPos)
    {
        foreach (dice obj in diceSet)
        {
            if (obj.IsMouseOver)
                return obj;
        }
        return null;
    }

    public void SetDiceCollision(bool isPrimary)
    {
        diceCollisionFlag = isPrimary;

        foreach (dice d in diceSet)
        {
            d.SetDiceCollision(isPrimary);
        }
    }

    private dice AddDice(string diceValue, GobbleGame game, WordType type = WordType.Normal, bool rollSet = true)
    {
        if (!string.IsNullOrEmpty(diceValue))
        {
            dice newDice = Instantiate<dice>(dicePrefab, transform);

            Vector3 diceScale = newDice.transform.localScale;
            diceScale.x *= curGridItemScale;
            diceScale.y *= curGridItemScale;
            diceScale.z *= curGridItemScale;
            newDice.transform.localScale = diceScale;

            newDice.InitDice(diceValue, game, type, rollSet);
            newDice.SetPositionIndex(diceSet.Count, grid.constraintCount);
            newDice.SetDiceCollision(diceCollisionFlag);
            diceSet.Add(newDice);

            if (curBoardLayout.Length > 0)
            {
                curBoardLayout += "," + newDice.FaceValue;
            }
            else
            {
                curBoardLayout = newDice.FaceValue;
            }

            if (WordType.Normal != type)
            {
                int typeVal = (int)type;
                curBoardLayout += string.Format(":{0}", typeVal.ToString());
            }
            return newDice;
        }
        return null;
    }

    private void    ResizeBoard()
    {
        grid.constraintCount = curBoardSize.x;
        grid.cellSize = curGridItemScale * baseCellSize;
        grid.spacing = curGridItemScale * baseCellSpacing;
    }
}
