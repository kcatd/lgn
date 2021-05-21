using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerTeamBtn : MonoBehaviour
{
    [SerializeField] int    teamID;
    Toggle                  tog;

    public int              TeamID { get { return teamID; } }
    public bool             IsToggled { get { return tog.isOn; } set { tog.SetIsOnWithoutNotify(value); } }

    // Start is called before the first frame update
    void Start()
    {
        tog = GetComponent<Toggle>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitState()
    {
        if (null == tog)
            tog = GetComponent<Toggle>();
    }
}
