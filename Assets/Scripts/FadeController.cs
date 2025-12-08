using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    [Header("Fade Settings")]
    [Tooltip("The UI Image used for fading (black panel)")]
    public Image fadeImage;
    
    [Tooltip("How long the fade takes (seconds)")]
    public float fadeDuration = 1f;
    
    [Tooltip("Color to fade to (usually black)")]
    public Color fadeColor = Color.black;
    
    private static FadeController instance;
    private Coroutine currentFade;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        if (fadeImage == null)
        {
            fadeImage = GetComponent<Image>();
        }
    }

    private void Start()
    {
        if (fadeImage != null)
        {
            fadeImage.enabled = true;
            Color color = fadeColor;
            color.a = 0f;
            fadeImage.color = color;
            fadeImage.raycastTarget = false;
            Debug.Log("FadeController initialized with Image: " + fadeImage.name);
        }
        else
        {
            Debug.LogError("FadeController: No fade image assigned or found!");
        }
    }

    public static FadeController Instance
    {
        get { return instance; }
    }

    public void FadeOut(System.Action onComplete = null)
    {
        Debug.Log("FadeOut called");
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeCoroutine(0f, 1f, onComplete));
    }

    public void FadeIn(System.Action onComplete = null)
    {
        Debug.Log("FadeIn called");
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeCoroutine(1f, 0f, onComplete));
    }

    public void FadeOutAndIn(System.Action onFadedOut = null, System.Action onFadedIn = null)
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeOutAndInCoroutine(onFadedOut, onFadedIn));
    }

    private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, System.Action onComplete)
    {
        if (fadeImage == null)
        {
            Debug.LogError("FadeController: No fade image assigned!");
            onComplete?.Invoke();
            yield break;
        }

        fadeImage.enabled = true;
        
        float elapsed = 0f;
        Color color = fadeColor;

        Debug.Log($"Starting fade from {startAlpha} to {endAlpha}");

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            fadeImage.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadeImage.color = color;

        Debug.Log($"Fade complete. Final alpha: {endAlpha}");

        onComplete?.Invoke();
        currentFade = null;
    }

    private IEnumerator FadeOutAndInCoroutine(System.Action onFadedOut, System.Action onFadedIn)
    {
        yield return StartCoroutine(FadeCoroutine(0f, 1f, onFadedOut));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(FadeCoroutine(1f, 0f, onFadedIn));
        currentFade = null;
    }
}
