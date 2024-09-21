using Assets.Scripts.CommonTypes;
using Assets.Scripts.GameManager;
using Assets.Scripts.GameManager.Learnings;
using Assets.Scripts.User;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

public class MainMenu : MonoBehaviour
{
    #region data
    // word for testing the scene 
    public List<Word> wordList = new List<Word>(); // Populate this list with words
    #endregion

    #region GUI
    // main panel - pick a word
    [SerializeField] GameObject wordButtonPrefab; // Prefab for the word buttons
    [SerializeField] Transform wordButtonParent; // Parent transform for the buttons
    [SerializeField] GameObject wordPanel; // Panel that displays word information
    [SerializeField] Button back; // Button to go back to the main menu

    [SerializeField] TextMeshProUGUI amountOfWordsLearned; // Button to go back to the main menu
    [SerializeField] TextMeshProUGUI amountHowWordsToLearn; // Button to go back to the main menu

    [SerializeField] Sprite fullStar, emptyStar;
    Dictionary<Word, Button> wordButtons = new Dictionary<Word, Button>();
    // main stats
    public Text wordHeader;
    #endregion

    #region
    public GameObject mainmenu;

    private LearningSceneManager _mainmenu;
    #endregion

    void Start()
    {
        PopulateWordButtons();
        amountOfWordsLearned.text = User.Instance.wordsLearned.Count.ToString();
        amountHowWordsToLearn.text = (wordList.Count - User.Instance.wordsLearned.Count).ToString();
        showWords();
        back.onClick.AddListener(goToHomeScene);
    }
    void Awake()
    {
        wordList = GameManager.Instance.GetWords();

        _mainmenu = mainmenu.GetComponent<LearningSceneManager>();

    }
    void OnEnable()
    {
        amountOfWordsLearned.text = User.Instance.wordsLearned.Count.ToString();
        amountHowWordsToLearn.text = (wordList.Count - User.Instance.wordsLearned.Count).ToString();
        updateStars();
    }

    private void goToHomeScene()
    {
        //SceneManager.LoadSceneAsync("Home Screen Forest");
        SceneManager.LoadSceneAsync(0);
    }

    private void showWords()
    {
        wordButtonParent.gameObject.SetActive(true);
        wordButtonParent.gameObject.SetActive(true);
    }

    void PopulateWordButtons()
    {
        Button newButton;
        foreach (Word word in wordList)
        {
            newButton = Instantiate(wordButtonPrefab, wordButtonParent).GetComponent<Button>();
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = word.word;
            newButton.onClick.AddListener(() => OnWordButtonClicked(word));
            //Image buttonImage = newButton.GetComponentInChildren<Image>();
            Image buttonChildImage = newButton.transform.Find("star").GetComponent<Image>();
            AddHoverEffect(newButton);
            if (User.Instance.wordsLearned.Contains(word))
            {
                buttonChildImage.sprite = fullStar;
            }
            else
            {
                buttonChildImage.sprite = emptyStar;
            }
            wordButtons.Add(word, newButton);
        }
    }

    void OnWordButtonClicked(Word selectedWord)
    {
        _mainmenu.state = LearningWordState.Sign;
        _mainmenu.UpdateActiveScreen(selectedWord);
    }
    private void updateStars()
    {
        Debug.Log("Updating stars");
        foreach (KeyValuePair<Word, Button> entry in wordButtons)
        {
            Word word = entry.Key;  // The word
            Button button = entry.Value;  // The learned status
            Image buttonChildImage = button.transform.Find("star").GetComponent<Image>();
            if (User.Instance.wordsLearned.Contains(word))
            {
                buttonChildImage.sprite = fullStar;
            }
            else
            {
                buttonChildImage.sprite = emptyStar;
            }
        
        }
    }

    private void AddHoverEffect(Button button)
    {
        // Add an EventTrigger component to the button
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

        // Create and configure the PointerEnter entry
        EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry();
        pointerEnterEntry.eventID = EventTriggerType.PointerEnter;
        pointerEnterEntry.callback.AddListener((eventData) => { buttonText.fontSize = 35; });
        trigger.triggers.Add(pointerEnterEntry);

        // Create and configure the PointerExit entry
        EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry();
        pointerExitEntry.eventID = EventTriggerType.PointerExit;
        pointerExitEntry.callback.AddListener((eventData) => { buttonText.fontSize = 30; });
        trigger.triggers.Add(pointerExitEntry);
    }


}
