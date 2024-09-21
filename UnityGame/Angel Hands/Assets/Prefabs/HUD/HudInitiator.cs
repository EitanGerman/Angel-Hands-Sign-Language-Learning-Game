using Assets.Scripts.GameManager;
using TMPro;
using UnityEngine;
using Assets.Logger;

public class HudInitiator : MonoBehaviour
{
    [SerializeField] private TMP_Text Difficulty;
    [SerializeField] private TMP_Text GameStyle;
    [SerializeField] private TMP_Text CurrentWord;

    private string GetCurrentWord()
    {
        var currentWord = GameManager.Instance.CurrentWord.ToString();
        if (CurrentWord != null && currentWord != null)
        {
            return currentWord;
        }
        return "";
    }

    private string GetGameStyle()
    {
        if(GameStyle != null)
        {
            return GameStyle.text;
        }
        return GameManager.Instance.GameStyle.ToString();
    }

    private string GetGameDifficulty()
    {
        if(Difficulty != null)
        {
            return Difficulty.text;
        }
        return GameManager.Instance.DifficutltyLevel.ToString();
    }

    void Start()
    {
        // Initialize the string property
        GameStyle.text = "GameStyle: " + GetGameStyle();
        Difficulty.text = "Difficulty: " + GetGameDifficulty();
        CurrentWord.text = "Current Word: ";
        FileLogger.Log(string.Format("Starting Game with prameters: {0} | {1}", GameStyle.text, Difficulty.text));
    }

    private void Update()
    {
        CurrentWord.text = "Current Word: " + GetCurrentWord();
    }
}
