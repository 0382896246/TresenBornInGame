using UnityEngine;

public class LoopingCreditScroll : MonoBehaviour
{
    public RectTransform scrollContainer;    // Cha chứa A và B
    public float scrollSpeed = 30f;

    private RectTransform contentA;
    private RectTransform contentB;
    private float contentHeight;

    void Start()
    {
        if (scrollContainer == null || scrollContainer.childCount < 2)
        {
            Debug.LogError("[LoopingCreditScroll] Chưa gán đúng hoặc thiếu nội dung!");
            return;
        }

        contentA = scrollContainer.GetChild(0).GetComponent<RectTransform>();
        contentB = scrollContainer.GetChild(1).GetComponent<RectTransform>();

        // Lấy chiều cao nội dung (phải giống nhau!)
        contentHeight = contentA.rect.height;

        // Gán vị trí: A ở trên, B bên dưới
        contentA.anchoredPosition = Vector2.zero;
        contentB.anchoredPosition = new Vector2(0, -contentHeight);

        // Reset container về (0, 0)
        scrollContainer.anchoredPosition = Vector2.zero;
    }

    void Update()
    {
        // Cuộn toàn bộ container lên
        scrollContainer.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        // Khi đã cuộn hết A → B, thì quay về đầu (trượt mượt)
        if (scrollContainer.anchoredPosition.y >= contentHeight)
        {
            scrollContainer.anchoredPosition -= new Vector2(0, contentHeight);
        }
    }
}
