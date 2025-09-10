using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class StartMenuUIZoom : MonoBehaviour
{
    [Header("References (UI)")]
    public RectTransform zoomGroup;     // nhóm chứa UI cần zoom
    public RectTransform focus;         // điểm giữa laptop để zoom vào
    public Button startButton;          // nút Bắt đầu
    public CanvasGroup fadeGroup;       // lớp đen để fade out (alpha 0 → 1)

    [Header("Zoom Settings")]
    public float zoomDuration = 0.6f;        // thời gian zoom
    public float targetScale = 1.4f;         // tỷ lệ scale
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Scene")]
    public string gameSceneName = "Scene_ingame";
    public float fadeOutTime = 0.4f;

    private Vector3 startScale;
    private Vector3 startPosition;
    private bool isZooming = false;

    void Awake()
    {
        if (!zoomGroup || !focus || !startButton)
        {
            Debug.LogError("[UIZoom] Thiếu tham chiếu UI!");
            return;
        }

        startScale = zoomGroup.localScale;
        startPosition = zoomGroup.position;

        if (fadeGroup) fadeGroup.alpha = 0;

        // Gắn listener nếu chưa gọi từ MainMenu.cs
        startButton.onClick.AddListener(() =>
        {
            StartZoomEffect();
        });
    }

    /// <summary>
    /// Gọi từ MainMenu.cs để bắt đầu hiệu ứng zoom.
    /// </summary>
    public void StartZoomEffect()
    {
        if (!isZooming)
            StartCoroutine(ZoomAndLoadScene());
    }

    IEnumerator ZoomAndLoadScene()
    {
        isZooming = true;

        // Bước 1: Tính offset để focus nằm giữa màn hình
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Vector3 focusScreenPos = RectTransformUtility.WorldToScreenPoint(null, focus.position);
        Vector3 offset = screenCenter - focusScreenPos;
        Vector3 targetPos = zoomGroup.position + offset;

        float timer = 0f;

        while (timer < zoomDuration)
        {
            float t = curve.Evaluate(timer / zoomDuration);

            zoomGroup.position = Vector3.Lerp(startPosition, targetPos, t);
            zoomGroup.localScale = Vector3.Lerp(startScale, Vector3.one * targetScale, t);

            timer += Time.deltaTime;
            yield return null;
        }

        zoomGroup.position = targetPos;
        zoomGroup.localScale = Vector3.one * targetScale;

        // Bước 2: Fade to black (nếu có)
        if (fadeGroup)
        {
            float fadeTimer = 0f;
            while (fadeTimer < fadeOutTime)
            {
                fadeGroup.alpha = Mathf.Lerp(0, 1, fadeTimer / fadeOutTime);
                fadeTimer += Time.deltaTime;
                yield return null;
            }
            fadeGroup.alpha = 1;
        }

        // Bước 3: Load scene
        SceneManager.LoadScene(gameSceneName);
    }
}

