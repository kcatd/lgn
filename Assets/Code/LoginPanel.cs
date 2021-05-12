using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum LoginStateID
{
    LoggedOut,
    LoggingIn,
    LoggedIn,
}

public class LoginPanel : MonoBehaviour
{
    [Header("Game")]
    [SerializeField] GobbleGame game;

    [Header("UI")]
    [SerializeField] TMP_InputField userNameField;
    [SerializeField] TMP_InputField accountNameField;
    [SerializeField] TMP_InputField passwordField;
    [SerializeField] Button         signUpBtn;
    [SerializeField] Button         loginBtn;
    [SerializeField] Button         offlineBtn;
    [SerializeField] TextMeshProUGUI    statusText;

    LoginStateID                    loginState;

    public LoginStateID LoginState
    {
        get
        {
            return loginState;
        }
        set
        {
            if (value != loginState)
            {
                loginState = value;

                switch (loginState)
                {
                    case LoginStateID.LoggedOut:    OnLoggedOut(); break;
                    case LoginStateID.LoggingIn:    OnLoggingIn(); break;
                    case LoginStateID.LoggedIn:     OnLoggedIn(); break;
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        signUpBtn.onClick.AddListener(OnSignUpBtn);
        loginBtn.onClick.AddListener(OnLoginBtn);
        offlineBtn.onClick.AddListener(OnOfflineBtn);

        signUpBtn.interactable = true;
        loginBtn.interactable = true;
        offlineBtn.interactable = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        userNameField.text = "Bob";
        accountNameField.text = "bob@boomzap.com";
        passwordField.text = "1234";

        userNameField.interactable = true;
        accountNameField.interactable = true;
        passwordField.interactable = true;
        signUpBtn.interactable = true;
        loginBtn.interactable = true;
        offlineBtn.interactable = true;

        LoginState = LoginStateID.LoggedOut;
        statusText.text = "Disconnected";
    }

    private void OnDisable()
    {
    }

    void    OnSignUpBtn()
    {
        game.StartLogin(userNameField.text, accountNameField.text, passwordField.text, true);
    }

    void    OnLoginBtn()
    {
        game.StartLogin(userNameField.text, accountNameField.text, passwordField.text, false);
    }

    void    OnOfflineBtn()
    {
        game.StartOffline();
    }

    void    OnLoggedOut()
    {
        statusText.text = "Disconnected";
        userNameField.interactable = true;
        accountNameField.interactable = true;
        passwordField.interactable = true;
        signUpBtn.interactable = true;
        loginBtn.interactable = true;
        offlineBtn.interactable = true;
    }

    void    OnLoggingIn()
    {
        statusText.text = "Connecting...";
        userNameField.interactable = false;
        accountNameField.interactable = false;
        passwordField.interactable = false;
        signUpBtn.interactable = false;
        loginBtn.interactable = false;
        offlineBtn.interactable = false;
    }

    void    OnLoggedIn()
    {
        statusText.text = "Connected!";
        game.LoginDone();
    }
}
