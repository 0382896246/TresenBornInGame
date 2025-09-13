using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroInGame_NoOverlap : MonoBehaviour
{
    [System.Serializable]
    public class Slide { public Sprite sprite; public float hold = 2f; }

    [Header("Roots")]
    [SerializeField] GameObject inGameRoot;   // Canvas/InGame (tắt sẵn)
    [SerializeField] GameObject introRoot;    // Canvas/Intro (bật sẵn)

    [Header("UI")]
    [SerializeField] Image bg;                // Ảnh full-screen cho các slide
    [SerializeField] Image fade;              // Màn đen overlay
    [SerializeField] RectTransform screenWindow; // VÙNG cần zoom vào (không phải mask/ảnh)

    [Header("Camera (Screen Space - Camera)")]
    [SerializeField] Camera uiCam;            // UICamera (Orthographic)

    [Header("Slides")]
    [SerializeField] List<Slide> slides = new List<Slide>(); // 0:#4 street, 1:#5 sleep
    [SerializeField] int revealAfterIndex = 1;  // sau slide index=1 thì zoom-in

    [Header("Timing")]
    [SerializeField] float fadeDuration = 0.6f;
    [SerializeField] float zoomDuration = 1.1f;

    [SerializeField] float blinkIn = 0.25f;  // từ sáng -> đen (gợi ý 0.20–0.35s)
    [SerializeField] float blinkHold = 0.15f; // giữ màn đen (0.10–0.30s)
    [SerializeField] float blinkOut = 0.35f;  // từ đen -> sáng (0.25–0.45s)

    [Tooltip("Zoom sâu thêm 1 chút (nhỏ hơn kích thước cần thiết). 1.0 = vừa khít.")]
    [SerializeField, Range(0.8f, 1.0f)] float zoomTightFactor = 0.95f;

    [Header("Managers")]
    [SerializeField] GameManager gameManager;

    // lưu trạng thái camera để khôi phục
    float camStartSize;
    Vector3 camStartPos;

    void Awake()
    {
        if (!introRoot) introRoot = gameObject;
        if (!uiCam) uiCam = Camera.main;

        // Kiểm tra tham chiếu
        if (!inGameRoot || !introRoot || !bg || !fade || !screenWindow || !uiCam || !gameManager)
        {
            Debug.LogError("[Intro] Missing refs! Hãy kéo đủ InGameRoot, IntroRoot, BG, Fade, screenWindow, uiCam, gameManager.");
            enabled = false; return;
        }

        // chuẩn bị
        inGameRoot.SetActive(false);
        introRoot.SetActive(true);

        camStartSize = uiCam.orthographicSize;
        camStartPos = uiCam.transform.position;

        // mới vào -> màn đen
        SetAlpha(fade, 1f);
    }

    void Start() => StartCoroutine(Run());




    IEnumerator SwitchDuringBlink()
    {
        // 1) Fade lên đen lâu hơn
        yield return Fade(0f, 1f, blinkIn);

        // 2) Giữ đen một lúc
        yield return new WaitForSecondsRealtime(blinkHold);

        // 3) Đổi sang InGame ngay khi đang đen
        introRoot.SetActive(false);
        inGameRoot.SetActive(true);

        // Khôi phục camera & bắt đầu game
        uiCam.orthographicSize = camStartSize;
        uiCam.transform.position = camStartPos;
        gameManager.StartGameFromIntro();

        // 4) Đợi 1 frame cho UI dựng xong, rồi mở sáng chậm hơn
        yield return new WaitForEndOfFrame();
        yield return Fade(1f, 0f, blinkOut);
    }


    IEnumerator Run()
    {
        // mở từ đen
        yield return Fade(1f, 0f, fadeDuration);

        // chạy các slide
        for (int i = 0; i < slides.Count; i++)
        {
            bg.sprite = slides[i].sprite;
            yield return new WaitForSeconds(slides[i].hold);

            // Đến thời điểm zoom-in vào vùng màn hình
            if (i == revealAfterIndex)
            {
                // ảnh hiện tại fade-in trong lúc zoom
                Color c = bg.color; c.a = 0f; bg.color = c;
                // ... sau khi chạy xong ZoomCameraToRectAndFadeIn();
                yield return SwitchDuringBlink(); // 👈 nhấp nháy & chuyển luôn

            }
        }

        // chớp đen để chuyển sang UI chơi
        yield return Fade(0f, 1f, 0.2f);

        introRoot.SetActive(false);
        inGameRoot.SetActive(true);

        // khôi phục camera & mở sáng UI chơi
        uiCam.orthographicSize = camStartSize;
        uiCam.transform.position = camStartPos;

        SetAlpha(fade, 1f);
        gameManager.StartGameFromIntro();
        yield return Fade(1f, 0f, 0.25f);
    }

    // ---- Hiệu ứng: Zoom IN bằng camera + BG fade-in ----
    IEnumerator ZoomCameraToRectAndFadeIn()
    {
        // Lấy world corners của vùng cần zoom tới
        Vector3[] wc = new Vector3[4];
        screenWindow.GetWorldCorners(wc);
        float width = (wc[3] - wc[0]).magnitude; // br - bl
        float height = (wc[1] - wc[0]).magnitude; // tl - bl
        Vector3 center = (wc[0] + wc[2]) * 0.5f;  // tâm hình chữ nhật

        // orthographic size để vùng này phủ hết màn hình (zoom IN => size nhỏ đi)
        float sizeH = height * 0.5f;
        float sizeW = (width * 0.5f) / uiCam.aspect;
        float targetSize = Mathf.Max(sizeH, sizeW) * zoomTightFactor; // nhỏ hơn chút để zoom sâu

        float startSize = uiCam.orthographicSize;       // lớn -> nhỏ (zoom in)
        Vector3 startPos = uiCam.transform.position;
        Vector3 endPos = new Vector3(center.x, center.y, startPos.z);

        float t = 0f;
        while (t < zoomDuration)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / zoomDuration);
            uiCam.orthographicSize = Mathf.Lerp(startSize, targetSize, k);
            uiCam.transform.position = Vector3.Lerp(startPos, endPos, k);

            // fade-in ảnh nền 0 → 1 trong lúc zoom
            var c = bg.color; c.a = Mathf.Lerp(0f, 1f, k); bg.color = c;

            t += Time.deltaTime;
            yield return null;
        }
        uiCam.orthographicSize = targetSize;
        uiCam.transform.position = endPos;
    }

    // ---- tiện ích ----
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
