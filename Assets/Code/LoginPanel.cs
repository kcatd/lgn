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

        LoginState = LoginStateID.LoggedOut;
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

    void    OnLoggedOut()
    {
        userNameField.enabled = true;
        accountNameField.enabled = true;
        passwordField.enabled = true;
        signUpBtn.enabled = true;
        loginBtn.enabled = true;
    }

    void    OnLoggingIn()
    {
        userNameField.enabled = false;
        accountNameField.enabled = false;
        passwordField.enabled = false;
        signUpBtn.enabled = false;
        loginBtn.enabled = false;
    }

    void    OnLoggedIn()
    {
        game.LoginDone();
    }
}
