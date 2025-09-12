using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Managers")]
    public QuestionManager questionManager;
    public UIManager uiManager;
    public AudioManager audioManager;
    public GameOverUI gameOverUI;

    [Header("Điểm số")]
    public int pointsOnMatch = 1;
    public int pointsOnMismatch = 1; // nếu muốn trừ điểm khi sai, mở trong ApplyScore
    public int score = 0;

    [Header("Luật ‘sai quá 3’")]
    public int maxWrong = 3;
    private int wrongCount = 0;

    // Trạng thái
    private bool isPlaying = false;
    private bool chosen = false;
    private QuestionAsset currentQuestion;
    private bool endSequenceRunning = false;
    [SerializeField] private float preMenuDelay = 5f;  // đợi 5s trước khi mở GameOver menu

    private void Start()
    {
        if (uiManager != null) uiManager.WireButtons(this); // UI gán sự kiện cho 3 nút
        StartGame();
    }

    public void StartGame()
    {
        Time.timeScale = 1f;       // khôi phục thời gian khi chơi lại
        endSequenceRunning = false;

        wrongCount = 0;
        score = 0;
        isPlaying = true;
        chosen = false;

        uiManager.UpdateHearts(wrongCount, maxWrong);
        uiManager.ShowSkip(false);
        uiManager.SetButtonsInteractable(true);

        LoadNext();
    }

    private void LoadNext()
    {
        if (!questionManager.HasNext())
        {
            // Hết câu -> thắng
            StartCoroutine(EndGameSequence(isWin: true));
            return;
        }

        chosen = false;
        uiManager.ShowSkip(false);

        currentQuestion = questionManager.NextQuestion();
        uiManager.HideReason();
        uiManager.ShowQuestion(currentQuestion, questionManager.CurrentIndex, questionManager.TotalRounds);
        uiManager.SetButtonsInteractable(true);
    }

    // Người chơi bấm HỢP PHÁP(true) hoặc LỪA ĐẢO(false)
    public void PlayerAnswer(bool choseLegal)
    {
        if (!isPlaying || currentQuestion == null || chosen || endSequenceRunning) return;

        bool match = (currentQuestion.IsLegal == choseLegal);
        chosen = true;

        uiManager.SetButtonsInteractable(false);
        uiManager.ShowReason(currentQuestion);

        ApplyScore(match);
        if (!match)
        {
            wrongCount++;
            uiManager.UpdateHearts(wrongCount, maxWrong);
        }

        // Chỉ sau khi đã chọn mới hiện Skip
        uiManager.ShowSkip(true);

        if (wrongCount >= maxWrong)
        {
            // Thua
            StartCoroutine(EndGameSequence(isWin: false));
        }
    }

    // Bấm Skip chỉ khi đã trả lời
    public void SkipButton()
    {
        if (!isPlaying || !chosen || endSequenceRunning) return;
        LoadNext();
    }

    private void ApplyScore(bool match)
    {
        if (match)
        {
            score += pointsOnMatch;
            audioManager?.PlayCorrect();
        }
        else
        {
            // score -= pointsOnMismatch; // nếu muốn trừ điểm khi sai
            audioManager?.PlayWrong();
        }
    }

    // Đóng băng ngay, chờ 0.5s (theo thời gian thực), rồi mở màn hình GameOverUI
    private IEnumerator EndGameSequence(bool isWin)
    {
        if (endSequenceRunning) yield break;
        endSequenceRunning = true;

        isPlaying = false;
        chosen = true;
        uiManager.SetButtonsInteractable(false);
        uiManager.ShowSkip(false);
        audioManager?.StopBGM();

        // 1) Hiển thị thông báo thắng/thua trên UIManager
        if (isWin) uiManager.ShowGameWin();
        else uiManager.ShowGameOver();

        // 2) Đóng băng màn hình và chờ 5s theo thời gian thực
        Time.timeScale = 0f;
        yield return new WaitForEndOfFrame();                 // đảm bảo frame UI được vẽ ra
        yield return new WaitForSecondsRealtime(preMenuDelay);

        // 3) Mở menu Game Over (Retry/MainMenu)
        int total = questionManager.TotalRounds;
        int correct = Mathf.Clamp(score, 0, total);           // pointsOnMatch = 1 => score = số câu đúng
        gameOverUI.ShowGameOver(isWin, correct, total);
    }

}
