using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DirectArrow : MonoBehaviour
{
    [SerializeField] private GameObject hostage;
    [SerializeField] private GameObject goalZone;
    [SerializeField] private GameObject player;

    // Thứ tự bắt buộc trong Inspector: [0] = Left, [1] = Right, [2] = Up, [3] = Down
    [SerializeField] private List<Image> directArrows;

    [Header("Settings")]
    [Tooltip("Khoảng cách tới mục tiêu để ẩn hết arrow")]
    [SerializeField] private float hideDistance = 10f;

    [Tooltip("Độ lệch (độ) so với trục 0/90/180/270 — vượt quá mức này thì 2 arrow kề nhau cùng nhấp nháy")]
    [SerializeField] private float angleThreshold = 10f;

    [Tooltip("Tốc độ nhấp nháy")]
    [SerializeField] private float blinkSpeed = 4f;

    [Tooltip("Alpha thấp nhất khi nhấp nháy")]
    [SerializeField] private float minAlpha = 0.15f;

    private const int LEFT = 0;
    private const int RIGHT = 1;
    private const int UP = 2;
    private const int DOWN = 3;

    // Cardinal index (theo góc tăng dần ngược chiều kim đồng hồ: 0°=Right, 90°=Up, 180°=Left, 270°=Down)
    // → map sang index thực trong directArrows
    private static readonly int[] cardinalToArrowIndex = { RIGHT, UP, LEFT, DOWN };

    private HostageAI hostageAI;
    private bool[] activeArrow = new bool[4];

    void Awake()
    {
        if (hostage != null)
            hostageAI = hostage.GetComponent<HostageAI>();
    }

    void Update()
    {
        if (player == null || hostage == null || goalZone == null || directArrows.Count < 4)
            return;

        Vector2 targetPos = GetTargetPosition();
        Vector2 playerPos = player.transform.position;
        Vector2 diff = targetPos - playerPos;
        float dist = diff.magnitude;

        // Đủ gần mục tiêu → ẩn hết
        if (dist <= hideDistance)
        {
            SetAllArrowsVisible(false);
            return;
        }

        // Xác định hướng cần nhấp nháy dựa trên góc thực tế của diff
        for (int i = 0; i < 4; i++) activeArrow[i] = false;

        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg; // -180..180
        if (angle < 0f) angle += 360f;

        int nearestCardinal = Mathf.RoundToInt(angle / 90f) % 4;
        float cardinalAngle = nearestCardinal * 90f;
        float deviation = Mathf.DeltaAngle(cardinalAngle, angle); // góc lệch có dấu, trong khoảng -45..45

        // Luôn bật arrow gần nhất
        activeArrow[cardinalToArrowIndex[nearestCardinal]] = true;

        // Lệch quá angleThreshold so với trục vuông → bật thêm arrow kề (theo hướng lệch)
        if (Mathf.Abs(deviation) > angleThreshold)
        {
            int nextCardinal = deviation > 0f
                ? (nearestCardinal + 1) % 4
                : (nearestCardinal + 3) % 4;

            activeArrow[cardinalToArrowIndex[nextCardinal]] = true;
        }

        float blinkAlpha = Mathf.Lerp(minAlpha, 1f, (Mathf.Sin(Time.time * blinkSpeed) + 1f) * 0.5f);

        for (int i = 0; i < 4; i++)
        {
            Image img = directArrows[i];
            if (img == null) continue;

            if (activeArrow[i])
            {
                if (!img.gameObject.activeSelf) img.gameObject.SetActive(true);
                SetAlpha(img, blinkAlpha);
            }
            else
            {
                if (img.gameObject.activeSelf) img.gameObject.SetActive(false);
            }
        }
    }

    private Vector2 GetTargetPosition()
    {
        // Locked → mục tiêu là Hostage, ngược lại → GoalZone
        if (hostageAI != null && hostageAI.HostageState == HostageState.Locked)
            return hostage.transform.position;

        return goalZone.transform.position;
    }

    private void SetAllArrowsVisible(bool visible)
    {
        for (int i = 0; i < directArrows.Count; i++)
        {
            if (directArrows[i] == null) continue;
            if (directArrows[i].gameObject.activeSelf != visible)
                directArrows[i].gameObject.SetActive(visible);
        }
    }

    private void SetAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}