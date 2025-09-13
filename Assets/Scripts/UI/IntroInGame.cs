using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroInGame_NoOverlap : MonoBehaviour
{
    [System.Serializable]
    public class Slide { public Sprite sprite; public float hold = 2f; }

    [Header("Roots")]
    [SerializeField] GameObject inGameRoot;      // Canvas/InGame (tắt sẵn)
    [SerializeField] GameObject introRoot;       // Canvas/Intro (bật sẵn)
    [SerializeField] RectTransform zoomRoot;     // 👈 PARENT để phóng to (chứa BG + screenWindow)
    [SerializeField] RectTransform screenWindow; // 👈 Ô đích cần zoom tới (child của zoomRoot)

    [Header("UI")]
    [SerializeField] Image bg;                   // Image hiển thị slide (con của zoomRoot)
    [SerializeField] Image fade;                 // Màn đen overlay

    [Header("Slides")]
    [SerializeField] List<Slide> slides = new List<Slide>();  // các ảnh intro
    [SerializeField] int revealAfterIndex = 2;                 // zoom sau ảnh thứ mấy (index)

    [Header("Timing")]
    [SerializeField] float fadeDuration = 0.6f;  // fade mở intro
    [SerializeField] float slideFade = 0.35f;  // fade giữa các slide
    [SerializeField] float zoomDuration = 1.1f;  // thời gian zoom

    // nháy rồi chuyển sang InGame
    [SerializeField] float blinkIn = 0.25f;
    [SerializeField] float blinkHold = 0.15f;
    [SerializeField] float blinkOut = 0.35f;

    [Tooltip("Hệ số phóng: >1 phóng mạnh, =1 vừa khít. 1.05 là phóng sâu hơn ~5%.")]
    [SerializeField] float zoomExtra = 1.05f;

    [Header("Managers")]
    [SerializeField] GameManager gameManager;

    RectTransform canvasRT;

    void Awake()
    {
        canvasRT = GetComponentInParent<Canvas>()?.GetComponent<RectTransform>();

        if (!inGameRoot || !introRoot || !bg || !fade || !zoomRoot || !screenWindow || !canvasRT || !gameManager)
        {
            Debug.LogError("[Intro] Missing refs! Kéo đủ InGameRoot, IntroRoot, BG, Fade, ZoomRoot, screenWindow, GameManager.");
            enabled = false; return;
        }

        inGameRoot.SetActive(false);
        introRoot.SetActive(true);

        // đảm bảo pivot/anchors chuẩn
        zoomRoot.pivot = screenWindow.pivot = new Vector2(0.5f, 0.5f);
        SetAlpha(fade, 1f);            // vào màn đen
        zoomRoot.localScale = Vector3.one;
        zoomRoot.anchoredPosition = Vector2.zero;
    }

    void Start() => StartCoroutine(Run());

    IEnumerator Run()
    {
        if (slides.Count == 0) { yield return SwitchDuringBlink(); yield break; }

        // Slide đầu: hiện từ đen
        bg.sprite = slides[0].sprite;
        yield return Fade(1f, 0f, fadeDuration);
        yield return new WaitForSeconds(slides[0].hold);

        // Các slide tiếp theo: fade-out -> đổi ảnh -> fade-in -> hold
        for (int i = 1; i < slides.Count; i++)
        {
            yield return Fade(0f, 1f, slideFade);
            bg.sprite = slides[i].sprite;
            yield return Fade(1f, 0f, slideFade);
            yield return new WaitForSeconds(slides[i].hold);

            if (i == revealAfterIndex)
            {
                // Trong lúc zoom, bg sẽ fade-in từ 0 -> 1 để có cảm giác "xuất hiện"
                var c = bg.color; c.a = 0f; bg.color = c;
                yield return ZoomRectToFillAndFadeIn();
                break;
            }
        }

        // Nháy & chuyển sang InGame ngay trong lúc đang đen
        yield return SwitchDuringBlink();
    }

    // ===== ZOOM UI bằng RectTransform (không dùng camera) =====
    IEnumerator ZoomRectToFillAndFadeIn()
    {
        // scale cần để screenWindow phủ hết Canvas
        float sW = canvasRT.rect.width / screenWindow.rect.width;
        float sH = canvasRT.rect.height / screenWindow.rect.height;
        float targetScale = Mathf.Max(sW, sH) * Mathf.Max(1f, zoomExtra);

        // ta zoom parent (zoomRoot). Để tâm screenWindow ra giữa Canvas:
        // vị trí hiệu quả sau scale = scale * childPos + parentPos -> parentPos = -scale * childPos
        Vector2 childPos = screenWindow.anchoredPosition;            // (anchors center)
        Vector3 startScale = zoomRoot.localScale;
        Vector2 startPos = zoomRoot.anchoredPosition;

        Vector3 endScale = new Vector3(targetScale, targetScale, 1f);
        Vector2 endPos = -childPos * targetScale;

        float t = 0f;
        while (t < zoomDuration)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / zoomDuration);
            zoomRoot.localScale = Vector3.Lerp(startScale, endScale, k);
            zoomRoot.anchoredPosition = Vector2.Lerp(startPos, endPos, k);

            // BG alpha 0 -> 1 khi đang zoom
            var c = bg.color; c.a = Mathf.Lerp(0f, 1f, k); bg.color = c;

            t += Time.deltaTime; yield return null;
        }
        zoomRoot.localScale = endScale;
        zoomRoot.anchoredPosition = endPos;
    }

    // ===== Blink + Switch =====
    IEnumerator SwitchDuringBlink()
    {
        yield return Fade(0f, 1f, blinkIn);
        yield return new WaitForSecondsRealtime(blinkHold);

        introRoot.SetActive(false);
        inGameRoot.SetActive(true);

        gameManager.StartGameFromIntro();

        yield return new WaitForEndOfFrame();
        yield return Fade(1f, 0f, blinkOut);
    }

    // ===== Fade overlay tiện ích =====
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

    void SetAlpha(Image img, float a) { var c = img.color; c.a = a; img.color = c; }
}
