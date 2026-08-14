// Assets/Scripts/Managers/GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using TMPro; // dùng TextMeshPro để hiện thời gian, xóa nếu không cần

public class GameManager : MonoBehaviour
{
    [Header("Dialog Panels")]
    [SerializeField] private GameObject tutorialDialog;
    public static GameManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Level Timer")]
    public float levelTime = 60f;      // thời gian giới hạn của màn (giây)
    public TMP_Text timeText;          // optional, kéo thả UI text để hiện đếm ngược
    private float timeRemaining;
    private bool timerRunning = false;

    public float TimeRemaining => timeRemaining; // để script khác đọc (vd: màn hình kết quả)

    [Header("Events")]
    public UnityEvent onWin;   // WinResultDisplay và các script khác lắng nghe

    private bool gameOver = false;

    void Awake()
    {
        losePanel.SetActive(false);
        winPanel.SetActive(false);

        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        StartTimer();
        AudioManager.Instance.Play("BackgroundMusic");
        tutorialDialog.SetActive(true);
    }

    void Update()
    {
        if (!timerRunning || gameOver) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            timerRunning = false;
            TriggerLose();
        }

        UpdateTimeUI();
    }

    public void StartTimer()
    {
        timeRemaining = levelTime;
        timerRunning = true;
        UpdateTimeUI();
    }

    void UpdateTimeUI()
    {
        if (timeText == null) return;

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timeText.text = $"{minutes:00}:{seconds:00}";
    }

    public void TriggerWin()
    {
        if (gameOver) return;
        gameOver = true;
        timerRunning = false;
        Time.timeScale = 0f;
        winPanel?.SetActive(true);
        AudioManager.Instance?.Play("Win");
        Debug.Log("[GameManager] WIN!");
        onWin?.Invoke();
    }

    public void TriggerLose()
    {
        if (gameOver) return;
        gameOver = true;
        timerRunning = false;
        Time.timeScale = 0f;
        losePanel?.SetActive(true);
        AudioManager.Instance?.Play("LoseSound");
        Debug.Log("[GameManager] LOSE!");
    }

    public void Restart()
    {
        gameOver = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}