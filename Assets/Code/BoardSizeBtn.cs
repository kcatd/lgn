using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoardSizeBtn : MonoBehaviour
{
    [SerializeField] Vector2Int gridSize = new Vector2Int(4, 4);
    [SerializeField] Image      selectedBorderImg;
    Toggle                      tog;
    bool                        initButton = true;

    public int                  Width { get { return gridSize.x; } }
    public int                  Height { get { return gridSize.y; } }

    public bool                 IsToggled { get { return tog.isOn; } set { tog.isOn = value; } }

    // Start is called before the first frame update
    void Start()
    {
        InitButton();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void    OnToggle(bool isOn)
    {
        selectedBorderImg.gameObject.SetActive(isOn);
    }

    public void InitButton()
    {
        if (initButton)
        {
            initButton = false;

            tog = GetComponent<Toggle>();
            tog.onValueChanged.AddListener(OnToggle);

            OnToggle(tog.isOn);
        }
    }
}
