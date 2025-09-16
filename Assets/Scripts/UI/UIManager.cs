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
        public void SetAlive(bool alive)
        {
            if (full) full.gameObject.SetActive(alive);
            if (empty) empty.gameObject.SetActive(!alive);
        }
    }

    [Header("Hearts (Full + Empty)")]
    public HeartPair[] hearts;

    // ======= DAMAGE FLASH =======
    [Header("Damage Flash (chớp khi thua hoặc trả lời sai)")]
    [Tooltip("Ảnh overlay full-screen (UI > Image) để chớp khi mất máu)")]
    public Image damageFlash;
    [Tooltip("Độ trong suốt tối đa khi chớp")]
    [Range(0f, 1f)] public float damageMaxAlpha = 0.55f;
    [Tooltip("Tổng thời lượng chớp (giây)")]
    public float damageFlashDuration = 0.35f;
    [Tooltip("Có dùng UnscaledTime (bỏ qua Time.timeScale) để flash vẫn chạy khi pause)")]
    public bool useUnscaledForFlash = true;

    private Coroutine damageRoutine;

    private void Start()
    {
        if (reasonText) reasonText.text = "";
        // Đảm bảo damageFlash tắt alpha lúc đầu
        if (damageFlash)
        {
            var c = damageFlash.color;
            c.a = 0f;
            damageFlash.color = c;
            // không cần tắt GameObject để tránh layout giật, chỉ set alpha = 0
            damageFlash.raycastTarget = false; // tránh chặn bấm UI
        }
    }

    // UI → GameManager: gán sự kiện
    public void WireButtons(GameManager gm)
    {
        if (legalButton)
        {
            legalButton.onClick.RemoveAllListeners();
            legalButton.onClick.AddListener(() => gm.PlayerAnswer(true));
        }
        if (scamButton)
        {
            scamButton.onClick.RemoveAllListeners();
            scamButton.onClick.AddListener(() => gm.PlayerAnswer(false));
        }
        if (skipButton)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(gm.SkipButton);
        }
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
        if (AnswerText) AnswerText.text = (q.IsLegal ? "✔ HỢP PHÁP" : "⚠ LỪA ĐẢO");
        if (q.reasons != null)
        {
            foreach (var line in q.reasons)
                if (!string.IsNullOrWhiteSpace(line)) sb.AppendLine("• " + line);
        }
        reasonText.text = sb.ToString();
    }

    public void HideReason() { if (reasonText) reasonText.text = ""; }
    public void UpdateProgress(int current, int total) { if (progressText) progressText.text = $"{current}/{total}"; }

    public void SetButtonsInteractable(bool value)
    {
        if (legalButton) legalButton.interactable = value;
        if (scamButton) scamButton.interactable = value;
    }

    public void ShowGameWin()
    {
        if (descriptionText) descriptionText.text = "Chúc mừng! Bạn đã hoàn thành tất cả tình huống.";
        if (reasonText) reasonText.text = "";
    }

    public void ShowGameOver()
    {
        if (descriptionText) descriptionText.text = "Trò chơi kết thúc! Bạn đã trả lời sai quá nhiều.";
        if (reasonText) reasonText.text = "";
    }

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

    // ======= API: gọi để chớp ảnh mất máu =======
    public void FlashDamage()
    {
        if (!damageFlash) return;
        if (damageRoutine != null) StopCoroutine(damageRoutine);
        damageRoutine = StartCoroutine(CoFlashDamage());
    }

    private System.Collections.IEnumerator CoFlashDamage()
    {
        // Flash theo 2 pha: nhanh lên alpha max, rồi hạ dần về 0
        // Tổng thời lượng = damageFlashDuration (mặc định 0.35s)
        float upTime = damageFlashDuration * 0.25f;    // 25% thời gian tăng
        float downTime = damageFlashDuration * 0.75f;  // 75% thời gian giảm

        float t = 0f;
        Color c = damageFlash.color;

        // Pha lên
        while (t < upTime)
        {
            t += useUnscaledForFlash ? Time.unscaledDeltaTime : Time.deltaTime;
            float k = Mathf.Clamp01(t / Mathf.Max(0.0001f, upTime));
            c.a = Mathf.Lerp(0f, damageMaxAlpha, k);
            damageFlash.color = c;
            yield return null;
        }

        // Pha xuống
        t = 0f;
        while (t < downTime)
        {
            t += useUnscaledForFlash ? Time.unscaledDeltaTime : Time.deltaTime;
            float k = Mathf.Clamp01(t / Mathf.Max(0.0001f, downTime));
            c.a = Mathf.Lerp(damageMaxAlpha, 0f, k);
            damageFlash.color = c;
            yield return null;
        }

        // đảm bảo tắt alpha
        c.a = 0f;
        damageFlash.color = c;
        damageRoutine = null;
    }
}
