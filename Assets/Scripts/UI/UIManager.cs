using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Text descriptionText;
    public Text progressText;
    public Text reasonText;
    public Text categoryText;
    public Text AnswerText;

    [Header("Buttons")]
    public Button legalButton;
    public Button scamButton;
    public Button skipButton;                 // <— thêm ở đây

    [System.Serializable]
    public class HeartPair
    {
        public Image full; public Image empty;
        public void SetAlive(bool alive) { if (full) full.gameObject.SetActive(alive); if (empty) empty.gameObject.SetActive(!alive); }
    }
    [Header("Hearts (Full + Empty)")]
    public HeartPair[] hearts;

    private void Start() { if (reasonText) reasonText.text = ""; }

    // UI → GameManager: gán sự kiện
    public void WireButtons(GameManager gm)
    {
        if (legalButton) { legalButton.onClick.RemoveAllListeners(); legalButton.onClick.AddListener(() => gm.PlayerAnswer(true)); }
        if (scamButton) { scamButton.onClick.RemoveAllListeners(); scamButton.onClick.AddListener(() => gm.PlayerAnswer(false)); }
        if (skipButton) { skipButton.onClick.RemoveAllListeners(); skipButton.onClick.AddListener(gm.SkipButton); }
    }

    public void ShowSkip(bool show)
    {
        if (skipButton) skipButton.gameObject.SetActive(show);
    }

    public void ShowQuestion(QuestionAsset q, int index, int total)
    {
        if (q == null) return;
        if (descriptionText) descriptionText.text = q.questionText ?? "";
        if (categoryText) categoryText.text = string.IsNullOrEmpty(q.category) ? "" : q.category;
        if (progressText) progressText.text = $"{index}/{total}";
        if (reasonText) reasonText.text = "";
        if (AnswerText) AnswerText.text = "Bảng lí do";
        SetButtonsInteractable(true);
    }

    public void ShowReason(QuestionAsset q)
    {
        if (q == null || reasonText == null) return;
        var sb = new StringBuilder();
        AnswerText.text= (q.IsLegal ? "✔ HỢP PHÁP" : "⚠ LỪA ĐẢO");
        if (q.reasons != null) foreach (var line in q.reasons)
                if (!string.IsNullOrWhiteSpace(line)) sb.AppendLine("• " + line);
        reasonText.text = sb.ToString();
    }

    public void HideReason() { if (reasonText) reasonText.text = ""; }
    public void UpdateProgress(int current, int total) { if (progressText) progressText.text = $"{current}/{total}"; }

    public void SetButtonsInteractable(bool value)
    { if (legalButton) legalButton.interactable = value; if (scamButton) scamButton.interactable = value; }

    public void ShowGameWin()
    { if (descriptionText) descriptionText.text = "Chúc mừng! Bạn đã hoàn thành tất cả tình huống."; if (reasonText) reasonText.text = ""; }

    public void ShowGameOver()
    { if (descriptionText) descriptionText.text = "Trò chơi kết thúc! Bạn đã trả lời sai quá nhiều."; if (reasonText) reasonText.text = ""; }

    public void UpdateHearts(int wrongCount, int maxWrong)
    {
        int count = hearts != null ? hearts.Length : 0;
        int livesRemaining = Mathf.Clamp(maxWrong - wrongCount, 0, count);
        for (int i = 0; i < count; i++)
        {
            bool activeSlot = i < maxWrong;
            if (hearts[i].full) hearts[i].full.transform.parent.gameObject.SetActive(activeSlot);
            if (hearts[i].empty) hearts[i].empty.transform.parent.gameObject.SetActive(activeSlot);
            hearts[i].SetAlive(i < livesRemaining);
        }
    }
}
