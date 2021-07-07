using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuPanel : MonoBehaviour
{
    [SerializeField] Toggle     soundFXOff;
    [SerializeField] Toggle     soundFXOn;
    [SerializeField] Toggle     musicOff;
    [SerializeField] Toggle     musicOn;
    [SerializeField] Button     endGameBtn;
    [SerializeField] Button     closeMenuBtn;

    GobbleGame                  game = null;
    bool                        localControlFlag = false;

    // Start is called before the first frame update
    void Start()
    {
        soundFXOff.onValueChanged.AddListener(OnSoundFXOff);
        soundFXOn.onValueChanged.AddListener(OnSoundFXOn);
        musicOff.onValueChanged.AddListener(OnMusicOff);
        musicOn.onValueChanged.AddListener(OnMusicOn);
        endGameBtn.onClick.AddListener(OnEndGame);
        closeMenuBtn.onClick.AddListener(OnCloseMenu);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartMenu(GobbleGame parent, bool allowLocalCtrl)
    {
        game = parent;
        localControlFlag = allowLocalCtrl;

        // TODO: animate me
        gameObject.SetActive(true);

        RefreshMenuUI();
    }

    void    RefreshMenuUI()
    {
        endGameBtn.gameObject.SetActive(localControlFlag);
    }

    void    OnSoundFXOff(bool isOn)
    {
        if (isOn)
        {
        }
    }
    void    OnSoundFXOn(bool isOn)
    {
        if (isOn)
        {
        }
    }

    void    OnMusicOff(bool isOn)
    {
        if (isOn)
        {
        }
    }
    void    OnMusicOn(bool isOn)
    {
        if (isOn)
        {
        }
    }

    void    OnEndGame()
    {
        if (localControlFlag)
        {
            game.ForceEndGame();
            OnCloseMenu();
        }
    }

    void    OnCloseMenu()
    {
        // TODO: animate me
        gameObject.SetActive(false);
    }
}
