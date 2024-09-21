using Assets.Logger;
using Assets.ModelCommunication;
using Assets.Scripts.CommonTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

namespace Assets.Scripts.GameManager

{
    public sealed class GameManager
    {
        #region PublicFields
        public static string pathToLearnedWordsJsonFile = System.IO.Path.Combine(Application.streamingAssetsPath, "LearnedWords.json");
        public static string pathToModelProvidedWrodJson = System.IO.Path.Combine(Application.streamingAssetsPath, "ModelCommunication\\ActiveModel\\ModelData.json");
        internal static string relativeModelPath = System.IO.Path.Combine(Application.streamingAssetsPath, "ModelCommunication\\ActiveModel\\");

        public string CurrentWord { get; private set; } = "None";

        public StateType State { get; private set; }
        public DiffucltyLevel DifficutltyLevel { get; private set; }
        public GameStyle GameStyle { get; private set; }
        public bool IsGameRunning { get; internal set; } = false;
        #endregion PublicFields

        #region privateFields
        public ExternalProcessManager epm = new ExternalProcessManager();
        private static GameManager instance = null;
        private List<Word> words = new List<Word>();//words supported by the model
        private List<ModelWord> modelWords = new List<ModelWord>();

        //ClientController clientController = new ClientController();
        #endregion privateFields

        #region ctor's
        private GameManager()
        {
            State = StateType.MENU;
            LoadWordsFromJsonFile();
            try
            {
                epm.StartExternalProcess();
            }
            catch
            {
                FileLogger.LogError("Failed to intialize model");
            }
            // initialize your game manager here. Do not reference to GameObjects here (i.e. GameObject.Find etc.)
            // because the game manager will be created before the objects
            //epm.StartExternalProcess();
        }

        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameManager();
                }

                return instance;
            }
        }


        #endregion ctor's

        #region Setters
        public void SetGameStyle(GameStyle style)
        {
            GameStyle = style;
            FileLogger.Log("set Game Style Mode: " + style.ToString());
        }

        public void SetDifficultyLevel(DiffucltyLevel diffuclty)
        {
            DifficutltyLevel = diffuclty;
            FileLogger.Log("set Game Difficulty Mode: " + diffuclty.ToString());
        }

        public void SetCurrentWord(string currentWord)
        {
            CurrentWord = currentWord;
            //FileLogger.Log("set Current Word to: " + CurrentWord??"Null");
        }

        #endregion Setters


        // Add your game mananger members here
        public void Pause(bool paused)
        {
        }


        public void OnApplicationQuit()
        {
            // Check if the application is running in the Unity Editor
            if (Application.isEditor)
            {
                //CameraManager.Instance.OnApplicationQuit();

                Debug.Log("Application stopped by pressing Play/Stop in the Unity Editor.");
                // Add your logic here that you want to happen when stopped in the Editor
                epm.StopExternalProcess();
            }
        }

        public void LoadWordsFromJsonFile()
        {
            try
            {
                //var fromModelWords = Word.LoadWordsFromModelWords(modelWords);
                modelWords = ModelWord.LoadFromFile(pathToModelProvidedWrodJson);
                words = Word.LoadWordsFromJson(pathToLearnedWordsJsonFile);
                words = Word.UpdateWordsFromModelWords(modelWords, words);
                Word.SaveWordsToJson(pathToLearnedWordsJsonFile,words);
            }
            catch(Exception ex)
            {
                FileLogger.LogError($"Failed to load the model words file. {ex} ");
            }
        }

        public List<string> GetWordsSupportedByModel()
        {
            return words.Select(w => w.word).ToList();
        }
        public List<Word> GetWords()
        {
            if (!words.Any())
                LoadWordsFromJsonFile();
            return words;
        }
        public void onGameModeChanged()
        {
            //switch (state)
            //{
            //    case StateType.PLACING:
            //        if (objectToPlace != null)
            //            PlaceObject();
            //        else
            //            Debug.Log("ERROR: No object to place.");
            //        break;
            //    case StateType.BUYING:
            //        break;
            //    case StateType.BALLWAIT:
            //        //StopBouncing();
            //        break;
            //    case StateType.STARTTURN:
            //        print("New Turn by: " + nowPlaying);
            //        IEvent startTurn = new StartTurn();
            //        EventManager.instance.QueueEvent(startTurn);
            //        AddEnergy();
            //        SetState(StateType.PLAYING, this);
            //        break;
            //    case StateType.PLAYING:
            //        //check for numActions > MaxActions
            //        break;
            //    case StateType.SHOOTING:
            //        break;
            //    case StateType.TURNOVER:
            //        ChangePlayer(); //Do this last (from server!)
            //        SetState(StateType.STARTTURN, this);
            //        print("Turn is over!");
            //        break;
            //    default:
            //        Debug.Log("ERROR: Unknown game state: " + state);
            //        break;
            //}
        }
    }
}

