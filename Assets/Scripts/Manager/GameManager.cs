// Assets/Scripts/Managers/GameManager.cs

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject winPanel;
    public GameObject losePanel;

    private bool gameOver = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void TriggerWin()
    {
        if (gameOver) return;
        gameOver       = true;
        Time.timeScale = 0f;
        winPanel?.SetActive(true);
        Debug.Log("[GameManager] WIN!");
    }

    public void TriggerLose()
    {
        if (gameOver) return;
        gameOver       = true;
        Time.timeScale = 0f;
        losePanel?.SetActive(true);
        Debug.Log("[GameManager] LOSE!");
    }

    public void Restart()
    {
        gameOver       = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}