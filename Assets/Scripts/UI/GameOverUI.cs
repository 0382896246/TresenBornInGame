using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject panel;                // Root của panel GameOver (đặt là GameObject cha)
    public GameObject winTextObject;        // Text/Group hiển thị khi thắng
    public GameObject loseTextObject;       // Text/Group hiển thị khi thua
    public Text completedCountText;         // "Đúng: x / y"
    public Button retryButton;              // Nút Chơi lại
    public Button mainMenuButton;           // Nút Về menu (nếu có)

    void Awake()
    {
        // Luôn ẩn panel khi vào scene để không chặn raycast
        if (panel) panel.SetActive(false);

        if (retryButton) retryButton.onClick.AddListener(RestartGame);
        if (mainMenuButton) mainMenuButton.onClick.AddListener(ReturnToMainMenu);

        if (winTextObject) winTextObject.SetActive(false);
        if (loseTextObject) loseTextObject.SetActive(false);
        if (completedCountText) completedCountText.text = "";
    }

    /// <summary>Hiện UI GameOver kèm trạng thái Win/Lose và số câu đã hoàn thành.</summary>
    public void ShowGameOver(bool isWin, int correctCount, int totalQuestions)
    {
        if (winTextObject) winTextObject.SetActive(isWin);
        if (loseTextObject) loseTextObject.SetActive(!isWin);

        if (completedCountText)
            completedCountText.text = $"Đúng: {correctCount} / {totalQuestions}";

        if (panel) panel.SetActive(true);

        // Dừng game khi Game Over
        Time.timeScale = 0f;
    }

    void RestartGame()
    {
        // Trả thời gian trước khi load lại
        Time.timeScale = 1f;

        if (panel) panel.SetActive(false); // tránh chặn input khung hình cuối
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        if (panel) panel.SetActive(false);
        SceneManager.LoadScene("MainMenu");
    }
}
