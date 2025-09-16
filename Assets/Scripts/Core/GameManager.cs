using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    [Header("Managers")]
    public QuestionManager questionManager;
    public UIManager uiManager;
    public AudioManager audioManager;
    public GameOverUI gameOverUI;

    [Header("Điểm số")]
    public int pointsOnMatch = 1;
    public int pointsOnMismatch = 1; // nếu muốn trừ điểm khi sai thì dùng trong ApplyScore
    public int score = 0;

    [Header("Luật ‘sai quá 3’")]
    public int maxWrong = 3;
    private int wrongCount = 0;

    // Trạng thái
    private bool isPlaying = false;          // đang ở vòng chơi (còn câu hỏi)
    private bool chosen = false;             // đã chọn đáp án cho câu hiện tại
    private QuestionAsset currentQuestion;
    private bool endSequenceRunning = false; // đang mở GameOver (tránh gọi trùng)

    // Cờ “đợi mở Game Over khi bấm Skip”
    private bool pendingGameOver = false;    // true khi đã thua nhưng chưa bấm Skip để mở bảng

    [SerializeField] private float preMenuDelay = 0.5f;   // chờ realtime trước khi mở menu GameOver
    [SerializeField] private bool startOnAwake = true;    // false = chờ Intro gọi StartGameFromIntro

    void Awake()
    {
        // Luôn khôi phục thời gian để không bị treo từ scene trước
        Time.timeScale = 1f;

        // Đảm bảo có EventSystem
        if (EventSystem.current == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(es);
        }
    }

    void Start()
    {
        // Chuẩn bị UI
        if (uiManager != null)
        {
            uiManager.WireButtons(this);
            uiManager.ShowSkip(false);
            uiManager.SetButtonsInteractable(false);
            uiManager.HideReason();
            uiManager.UpdateHearts(0, maxWrong);
        }

        // Nhạc nền tuỳ ý
        audioManager?.PlayBGM();

        if (startOnAwake)
        {
            StartGame();
        }
    }

    // Được Intro gọi khi kết thúc Intro
    public void StartGameFromIntro() => StartGame();

    public void StartGame()
    {
        StopAllCoroutines();
        Time.timeScale = 1f;

        endSequenceRunning = false;
        isPlaying = true;
        chosen = false;
        wrongCount = 0;
        score = 0;
        pendingGameOver = false;

        uiManager?.UpdateHearts(wrongCount, maxWrong);
        uiManager?.ShowSkip(false);
        uiManager?.HideReason();
        uiManager?.SetButtonsInteractable(true);

        LoadNext();
    }

    private void LoadNext()
    {
        if (!questionManager || !uiManager)
        {
            Debug.LogError("[GameManager] Thiếu QuestionManager/UIManager.");
            return;
        }

        if (!questionManager.HasNext())
        {
            // Hết câu => thắng (mở GameOver luôn như cũ)
            StartCoroutine(EndGameSequence(true));
            return;
        }

        chosen = false;
        uiManager.ShowSkip(false);
        pendingGameOver = false;

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
        uiManager.ShowReason(currentQuestion); // Hiện phần giải thích/đáp án
        ApplyScore(match);

        if (!match)
        {
            wrongCount++;
            uiManager.UpdateHearts(wrongCount, maxWrong);

            // >>> CHỚP ẢNH "MẤT MÁU" KHI TRẢ LỜI SAI
            uiManager?.FlashDamage();
        }

        // Sau khi trả lời mới hiện nút Skip
        uiManager.ShowSkip(true);

        // Nếu vượt ngưỡng sai: KHÔNG mở gameover ngay, chờ bấm Skip
        if (wrongCount >= maxWrong)
        {
            pendingGameOver = true;
        }
    }

    public void SkipButton()
    {
        if (endSequenceRunning) return;

        // Chỉ cho Skip sau khi đã chọn xong đáp án câu hiện tại
        if (!isPlaying || !chosen) return;

        // Nếu đã thua và chờ Skip để mở GameOver
        if (pendingGameOver)
        {
            StartCoroutine(EndGameSequence(false)); // thua
            pendingGameOver = false;
            return;
        }

        // Chưa thua -> sang câu tiếp theo
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
            // score -= pointsOnMismatch; // bật nếu muốn trừ điểm khi sai
            audioManager?.PlayWrong();
        }
    }

    // Đóng băng game rồi mở GameOver/Win sau một nhịp realtime
    private IEnumerator EndGameSequence(bool isWin)
    {
        if (endSequenceRunning) yield break;
        endSequenceRunning = true;

        isPlaying = false;

        uiManager?.SetButtonsInteractable(false);
        uiManager?.ShowSkip(false);

        if (isWin) uiManager?.ShowGameWin();
        else uiManager?.ShowGameOver();

        audioManager?.StopBGM();

        // Pause gameplay nhưng vẫn cho UI chạy mượt
        Time.timeScale = 0f;
        yield return new WaitForEndOfFrame();
        if (preMenuDelay > 0f)
            yield return new WaitForSecondsRealtime(preMenuDelay);

        int total = questionManager ? questionManager.TotalRounds : 0;
        int correct = Mathf.Clamp(score, 0, total); // nếu mỗi câu đúng +1

        gameOverUI?.ShowGameOver(isWin, correct, total);
    }

    void OnDestroy()
    {
        // Nếu scene bị unload khi đang pause, mở lại để không mang theo Time.timeScale=0
        if (Time.timeScale == 0f) Time.timeScale = 1f;
    }
}
