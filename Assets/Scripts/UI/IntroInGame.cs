using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroInGame : MonoBehaviour
{
    [Header("References in this scene")]
    public PlayableDirector director;     // có thể để trống, script sẽ tự tìm trong con
    public GameObject startMenuRoot;      // panel màn hình Start (trong Canvas)
    private GameObject hudRoot;           // HUD khi chơi (nếu có, có thể để trống)
    private GameManager gameManager;      // để gọi StartGameFromIntro nếu muốn

    [Header("Behaviour")]
    public bool showStartMenu = true;     // true = hiện menu Start sau intro
    public bool autoStartGame = false;    // true = tự vào game, gọi StartGameFromIntro()
    public float postDelay = 0.5f;        // chờ 0.5s sau khi timeline dừng

    [Header("Skip by Double Click/Tap")]
    public bool allowDoubleClickSkip = true;      // Bật/tắt tính năng nhấn 2 lần để skip
    public float doubleClickMaxDelay = 0.35f;     // Khoảng thời gian tối đa giữa 2 lần nhấn (giây)

    // internal
    private bool finished;                        // tránh finish 2 lần
    private float lastTapTime = -999f;            // thời điểm lần nhấn gần nhất
    private int tapCount = 0;                     // đếm số lần nhấn trong khoảng cửa sổ

    void Reset()
    {
        director = GetComponentInChildren<PlayableDirector>(true);
    }

    void Awake()
    {
        if (startMenuRoot) startMenuRoot.SetActive(false); // ẩn UI Start lúc đầu
        if (hudRoot) hudRoot.SetActive(false);             // ẩn HUD nếu có
    }

    void OnEnable()
    {
        if (!director) director = GetComponentInChildren<PlayableDirector>(true);

        if (director)
        {
            director.stopped += OnDirectorStopped;
            if (director.state != PlayState.Playing)
                director.Play(); // phát intro
        }
        else
        {
            FinishIntro(); // fallback nếu không tìm thấy director
        }
    }

    void OnDisable()
    {
        if (director) director.stopped -= OnDirectorStopped;
    }

    void Update()
    {
        if (!allowDoubleClickSkip || finished) return;

        // Nhận sự kiện nhấn: chuột trái hoặc chạm (Touch)
        bool pressedThisFrame = false;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        if (Input.GetMouseButtonDown(0)) pressedThisFrame = true;
#endif

        // Mobile (và cả editor có mô phỏng touch)
        if (Input.touchCount > 0)
        {
            // Đếm 1 lần khi có bất kỳ ngón nào vừa chạm
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    pressedThisFrame = true;
                    break;
                }
            }
        }

        if (pressedThisFrame)
        {
            var now = Time.unscaledTime; // dùng unscaled để không phụ thuộc TimeScale
            if (now - lastTapTime <= doubleClickMaxDelay)
            {
                tapCount++;
            }
            else
            {
                tapCount = 1; // bắt đầu chuỗi nhấn mới
            }

            lastTapTime = now;

            if (tapCount >= 2)
            {
                // Đã double click/tap -> skip intro
                SkipIntro();
                // reset tránh lặp
                tapCount = 0;
                lastTapTime = -999f;
            }
        }

        // Nếu quá thời gian cửa sổ thì reset đếm
        if (tapCount > 0 && (Time.unscaledTime - lastTapTime) > doubleClickMaxDelay)
        {
            tapCount = 0;
        }
    }

    void OnDirectorStopped(PlayableDirector d)
    {
        StartCoroutine(AfterIntro());
    }

    IEnumerator AfterIntro()
    {
        yield return new WaitForSeconds(postDelay);
        FinishIntro();
    }

    /// <summary>
    /// Gọi khi muốn skip ngay lập tức do người chơi nhấn 2 lần.
    /// </summary>
    void SkipIntro()
    {
        if (finished) return;

        // Nếu timeline đang chạy: nhảy tới cuối & dừng để đảm bảo OnDirectorStopped không gây side-effect
        if (director && director.state == PlayState.Playing)
        {
            // Tuỳ ý: có thể jump tới cuối để đảm bảo evaluate
            director.time = director.duration;
            director.Evaluate();
            // Dừng để giải phóng
            director.Stop();
        }

        // Bỏ qua chờ postDelay, kết thúc luôn
        FinishIntro();
    }

    void FinishIntro()
    {
        if (finished) return;
        finished = true;

        if (showStartMenu && startMenuRoot) startMenuRoot.SetActive(true);
        if (!showStartMenu && hudRoot) hudRoot.SetActive(true);

        if (autoStartGame) gameManager?.StartGameFromIntro();

        // Huỷ khối Intro cho sạch
        Destroy(gameObject);
    }
}
