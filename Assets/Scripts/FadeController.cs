using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// this script fades the screen to black and back
// used for scene transitions or teleporting
public class FadeController : MonoBehaviour
{
    [Header("Fade Settings")]
    public Image fadeImage;
    public float fadeDuration = 1f;
    public Color fadeColor = Color.black;
    
    private static FadeController instance;
    private Coroutine currentFade;

    private void Awake()
    {
        // singleton so other scripts can easily access this
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
        }
    }

    public static FadeController Instance
    {
        get { return instance; }
    }

    // fade from clear to black
    public void FadeOut(System.Action onComplete = null)
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeCoroutine(0f, 1f, onComplete));
    }

    // fade from black to clear
    public void FadeIn(System.Action onComplete = null)
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeCoroutine(1f, 0f, onComplete));
    }

    // fade out then fade back in
    public void FadeOutAndIn(System.Action onFadedOut = null, System.Action onFadedIn = null)
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeOutAndInCoroutine(onFadedOut, onFadedIn));
    }

    // smoothly fade between two alpha values
    private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, System.Action onComplete)
    {
        if (fadeImage == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        fadeImage.enabled = true;
        
        float elapsed = 0f;
        Color color = fadeColor;

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
