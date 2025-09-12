using UnityEngine;
using UnityEngine.UI;
using System.Text;
using Unity.VisualScripting;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements (Legacy Text)")]
   // public Text titleText;        // tiêu đề câu hỏi
    public Text descriptionText;  // nội dung câu hỏi
    public Text progressText;     // 3/10
    public Text reasonText;       // bullet lý do sau khi trả lời
    public Text categoryText;     // ô "Loại:" (tuỳ chọn)
    //public Text timerText;        // đếm ngược
    //public Text scoreText;        // điểm (nếu có 1 text riêng cho điểm)

    [Header("Buttons")]
    public Button legalButton;    // nút HỢP PHÁP
    public Button scamButton;     // nút LỪA ĐẢO

    [Header("Timer colors")]
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;


    // ---------- HEARTS ----------
    [System.Serializable]
    public class HeartPair
    {
        public Image full;   // Ảnh trái tim đỏ (còn mạng)
        public Image empty;  // Ảnh trái tim xám (mất mạng)

        public void SetAlive(bool alive)
        {
            if (full) full.gameObject.SetActive(alive);
            if (empty) empty.gameObject.SetActive(!alive);
        }
    }

    [Header("Hearts (Full + Empty)")]
    public HeartPair[] hearts; // Kéo 3 cặp (Heart1/2/3) vào đây



    void Start()
    {
       
        if (reasonText) reasonText.text = "";
        //if (timerText) timerText.color = normalColor;
    }

    // Lên câu mới
    public void ShowQuestion(QuestionAsset q, int index, int total)
    {
        if (q == null) return; // Đảm bảo rằng câu hỏi không null

        // Hiển thị câu hỏi lên UI
       // if (titleText) titleText.text = q.questionText ?? "";
        if (descriptionText) descriptionText.text = q.questionText ?? "";
        if (categoryText) categoryText.text = string.IsNullOrEmpty(q.category) ? "" : q.category;

        if (progressText) progressText.text = $"{index}/{total}";
        if (reasonText) reasonText.text = ""; // Clear lý do cũ

        SetButtonsInteractable(true);
    }


    // Cập nhật timer (đếm ngược)
    //public void UpdateTimer(float time)
    //{
    //    if (!timerText) return;
    //    int sec = Mathf.CeilToInt(Mathf.Max(time, 0f));
    //    timerText.text = sec.ToString();
    //    timerText.color = (sec <= 10) ? warningColor : normalColor;
    //}

    // Sau khi người chơi trả lời: hiện Reason
    public void ShowReason(QuestionAsset q)
    {
        if (q == null || reasonText == null) return;

        var sb = new StringBuilder();
        sb.AppendLine(q.IsLegal ? "✔ HỢP PHÁP" : "⚠ LỪA ĐẢO");

        if (q.reasons != null && q.reasons.Length > 0)
        {
            foreach (var line in q.reasons)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    sb.AppendLine("• " + line);
            }
        }

        reasonText.text = sb.ToString();
    }


    //// Hiển thị lý do và trạng thái cho câu trả lời
    //public void ShowReason(bool isLegalAnswer, string[] reasons)
    //{
    //    if (reasonText==null) return;

    //    var sb = new StringBuilder();
    //    sb.AppendLine(isLegalAnswer ? "✔ HỢP PHÁP" : "⚠ LỪA ĐẢO");

    //    if (reasons != null && reasons.Length > 0)
    //    {
    //        foreach (var r in reasons)
    //        {
    //            if (!string.IsNullOrWhiteSpace(r))
    //                sb.AppendLine("• " + r);
    //        }
    //    }

    //    reasonText.text = sb.ToString();
    //}

    // Cập nhật điểm
    //public void UpdateScore(int score)
    //{
    //    if (scoreText) scoreText.text = $"Điểm: {score}";
    //}

    public void UpdateProgress(int current, int total)
    {
        if (progressText) progressText.text = $"{current}/{total}";
    }

    // Cập nhật trạng thái có thể nhấn cho các nút
    public void SetButtonsInteractable(bool value)
    {
        if (legalButton) legalButton.interactable = value;
        if (scamButton) scamButton.interactable = value;
        
    }

    // Thông báo khi chiến thắng
    public void ShowGameWin()
    {
        if (descriptionText) descriptionText.text = "Chúc mừng! Bạn đã hoàn thành tất cả tình huống.";
        if (reasonText) reasonText.text = "";
    }

    // Thông báo khi thua
    public void ShowGameOver()
    {
        if (descriptionText) descriptionText.text = "Trò chơi kết thúc! Bạn đã trả lời sai quá nhiều.";
        if (reasonText) reasonText.text = "";
    }
}
