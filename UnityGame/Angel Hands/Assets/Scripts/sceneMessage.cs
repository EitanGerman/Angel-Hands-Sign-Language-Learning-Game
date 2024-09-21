using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Assets.Scripts.GameManager;

public class sceneMessage : MonoBehaviour
{
    private TextMeshProUGUI levelText;
    public float displayDuration = 5.0f;  // Duration to display the message
    public string Message = null;  // Message to display
    public float fadeDuration = 1.0f;  // Duration to fade out the message

    void Awake()
    {
        levelText = GetComponent<TextMeshProUGUI>();
        if (levelText == null)
        {
            Debug.LogError("No TextMeshProUGUI  component found on this GameObject.");
        }
    }

    void Start()
    {
        
        if (Message == null || Message == "") {
            Message = GameManager.Instance.GameStyle.ToString();
        }
        StartCoroutine(DisplayLevelMessage(Message));
    }

    IEnumerator DisplayLevelMessage(string message)
    {
        if (levelText != null)
        {
            levelText.text = message;
            levelText.gameObject.SetActive(true);

            // Start the fade-in coroutine
            yield return StartCoroutine(FadeInText());

            // Wait for the specified display duration
            yield return new WaitForSeconds(displayDuration);

            // Start the fade-out coroutine
            yield return StartCoroutine(FadeOutText());
        }
    }
    IEnumerator FadeOutText()
    {
        float startAlpha = levelText.color.a;
        float rate = 1.0f / fadeDuration;
        float progress = 0.0f;

        while (progress < 1.0f)
        {
            Color tmpColor = levelText.color;
            tmpColor.a = Mathf.Lerp(startAlpha, 0, progress);
            levelText.color = tmpColor;
            progress += rate * Time.deltaTime;

            yield return null; // Wait for the next frame
        }

        // Ensure the text is fully transparent at the end
        Color finalColor = levelText.color;
        finalColor.a = 0.0f;
        levelText.color = finalColor;

        levelText.gameObject.SetActive(false);
    }
    IEnumerator FadeInText()
    {
        float startAlpha = 0.0f;
        float rate = 1.0f / fadeDuration;
        float progress = 0.0f;

        while (progress < 1.0f)
        {
            Color tmpColor = levelText.color;
            tmpColor.a = Mathf.Lerp(startAlpha, 1.0f, progress);
            levelText.color = tmpColor;
            progress += rate * Time.deltaTime;

            yield return null; // Wait for the next frame
        }

        // Ensure the text is fully opaque at the end
        Color finalColor = levelText.color;
        finalColor.a = 1.0f;
        levelText.color = finalColor;
    }
}
