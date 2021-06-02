using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpecialTilesInfo
{
    public Vector2Int  boardSize;
    public int         doubleLetterTiles;
    public int         tripleLetterTiles;
    public int         doubleWordTiles;
    public int         tripleWordTiles;
}

public class GameConstants : MonoBehaviour
{
    [Header("Tiles")]
    [SerializeField] SpecialTilesInfo[] specialTiles;

    [Header("Flags")]
    [SerializeField] bool       useDnDStyleMultipliers;

    public bool                 UseDnDStyleMultipliers { get { return useDnDStyleMultipliers; } }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public SpecialTilesInfo GetSpecialTilesInfo(int w, int h)
    {
        foreach (var data in specialTiles)
        {
            if ((w == data.boardSize.x) && (h == data.boardSize.y))
                return data;
        }
        return null;
    }
}
