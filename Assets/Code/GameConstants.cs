using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SpecialTilesInfo
{
    public Vector2Int  boardSize;
    public int         doubleWordTiles;
    public int         tripleWordTiles;
}

public class GameConstants : MonoBehaviour
{
    [SerializeField]
    SpecialTilesInfo[]          specialTiles;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool GetSpecialTileCount(int w, int h, ref int doubleWordCount, ref int tripleWordCount)
    {
        foreach (var data in specialTiles)
        {
            if ((w == data.boardSize.x) && (h == data.boardSize.y))
            {
                doubleWordCount = data.doubleWordTiles;
                tripleWordCount = data.tripleWordTiles;
                return true;
            }
        }
        return false;
    }
}
