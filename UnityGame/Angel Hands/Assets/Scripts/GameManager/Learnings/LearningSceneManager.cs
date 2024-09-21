using Assets.Scripts.CommonTypes;
using UnityEngine;

namespace Assets.Scripts.GameManager.Learnings
{
    public class LearningSceneManager : MonoBehaviour
    {
        //panels
        public GameObject mainPanel;//pick words to learn
        public GameObject videoPanel;//show video as a hint
        public GameObject cameraPanel;//do the word sign
        public LearningWordState state = LearningWordState.Main;

        public Word selectedWord { set; get; }

        private static LearningSceneManager instance = null;

        public static LearningSceneManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LearningSceneManager();
                }
                return instance;
            }
        }
        public LearningSceneManager()
        {
        }

        void Start()
        {
            UpdateActiveScreen();
        }
        void Update()
        {
            
        }
        public void UpdateActiveScreen(Word selectedWord=null)
        {
            if (selectedWord != null)
            {
                this.selectedWord = selectedWord;
            }
            switch (state)
            {
                case LearningWordState.Main:
                    videoPanel.SetActive(false);
                    cameraPanel.SetActive(false);
                    mainPanel.SetActive(true);
                    break;
                case LearningWordState.Sign:
                    mainPanel.SetActive(false);
                    videoPanel.SetActive(false);
                    cameraPanel.SetActive(true);
                    break;
                case LearningWordState.Learn:
                    //mainPanel.SetActive(false);
                    //cameraPanel.SetActive(false);
                    videoPanel.SetActive(true);
                    break;
            }

        }

    }
}
