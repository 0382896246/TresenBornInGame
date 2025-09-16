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
    private bool isPlaying = false;
    private bool chosen = false;
    private QuestionAsset currentQuestion;
    private bool endSequenceRunning = false;

    [SerializeField] private float preMenuDelay = 3f;   // chờ realtime trước khi mở menu GameOver
    private bool startOnAwake = true; // false = chờ Intro gọi StartGameFromIntro

    void Awake()
    {
        // Luôn khôi phục thời gian để không bị treo từ scene trước
        Time.timeScale = 1f;

        // Đảm bảo luôn có EventSystem
        if (EventSystem.current == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(es); // an toàn nếu bạn đổi scene
        }
    }

    void Start()
    {
        // Chuẩn bị UI
        if (uiManager != null)
        {
            uiManager.WireButtons(this);           // nếu bạn có hàm gán sự kiện
           uiManager.ShowSkip(false);
            uiManager.SetButtonsInteractable(false);
            uiManager.HideReason();
            uiManager.UpdateHearts(0, maxWrong);
        }

        if (startOnAwake)
        {
            StartGame();
        }
        else
        {
            // Cho nhạc nền chạy nếu muốn ngay khi vào scene
            audioManager?.PlayBGM();
        }
    }

    // Được Intro gọi khi kết thúc Intro (hoặc gọi thẳng nếu startOnAwake = true)
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
            // Hết câu => thắng
            StartCoroutine(EndGameSequence(true));
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

        // Sau khi trả lời mới hiện nút Skip
        uiManager.ShowSkip(true);

        if (wrongCount >= maxWrong)
        {
            StartCoroutine(EndGameSequence(false)); // thua
        }
    }

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
            // score -= pointsOnMismatch; // mở nếu muốn trừ điểm
            audioManager?.PlayWrong();
        }
    }

    // Đóng băng game rồi mở GameOver sau một khoảng realtime (để người chơi kịp đọc)
    private IEnumerator EndGameSequence(bool isWin)
    {
        if (endSequenceRunning) yield break;
        endSequenceRunning = true;

        isPlaying = false;
        chosen = true;

        uiManager?.SetButtonsInteractable(false);
        uiManager?.ShowSkip(false);

        if (isWin) uiManager?.ShowGameWin();
        else uiManager?.ShowGameOver();

        audioManager?.StopBGM();

        // Dừng game (timeScale=0) nhưng chờ bằng thời gian thực
        Time.timeScale = 0f;
        yield return new WaitForEndOfFrame();
        yield return new WaitForSecondsRealtime(preMenuDelay);

        int total = questionManager ? questionManager.TotalRounds : 0;
        int correct = Mathf.Clamp(score, 0, total); // pointsOnMatch = 1 => số câu đúng

        gameOverUI?.ShowGameOver(isWin, correct, total);
    }

    void OnDestroy()
    {
        // Nếu scene bị unload khi đang pause, mở lại để không "mang theo" timeScale=0
        if (Time.timeScale == 0f) Time.timeScale = 1f;
    }
}
