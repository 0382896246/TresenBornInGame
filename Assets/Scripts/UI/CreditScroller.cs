using UnityEngine;
using UnityEngine.UI;

public class CreditScroller : MonoBehaviour
{
    [Header("Cuộn nội dung")]
    public RectTransform creditContent;
    public float scrollSpeed = 30f;

    [Header("UI đóng bảng")]
    public Button closeButton;
    public GameObject creditPanel;

    private Vector2 startPos;
    private float contentHeight;
    private bool isScrolling = false;

    void Start()
    {
        if (!creditContent)
        {
            Debug.LogError("[CreditScroller] CreditContent chưa được gán!");
            return;
        }

        startPos = creditContent.anchoredPosition;
        contentHeight = creditContent.rect.height;

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseCredit);

        // Tắt panel ban đầu
        if (creditPanel != null)
            creditPanel.SetActive(false);
    }

    void OnEnable()
    {
        RestartScroll(); // Bắt đầu lại khi bật lại CreditPanel
    }

    void Update()
    {
        if (!isScrolling) return;

        float move = scrollSpeed * Time.unscaledDeltaTime;
        creditContent.anchoredPosition += new Vector2(0, move);

        // Nếu đã cuộn hết + ra khỏi khung → quay lại ban đầu
        if (creditContent.anchoredPosition.y >= contentHeight)
        {
            creditContent.anchoredPosition = startPos;
        }
    }

    public void RestartScroll()
    {
        if (!creditContent) return;

        creditContent.anchoredPosition = startPos;
        isScrolling = true;
    }

    public void CloseCredit()
    {
        isScrolling = false;

        if (creditPanel != null)
            creditPanel.SetActive(false);
    }

    public void OpenCredit()
    {
        if (creditPanel != null)
        {
            creditPanel.SetActive(true);
            RestartScroll(); // bắt đầu lại mỗi lần mở
        }
    }
}
