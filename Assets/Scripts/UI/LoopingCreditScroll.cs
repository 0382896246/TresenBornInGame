using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class LoopingCreditScroll : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform contentA;   // khối chứa chữ (Credit) – pivot = (0.5, 0)
    public RectTransform viewport;   // khung che (có Mask/RectMask2D)

    [Header("Scroll")]
    public float scrollSpeed = 50f;  // px/giây
    public bool useUnscaledTime = true;
    public bool loop = true;         // true: lặp mãi; false: chạy 1 lần rồi dừng

    float startY, contentH, viewportH;
    bool run;

    void OnEnable() => InitAndStart();

    void InitAndStart()
    {
        if (!contentA) contentA = GetComponent<RectTransform>();
        if (!contentA || !viewport)
        {
            Debug.LogError("[Credit] Chưa gán contentA/viewport");
            run = false; return;
        }

        // Ép width = viewport để Text wrap đúng, rồi rebuild để đo chiều cao chuẩn
        contentA.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, viewport.rect.width);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(viewport);
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentA);
        Canvas.ForceUpdateCanvases();

        // Lấy chiều cao (ưu tiên preferred height nếu cần)
        float h1 = contentA.rect.height;
        float h2 = LayoutUtility.GetPreferredHeight(contentA);
        contentH = Mathf.Max(h1, h2, 1f);
        viewportH = Mathf.Max(viewport.rect.height, 1f);

        // BẮT ĐẦU từ dòng đầu: để đáy content nằm ngay dưới mép dưới viewport
        // YÊU CẦU: pivot của contentA = (0.5, 0)
        startY = -viewportH;
        var pos = contentA.anchoredPosition;
        contentA.anchoredPosition = new Vector2(pos.x, startY);

        run = true;

        if (contentH <= 5f)
            Debug.LogWarning("[Credit] content height quá nhỏ. Hãy bật Content Size Fitter (Vertical=Preferred Size).");
    }

    void Update()
    {
        if (!run) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float dy = scrollSpeed * dt;

        contentA.anchoredPosition += new Vector2(0f, dy);

        // Khi toàn bộ content đã đi qua viewport
        if (contentA.anchoredPosition.y >= contentH)
        {
            if (loop)
            {
                // Lặp lại từ đầu
                contentA.anchoredPosition = new Vector2(contentA.anchoredPosition.x, startY);
            }
            else
            {
                // Dừng tại cuối
                run = false;
            }
        }
    }
}
