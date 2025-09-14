using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroInGame : MonoBehaviour
{
    [System.Serializable]
    public class Slide { public Sprite sprite; public float hold = 2f; }

    [Header("Roots")]
    [SerializeField] GameObject inGameRoot;
    [SerializeField] GameObject introRoot;
    [SerializeField] RectTransform zoomRoot;
    [SerializeField] RectTransform screenWindow;

    [Header("UI")]
    [SerializeField] Image bg;
    [SerializeField] Image fade;
    [SerializeField] Button skipIntroButton;     // << NÚT SKIP INTRO

    [Header("Slides")]
    [SerializeField] List<Slide> slides = new List<Slide>();
    [SerializeField] int revealAfterIndex = 2;

    [Header("Timing")]
    [SerializeField] float fadeDuration = 0.6f;
    [SerializeField] float slideFade = 0.35f;
    [SerializeField] float zoomDuration = 1.1f;
    [SerializeField] float blinkIn = 0.25f;
    [SerializeField] float blinkHold = 0.15f;
    [SerializeField] float blinkOut = 0.35f;
    [SerializeField] float zoomExtra = 1.05f;

    [Header("Managers")]
    [SerializeField] GameManager gameManager;

    RectTransform canvasRT;
    bool skipped = false;

    void Awake()
    {
        canvasRT = GetComponentInParent<Canvas>()?.GetComponent<RectTransform>();

        if (!inGameRoot || !introRoot || !bg || !fade || !zoomRoot || !screenWindow || !canvasRT || !gameManager)
        {
            Debug.LogError("[Intro] Missing refs!");
            enabled = false; return;
        }

        inGameRoot.SetActive(false);
        introRoot.SetActive(true);

        SetAlpha(fade, 1f);
        fade.raycastTarget = true;

        zoomRoot.pivot = screenWindow.pivot = new Vector2(0.5f, 0.5f);
        zoomRoot.localScale = Vector3.one;
        zoomRoot.anchoredPosition = Vector2.zero;

        // Gán sự kiện skip
        if (skipIntroButton)
        {
            skipIntroButton.onClick.RemoveAllListeners();
            skipIntroButton.onClick.AddListener(SkipIntroNow);
        }
    }

    void Start() => StartCoroutine(Run());

    IEnumerator Run()
    {
        if (slides.Count == 0)
        {
            yield return SwitchToGame();
            yield break;
        }

        // Slide đầu
        bg.sprite = slides[0].sprite;
        yield return Fade(1f, 0f, fadeDuration);
        yield return new WaitForSeconds(slides[0].hold);

        for (int i = 1; i < slides.Count; i++)
        {
            if (skipped) yield break; // nếu đã skip thì dừng coroutine
            yield return Fade(0f, 1f, slideFade);
            bg.sprite = slides[i].sprite;
            yield return Fade(1f, 0f, slideFade);
            yield return new WaitForSeconds(slides[i].hold);

            if (i == revealAfterIndex)
            {
                var c = bg.color; c.a = 0f; bg.color = c;
                yield return ZoomRectToFillAndFadeIn();
                break;
            }
        }

        if (!skipped)
            yield return SwitchToGame();
    }

    IEnumerator ZoomRectToFillAndFadeIn()
    {
        float sW = canvasRT.rect.width / screenWindow.rect.width;
        float sH = canvasRT.rect.height / screenWindow.rect.height;
        float targetScale = Mathf.Max(sW, sH) * Mathf.Max(1f, zoomExtra);

        Vector2 childPos = screenWindow.anchoredPosition;
        Vector3 startScale = zoomRoot.localScale;
        Vector2 startPos = zoomRoot.anchoredPosition;

        Vector3 endScale = new Vector3(targetScale, targetScale, 1f);
        Vector2 endPos = -childPos * targetScale;

        float t = 0f;
        while (t < zoomDuration)
        {
            if (skipped) yield break;
            float k = Mathf.SmoothStep(0f, 1f, t / zoomDuration);
            zoomRoot.localScale = Vector3.Lerp(startScale, endScale, k);
            zoomRoot.anchoredPosition = Vector2.Lerp(startPos, endPos, k);

            var c = bg.color; c.a = Mathf.Lerp(0f, 1f, k); bg.color = c;
            t += Time.deltaTime; yield return null;
        }
        zoomRoot.localScale = endScale;
        zoomRoot.anchoredPosition = endPos;
    }

    // Chuyển hẳn sang game
    IEnumerator SwitchToGame()
    {
        yield return Fade(0f, 1f, blinkIn);
        yield return new WaitForSecondsRealtime(blinkHold);

        introRoot.SetActive(false);
        inGameRoot.SetActive(true);

        gameManager.StartGameFromIntro();

        yield return new WaitForEndOfFrame();
        yield return Fade(1f, 0f, blinkOut);

        if (fade) { fade.raycastTarget = false; fade.enabled = false; }
    }

    // Hàm skip gọi khi bấm nút
    void SkipIntroNow()
    {
        if (skipped) return;
        skipped = true;
        StopAllCoroutines();
        StartCoroutine(SwitchToGame());
    }

    IEnumerator Fade(float from, float to, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            SetAlpha(fade, Mathf.Lerp(from, to, t / dur));
            t += Time.deltaTime; yield return null;
        }
        SetAlpha(fade, to);
    }

    void SetAlpha(Image img, float a)
    {
        var c = img.color; c.a = a; img.color = c;
    }
}
