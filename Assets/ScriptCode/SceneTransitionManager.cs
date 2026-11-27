using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    private static SceneTransitionManager _instance;
    public static SceneTransitionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SceneTransitionManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("SceneTransitionManager");
                    _instance = obj.AddComponent<SceneTransitionManager>();
                }
            }
            return _instance;
        }
    }

    [Header("Optional Custom Fade Overlay")]
    public CanvasGroup fadeCanvasGroup;
    public float defaultFadeDuration = 0.5f;
    public float holdBlackDuration = 0.35f;

    private bool isTransitioning = false;

    void Awake()
    {
        if (_instance == null) _instance = this;
        else if (_instance != this) { Destroy(gameObject); return; }

        EnsureFadeOverlay();
    }

    private void EnsureFadeOverlay()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            return;
        }

        // Auto-generate a top-level black fade overlay if not manually assigned
        GameObject canvasObj = new GameObject("TransitionFadeCanvas");
        canvasObj.transform.SetParent(transform);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        canvasObj.AddComponent<CanvasScaler>();

        GameObject panelObj = new GameObject("BlackOverlay");
        panelObj.transform.SetParent(canvasObj.transform, false);

        RectTransform rect = panelObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = panelObj.AddComponent<Image>();
        img.color = Color.black;

        fadeCanvasGroup = panelObj.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }

    public void PlayTransition(Action onScreenBlack, float fadeDuration = -1f)
    {
        if (isTransitioning)
        {
            onScreenBlack?.Invoke();
            return;
        }

        float duration = fadeDuration > 0f ? fadeDuration : defaultFadeDuration;
        StartCoroutine(TransitionRoutine(onScreenBlack, duration));
    }

    private IEnumerator TransitionRoutine(Action onScreenBlack, float duration)
    {
        isTransitioning = true;
        EnsureFadeOverlay();
        fadeCanvasGroup.blocksRaycasts = true;

        // 1. Fade to black
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;

        // 2. Execute mid-transition action (e.g., despawn side-mission NPC)
        onScreenBlack?.Invoke();

        // 3. Brief pause while black
        yield return new WaitForSecondsRealtime(holdBlackDuration);

        // 4. Fade back in
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        isTransitioning = false;
    }
}