using Assets.Logger;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FloatingIsland : MonoBehaviour
{
    public float amplitude = 0.5f;
    public float frequency = 1f;
    public float roatate = 0.5f;
    Vector3 startPos;
    public string TargetSceneNmae = "";
    public bool IsSelected { private get; set; }
    public bool IsInCameraFocus { private get; set; } = true;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        if (IsInCameraFocus)
        {
            float yOffset = Mathf.Sin(Time.time * frequency) * amplitude;
            transform.position = startPos + new Vector3(0, yOffset, 0);
            transform.RotateAroundLocal(new Vector3(0, 1, 0), roatate * Time.deltaTime);     
        }
    }

    private void OnMouseDown()
    {
        FileLogger.Log("GameObject clicked!");
        if(!string.IsNullOrEmpty(TargetSceneNmae))
        {
            try
            {
            FileLogger.Log("switching to scene: " + TargetSceneNmae);
            SceneManager.LoadSceneAsync(TargetSceneNmae);
            return;

            }
            catch
            {
                FileLogger.LogError("A Targetscene Does not exist!!!");
            }
        }
        FileLogger.LogWarning("A Target scene was not set, staying at the same scene");
    }
}
