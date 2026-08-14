using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WinPanelButtons : MonoBehaviour
{
    [Header("Buttons trong ButtonRow")]
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button replayButton;
    [SerializeField] private Button nextLevelButton;

    [Header("Scene Names (điền sau khi bạn tạo scene)")]
    [Tooltip("Để trống nếu chưa có scene Menu")]
    [SerializeField] private string mainMenuSceneName = "";

    [Tooltip("Để trống nếu chưa có scene Level tiếp theo. Nếu để trống sẽ tự lấy scene kế tiếp trong Build Settings.")]
    [SerializeField] private string nextLevelSceneName = "";

    // TODO: Star, Finish Time, Coin sẽ xử lý riêng ở đây sau
    // [SerializeField] private ??? starsHandler;
    // [SerializeField] private ??? finishTimeHandler;
    // [SerializeField] private ??? coinHandler;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        replayButton.onClick.AddListener(OnReplayClicked);
        nextLevelButton.onClick.AddListener(OnNextLevelClicked);
    }

    private void OnDestroy()
    {
        mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        replayButton.onClick.RemoveListener(OnReplayClicked);
        nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);
    }

    private void OnMainMenuClicked()
    {
        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogWarning("[WinPanelButtons] Chưa thiết lập Main Menu Scene.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnReplayClicked()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private void OnNextLevelClicked()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(nextLevelSceneName))
        {
            SceneManager.LoadScene(nextLevelSceneName);
            return;
        }

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.LogWarning("[WinPanelButtons] Chưa có Level tiếp theo trong Build Settings.");
        }
    }
}