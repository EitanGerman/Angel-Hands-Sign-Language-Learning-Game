using Assets.CameraFeed;
using Assets.Logger;
using Assets.Scripts.GameManager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class EscapeMenuControl : MonoBehaviour
{
    [SerializeField] VisualTreeAsset SettingsScreen;

    // Start is called before the first frame update
    private void OnEnable()
    {
        this.GetComponent<UIDocument>().rootVisualElement.visible = false;
        GameManager.Instance.IsGameRunning = true;
        RegisterEscapeMenuButtons();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance.IsGameRunning)
            {
                PauseGame();
                GameManager.Instance.IsGameRunning = false;
            }
            else
            {
                GameManager.Instance.IsGameRunning = true;
                ResumeGame();
            }
        }
    }

    private void PauseGame()
    {
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        this.GetComponent<UIDocument>().rootVisualElement.visible = true;
        ToggleAvaterMovement(false);
    }

    private void ToggleAvaterMovement(bool state)
    {
        GameObject targetObject = GameObject.Find("RPM Camera Rig");

        if (targetObject != null)
        {
            ReadyPlayerMe.Samples.QuickStart.CameraOrbit Co = targetObject.GetComponent<ReadyPlayerMe.Samples.QuickStart.CameraOrbit>();

            if (Co != null)
            {
                Co.enabled = state;

            }
            else
            {
                FileLogger.LogError("PlayerController script not found on the target GameObject.");
            }
        }
        else
        {
            FileLogger.LogError("GameObject with the specified name not found.");
        }
    }

    private void RegisterEscapeMenuButtons()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        Button buttonStart = root.Q<Button>("Resume");
        Button buttonSettings = root.Q<Button>("Settings");
        Button buttonQuitToMainMenu = root.Q<Button>("ExitToMainMenu");
        Button buttonQuitToDesktop = root.Q<Button>("ExitToDesktop");

        buttonStart.clicked += () => ResumeGame();
        buttonSettings.clicked += () => OpenSettingMenu();
        buttonQuitToMainMenu.clicked += () => QuitToMainMenu();
        buttonQuitToDesktop.clicked += () => QuitToDesktop();
    }

    private void QuitToDesktop()
    {
        try
        {
            GameManager.Instance.epm.StopExternalProcess();
        }
        catch
        {

        }
        //TODO add game save later and only then exit
        //TODO ask user if he really wants to quit
#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlaying)
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    private void QuitToMainMenu()
    {

        //TODO add game save later and only then exit
        CameraManager.Instance.OnApplicationQuit();
        SceneManager.LoadSceneAsync(0);
    }

    private void OpenSettingMenu()
    {
        //TODO implement setiings menu
        throw new NotImplementedException();
    }

    private void ResumeGame()
    {
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        this.GetComponent<UIDocument>().rootVisualElement.visible = false;
        ToggleAvaterMovement(true);
    }
}
