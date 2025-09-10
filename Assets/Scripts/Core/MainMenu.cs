using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Credit Panel")]
    public CreditScroller creditScroller;  // Script CreditScroller

    [Header("Help Panel")]
    public HelpPanelController helpPanel;  // Script HelpPanelController

    [Header("Zoom UI")]
    public StartMenuUIZoom zoomUI;         // Script Zoom UI khi vào game

    public void StartGame()
    {
        Time.timeScale = 1f;

        if (zoomUI != null)
        {
            zoomUI.StartZoomEffect();  // Gọi hiệu ứng zoom rồi load scene
        }
        else
        {
            SceneManager.LoadScene("Scene_ingame");  // Nếu không gán zoomUI
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quit!");
        Application.Quit();  // Chỉ hoạt động khi build
    }

    public void ShowCredits()
    {
        if (creditScroller != null)
        {
            creditScroller.OpenCredit();  // Hiện Credit + bắt đầu cuộn
        }
    }

    public void CloseCredits()
    {
        if (creditScroller != null)
        {
            creditScroller.CloseCredit();  // Tắt Credit + dừng cuộn
        }
    }

    public void ShowHelp()
    {
        Debug.Log("ShowHelp() được gọi");

        if (helpPanel != null)
        {
            helpPanel.OpenHelp();  // Hiện bảng hướng dẫn + pause game
        }
        else
        {
            Debug.LogWarning("HelpPanel chưa được gán trong Inspector!");
        }
    }

    public void CloseHelp()
    {
        if (helpPanel != null)
        {
            helpPanel.CloseHelp();  // Tắt bảng hướng dẫn + resume game
        }
    }
}
