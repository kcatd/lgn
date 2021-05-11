using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceBoardGrid : MonoBehaviour
{
    [SerializeField] dice   dicePrefab;
    [SerializeField] int    boardSize;

    private List<dice>      diceSet = new List<dice>();

    private string          curBoardLayout = "";

    public string           BoardLayout { get { return curBoardLayout; } }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void InitializeDefault()
    {
        int idx = 0;

        ClearBoard();

        AddDice("L"); ++idx;
        AddDice("G"); ++idx;
        AddDice("N"); ++idx;
        AddDice("."); ++idx;
        AddDice("."); ++idx;
        AddDice("G"); ++idx;
        AddDice("O"); ++idx;
        AddDice("B"); ++idx;
        AddDice("L"); ++idx;
        AddDice("E"); ++idx;

        for (int i = idx; i < boardSize; ++i)
            AddDice(".");
    }

    public void InitializeDiceBoard(string[] diceList)
    {
        List<string> dicePool = new List<string>();
        List<string> diceUsed = new List<string>();

        ClearBoard();

        foreach (string d in diceList)
        {
            dicePool.Add(d);
        }

        for (int i = 0; i < boardSize; ++i)
        {
            int idx = Random.Range(0, dicePool.Count);
            string diceValue = dicePool[idx];

            AddDice(diceValue);

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

    public void UpdateBoardLayout(string data)
    {
        if (!string.IsNullOrEmpty(data) && (0 != string.Compare(data, curBoardLayout, true)))
        {
            GridLayoutGroup grid = GetComponent<GridLayoutGroup>();

            ClearBoard();

            string[] tokens = data.Split(',');
            foreach (string t in tokens)
            {
                AddDice(t);
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

    private dice AddDice(string diceValue)
    {
        if (!string.IsNullOrEmpty(diceValue))
        {
            GridLayoutGroup grid = GetComponent<GridLayoutGroup>();
            dice newDice = Instantiate<dice>(dicePrefab, transform);

            newDice.InitDice(diceValue);
            newDice.SetPositionIndex(diceSet.Count, grid.constraintCount);
            diceSet.Add(newDice);

            if (curBoardLayout.Length > 0)
            {
                curBoardLayout += "," + newDice.FaceValue;
            }
            else
            {
                curBoardLayout = newDice.FaceValue;
            }
            return newDice;
        }
        return null;
    }
}
