using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIIntroCameraZoom : MonoBehaviour
{
    [System.Serializable]
    public class Slide { public Sprite sprite; public float hold = 2f; }

    [Header("Refs")]
    [SerializeField] Camera uiCam;             // UICamera (orthographic)
    [SerializeField] Image bg;                 // Intro/BG
    [SerializeField] Image fade;               // Intro/Fade (đen)
    [SerializeField] RectTransform zoomTarget; // vùng cần zoom tới (màn hình laptop)
    [SerializeField] GameObject introRoot;     // Canvas/Intro
    [SerializeField] GameObject inGameRoot;    // Canvas/InGame (tắt sẵn)
    [SerializeField] GameManager gameManager;  // để gọi StartGameFromIntro()

    [Header("Slides (chỉ 1 ảnh dùng cho tất cả)")]
    [SerializeField] List<Slide> slides = new List<Slide>(); // 0 = #4, 1 = #5

    [Header("Timing")]
    [SerializeField] float fadeDuration = 0.6f;
    [SerializeField] float zoomDuration = 1.1f;
    [SerializeField] float zoomOvershoot = 1.03f; // tăng nhẹ cho chắc chắn phủ kín

    // lưu trạng thái camera để khôi phục
    float camStartSize;
    Vector3 camStartPos;

    void Awake()
    {
        if (!introRoot) introRoot = gameObject;
        inGameRoot.SetActive(false);
        introRoot.SetActive(true);

        if (!uiCam) uiCam = Camera.main;
        camStartSize = uiCam.orthographicSize;
        camStartPos = uiCam.transform.position;

        // BG sẽ được fade-in khi zoom
        var c = bg.color; c.a = 1f; bg.color = c;
        SetAlpha(fade, 1f); // màn đen lúc mới vào
    }

    void Start() => StartCoroutine(Run());

    IEnumerator Run()
    {
        // mở từ đen
        yield return Fade(1f, 0f, fadeDuration);

        // chạy các slide bình thường (chỉ đổi sprite + chờ hold)
        for (int i = 0; i < slides.Count; i++)
        {
            bg.sprite = slides[i].sprite;
            yield return new WaitForSeconds(slides[i].hold);
        }

        // tới lúc ZOOM-IN: đặt BG alpha = 0 rồi vừa zoom vừa fade-in ảnh hiện tại
        Color bgCol = bg.color; bgCol.a = 0f; bg.color = bgCol;
        yield return ZoomInToRectAndFadeIn();

        // đen hẳn → bật InGame → khôi phục camera → fade mở UI chơi
        yield return Fade(0.25f, 1f, 0.2f);

        introRoot.SetActive(false);
        inGameRoot.SetActive(true);
        uiCam.orthographicSize = camStartSize;
        uiCam.transform.position = camStartPos;

        SetAlpha(fade, 1f);
        gameManager.StartGameFromIntro();
        yield return Fade(1f, 0f, 0.25f);
    }

    IEnumerator ZoomInToRectAndFadeIn()
    {
        // world corners của vùng cần zoom tới
        Vector3[] wc = new Vector3[4];
        zoomTarget.GetWorldCorners(wc);
        float width = (wc[2] - wc[1]).magnitude; // top-right - top-left
        float height = (wc[1] - wc[0]).magnitude; // top-left - bottom-left
        Vector3 center = (wc[0] + wc[2]) * 0.5f;  // tâm hình chữ nhật

        // orthographic size cần để vùng này phủ toàn màn
        float sizeH = height / 2f;
        float sizeW = (width / 2f) / uiCam.aspect;
        float targetSize = Mathf.Max(1f, Mathf.Max(sizeH, sizeW) * zoomOvershoot);

        // bắt đầu từ vị trí/size hiện tại → ZOOM IN
        float startSize = uiCam.orthographicSize;
        Vector3 startPos = uiCam.transform.position;
        Vector3 endPos = new Vector3(center.x, center.y, startPos.z);

        float t = 0f;
        while (t < zoomDuration)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / zoomDuration);
            uiCam.orthographicSize = Mathf.Lerp(startSize, targetSize, k);
            uiCam.transform.position = Vector3.Lerp(startPos, endPos, k);

            // ảnh đang zoom được FADE-IN 0 → 1
            var c = bg.color; c.a = Mathf.Lerp(0f, 1f, k); bg.color = c;

            t += Time.deltaTime; yield return null;
        }
        uiCam.orthographicSize = targetSize;
        uiCam.transform.position = endPos;
    }

    IEnumerator Fade(float from, float to, float dur)
    {
        float t = 0f;
        while (t < dur) { SetAlpha(fade, Mathf.Lerp(from, to, t / dur)); t += Time.deltaTime; yield return null; }
        SetAlpha(fade, to);
    }
    void SetAlpha(Image img, float a) { var c = img.color; c.a = a; img.color = c; }
}
