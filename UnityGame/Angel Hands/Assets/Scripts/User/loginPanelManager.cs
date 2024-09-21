using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using Assets.Scripts.User;
using UnityEngine.SceneManagement;



public class loginPanelManager : MonoBehaviour
{
    #region ui elements
    [SerializeField] TMP_InputField passwordField;
    [SerializeField] TMP_InputField usernameField;
    [SerializeField] Button loginButton;
    [SerializeField] Button registerButton;
    [SerializeField] Button playAsGuestButton;
    [SerializeField] TextMeshProUGUI errorText;
    #endregion

    #region data
    string password;
    string username;
    User user;
    #endregion

    #region unity calls
    // Start is called before the first frame update
    async void Start()
    {
        //await UnityServices.InitializeAsync();
        //await SignInAnonymouslyAsync();
        registerButton.onClick.AddListener(() => { RegisterWithUsername(); });
        loginButton.onClick.AddListener(() => { LoginWithUsername(); });
        playAsGuestButton.onClick.AddListener(() => { SceneManager.LoadSceneAsync(0); });
        user = User.Instance;

    }

    // Update is called once per frame
    void Update()
    {
       
    }
    #endregion

#   region data validation
    private bool isDataValid()
    {
        password = passwordField.text;
        username = usernameField.text;
        if (username == "")
        {
            errorText.text = "missing User name";
            Debug.LogError("username field is null");
            return false;
        }
        if (password == "")
        {
            errorText.text = "missing password";
            Debug.LogError("Password field is null");
            return false;
        }
        if (username.Length < 3 || username.Length > 26)
        {
            errorText.text = "username can be from 3 to 26 chars";
            Debug.LogError("username length out of bound");
            return false;
        }
        if (password.Length < 6 || password.Length > 100)
        {
            errorText.text = "password can be from 6 to 100 chars";
            Debug.LogError("password length out of bound");
            return false;
        }
        return true;
    }
    #endregion

    #region login
    // Call this method to initiate login
    // Call this method to initiate login using the username
    public void LoginWithUsername()
    {
        if (!isDataValid())
        {
            return;
        }
        var request = new LoginWithPlayFabRequest
        {
            Username = username,
            Password = password
        };
        PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnLoginFailure);
    }


    private void OnLoginSuccess(LoginResult result)
    {
        user.logIn(username);

        errorText.text = "Login successful!";
        Debug.Log("Logged in successfuly!");

        SceneManager.LoadSceneAsync(0);
        //SceneManager.LoadScene("Home Screen Forest");
    }

    private void OnLoginFailure(PlayFabError error)
    {
        errorText.text = "Login failed";
        Debug.LogError("Login failed: " + error.GenerateErrorReport());
    
    }
    #endregion

    #region register
    // Call this function when the user submits their registration info
    public void RegisterWithUsername()
    {
        if (!isDataValid())
            {
            return;
        }
        var request = new RegisterPlayFabUserRequest
        {
            Username = username,
            Password = password,
            RequireBothUsernameAndEmail = false // We don't need the email
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnRegisterFailure);
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        errorText.text = "Registered  successfuly!";
        Debug.Log("Registered successfuly!");
        // Handle successful registration, such as loading a new scene or displaying a success message
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        switch (error.Error)
        {
            case PlayFabErrorCode.UsernameNotAvailable:
                errorText.text = "Username is already taken. Please choose another one.";
                break;
            case PlayFabErrorCode.InvalidPassword:
                errorText.text = "The password is too weak. Please choose a stronger password.";
                break;
            // Add more cases for other error types you want to handle
            default:
                errorText.text = "Registration failed. Please try again.";
                break;
        }
        Debug.LogError("Registration failed" + error.GenerateErrorReport());
        // Handle registration failure, such as displaying an error message to the user
    }
    #endregion

    #region logout
    public static void Logout()
    {
        // Invalidate the current session by clearing PlayFab's session ticket
        PlayFabClientAPI.ForgetAllCredentials();
        Debug.Log("Player logged out. Session cleared.");
        
    }
    #endregion

}
