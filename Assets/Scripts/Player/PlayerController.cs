// Assets/Scripts/Player/PlayerController.cs

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LastFacing { get; private set; } = Vector2.down;

    private Vector2 joystickInput = Vector2.zero;
    public bool IsMoving => MoveInput != Vector2.zero;
    public int FacingIndex { get; private set; } = 0;

    private Rigidbody2D rb;
    private Animator anim;
    private bool inputLocked = false;

    private float speedMultiplier = 1f;  // 1 = bình thường, 0.5 = nửa tốc độ

    void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        if (inputLocked)
        {
            // Khoá di chuyển — giữ MoveInput = zero, không cập nhật facing
            MoveInput = Vector2.zero;
            if (anim != null)
            {
                anim.SetInteger("FacingIndex", FacingIndex);
                anim.SetBool("IsMoving", false);
            }
            return;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // Nếu không có input bàn phím thì dùng joystick
        if (Mathf.Approximately(x, 0f) &&
            Mathf.Approximately(y, 0f))
        {
            x = joystickInput.x;
            y = joystickInput.y;
        }

        Vector2 snapped = Vector2.zero;
        if (x != 0 || y != 0)
        {
            if (Mathf.Abs(x) >= Mathf.Abs(y))
                snapped = new Vector2(Mathf.Sign(x), 0f);
            else
                snapped = new Vector2(0f, Mathf.Sign(y));
        }

        MoveInput = snapped;

        if (MoveInput != Vector2.zero)
        {
            LastFacing = MoveInput;
            FacingIndex = ToIndex(LastFacing);
        }

        if (anim != null)
        {
            anim.SetInteger("FacingIndex", FacingIndex);
            anim.SetBool("IsMoving", IsMoving);
        }
    }

    void FixedUpdate()
    {
        rb.velocity = MoveInput * moveSpeed * speedMultiplier;
    }

    /// <summary>Gọi từ PlayerCombat để khoá/mở input di chuyển.</summary>
    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
        if (locked)
            rb.velocity = Vector2.zero;
    }

    public static int ToIndex(Vector2 dir)
    {
        if (dir.y < 0f) return 0;
        if (dir.y > 0f) return 1;
        if (dir.x < 0f) return 2;
        return 3;
    }

    public void SetJoystickInput(Vector2 input)
    {
        joystickInput = input;
    }

    /// <summary>
    /// Đặt hệ số nhân tốc độ.
    /// 1f = bình thường, 0.5f = nửa tốc độ, 0f = đứng yên.
    /// </summary>
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = Mathf.Clamp01(multiplier);
    }
}