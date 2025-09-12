using UnityEngine;
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

    bool match=false;
    private int dem=0;
    private bool chosen=false;
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

    public void StartGame()
    {
        
        wrongCount = 0;
        isPlaying = true;
        chosen=false;
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

        // Đảm bảo không gọi LoadNext() khi chưa kết thúc việc hiển thị câu hỏi
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

   


    public void SkipButton()
    {
        if(!chosen) return; 
        // Kiểm tra ngưỡng sai: "sai QUÁ 3" => > 3
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
      //  uiManager.UpdateScore(score);
    }

    private void GameOver()
    {
        isPlaying = false;

        // Gọi Coroutine để delay 5 giây trước khi hiển thị GameOver UI
        StartCoroutine(ShowGameOverAfterDelay(3f));
    }

    private IEnumerator ShowGameOverAfterDelay(float delay)
    {
        // Đợi trong thời gian delay
        yield return new WaitForSeconds(delay);

        // Hiển thị Game Over UI sau khi delay
        bool isWin = score >= (questionManager.TotalRounds - maxWrong);  // Ví dụ: game thắng nếu điểm >= tổng câu hỏi - maxWrong
        if(score < 0) score = 0;
        gameOverUI.ShowGameOver(isWin, score, questionManager.TotalRounds); // Hiển thị Game Over UI

        audioManager?.StopBGM();   // Dừng nhạc nền khi game kết thúc
        uiManager.SetButtonsInteractable(false);  // Tắt các nút khi game over
        Time.timeScale = 0f;  // Dừng thời gian khi game over
    }




}
