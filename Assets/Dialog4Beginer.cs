using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Dialog4Beginer : MonoBehaviour
{
    [SerializeField] private Button buttonNext;
    [SerializeField] private TextMeshProUGUI textDialog;
    [SerializeField] private Image textBox;

    private readonly List<string> dialogLines = new List<string>
    {
        "Chào mừng bạn đã trở lại!",
        "Nhiệm vụ của lần này là giải cứu người anh em sinh đôi.",
        "Hãy đi tìm người đó và dẫn cậu ta quay về đây!",
        "Hãy đi về hướng các mũi tên chỉ dẫn đang nhấp nháy!",
        "Chúc may mắn!"
    };

    private int currentLine = 0;

    void Start()
    {
        ShowLine();
    }

    void OnEnable()
    {
        buttonNext.onClick.AddListener(OnNextClicked);
    }

    void OnDisable()
    {
        buttonNext.onClick.RemoveListener(OnNextClicked);
    }

    void OnNextClicked()
    {
        Debug.Log("Next button clicked");
        currentLine++;

        if (currentLine < dialogLines.Count)
        {
            ShowLine();
        }
        else
        {
            gameObject.SetActive(false);
            
        }
    }

    void ShowLine()
    {
        textDialog.text = dialogLines[currentLine];
    }
}