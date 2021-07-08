using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum DiceCollisionType
{
    Disabled,
    Primary,
    Secondary,
}

public enum WordType
{
    Normal              = 0,
    DoubleLetterScore   = 1,
    TripleLetterScore   = 2,
    DoubleWordScore     = 3,
    TripleWordScore     = 4,
}

public class Dice : MonoBehaviour
{
    [System.Serializable]
    public class LetterState
    {
        public Image           normal;
        public Image           over;
        public Image           active;

        public void Update(Image current)
        {
            normal.gameObject.SetActive(normal == current);    
            over.gameObject.SetActive(over == current);    
            active.gameObject.SetActive(active == current);    
		}
	}
    [SerializeField] LetterState         normalTile;
    [SerializeField] LetterState         doubleLetterTile;
    [SerializeField] LetterState         trippleLetterTile;
    [SerializeField] LetterState         doubleWordTile;
    [SerializeField] LetterState         trippleWordTile;
   
    [SerializeField] TextMeshProUGUI    diceFaceText;
    [SerializeField] TextMeshProUGUI    diceFaceScoreText;

    private int         posIdx = -1;
    private int         posX = -1;
    private int         posY = -1;
    private string      diceFaceValue = "a";
    private WordType    diceWordType = WordType.Normal;
    private bool        isHighlighted = false;
    private bool        isMouseOver = false;
    private bool        isEnabled = true;

    private BoxCollider2D               primaryCollider;
    private CircleCollider2D            secondaryCollider;
    private List<string>                diceFaces = new List<string>();
    private Color                       color  = Color.white;


    public string   FaceValue { get { return diceFaceValue; } }
    public WordType DiceType { get { return diceWordType; } }
    public int      Idx { get { return posIdx; } }
    public int      X { get { return 1; } }
    public int      Y { get { return posY; } }
    public bool     IsHighlighted { get { return isHighlighted; } }
    public bool     IsMouseOver { get { return isMouseOver; } }

    // Start is called before the first frame update
    void Start()
    {
    }


    private void OnMouseEnter()
    {
        if (isEnabled)
        {
            isMouseOver = true;
            SetHighlight(isHighlighted);
        }
    }

    private void OnMouseExit()
    {
        if (isEnabled)
        {
            isMouseOver = false;
            SetHighlight(isHighlighted);
        }
    }

    public void SetDiceCollision(DiceCollisionType type)
    {
        if (null == primaryCollider)
            primaryCollider = GetComponent<BoxCollider2D>();

        if (null == secondaryCollider)
            secondaryCollider = GetComponent<CircleCollider2D>();

        primaryCollider.enabled = DiceCollisionType.Primary == type;
        secondaryCollider.enabled = DiceCollisionType.Secondary == type;
        isEnabled = DiceCollisionType.Disabled != type;

        diceFaceScoreText.gameObject.SetActive(isEnabled);
    }

    public void InitDice(string Data, GobbleGame game, WordType type = WordType.Normal, bool rollSet = true)
    {
        if (!string.IsNullOrEmpty(Data))
        {
            string[] Tokens = Data.Split(',');
            diceFaces.Clear();

            if (rollSet)
            {
                if (Tokens.Length > 1)
                {
                    foreach (string tok in Tokens)
                    {
                        diceFaces.Add(tok);
                    }
                }
                else
                {
                    for (int i = 0; i < Data.Length; ++i)
                    {
                        diceFaces.Add(Data[i].ToString());
                    }
                }
            }
            else
            {
                diceFaces.Add(Data);
            }

            diceWordType = type;

/*            diceHighlight.gameObject.SetActive(false);
            diceHighlight3D.gameObject.SetActive(false);

            doubleLetterScoreIcon.gameObject.SetActive(WordType.DoubleLetterScore == diceWordType);
            tripleLetterScoreIcon.gameObject.SetActive(WordType.TripleLetterScore == diceWordType);
            doubleWordScoreIcon.gameObject.SetActive(WordType.DoubleWordScore == diceWordType);
            tripleWordScoreIcon.gameObject.SetActive(WordType.TripleWordScore == diceWordType);
            */
            RollDice(game);
        }
    }

    Image DesiredFace()
    {
        LetterState current = normalTile;
        if (isEnabled)
        {
            switch (diceWordType)
            {
                case WordType.Normal: current = normalTile; break;
                case WordType.DoubleLetterScore: current = doubleLetterTile; break;
                case WordType.TripleLetterScore: current = trippleLetterTile; break;
                case WordType.DoubleWordScore: current = doubleWordTile; break;
                case WordType.TripleWordScore: current = trippleWordTile; break;
            }
        }
        if (isMouseOver) return current.over;
        if (isHighlighted) return current.active;
        return current.normal;
	}

    void Update()
    {
        Image current = DesiredFace();
        normalTile.Update(current);
        doubleLetterTile.Update(current);
        trippleLetterTile.Update(current);
        doubleWordTile.Update(current);
        trippleWordTile.Update(current);

        current.color = color;
	}

    IEnumerator BounceCo(float duration)
    {
        Vector3 defaultScale = transform.localScale;
        float t = 0;
        while (t < duration)
        {
            t+=Time.deltaTime;
            float scale = Mathf.Sin((t/duration) * Mathf.PI);
            transform.localScale = defaultScale * (1.0f + (scale * 0.1f));
            yield return new WaitForEndOfFrame();
		}
        transform.localScale = defaultScale;
    }

    IEnumerator FadeInCo(float initialDelay, float duration)
    {
        float t = 0;
        color = new Color(1,1,1,0);
        DesiredFace().color = color;

        yield return new WaitForSeconds(initialDelay);
        while (t < duration)
        {
            t+=Time.deltaTime;
            float alpha = Mathfx.Hermite(0, 1, t/duration);
            color = new Color(1,1,1,alpha);
            DesiredFace().color = color;
            yield return new WaitForEndOfFrame();
		}

        color = Color.white;
        DesiredFace().color = color;        
	}

    public void FadeIn(float initialDelay, float duration)
    {
        StartCoroutine(FadeInCo(initialDelay, duration));
	}
    
    public void Pulse()
    {
        StartCoroutine(BounceCo(0.5f));
	}
    public void RollDice(GobbleGame game)
    {
        if (diceFaces.Count > 0)
        {
            string faceText = "";
            int idx = Random.Range(0, diceFaces.Count);

            diceFaceValue = diceFaces[idx];

            for (int i = 0; i < diceFaceValue.Length; ++i)
            {
                if (i > 0)
                    faceText += char.ToLower(diceFaceValue[i]).ToString();
                else
                    faceText = char.ToUpper(diceFaceValue[i]).ToString();
            }

            diceFaceText.text = faceText;
            diceFaceScoreText.text = game.GetWordScore(faceText).ToString();



//            diceFaceText3D.text = faceText;
//            diceFaceScoreText3D.text = game.GetWordScore(faceText).ToString();

            //GetComponent<Image>().enabled = !enable3D;
            //diceFaceText.gameObject.SetActive(!enable3D);
            //diceFaceText3D.gameObject.SetActive(enable3D);
        }
    }

    public void SetPositionIndex(int idx, int constraintCount)
    {
        posIdx = idx;
        posX = idx % constraintCount;
        posY = idx / constraintCount;
    }

    public bool IsAdjacent(Dice obj)
    {
        if (this != obj)
        {
            int dx = Mathf.Abs(posX - obj.posX);
            int dy = Mathf.Abs(posY - obj.posY);

            return ((1 == dx) && (dy <= 1)) || ((1 == dy) && (dx <= 1));
        }
        return false;
    }

    public void SetHighlight(bool b)
    {
        isHighlighted = b;
//        diceHighlight.gameObject.SetActive(!enable3D && (isHighlighted || isMouseOver));
//       diceHighlight3D.gameObject.SetActive(enable3D && (isHighlighted || isMouseOver));
    }

    public int  GetLetterMultiplier()
    {
        switch (diceWordType)
        {
            case WordType.DoubleLetterScore:    return 2;
            case WordType.TripleLetterScore:    return 3;
        }
        return 1;
    }
    public int  GetWordMultiplier()
    {
        switch (diceWordType)
        {
            case WordType.DoubleWordScore:  return 2;
            case WordType.TripleWordScore:  return 3;
        }
        return 1;
    }
}
