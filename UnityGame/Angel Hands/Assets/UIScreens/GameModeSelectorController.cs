using UnityEngine;
using UnityEngine.UIElements;

public class GameModeSelectorController : MonoBehaviour
{
    [SerializeField] UIScreenManager screenManager;

    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        Button buttonLearning = root.Q<Button>("Learning");
        Button buttonArcade = root.Q<Button>("Arcade");
        Button buttonStory = root.Q<Button>("Story");
        Button buttonBack = root.Q<Button>("Back");

        buttonBack.clicked += () => BackToMainMenu();
        //buttonSettings.clicked += () => OpenSettingMenu();
        //buttonQuit.clicked += () => QuitToDesktop();
    }

    private void BackToMainMenu()
    {
        if (screenManager == null)
            screenManager = new UIScreenManager();
        //screenManager.SwitchToMainScreen();
        screenManager.ToggleUI();

    }

}
