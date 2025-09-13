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
    [SerializeField] RectTransform zoomRoot;     // PARENT để phóng to (chứa BG + screenWindow)
    [SerializeField] RectTransform screenWindow; // Ô đích cần zoom tới (child của zoomRoot)

    [Header("UI")]
    [SerializeField] Image bg;                   // Image hiển thị slide (con của zoomRoot)
    [SerializeField] Image fade;                 // Màn đen overlay

    [Header("Slides")]
    [SerializeField] List<Slide> slides = new List<Slide>();  // các ảnh intro
    [SerializeField] int revealAfterIndex = 2;                 // zoom sau ảnh thứ mấy (index)

    [Header("Timing")]
    [SerializeField] float fadeDuration = 0.6f;   // fade mở intro
    [SerializeField] float slideFade = 0.35f;     // fade giữa các slide
    [SerializeField] float zoomDuration = 1.1f;   // thời gian zoom

    // nháy rồi chuyển sang InGame
    [SerializeField] float blinkIn = 0.25f;
    [SerializeField] float blinkHold = 0.15f;
    [SerializeField] float blinkOut = 0.35f;

    [Tooltip("Hệ số phóng: >1 phóng mạnh, =1 vừa khít. 1.05 là phóng sâu hơn ~5%.")]
    [SerializeField] float zoomExtra = 1.05f;

    [Header("Managers")]
    [SerializeField] GameManager gameManager;

    [Header("Intro Once Settings")]
    [Tooltip("Tick để luôn ép hiển thị Intro (debug). Bỏ tick để chạy logic chỉ-hiện-lần-đầu.")]
    [SerializeField] bool forceShowIntro = false;

    const string INTRO_SEEN_KEY = "INTRO_SEEN_FLAG";
    RectTransform canvasRT;

    void Awake()
    {
        // Nếu đã xem Intro (và không ép hiện), bỏ qua Intro và vào game ngay
        if (HasSeenIntro() && !forceShowIntro)
        {
            if (!inGameRoot || !introRoot || !gameManager)
            {
                Debug.LogError("[Intro] Missing refs để skip intro! Kéo đủ InGameRoot, IntroRoot, GameManager.");
                // Nếu thiếu ref thì thôi cứ chạy như cũ để tránh đen màn
            }
            else
            {
                introRoot.SetActive(false);
                inGameRoot.SetActive(true);
                // Bắt đầu game ngay (giữ nguyên flow cũ)
                gameManager.StartGameFromIntro();
                enabled = false; // tắt script vì Intro không cần chạy nữa
                return;
            }
        }

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
        if (slides.Count == 0) { yield return SwitchDuringBlink_AndMarkSeen(); yield break; }

        // Slide đầu: hiện từ đen
        bg.sprite = slides[0].sprite;
        yield return Fade(1f, 0f, fadeDuration);
        yield return new WaitForSeconds(slides[0].hold);

        // Các slide tiếp theo
        for (int i = 1; i < slides.Count; i++)
        {
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

        // Nháy & chuyển sang InGame ngay trong lúc đang đen
        yield return SwitchDuringBlink_AndMarkSeen();
    }

    // ===== ZOOM UI bằng RectTransform (không dùng camera) =====
    IEnumerator ZoomRectToFillAndFadeIn()
    {
        float sW = canvasRT.rect.width / screenWindow.rect.width;
        float sH = canvasRT.rect.height / screenWindow.rect.height;
        float targetScale = Mathf.Max(sW, sH) * Mathf.Max(1f, zoomExtra);

        Vector2 childPos = screenWindow.anchoredPosition; // (anchors center)
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

            var c = bg.color; c.a = Mathf.Lerp(0f, 1f, k); bg.color = c;

            t += Time.deltaTime; yield return null;
        }
        zoomRoot.localScale = endScale;
        zoomRoot.anchoredPosition = endPos;
    }

    // ===== Blink + Switch + ĐÁNH DẤU ĐÃ XEM =====
    IEnumerator SwitchDuringBlink_AndMarkSeen()
    {
        yield return Fade(0f, 1f, blinkIn);
        yield return new WaitForSecondsRealtime(blinkHold);

        introRoot.SetActive(false);
        inGameRoot.SetActive(true);

        // Đánh dấu "đã xem Intro" để các lần reload sau sẽ bỏ qua
        MarkIntroSeen();

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

    // ===== PlayerPrefs helpers =====
    static bool HasSeenIntro() => PlayerPrefs.GetInt(INTRO_SEEN_KEY, 0) == 1;

    static void MarkIntroSeen()
    {
        PlayerPrefs.SetInt(INTRO_SEEN_KEY, 1);
        PlayerPrefs.Save();
    }

    // Tuỳ chọn: hàm public để reset flag khi cần debug
    [ContextMenu("Reset Intro Seen Flag")]
    public void ResetIntroFlagForDebug()
    {
        PlayerPrefs.DeleteKey(INTRO_SEEN_KEY);
        PlayerPrefs.Save();
        Debug.Log("[Intro] Đã xoá cờ INTRO_SEEN_FLAG (debug).");
    }
}
