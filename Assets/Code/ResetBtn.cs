using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResetBtn : MonoBehaviour
{
    [SerializeField] GobbleGame game;

    Button      resetBtn;

    // Start is called before the first frame update
    void Start()
    {
        if (null == resetBtn)
        {
            resetBtn = GetComponent<Button>();

            if (null != resetBtn)
            {
                resetBtn.onClick.AddListener(OnClickReset);
                resetBtn.interactable = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (resetBtn.IsInteractable())
        {
            if (!game.IsOfflineMode && !game.IsHost)
                resetBtn.interactable = false;
        }
        else
        {
            if (game.IsOfflineMode || game.IsHost)
                resetBtn.interactable = true;
        }
    }

    void OnClickReset()
    {
        //game.ForceEndGame();
        game.StartMenu();
    }
}
