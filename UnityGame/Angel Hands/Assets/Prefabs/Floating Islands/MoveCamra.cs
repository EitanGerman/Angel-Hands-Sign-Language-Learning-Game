using Assets.Logger;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveCamra : MonoBehaviour
{
    public float rotationDuration = 2f; // Duration for the rotation to complete
    public float rotationAngle = 45f;   // Angle by which to rotate

    private Button myButton;

    void Start()
    {
        // Get the Button component attached to the same GameObject
        myButton = GetComponent<Button>();

        // Ensure the button component is found
        if (myButton != null)
        {
            myButton.onClick.AddListener(OnButtonClick);
        }
        else
        {
            FileLogger.LogError("Button component not found!");
        }
    }

    // Method to handle button click event
    void OnButtonClick()
    {
        // Add your custom logic here
        FileLogger.Log("Button clicked!");
        StartCoroutine(RotateCameraSmoothly());
        // RotateCamera();
    }

    void RotateCamera()
    {
        Vector3 currentRotation = Camera.main.transform.eulerAngles;

        // Increase the Y-axis rotation by 20 degrees
        currentRotation.y += rotationAngle;

        // Apply the new rotation to the camera
        Camera.main.transform.eulerAngles = currentRotation;
    }

    IEnumerator RotateCameraSmoothly()
    {
        float elapsedTime = 0f;
        Vector3 startingRotation = Camera.main.transform.eulerAngles;
        Vector3 targetRotation = startingRotation + new Vector3(0, rotationAngle, 0);

        while (elapsedTime < rotationDuration)
        {
            float t = elapsedTime / rotationDuration;
            float easedT = t * t * (3f - 2f * t); // SmoothStep interpolation
            Camera.main.transform.eulerAngles = Vector3.Lerp(startingRotation, targetRotation, easedT);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final rotation is exactly the target rotation
        Camera.main.transform.eulerAngles = targetRotation;
    }
}
