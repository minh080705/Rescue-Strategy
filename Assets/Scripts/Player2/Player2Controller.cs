// Assets/Scripts/Player2/Player2Controller.cs
// Di chuyển 8 hướng, dash với i-frame.
// Dash theo hướng MoveInput, fallback LastFacing nếu đứng yên.
// Trong lúc dash: vẫn attack được, bất tử (i-frame).

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player2Controller : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Dash")]
    public KeyCode dashKey = KeyCode.Space;
    public float dashSpeed = 12f;
    public float dashDuration = 0.15f;   // giây dash
    public float dashCooldown = 0.8f;    // giây hồi chiêu

    [Header("I-Frame")]
    [Tooltip("Layer của enemy projectile / hitbox — tắt collision khi dash")]
    public string enemyLayerName = "Enemy";

    // ── Properties ────────────────────────────────────────
    public Vector2 MoveInput { get; private set; }
    public Vector2 LastFacing { get; private set; } = Vector2.down;
    public int FacingIndex { get; private set; } = 0;
    public bool IsMoving => MoveInput != Vector2.zero;
    public bool IsDashing { get; private set; }
    public bool IsIFrame { get; private set; }

    // ── Internal ──────────────────────────────────────────
    private Rigidbody2D rb;
    private Animator anim;

    private bool inputLocked = false;
    private float speedMultiplier = 1f;

    private Vector2 dashDirection;
    private float dashTimer = 0f;
    private float cooldownTimer = 0f;

    private Vector2 joystickInput = Vector2.zero;

    // Layer collision
    private int playerLayer;
    private int enemyLayer;

    // ── Animator hashes ───────────────────────────────────
    private static readonly int HashFacingIndex = Animator.StringToHash("FacingIndex");
    private static readonly int HashIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int HashIsDashing = Animator.StringToHash("IsDashing");

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        playerLayer = gameObject.layer;
        enemyLayer = LayerMask.NameToLayer(enemyLayerName);
    }

    void Update()
    {
        HandleDashInput();
        HandleMoveInput();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (IsDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
                EndDash();
            else
                rb.velocity = dashDirection * dashSpeed;
            return;
        }

        if (inputLocked)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        rb.velocity = MoveInput * moveSpeed * speedMultiplier;
    }

    // ── Move input ────────────────────────────────────────
    void HandleMoveInput()
    {
        if (inputLocked && !IsDashing)
        {
            MoveInput = Vector2.zero;
            return;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // Fallback joystick
        if (Mathf.Approximately(x, 0f) && Mathf.Approximately(y, 0f))
        {
            x = joystickInput.x;
            y = joystickInput.y;
        }

        // Snap về 8 hướng
        Vector2 snapped = Vector2.zero;
        if (x != 0f || y != 0f)
        {
            float ax = Mathf.Abs(x);
            float ay = Mathf.Abs(y);

            // Diagonal khi cả 2 trục đều có input rõ (không quá lệch)
            if (ax > 0.1f && ay > 0.1f)
                snapped = new Vector2(Mathf.Sign(x), Mathf.Sign(y)).normalized;
            else if (ax >= ay)
                snapped = new Vector2(Mathf.Sign(x), 0f);
            else
                snapped = new Vector2(0f, Mathf.Sign(y));
        }

        MoveInput = snapped;

        if (MoveInput != Vector2.zero)
        {
            LastFacing = MoveInput;
            FacingIndex = ToIndex(MoveInput);
        }
    }

    // ── Dash input ────────────────────────────────────────
    void HandleDashInput()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        if (!Input.GetKeyDown(dashKey)) return;
        if (IsDashing) return;

        StartDash();
    }

    void StartDash()
    {
        // Hướng dash: MoveInput nếu đang di chuyển, LastFacing nếu đứng yên
        dashDirection = IsMoving ? MoveInput.normalized : LastFacing.normalized;

        IsDashing = true;
        dashTimer = dashDuration;
        cooldownTimer = dashCooldown;

        // Bật i-frame
        SetIFrame(true);

        Debug.Log($"[Player2] Dash → {dashDirection}");
    }

    void EndDash()
    {
        IsDashing = false;
        rb.velocity = Vector2.zero;

        // Tắt i-frame
        SetIFrame(false);
    }

    // ── I-Frame ───────────────────────────────────────────
    void SetIFrame(bool active)
    {
        IsIFrame = active;

        if (enemyLayer < 0) return;
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, active);
    }

    // ── Animator ──────────────────────────────────────────
    void UpdateAnimator()
    {
        if (anim == null) return;
        anim.SetInteger(HashFacingIndex, FacingIndex);
        anim.SetBool(HashIsMoving, IsMoving);
        anim.SetBool(HashIsDashing, IsDashing);
    }

    // ── Helpers ───────────────────────────────────────────

    /// <summary>
    /// Chuyển vector hướng thành FacingIndex 0-7.
    /// 0=Down 1=Up 2=Left 3=Right
    /// 4=RightDown 5=RightUp 6=LeftDown 7=LeftUp
    /// </summary>
    public static int ToIndex(Vector2 dir)
    {
        bool up = dir.y > 0f;
        bool down = dir.y < 0f;
        bool right = dir.x > 0f;
        bool left = dir.x < 0f;

        if (down && right) return 4; // Right_Down
        if (up && right) return 5; // Right_Up
        if (down && left) return 6; // Left_Down
        if (up && left) return 7; // Left_Up
        if (down) return 0; // Down
        if (up) return 1; // Up
        if (left) return 2; // Left
        return 3;                      // Right
    }

    // ── Public API ────────────────────────────────────────
    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
        if (locked && !IsDashing)
            rb.velocity = Vector2.zero;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = Mathf.Clamp01(multiplier);
    }

    public void SetJoystickInput(Vector2 input)
    {
        joystickInput = input;
    }
}