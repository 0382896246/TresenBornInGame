using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Scale Settings")]
    public float normalScale = 1f;
    public float hoverScale = 1.1f;
    public float pressedScale = 0.9f;
    public float smooth = 10f;

    private float targetScale;

    void Start()
    {
        targetScale = normalScale;
        transform.localScale = Vector3.one * normalScale;
    }

    void Update()
    {
        // Dùng unscaledDeltaTime để vẫn hoạt động khi game bị pause (Time.timeScale = 0)
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            Vector3.one * targetScale,
            Time.unscaledDeltaTime * smooth
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = normalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = pressedScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = hoverScale;
    }
}
