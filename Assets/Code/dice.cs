using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class dice : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] TextMeshProUGUI    diceFaceText;
    [SerializeField] GameObject         diceHighlight;
    [SerializeField] TextMeshProUGUI    diceFaceText3D;
    [SerializeField] TextMeshProUGUI    diceFaceScoreText3D;
    [SerializeField] GameObject         diceHighlight3D;
    [SerializeField] GameObject         diceCube3D;
    [SerializeField] float              rotationScale;
    [SerializeField] float              rotationSpeed;
    [SerializeField] bool               enable3D;
    
    private List<string>    diceFaces = new List<string>();
    private Vector3         desiredFacing3D = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3         actualFacing3D = new Vector3(0.0f, 0.0f, 0.0f);

    private Collider2D  diceCollider;
    private int         posX = -1;
    private int         posY = -1;
    private string      diceFaceValue = "a";
    private bool        isHighlighted = false;
    private bool        isMouseOver = false;

    public string   FaceValue { get { return diceFaceValue; } }
    public bool     IsHighlighted { get { return isHighlighted; } }
    public bool     IsMouseOver { get { return isMouseOver; } }

    // Start is called before the first frame update
    void Start()
    {
        diceCollider = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (enable3D)
        {
            float speedMod = 1.0f;

            if (isMouseOver)
            {
                Vector3 mousePos = Input.mousePosition;
                Vector3 localPos = Camera.main.ScreenToWorldPoint(mousePos);

                /*
                if (localPos.x < transform.position.x)
                    desiredFacing3D.y = rotationAngle;
                else if (localPos.x > transform.position.x)
                    desiredFacing3D.y = -rotationAngle;

                if (localPos.y < transform.position.y)
                    desiredFacing3D.x = -rotationAngle;
                else if (localPos.y > transform.position.y)
                    desiredFacing3D.x = rotationAngle;
                */
                desiredFacing3D.y = (transform.position.x - localPos.x) * rotationScale;
                desiredFacing3D.x = (localPos.y - transform.position.y) * rotationScale;
            }
            else
            {
                desiredFacing3D.x = 0.0f;
                desiredFacing3D.y = 0.0f;
                speedMod = 0.25f;
            }

            if (actualFacing3D.x != desiredFacing3D.x)
            {
                float dt = desiredFacing3D.x - actualFacing3D.x;
                float dir = dt / Mathf.Abs(dt);
                float spd = Time.deltaTime * speedMod * rotationSpeed;

                if (spd < (dir * dt))
                {
                    actualFacing3D.x += dir * spd;
                }
                else
                {
                    actualFacing3D.x = desiredFacing3D.x;
                }
            }

            if (actualFacing3D.y != desiredFacing3D.y)
            {
                float dt = desiredFacing3D.y - actualFacing3D.y;
                float dir = dt / Mathf.Abs(dt);
                float spd = Time.deltaTime * speedMod * rotationSpeed;

                if (spd < (dir * dt))
                {
                    actualFacing3D.y += dir * spd;
                }
                else
                {
                    actualFacing3D.y = desiredFacing3D.y;
                }
            }

            diceCube3D.transform.rotation = Quaternion.Euler(actualFacing3D);
        }
    }

    private void OnMouseEnter()
    {
        isMouseOver = true;
        SetHighlight(isHighlighted);
    }

    private void OnMouseExit()
    {
        isMouseOver = false;
        SetHighlight(isHighlighted);
    }

    public bool IsInside(Vector2 Pos)
    {
        return false;
    }

    public void InitDice(string Data, GobbleGame game)
    {
        if (!string.IsNullOrEmpty(Data))
        {
            string[] Tokens = Data.Split(',');
            diceFaces.Clear();

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

            diceHighlight.gameObject.SetActive(false);
            diceHighlight3D.gameObject.SetActive(false);

            RollDice(game);
        }
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
            diceFaceText3D.text = faceText;
            diceFaceScoreText3D.text = game.GetWordScore(faceText).ToString();

            GetComponent<Image>().enabled = !enable3D;
            diceFaceText.gameObject.SetActive(!enable3D);
            diceFaceText3D.gameObject.SetActive(enable3D);
        }
    }

    public void SetPositionIndex(int idx, int constraintCount)
    {
        posX = idx % constraintCount;
        posY = idx / constraintCount;
    }

    public bool IsAdjacent(dice obj)
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
        diceHighlight.gameObject.SetActive(!enable3D && (isHighlighted || isMouseOver));
        diceHighlight3D.gameObject.SetActive(enable3D && (isHighlighted || isMouseOver));
    }
}
