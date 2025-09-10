using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject panel;                // Bảng GameOver
    public GameObject winTextObject;        // Hiển thị khi thắng
    public GameObject loseTextObject;       // Hiển thị khi thua
    public Text completedCountText;         // Hiển thị số câu đã hoàn thành
    public Button retryButton;              // Nút chơi lại
    public Button mainMenuButton;           // Nút trở về menu

    void Awake()
    {
        panel.SetActive(false);

        retryButton.onClick.AddListener(RestartGame);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);

        if (winTextObject) winTextObject.SetActive(false);
        if (loseTextObject) loseTextObject.SetActive(false);
        if (completedCountText) completedCountText.text = "";
    }

    /// <summary>
    /// Hiện UI GameOver kèm trạng thái Win/Lose và số câu đã hoàn thành
    /// </summary>
    public void ShowGameOver(bool isWin, int correctCount, int totalQuestions)
    {
        panel.SetActive(true);  // Kích hoạt bảng Game Over
        Time.timeScale = 0f;    // Dừng game khi Game Over

        if (winTextObject) winTextObject.SetActive(isWin);  // Hiển thị win text nếu thắng
        if (loseTextObject) loseTextObject.SetActive(!isWin);  // Hiển thị lose text nếu thua

        if (completedCountText)
            completedCountText.text = $"Đúng: {correctCount} / {totalQuestions}";
    }





    void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
