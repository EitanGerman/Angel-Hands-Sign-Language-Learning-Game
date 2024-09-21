using Assets.Logger;
using Assets.ModelCommunication;
using Assets.Scripts.CommonTypes;
using Assets.Scripts.GameManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.GameManager.WordRush
{
    public class WordRushSceneManager : MonoBehaviour
    {
        private WordRushState prevState;

        //initial state
        [SerializeField] WordRushState State = WordRushState.Main;

        //screens
        [SerializeField] GameObject MainScreen;
        [SerializeField] GameObject PlayScreen;
        [SerializeField] GameObject PauseScreen;
        [SerializeField] GameObject CompleteScreen;

        //Panels
        [SerializeField] GameObject HelpPanel;

        //Game Labels
        [SerializeField] TMP_Text PlayScoreLabel;
        [SerializeField] TMP_Text WordCountLabel;
        [SerializeField] TMP_Text CompleteScoreLabel;
        [SerializeField] TMP_Text ScorePausedLabel;
        [SerializeField] TMP_Text TimeLabel;
        [SerializeField] TMP_Text TimeTitleLabel;

        [SerializeField] TMP_Text WordToPresent;

        //Presets
        [SerializeField] int TimeToAnswer = 20;
        [SerializeField] int NumberOfWordsInGame = 10;
        [SerializeField] bool EndGameOnTimerRunout = false;

        //Stars
        [SerializeField] Image[] stars; // Array to store your 5 stars
        private int maxStars = 5;   // Total number of stars (usually 5)


        //private fields
        private int score = 0;
        private int maxScore = 0;
        private int TimeToFail;
        private bool CountDownRequired = true;
        private List<string> words = new List<string> { "Apple", "Banana", "Cherry", "Date", "Elderberry" };
        private List<string> modelWords;
        private bool newWordRequired = true;
        private bool newCroutineRequred;
        private int wordsAnswered = 0;
        private int pauseTimer = 0;

        private Coroutine countdownCoroutine;
        private Coroutine gameOverCoroutine;

        private GameSession gameSession;
        private readonly string GameModeName = "SignRush";

        // [SerializeField] List<GameObject> GameScreens;
        void Start()
        {
            FileLogger.Log("initializing sign rush game Mode");
            maxScore = NumberOfWordsInGame * 100;
            modelWords = GameManager.Instance.GetWordsSupportedByModel();
            FileLogger.Log(modelWords.Count.ToString() + " words found: " + string.Join(", ", modelWords));
            if (modelWords.Count > 0)
                words = modelWords;
            State = WordRushState.Main;
            UpdateActiveScreen();
        }

        void Update()
        {
            switch (State)
            {
                case WordRushState.CountDown:
                    if (CountDownRequired)
                    {
                        TimeTitleLabel.SetText("Game Starts In");
                        CountDownRequired = false;
                        TimeToFail = TimeToAnswer;
                        countdownCoroutine = StartCoroutine(UpdateTimer(5));
                    }
                    break;
                case WordRushState.Play:
                    if (CountDownRequired)
                    {
                        TimeTitleLabel.SetText("Remaining Time");
                        CountDownRequired = false;
                        TimeToFail = TimeToAnswer;
                        gameOverCoroutine = StartCoroutine(UpdateTimerGameOver());
                    }
                    else
                    {
                        while (newWordRequired)
                        {

                            int randomIndex = UnityEngine.Random.Range(0, words.Count);
                            string newWord = words[randomIndex];
                            if (newWord != WordToPresent.text && !newWord.Equals("None", StringComparison.OrdinalIgnoreCase))
                            {
                                WordCountLabel.SetText($"{wordsAnswered+1}/{NumberOfWordsInGame}");
                                WordToPresent.SetText(newWord);
                                newWordRequired = false;
                                TimeToFail = TimeToAnswer;
                                if (newCroutineRequred)
                                {
                                    gameOverCoroutine = StartCoroutine(UpdateTimerGameOver());
                                    newCroutineRequred= false;
                                }
                            }

                        }
                        if (GameManager.Instance.CurrentWord.Equals(WordToPresent.text, StringComparison.OrdinalIgnoreCase))
                        {
                            wordsAnswered += 1;
                            UpdateScore();
                            gameSession.AddWordResponse(WordToPresent.text, TimeToAnswer - TimeToFail, false);
                            if (wordsAnswered == NumberOfWordsInGame)
                                GameOver();
                            else
                                newWordRequired = true;
                        }

                        PlayScoreLabel.SetText(score.ToString());

                    }
                    break;
                case WordRushState.Pause:
                    ScorePausedLabel.SetText(score.ToString());
                    break;
            }
        }

        void StopTimers()
        {
            pauseTimer = TimeToFail;
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null; // Reset the reference
            }

            if (gameOverCoroutine != null)
            {
                StopCoroutine(gameOverCoroutine);
                gameOverCoroutine = null;
            }
        }

        private void UpdateScore()
        {
            if (TimeToFail > TimeToAnswer / 2)
                score += 100;
            else if (TimeToFail > TimeToAnswer / 3)
                score += 75;
            else if (TimeToFail > TimeToAnswer / 4)
                score += 50;
            else score += 25;
        }

        private void UpdateActiveScreen()
        {
            switch (State)
            {
                case WordRushState.Main:
                    MainScreen.SetActive(true);
                    PlayScreen.SetActive(false);
                    PauseScreen.SetActive(false);
                    CompleteScreen.SetActive(false);
                    break;
                case WordRushState.Play:
                    MainScreen.SetActive(false);
                    PlayScreen.SetActive(true);
                    PauseScreen.SetActive(false);
                    CompleteScreen.SetActive(false);
                    HelpPanel.SetActive(false);
                    break;
                case WordRushState.Pause:
                    MainScreen.SetActive(false);
                    PlayScreen.SetActive(false);
                    PauseScreen.SetActive(true);
                    CompleteScreen.SetActive(false);

                    break;
                case WordRushState.Complete:
                    MainScreen.SetActive(false);
                    PlayScreen.SetActive(false);
                    PauseScreen.SetActive(false);
                    CompleteScreen.SetActive(true);
                    break;
                case WordRushState.Help:
                    HelpPanel.SetActive(true);
                    break;
                case WordRushState.CountDown:
                    MainScreen.SetActive(false);
                    PlayScreen.SetActive(true);
                    PauseScreen.SetActive(false);
                    CompleteScreen.SetActive(false);
                    HelpPanel.SetActive(false);
                    CountDownRequired = true;
                    break;
            }
        }

        IEnumerator UpdateTimerGameOver()
        {
            if (pauseTimer > 0)
                TimeToFail = pauseTimer;
            while (TimeToFail >= 0)
            {
                TimeLabel.SetText(TimeToFail.ToString("0"));
                if (State == WordRushState.Play && !newWordRequired)
                {
                    TimeToFail--;
                }
                yield return new WaitForSeconds(1f);
            }
            if (EndGameOnTimerRunout)
            {
                GameOver();
            }
            else
            {
                if (wordsAnswered == NumberOfWordsInGame)
                    GameOver();
                else
                {
                    newWordRequired = true;
                    newCroutineRequred = true;
                }
            }
        }

        IEnumerator UpdateTimer(int time)
        {
            while (time > 0)
            {
                TimeLabel.SetText(time.ToString("0"));
                if (State == WordRushState.CountDown)
                {
                    time--;
                }
                yield return new WaitForSeconds(1f);
            }
            CountDownRequired = true;
            State = WordRushState.Play;
        }

        void GameOver()
        {
            StopTimers();
            gameSession.score = score;
            GameSession.SaveGameSessionAsync(gameSession);
            State = WordRushState.Complete;
            UpdateActiveScreen();
            CompleteScoreLabel.SetText(score.ToString());
            UpdateStars();
        }

        private void InitNewGame()
        {
            WordCountLabel.SetText($"{0}/{NumberOfWordsInGame}");
            gameSession = new GameSession(NumberOfWordsInGame, GameModeName);
            score = 0;
            maxScore = gameSession.maxScore;
            CountDownRequired = true;
            PlayScoreLabel.SetText("0");
            newWordRequired = true;
            WordToPresent.SetText("");
            wordsAnswered = 0;
            pauseTimer = 0;

        }

        // Call this function to update the stars based on score and max score
        public void UpdateStars()
        {
            // Calculate the percentage score
            float scorePercentage = (float)score / maxScore;

            // Determine how many full stars we have
            int fullStars = Mathf.FloorToInt(scorePercentage * maxStars);

            // Calculate the remaining fill for the partial star (if any)
            float partialFill = (scorePercentage * maxStars) - fullStars;

            // Start the filling process
            StartCoroutine(FillStars(fullStars, partialFill));
        }

        // Coroutine to gradually fill stars over 1 second
        private IEnumerator FillStars(int fullStars, float partialFill)
        {
            // Fill complete stars
            for (int i = 0; i < stars.Length; i++)
            {
                float targetFill = i < fullStars ? 1f : (i == fullStars ? partialFill : 0f);

                // Fill each star over time
                yield return StartCoroutine(FillStar(stars[i], targetFill));
            }
        }

        // Coroutine to gradually fill a single star over time
        private IEnumerator FillStar(Image star, float targetFill)
        {
            float fillDuration = 0.3f; // One second to fill
            float elapsedTime = 0f;
            float initialFill = star.fillAmount;

            while (elapsedTime < fillDuration)
            {
                elapsedTime += Time.deltaTime;
                star.fillAmount = Mathf.Lerp(initialFill, targetFill, elapsedTime / fillDuration);
                yield return null;
            }

            star.fillAmount = targetFill; // Ensure the final fill amount is set
        }


        private void ResetStars()
        {
            foreach (Image star in stars)
            {
                star.gameObject.SetActive(true); // Ensure stars are visible
                star.fillAmount = 0;             // Start with no fill
            }
        }
        //// Call this function to update the stars based on score and max score
        //public void UpdateStarsFull()
        //{
        //    // Calculate the percentage score
        //    float scorePercentage = (float)score / maxScore;

        //    // Determine how many stars should be active
        //    int starsToShow = Mathf.CeilToInt(scorePercentage * maxStars);

        //    // Show or hide stars based on starsToShow value
        //    for (int i = 0; i < stars.Length; i++)
        //    {
        //        if (i < starsToShow)
        //        {
        //            stars[i].SetActive(true);  // Show the star
        //        }
        //        else
        //        {
        //            stars[i].SetActive(false); // Hide the star
        //        }
        //    }
        //}

        //private void ResetStarsFull()
        //{
        //    foreach(var star in stars)
        //    {
        //        star.SetActive(false);
        //    }
        //}

        #region MainButtons
        public void OnStartButtonClicked()
        {
            gameSession = new GameSession(NumberOfWordsInGame,GameModeName);
            State = WordRushState.CountDown;
            UpdateActiveScreen();
        }

        public void OnExitButtonClicked()
        {
            SceneManager.LoadSceneAsync(0);
        }
        #endregion MainButtons

        #region PlayButtons
        public void OnPauseButtonClicked()
        {
            StopTimers();
            State = WordRushState.Pause;
            UpdateActiveScreen();
        }

        public void OnHelpButtonPressed()
        {
            prevState = State;
            State = WordRushState.Help;
            UpdateActiveScreen();
        }

        public void OnHelpOkButtonPressed()
        {
            State = prevState;
            UpdateActiveScreen();
            CountDownRequired = false;
        }
        #endregion PlayButtons

        #region PauseButtons
        public void OnResumeButtonCLikced()
        {
            State = WordRushState.CountDown;
            UpdateActiveScreen();
        }

        public void OnRetryButtonCLikced()
        {
            ResetStars();
            InitNewGame();
            State = WordRushState.CountDown;
            UpdateActiveScreen();
        }

        public void OnExitPauseClicked()
        {
            OnExitButtonClicked();
        }
        #endregion PauseButtons

        public void OnDebugNextButtonClicked()
        {
            UpdateScore();
            wordsAnswered += 1;
            if (wordsAnswered == NumberOfWordsInGame)
                GameOver();
            else
            {
                newWordRequired = true;
            }
        }

        public void OnSkipButtonClicked()
        {
            if (string.IsNullOrEmpty(WordToPresent.text))
                return;
            gameSession.AddWordResponse(WordToPresent.text, TimeToAnswer, true);
            wordsAnswered += 1;
            if (wordsAnswered == NumberOfWordsInGame)
                GameOver();
            else
            {
                newWordRequired = true;
            }
        }

        void OnApplicationQuit()
        {
            // Check if the application is running in the Unity Editor
            if (Application.isEditor)
            {
                //CameraManager.Instance.OnApplicationQuit();

                Debug.Log("Application stopped by pressing Play/Stop in the Unity Editor.");
                // Add your logic here that you want to happen when stopped in the Editor
                GameManager.Instance.OnApplicationQuit();
            }
        }
    }
}