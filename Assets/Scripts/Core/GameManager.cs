using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    [Header("Managers")]
    public QuestionManager questionManager;
    public UIManager uiManager;
    public AudioManager audioManager; // optional
    public GameOverUI gameOverUI;     // Game over UI for displaying results

    [Header("Điểm số")]
    public int pointsOnMatch = 1;     // + điểm khi chọn trùng nhãn
    public int pointsOnMismatch = 1;  // - điểm khi chọn khác nhãn
    public int score = 0;

    [Header("Luật ‘sai quá 3’")]
    public int maxWrong = 3;          // NGƯỠNG: sai quá 3 (tức 4) thì thua
    private int wrongCount = 0;

    [Header("Thời gian & nhịp hiển thị")]
   // [SerializeField] private float reasonRevealDelay = 1.25f; // giữ bảng lí do trước khi sang câu
   // private bool isRevealing = false;                         // đang hiển thị lý do -> tạm dừng timer
    private bool isPlaying = false;

    private QuestionAsset currentQuestion;
    private float timeRemaining;

    public static GameManager Instance;

 HEAD
    void Start()
    {
        // Nếu trước đó có dùng Time.timeScale = 0 ở màn trước, đảm bảo bật lại:
        Time.timeScale = 1f;

        StartGame();
    }


    bool match=false;
    private int dem=0;
    private bool chosen=false;
    [SerializeField] GameObject skipButton;
    void Start() { StartGame(); }

    //void Update()
    //{
    //    if (!isPlaying || isRevealing) return;

    //    timeRemaining -= Time.deltaTime;
    //  //  uiManager.UpdateTimer(timeRemaining);

    //    if (timeRemaining <= 0f)
    //    {
    //        // Hết giờ: coi như CHỌN KHÁC NHÃN -> trừ điểm, tăng wrongCount
    //        OnTimeout();
    //    }
    //}
    private void Update()
    {
        skipButton.SetActive(chosen);
    }
 Skip
    public void StartGame()
    {
        wrongCount = 0;
        isPlaying = true;
 HEAD

        // Cập nhật trái tim ở UI theo số lần sai ban đầu (0)
        if (uiManager != null)
            uiManager.UpdateHearts(wrongCount, maxWrong);


        chosen=false;
Skip
        LoadNext();
    }

    void LoadNext()
    {
        if (!questionManager.HasNext())
        {
            isPlaying = false;
           
            uiManager.ShowGameWin();
            GameOver();
            audioManager?.PlayBGM();  // Play background music when the game wins
            return;
        }
        dem = 0;
        chosen = false;
        currentQuestion = questionManager.NextQuestion(); // Câu hỏi tiếp theo

        uiManager.ShowQuestion(currentQuestion, questionManager.CurrentIndex, questionManager.TotalRounds);
        uiManager.SetButtonsInteractable(true);
    }

    // Gọi từ 2 nút: HỢP PHÁP(true) / LỪA ĐẢO(false)
    public void PlayerAnswer(bool choseLegal)
    {
        if (!isPlaying || currentQuestion == null) return;
         match = (currentQuestion.IsLegal == choseLegal);
        
            dem++;
        
        if (dem == 1)
        {
            chosen = true;
        }
        // Luôn hiển thị NHÃN ĐÚNG của câu hỏi + lý do:
        uiManager.ShowReason(currentQuestion);

        CheckAfter(match);
       
    }

HEAD
    private IEnumerator NextAfter(bool match, float delay)
    {
        isRevealing = true;
        uiManager.SetButtonsInteractable(false);

        ApplyScore(match);

        if (!match)
        {
            wrongCount++;

            // Cập nhật trái tim ngay khi sai
            if (uiManager != null)
                uiManager.UpdateHearts(wrongCount, maxWrong);
        }

        // Giữ bảng lý do cho người chơi đọc
        yield return new WaitForSeconds(delay);

        // Kiểm tra ngưỡng sai: "sai QUÁ 3" => >= maxWrong

   


    public void SkipButton()
    {
        if(!chosen) return; 
        // Kiểm tra ngưỡng sai: "sai QUÁ 3" => > 3
 Skip
        if (wrongCount >= maxWrong)
        {
            GameOver();
            
        }
        else
        {
            LoadNext();
        }


    }

        private void CheckAfter(bool match)
    {
    
        uiManager.SetButtonsInteractable(false);
        
        ApplyScore(match);
        if (!match) wrongCount++;

    }

    private void ApplyScore(bool match)
    {
        if (match)
        {
            score += pointsOnMatch;
            audioManager?.PlayCorrect();  // Play correct sound when answer is correct
        }
        else
        {
            // score -= pointsOnMismatch;
            audioManager?.PlayWrong();   // Play wrong sound when answer is incorrect
        }
        // uiManager.UpdateScore(score);
    }

    private void GameOver()
    {
        isPlaying = false;

        // Dừng tương tác nút
        uiManager.SetButtonsInteractable(false);

        // Gọi Coroutine để delay rồi hiện GameOver UI
        StartCoroutine(ShowGameOverAfterDelay(3f));
    }

    private IEnumerator ShowGameOverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        bool isWin = score >= (questionManager.TotalRounds - maxWrong);
        if (score < 0) score = 0;

        gameOverUI.ShowGameOver(isWin, score, questionManager.TotalRounds);

        audioManager?.StopBGM();
        Time.timeScale = 0f;  // Dừng thời gian khi game over
    }
}
