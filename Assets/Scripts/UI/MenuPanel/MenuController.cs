using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject panel;         // Chứa Home/Settings/Replay
    [SerializeField] private GameObject muteButton;
    [SerializeField] private GameObject unMuteButton; // Panel âm lượng, đồ họa...
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject resumeButton;

    [Header("Scene Settings")]
    [SerializeField] private string homeSceneName = "Home";

    private bool isPaused = false;
    private bool isMenuOpen = false;

    private void Start()
    {
        if (menu != null) menu.SetActive(false);
        isMenuOpen = false;

        if (panel != null) panel.SetActive(false);
        if (muteButton != null) muteButton.SetActive(false);
        if (unMuteButton != null) unMuteButton.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(true);
        if (resumeButton != null) resumeButton.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnEscPressed();
        }
    }

    // ----- MENU TOGGLE -----
    public void OnMenuButton()
    {
        isMenuOpen = !isMenuOpen;

        if (isMenuOpen)
        {
            if (menu != null) menu.SetActive(true);
            if (panel != null) panel.SetActive(true);
            
            Time.timeScale = 0f;
            isPaused = true;
            if (muteButton != null) muteButton.SetActive(true);
            if (unMuteButton != null) unMuteButton.SetActive(true);
            if (pauseButton != null) pauseButton.SetActive(true);
            if (resumeButton != null) resumeButton.SetActive(false);

        }
        else
        {
            if (menu != null) menu.SetActive(false);

            Time.timeScale = 1f;
            isPaused = false;
        }
    }

    // ----- HOME -----
    public void OnHomeButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(homeSceneName);
    }

    // ----- MUTE -----
    public bool isMuted = false;
    public void OnMuteButton()
    {
        isMuted = !isMuted;

        AudioListener.volume = isMuted ? 0f : 1f;

        PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
        if (muteButton != null) muteButton.SetActive(false);
        if (unMuteButton != null) unMuteButton.SetActive(true);
        Debug.Log(isMuted ? "Đã tắt âm thanh" : "Đã bật âm thanh");
    }
    public void OnUnMuteButton()
    {
        isMuted = !isMuted;
        AudioListener.volume = isMuted ? 0f : 1f;
        PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
        if (muteButton != null) muteButton.SetActive(true);
        if (unMuteButton != null) unMuteButton.SetActive(false);
        Debug.Log(isMuted ? "Đã tắt âm thanh" : "Đã bật âm thanh");
    }



    // ----- REPLAY -----
    public void OnReplayButton()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    // ----- PAUSE -----
    public void OnPauseButton()
    {
        Time.timeScale = 0f;
        isPaused = true;

        if (pauseButton != null) pauseButton.SetActive(false);
        if (resumeButton != null) resumeButton.SetActive(true);
    }

    // ----- RESUME -----
    public void OnResumeButton()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (pauseButton != null) pauseButton.SetActive(true);
        if (resumeButton != null) resumeButton.SetActive(false);
    }

    // ----- ESC -----
    public void OnEscPressed()
    {
        if (isMenuOpen)
        {
           

            if (menu != null) menu.SetActive(false);
            isMenuOpen = false;

            Time.timeScale = 1f;
            isPaused = false;
        }
    }
}