using Assets.Scripts.CommonTypes;
using Assets.Scripts.Statistics;
using Assets.Scripts.User.playfab;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
namespace Assets.Scripts.User
{

    #region data

    #endregion

    public class User
    {
        private static User instance;
        public static User Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new User();
                }
                return instance;
            }
        }
        UserStatistics userStatistics = new UserStatistics();

        public string UserName { get; set; } = "Guest";//user name

        public bool isUserLoggedIn { get; set; } = false;//enable guest mode

        public bool isUserAdmin { get; set; } = false;//privileges

        public List<Word> wordsLearned { get; set; } = new List<Word>(); //words learned by the user, contains the word and the history 

        private User()
        {
            loadInUserFromDB();
        }

        public bool LearnWord(Word newWord)
        {
            if (wordsLearned.Contains(newWord))
            {
                return false;
            }
            newWord.is_learned = true;
            wordsLearned.Add(newWord);
            userStatistics.learnNewWord();
            //saveWordLearned(); 
            //if (!isUserLoggedIn)
            //{
            //    Word.SaveWordsToJson(GameManager.GameManager.pathToLearnedWordsJsonFile, wordsLearned);
            //}
            return true;
        }

        public async void loadInUserFromDB()
        {
            if (isUserLoggedIn)
            {
                PlayFabWordManager.LoadWordsLearned();
                PlayFabWordManager.loadUserHistory();
                await Task.Delay(3000);
                if (PlayFabWordManager.wordsLearned != null)
                {
                    wordsLearned = PlayFabWordManager.wordsLearned;
                }
                else
                {
                    wordsLearned = new List<Word>();
                }
            }
            else
            {
                var gameManagerWords = GameManager.GameManager.Instance.GetWords().Where(w => w.is_learned).ToList();
                if (gameManagerWords.Count > 0)
                    wordsLearned = gameManagerWords;
            }
        }
        private IEnumerator SleepCoroutine()
        {
            Debug.Log("Sleeping for 3 seconds...");

            // Wait for 3 seconds
            yield return new WaitForSeconds(3f);

            Debug.Log("Woke up after 3 seconds!");
        }
        //public void saveWordLearned()
        //{
        //    if (isUserLoggedIn)
        //    {
        //        PlayFabWordManager.wordsLearned = wordsLearned;
        //        PlayFabWordManager.SaveWordsLearned();
        //    }
        //    else
        //    {
        //        Word.SaveWordsToJson(GameManager.GameManager.pathToLearnedWordsJsonFile, wordsLearned);
        //    }
        //}
        public void logIn(string username)
        {
            Word.SaveWordsToJson(GameManager.GameManager.pathToLearnedWordsJsonFile, wordsLearned);
            isUserLoggedIn = true;
            UserName = username;
            loadInUserFromDB();

        }
        public async void logOut()
        {
            PlayFabWordManager.wordsLearned = wordsLearned;
            PlayFabWordManager.SaveWordsLearned();
            isUserLoggedIn = false;
            UserName = "Guest";
            wordsLearned = new List<Word>();
            PlayFabWordManager.SaveHistory();
            await Task.Delay(3000);
            loginPanelManager.Logout();
            loadInUserFromDB();
        }
        public void OnDestroy()
        {
            if (isUserLoggedIn)
            {
                logOut();
            }
            else
            {
                Word.SaveWordsToJson(GameManager.GameManager.pathToLearnedWordsJsonFile, wordsLearned);//OnDestroy not always working properly
            }

        }


    }
}