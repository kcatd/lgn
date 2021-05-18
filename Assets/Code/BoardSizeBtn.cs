using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoardSizeBtn : MonoBehaviour
{
    [SerializeField] Vector2Int gridSize = new Vector2Int(4, 4);
    Toggle                      tog;

    public int                  Width { get { return gridSize.x; } }
    public int                  Height { get { return gridSize.y; } }

    public bool                 IsToggled { get { return tog.isOn; } set { tog.isOn = value; } }

    // Start is called before the first frame update
    void Start()
    {
        tog = GetComponent<Toggle>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
