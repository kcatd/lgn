using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceBoardGrid : MonoBehaviour
{
	[System.Serializable]
	public class GridData
	{ 
        public Vector2 cellSize;
        public Vector2 spacing;
    }

    [SerializeField] GridData   size4x4;
    [SerializeField] GridData   size5x5;
    [SerializeField] GridData   size6x6;

    [SerializeField] Dice       dicePrefab;
    [SerializeField] Vector2Int baseBoardSize = new Vector2Int(5, 5);

    GridLayoutGroup             grid;
    Vector2                     baseCellSize;
    Vector2                     baseCellSpacing;

    private List<Dice>          diceSet = new List<Dice>();
    DiceCollisionType           diceCollision = DiceCollisionType.Primary;

    private Vector2Int          curBoardSize = new Vector2Int(5, 5);
    private float               curGridItemScale = 1.0f;
    private string              curBoardLayout = "";
    private bool                initBoard = true;

    public string               BoardLayout { get { return curBoardLayout; } }

    // Start is called before the first frame update
    void Start()
    {
        InitBoard();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void InitializeDiceBoard(string[] diceList, GobbleGame game, GameModeSettings settings = null)
    {
        List<string> dicePool = new List<string>();
        List<string> diceUsed = new List<string>();
        Dictionary<int, WordType> specialDiceSet = new Dictionary<int, WordType>();
        int diceCount = curBoardSize.x * curBoardSize.y;

        InitBoard();
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

            Dice newDice = AddDice(diceValue, game, diceType);
            newDice.FadeIn((i * 0.025f), 0.5f);


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

    public void InitializeDiceBoard(int w, int h, string boardLayout, WordPlayerScore foundWord, GobbleGame game)
    {
        InitBoard();

        if ((w > 0) && (h > 0) && !string.IsNullOrEmpty(boardLayout) && (null != foundWord))
        {
            SetDiceCollision(DiceCollisionType.Disabled);
            ClearBoard();
            SetBoardSize(w, h);
            ResizeBoard();

            PopulateBoard(boardLayout, game);

            foreach (var idx in foundWord.diceSet)
            {
                Dice d = GetDice(idx);
                if (null != d)
                {
                    d.SetHighlight(true);
                }
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
            InitBoard();
            ClearBoard();
            ResizeBoard();

            if ("nil" != data)
            {
                PopulateBoard(data, game);
            }
        }
    }

    public void ClearBoard()
    {
        curBoardLayout = "";

        foreach (Dice obj in diceSet)
        {
            Destroy(obj.gameObject);
        }
        diceSet.Clear();

        if (DiceCollisionType.Secondary == diceCollision)
            diceCollision = DiceCollisionType.Primary;
    }

    public Dice GetDice(Vector3 hitPos)
    {
        foreach (Dice obj in diceSet)
        {
            if (obj.IsMouseOver)
                return obj;
        }
        return null;
    }

    public Dice GetDice(int x, int y)
    {
        foreach(Dice obj in diceSet)
        {
            if ((obj.X == x) && (obj.Y == y))
                return obj;
        }
        return null;
    }

    public Dice GetDice(int idx)
    {
        foreach (Dice obj in diceSet)
        {
            if (obj.Idx == idx)
                return obj;
        }
        return null;
    }

    public void SetDiceCollision(DiceCollisionType type)
    {
        diceCollision = type;

        foreach (Dice d in diceSet)
        {
            d.SetDiceCollision(diceCollision);
        }
    }

    private void PopulateBoard(string data, GobbleGame game)
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

    private Dice AddDice(string diceValue, GobbleGame game, WordType type = WordType.Normal, bool rollSet = true)
    {
        InitBoard();

        if (!string.IsNullOrEmpty(diceValue))
        {
            Dice newDice = Instantiate<Dice>(dicePrefab, transform);

            Vector3 diceScale = newDice.transform.localScale;
            diceScale.x *= curGridItemScale;
            diceScale.y *= curGridItemScale;
            diceScale.z *= curGridItemScale;
            newDice.transform.localScale = diceScale;

            newDice.InitDice(diceValue, game, type, rollSet);
            newDice.SetPositionIndex(diceSet.Count, grid.constraintCount);
            newDice.SetDiceCollision(diceCollision);
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

        int count = (int)curBoardSize.x;
        if (count == 4)
        {
            grid.cellSize = size4x4.cellSize;
            grid.spacing = size4x4.spacing;
		} else
        if (count == 5)
        {
            grid.cellSize = size5x5.cellSize;
            grid.spacing = size5x5.spacing;
		}
        if (count == 6)
        {
            grid.cellSize = size6x6.cellSize;
            grid.spacing = size6x6.spacing;
		}
    }

    private void    InitBoard()
    {
        if (initBoard)
        {
            grid = GetComponent<GridLayoutGroup>();
            baseCellSize = grid.cellSize;
            baseCellSpacing = grid.spacing;
            initBoard = false;
        }
    }
}
