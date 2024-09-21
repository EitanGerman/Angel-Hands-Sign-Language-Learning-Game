using Assets.CameraFeed;
using Assets.Logger;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitToMainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            FileLogger.Log("Escape key was pressed");
        }


        if (Input.GetKeyUp(KeyCode.Escape))
        {
            FileLogger.Log("Escape key was released");
            CameraManager.Instance.OnApplicationQuit();
            SceneManager.LoadSceneAsync(0);
            FileLogger.Log("Exiting to Main Menu");
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            FileLogger.Log("Escape key is being pressed");
        }
    }
}
