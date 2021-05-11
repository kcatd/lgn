using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FoundWordFXEvent
{
    public dice         srcObj;
    public FoundWord    destObj;
    int                 sequenceVal;

    public FoundWordFXEvent(dice d, FoundWord w, int i)
    {
        srcObj = d;
        destObj = w;
        sequenceVal = i;
    }
}

public class FoundWordFX : MonoBehaviour
{
    [SerializeField] float  stage1Speed;
    [SerializeField] float  stage2Speed;

    FoundWordFXEvent    initEvent = null;
    int                 animStageID = 0;

    FoundWord           destWord;
    Vector3             srcPos;
    Vector3             destPos;
    Vector3             origScale;
    float               progCtr;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (null != initEvent)
        {
            if (initEvent.destObj.HasStarted)
            {
                SetupAnimation();
                initEvent = null;
            }
        }
    }

    void FixedUpdate()
    {
        if (animStageID > 0)
        {
            switch (animStageID)
            {
                case 1:
                    UpdateAnimation1();
                    break;
                case 2:
                    UpdateAnimation2();
                    break;
            }
        }
    }

    public void InitFoundWordFX(dice srcObj, FoundWord destObj, int sequenceVal)
    {
        initEvent = new FoundWordFXEvent(srcObj, destObj, sequenceVal);
    }

    void    SetupAnimation()
    {
        var newPos = transform.position;
        origScale = transform.localScale;

        destWord = initEvent.destObj;
        srcPos = initEvent.srcObj.transform.position;
        destPos = initEvent.destObj.transform.position;
        progCtr = 0.0f;

        animStageID = 1;

        TextMeshPro textObj = GetComponent<TextMeshPro>();
        if (null != textObj)
        {
            textObj.text = initEvent.srcObj.FaceValue;
        }

        newPos.x = srcPos.x;
        newPos.y = srcPos.y;
        transform.position = newPos;
        transform.localScale = 0.0f * origScale;
    }

    void    UpdateAnimation1()
    {
        var newScale = transform.localScale;

        progCtr += Time.deltaTime * stage1Speed;
        if (progCtr < 1.0f)
        {
            float progScale = Mathf.Sin(Mathf.Deg2Rad * 90.0f * progCtr);
            newScale = progScale * origScale;
            transform.localScale = newScale;
        }
        else
        {
            transform.localScale = origScale;
            progCtr = 0.0f;
            animStageID = 2;
        }
    }

    void    UpdateAnimation2()
    {
        var newPos = transform.position;
        var newScale = transform.localScale;

        progCtr += Time.deltaTime * stage2Speed;
        if (progCtr < 1.0f)
        {
            float progPos = Mathf.Sin(Mathf.Deg2Rad * 90.0f * progCtr);
            float progScale = 1.0f - progPos;
            Vector3 curPos = srcPos + (progPos * (destPos - srcPos));

            newPos.x = curPos.x;
            newPos.y = curPos.y;
            transform.position = newPos;

            newScale = progScale * origScale;
            transform.localScale = newScale;
        }
        else
        {
            destWord.SetRevealed();

            newPos.x = destPos.x;
            newPos.y = destPos.y;
            transform.position = newPos;

            animStageID = -1;
            Destroy(gameObject);
        }
    }
}