using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTimeBtn : MonoBehaviour
{
    [SerializeField] int    timeInSeconds = 60;
    Toggle                  tog;

    public int              GameTime { get { return timeInSeconds; } }

    public bool             IsToggled { get { return tog.isOn; } set { tog.isOn = value; } }

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
