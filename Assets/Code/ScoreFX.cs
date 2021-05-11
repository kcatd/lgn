using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using KitsuneCore.Services.Players;

public class ScoreFX : MonoBehaviour
{
    [SerializeField] float  scoreSpeed;

    PlayerId    playerID;
    int         scoreVal;

    GobbleGame  gameParent;
    Vector3     startPos;
    Vector3     destPos;
    Vector3     origScale;
    float       progCtr;

    public int  PlayerID { get { return playerID; } }
    public int  Score { get { return scoreVal; } set { scoreVal = value; } }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        var newPos = transform.position;
        var newScale = transform.localScale;

        progCtr += Time.deltaTime * scoreSpeed;
        if (progCtr < 1.0f)
        {
            float progPos = Mathf.Sin(Mathf.Deg2Rad * 90.0f * progCtr);
            float progScale = Mathf.Sin(Mathf.Deg2Rad * 180.0f * progCtr);
            Vector3 curPos = startPos + (progPos * (destPos - startPos));

            newPos.x = curPos.x;
            newPos.y = curPos.y;
            transform.position = newPos;

            newScale = progScale * origScale;
            transform.localScale = newScale;
        }
        else
        {
            newPos.x = destPos.x;
            newPos.y = destPos.y;
            transform.position = newPos;
            gameParent.ScoreBoard.AddScore(playerID, scoreVal);
            Destroy(gameObject);
        }
    }

    public void    InitScoreFX(PlayerId srcPlayerID, int srcScoreVal, FoundWord objFrom, PlayerScore objTo, GobbleGame objParent)
    {
        var newPos = transform.position;
        origScale = transform.localScale;

        playerID = srcPlayerID;
        scoreVal = srcScoreVal;
        startPos = objFrom.transform.position;
        destPos = objTo.transform.position;
        gameParent = objParent;

        TextMeshProUGUI srcText = objFrom.GetComponent<TextMeshProUGUI>();
        RectTransform srcRect = objFrom.GetComponent<RectTransform>();
        if ((null != srcText) && (null != srcRect))
        {
            Vector3 srcTextOffset = new Vector3(0.5f * (srcText.preferredWidth - srcRect.rect.width), 0.0f, 0.0f);
            startPos.x += objFrom.transform.parent.TransformPoint(srcTextOffset).x;
        }

        TextMeshPro textObj = GetComponent<TextMeshPro>();
        if (null != textObj)
        {
            textObj.text = scoreVal.ToString();
        }

        newPos.x = startPos.x;
        newPos.y = startPos.y;
        transform.position = newPos;
        transform.localScale = 0.01f * origScale;
        progCtr = 0.0f;
    }
}
