using Assets.Scripts.GameManager;
using Assets.Scripts.GameManager.Learnings;
using System;
using System.IO;
using Assets.Logger;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using Button = UnityEngine.UI.Button;

public class LearningWithCamera : MonoBehaviour
{
    #region manager
    public GameObject mainmenu;
    private LearningSceneManager _mainmenu;
    #endregion
    #region GUI
    public VideoPlayer videoPlayer;
    public GameObject screen;
    public Button playButton, stopButton;
    public Button backButton;
    public Button NextVideoButton, PreviousVideoButton;
    public TextMeshProUGUI wordHeader;
    private string[] videoPaths;
    private string selectedVideoPath;
    private int currentVideo = 0;

    #endregion
    void Start()
    {
        videoPaths = getVideoes();

        playButton.onClick.AddListener(() => OnPlayButtonClicked());
        stopButton.onClick.AddListener(() => videoPlayer.Stop());
        backButton.onClick.AddListener(OnBackButtonClicked);
        NextVideoButton.onClick.AddListener(() => OnSeekVideoButtonClicked(1));
        PreviousVideoButton.onClick.AddListener(() => OnSeekVideoButtonClicked(-1));
        videoPlayer.playOnAwake = false;
    }

    //cyclic seeking of videos in both directions
    private void OnSeekVideoButtonClicked(int direction)
    {
        currentVideo += direction;
        currentVideo %= videoPaths.Length;
        currentVideo = Math.Abs(currentVideo);
        OnPlayButtonClicked();
    }

    private void OnPlayButtonClicked()
    {
        try
        {
            videoPlayer.url = getVideo();
            videoPlayer.Prepare();
            videoPlayer.Play();
        }
        catch (Exception ex)
        {
            FileLogger.LogWarning("Cant load video because no path was provided");
        }
    }

    private void Update()
    {
        if (videoPlayer.isPlaying)
        {
            playButton.interactable = false;
            stopButton.interactable = true;
        }
        else
        {
            playButton.interactable = true;
            stopButton.interactable = false;
        }
    }
    void OnEnable()
    {
        if (_mainmenu == null)
            _mainmenu = mainmenu.GetComponent<LearningSceneManager>();
        wordHeader.text = "Learning Word : " + _mainmenu.selectedWord.word;
        videoPaths = getVideoes();
        currentVideo = 0;
        videoPlayer.Stop();
        videoPlayer.targetTexture.Release();
        videoPlayer.url = getVideo();
        videoPlayer.Prepare();
        OnPlayButtonClicked();
    }

    private string[] getVideoes()
    {
        try
        {
            string value = Path.Combine(GameManager.relativeModelPath, _mainmenu.selectedWord.videoDirectoryPath);
            return Directory.GetFiles(value,"*.mp4");
        }
        catch (Exception ex)
        {
            return new string[0];
        }
    }

    private string getVideo()
    {
        try
        {
            selectedVideoPath = videoPaths[currentVideo] ?? "";
            return selectedVideoPath;
        }
        catch {
            return "";
        }
    }
    public void OnBackButtonClicked()
    {
        _mainmenu.state = LearningWordState.Sign;
        _mainmenu.UpdateActiveScreen(_mainmenu.selectedWord);
        selectedVideoPath = "";
    }
}
