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

    TMP_InputField[]                tabEntrySet;

    LoginStateID                    loginState;

    public LoginStateID             LoginState { get { return loginState; } }

    // Start is called before the first frame update
    void Start()
    {
        signUpBtn.onClick.AddListener(OnSignUpBtn);
        loginBtn.onClick.AddListener(OnLoginBtn);
        offlineBtn.onClick.AddListener(OnOfflineBtn);

        signUpBtn.interactable = true;
        loginBtn.interactable = true;
        offlineBtn.interactable = true;

        tabEntrySet = new TMP_InputField[3];
        tabEntrySet[0] = userNameField;
        tabEntrySet[1] = accountNameField;
        tabEntrySet[2] = passwordField;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TMP_InputField current = GetCurrentInputField();
            if (null != current)
            {
                TMP_InputField next = GetNextInputField(current);
                if (null != next)
                {
                    next.Select();
                    next.ActivateInputField();
                }
            }
        }
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        userNameField.text = "Bob";
        accountNameField.text = "bob@boomzap.com";
        passwordField.text = "1234";
#endif //UNITY_EDITOR

        userNameField.interactable = true;
        accountNameField.interactable = true;
        passwordField.interactable = true;
        signUpBtn.interactable = true;
        loginBtn.interactable = true;
        offlineBtn.interactable = true;

        SetLoginState(LoginStateID.LoggedOut);
    }

    private void OnDisable()
    {
    }

    public void SetLoginState(LoginStateID state, string messageStr = "")
    {
        if (state != loginState)
        {
            loginState = state;

            switch (loginState)
            {
                case LoginStateID.LoggedOut: OnLoggedOut(messageStr); break;
                case LoginStateID.LoggingIn: OnLoggingIn(messageStr); break;
                case LoginStateID.LoggedIn: OnLoggedIn(messageStr); break;
            }
        }
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

    void    OnLoggedOut(string messageStr = "")
    {
        if (string.IsNullOrEmpty(messageStr))
            statusText.text = "Disconnected";
        else
            statusText.text = messageStr;

        userNameField.interactable = true;
        accountNameField.interactable = true;
        passwordField.interactable = true;
        signUpBtn.interactable = true;
        loginBtn.interactable = true;
        offlineBtn.interactable = true;
    }

    void    OnLoggingIn(string messageStr = "")
    {
        if (string.IsNullOrEmpty(messageStr))
            statusText.text = "Connecting...";
        else
            statusText.text = messageStr;

        userNameField.interactable = false;
        accountNameField.interactable = false;
        passwordField.interactable = false;
        signUpBtn.interactable = false;
        loginBtn.interactable = false;
        offlineBtn.interactable = false;
    }

    void    OnLoggedIn(string messageStr = "")
    {
        if (string.IsNullOrEmpty(messageStr))
            statusText.text = "Connected!";
        else
            statusText.text = messageStr;

        game.LoginDone();
    }

    TMP_InputField  GetCurrentInputField()
    {
        foreach (var obj in tabEntrySet)
        {
            if (obj.isFocused)
            {
                return obj;
            }
        }
        return null;
    }

    TMP_InputField  GetNextInputField(TMP_InputField current)
    {
        int inputCount = tabEntrySet.Length;
        for (int i = 0; i < inputCount; ++i)
        {
            if (current == tabEntrySet[i])
            {
                for (int j = 1; j < inputCount; ++j)
                {
                    TMP_InputField next = tabEntrySet[(i + j) % inputCount];
                    if (next.IsInteractable())
                        return next;
                }
            }
        }
        return null;
    }
}
