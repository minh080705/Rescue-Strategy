using UnityEngine;
using TMPro;

public class FPSSettingsManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField fpsInputField;

    [Header("Giới hạn cho phép")]
    public int minFPS = 15;
    public int maxFPS = 240;

    private const string FPS_PREF_KEY = "TargetFPS";
    private const int DEFAULT_FPS = 60;

    void Awake()
    {
        // Bắt buộc tắt VSync thì targetFrameRate mới có tác dụng
        QualitySettings.vSyncCount = 0;

        int savedFPS = PlayerPrefs.GetInt(FPS_PREF_KEY, DEFAULT_FPS);
        ApplyFPS(savedFPS);
    }

    void Start()
    {
        if (fpsInputField != null)
        {
            // Hiển thị giá trị hiện tại khi mở Settings
            fpsInputField.text = Application.targetFrameRate.ToString();

            // Chỉ cho nhập số
            fpsInputField.contentType = TMP_InputField.ContentType.IntegerNumber;

            // Xử lý khi người chơi nhấn Enter hoặc click ra ngoài ô
            fpsInputField.onEndEdit.AddListener(OnFPSInputSubmit);
        }
    }

    private void OnFPSInputSubmit(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            // Nếu để trống thì trả về giá trị mặc định
            fpsInputField.text = Application.targetFrameRate.ToString();
            return;
        }

        if (int.TryParse(input, out int fps))
        {
            fps = Mathf.Clamp(fps, minFPS, maxFPS);
            ApplyFPS(fps);
            fpsInputField.text = fps.ToString(); // cập nhật lại nếu bị clamp
        }
        else
        {
            // Nhập sai định dạng -> reset về giá trị cũ
            fpsInputField.text = Application.targetFrameRate.ToString();
        }
    }

    private void ApplyFPS(int fps)
    {
        Application.targetFrameRate = fps;
        PlayerPrefs.SetInt(FPS_PREF_KEY, fps);
        PlayerPrefs.Save();

        Debug.Log($"Target FPS set to: {fps}");
    }
}