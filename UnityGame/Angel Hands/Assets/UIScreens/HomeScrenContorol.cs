using Assets.Logger;
using Assets.Scripts.CommonTypes;
using Assets.Scripts.GameManager;
using Assets.Scripts.User;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class HomeScrenContorol : MonoBehaviour
{
    [SerializeField] VisualTreeAsset MainMenuScreen;
    [SerializeField] VisualTreeAsset StartNewGameScreen;
    [SerializeField] VisualTreeAsset SettingsScreen;
    [SerializeField] VisualTreeAsset DifficultySelectionScreen;
    Button buttonLogin;
    Label loginText;
    List<Word> currentWords=new List<Word>();
    public UIScreenManager screenManager;
    private void OnEnable()
    {
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        RegisterMainMenuButtons();
    }

    #region Buttons
    #region MainMenuButtons
    private void QuitToDesktop()
    {
        try
        {
            GameManager.Instance.epm.StopExternalProcess();
        }
        catch
        {

        }
        User.Instance.OnDestroy();
#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlaying)
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    private void OpenSettingMenu()
    {
        //GetComponent<UIDocument>().visualTreeAsset = SettingsScreen;
        //RegisterSettingsButtons();
    }

    private void StartNewGame()
    {
        GetComponent<UIDocument>().visualTreeAsset = StartNewGameScreen;
        RegisterGemeModeButtons();
    }

    private void OpenStatistics()
    {
        FileLogger.Log("Opening Statistics view");
        SceneManager.LoadSceneAsync("StatisticsView");
    }

    #region MainMenuCallBacks
    public void RegisterMainMenuButtons()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        Button buttonStart = root.Q<Button>("NewGame");
        Button buttonStatistics = root.Q<Button>("Statistics");
        Button buttonSettings = root.Q<Button>("Settings");
        Button buttonQuit = root.Q<Button>("Quit");
        buttonLogin = root.Q<Button>("Login");
        loginText = root.Q<Label>("userLabel");


        buttonStart.clicked += () => StartNewGame();
        buttonSettings.clicked += () => OpenSettingMenu();
        buttonQuit.clicked += () => QuitToDesktop();
        buttonStatistics.clicked += () => OpenStatistics();
        updateLoginButton();



    }
    private void updateLoginButton()
    {
        if (User.Instance.isUserLoggedIn)
        {
            buttonLogin.text = "Logout";
            buttonLogin.clicked += () => logout();
            loginText.text = User.Instance.UserName;
            loginText.visible = true;
            loginText.style.display = DisplayStyle.Flex;
        }
        else
        {
            buttonLogin.text = "Login";
            buttonLogin.clicked += () => login();
            loginText.text = "";
            loginText.style.display = DisplayStyle.None;

        }
    }
    #endregion MainMenuCallBacks

    #endregion MainMenuButtons



    #region GameModeButtons
    private void BackToMainMenu()
    {
        GetComponent<UIDocument>().visualTreeAsset = MainMenuScreen;
        RegisterMainMenuButtons();
    }


    private void LearningMode()
    {

        SetGameStyle(GameStyle.Learning);
        SceneManager.LoadSceneAsync(6);
        //GetComponent<UIDocument>().visualTreeAsset = DifficultySelectionScreen;
        //RegisterGameDifficutlyButtons();
    }

    private void ArcadeMode()
    {
        SetGameStyle(GameStyle.Arcade);
        GetComponent<UIDocument>().visualTreeAsset = DifficultySelectionScreen;
        RegisterGameDifficutlyButtons();
    }
    private void StoryMode()
    {
        SetGameStyle(GameStyle.Story);
        //GetComponent<UIDocument>().visualTreeAsset = DifficultySelectionScreen;
        //RegisterGameDifficutlyButtons();
        SetDifficultyLevel(DiffucltyLevel.None);
        SceneManager.LoadSceneAsync("StoryModeWorldSelection");
    }
    private void SetGameStyle(GameStyle style)
    {
        GameManager.Instance.SetGameStyle(style);
    }
    private void login()
    {
        SceneManager.LoadSceneAsync("Login");
    }
    private void logout()
    {
        User.Instance.logOut();
        updateLoginButton();
    }
    #region GameModeMenuCallbacks
    public void RegisterGemeModeButtons()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        Button buttonLearning = root.Q<Button>("Learning");
        Button buttonArcade = root.Q<Button>("Arcade");
        Button buttonStory = root.Q<Button>("Story");
        Button buttonBack = root.Q<Button>("Back");

        buttonLearning.clicked += () => LearningMode();
        buttonArcade.clicked += () => ArcadeMode();
        buttonStory.clicked += () => StoryMode();
        buttonBack.clicked += () => BackToMainMenu();
    }
    #endregion GameModeMenuCallbacks


    #endregion GameModeButtons


    #region GameDifficultyButtons


    private void HardMode()
    {
        SetDifficultyLevel(DiffucltyLevel.Hard);
        SceneManager.LoadSceneAsync("ForestMain");
    }

    private void MediumMode()
    {
        SetDifficultyLevel(DiffucltyLevel.Medium);
        SceneManager.LoadSceneAsync("TerrainTesting");
    }

    private void EasyMode()
    {
        SetDifficultyLevel(DiffucltyLevel.Easy);
        SceneManager.LoadSceneAsync(5);
    }


    private void SetDifficultyLevel(DiffucltyLevel diffuclty)
    {
        GameManager.Instance.SetDifficultyLevel(diffuclty);
    }

    private void BackToGameModeSelection()
    {
        GetComponent<UIDocument>().visualTreeAsset = StartNewGameScreen;
        RegisterGemeModeButtons();
    }


    #region GameDifficutlyCallbacks
    public void RegisterGameDifficutlyButtons()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        Button buttonEasy = root.Q<Button>("Easy");
        Button buttonMedium = root.Q<Button>("Medium");
        Button buttonHard = root.Q<Button>("Hard");
        Button buttonBack = root.Q<Button>("Back");

        buttonEasy.clicked += () => EasyMode();
        buttonMedium.clicked += () => MediumMode();
        buttonHard.clicked += () => HardMode();
        buttonBack.clicked += () => BackToGameModeSelection();
    }


    #endregion GameDifficutlyCallbacks
    #endregion GameDifficultyButtons

    #endregion Buttons
}
