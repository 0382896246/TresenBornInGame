using UnityEngine;
using UnityEngine.UI;

public class HelpPanelController : MonoBehaviour
{
    [Header("Pages")]
    public GameObject[] pages;
    private int currentPage = 0;

    [Header("Buttons")]
    public Button nextButton;
    public Button previousButton;
    public Button closeButton;

    [Header("UI Text")]
    public Text pageInfoText;

    private bool isInitialized = false;

    public void Init()  // GỌI TỪ MAINMENU
    {
        if (isInitialized) return;

        nextButton.onClick.AddListener(NextPage);
        previousButton.onClick.AddListener(PreviousPage);
        closeButton.onClick.AddListener(CloseHelp);
        isInitialized = true;
    }

    public void OpenHelp()
    {
        Init(); // Đảm bảo đã gán listener
        ShowPage(0);
        gameObject.SetActive(true);
        Time.timeScale = 0f; // Pause game
    }

    public void CloseHelp()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f; // Resume game
    }

    void ShowPage(int index)
    {
        for (int i = 0; i < pages.Length; i++)
            pages[i].SetActive(i == index);

        currentPage = index;
        UpdatePageInfo();
    }

    void NextPage()
    {
        if (currentPage < pages.Length - 1)
            ShowPage(currentPage + 1);
    }

    void PreviousPage()
    {
        if (currentPage > 0)
            ShowPage(currentPage - 1);
    }

    void UpdatePageInfo()
    {
        if (pageInfoText != null)
        {
            pageInfoText.text = $"Trang {currentPage + 1} / {pages.Length}";
        }
    }
}
