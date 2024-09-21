using Assets.Scripts.CommonTypes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Assets.Scripts.GameManager.Learnings;
using Assets.Scripts.GameManager;
using TMPro;
using System;
using System.Collections;
using Assets.Scripts.User;

public class LearningCaremaStage : MonoBehaviour
{
    #region manager
    public GameObject mainmenu;
    private LearningSceneManager _mainmenu;
    #endregion

    #region data
    // word for testing the scene 
    public Word word; // Populate this list with words
    public User user;
    bool islearned=false;
    #endregion

    #region GUI
    public GameObject statisticsPanel; // Panel that displays statistics
    public GameObject statisticTextPrefab; // Prefab for the text elements
    public Button backButton; // Panel that displays statistics
    public Button hintButton;//show video as a hint
    public TextMeshProUGUI wordName;
    public TextMeshProUGUI tries;
    public Button fakeLearning; // Panel that displays statistics
    public TextMeshProUGUI Title;
    public TextMeshProUGUI InstructionText;
    public Image fullStar;


    // learning panel with camera
    public GameObject camera;

    private int repeatedCorrectSigning = 0;

    private string lastCapturedSign = ""; // Variable to track the last captured sign
    private bool receivedDifferentSign = false; // Tracks if a different sign has been received
    #endregion

    private static readonly System.Random random = new System.Random(); // Create a single instance of Random

    private void Awake()
    {
        _mainmenu = mainmenu.GetComponent<LearningSceneManager>();
        user = User.Instance;

    }
    void Start()
    {
        
        backButton.onClick.AddListener(OnBackButtonClicked);
        
        hintButton.onClick.AddListener(OnHintButtonClicked);

        fakeLearning.onClick.AddListener(() => repeatedCorrectSigning += 1);//debug 
        //fakeLearning.gameObject.SetActive(false);//fakeLearning
    }
    private void OnEnable()
    {
        
        word = _mainmenu.selectedWord;
        wordName.text = word.word;
        repeatedCorrectSigning = 0;
        if (user.wordsLearned.Contains(word))
        {
            islearned = true;
            fullStar.gameObject.SetActive(true);
            Title.SetText("Practice the word");
            InstructionText.SetText("Practice the word or watch the instruction video here -->");
            Debug.Log("already learned " + word.word);
        }
        else
        {
            islearned = false;
            Title.SetText("Learn the word");
            InstructionText.SetText("Present the sign 3 times correctly to mark is as learned");
            fullStar.gameObject.SetActive(false);
            OnHintButtonClicked();
        }
    }


    private void Update()
    {

        string currentSign = GameManager.Instance.CurrentWord;

        // Check if the current sign is different from the last captured sign
        if (!currentSign.Equals(lastCapturedSign, StringComparison.OrdinalIgnoreCase))
        {
            receivedDifferentSign = true; // A different sign was received
        }

        // If the current sign matches the target word and a different sign was received before
        if (currentSign.Equals(word.word, StringComparison.OrdinalIgnoreCase) && receivedDifferentSign)
        {
            repeatedCorrectSigning += 1;
            lastCapturedSign = currentSign; // Update last captured sign
            receivedDifferentSign = false; // Reset the flag to wait for a different sign next time
            //StartCoroutine(SleepRoutine()); // Sleep for 3 seconds
        }

        tries.text = repeatedCorrectSigning.ToString();
        
        if (repeatedCorrectSigning >= 3 && !islearned)
        {
            islearned =  repeatedCorrectSigning >= 3;
            user.LearnWord(word);
            fullStar.gameObject.SetActive(true);
        }

    }


    public void OnBackButtonClicked()
    {
        // Hide statistics panel
        _mainmenu.state = LearningWordState.Main;
        _mainmenu.UpdateActiveScreen();

    }

    public void OnHintButtonClicked()
    {
        // Hide statistics panel
        _mainmenu.state = LearningWordState.Learn;
        _mainmenu.UpdateActiveScreen();

    }
}
