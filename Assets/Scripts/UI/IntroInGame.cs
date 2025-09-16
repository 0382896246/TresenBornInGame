using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroInGame : MonoBehaviour
{
    [Header("References in this scene")]
    public PlayableDirector director;     // để trống cũng được, script sẽ tự tìm trong con
    public GameObject startMenuRoot;      // panel màn hình Start (trong Canvas)
    private GameObject hudRoot;            // HUD khi chơi (nếu có, có thể để trống)
    private GameManager gameManager;       // để gọi StartGameFromIntro nếu muốn

    [Header("Behaviour")]
    public bool showStartMenu = true;     // true = hiện menu Start sau intro
    public bool autoStartGame = false;    // true = tự vào game, gọi StartGameFromIntro()
    public float postDelay = 0.5f;        // chờ 0.5s sau khi timeline dừng

    void Reset() { director = GetComponentInChildren<PlayableDirector>(true); }

    void Awake()
    {
        if (startMenuRoot) startMenuRoot.SetActive(false); // ẩn UI Start lúc đầu
        if (hudRoot) hudRoot.SetActive(false);       // ẩn HUD nếu có
    }

    void OnEnable()
    {
        if (!director) director = GetComponentInChildren<PlayableDirector>(true);
        if (director)
        {
            director.stopped += OnDirectorStopped;
            if (director.state != PlayState.Playing) director.Play(); // phát intro
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

    void OnDirectorStopped(PlayableDirector d) { StartCoroutine(AfterIntro()); }

    IEnumerator AfterIntro()
    {
        yield return new WaitForSeconds(postDelay);
        FinishIntro();
    }

    void FinishIntro()
    {
        if (showStartMenu && startMenuRoot) startMenuRoot.SetActive(true);
        if (!showStartMenu && hudRoot) hudRoot.SetActive(true);

        if (autoStartGame) gameManager?.StartGameFromIntro();

        Destroy(gameObject); // huỷ khối TimeLine (Intro) cho sạch
    }
}
