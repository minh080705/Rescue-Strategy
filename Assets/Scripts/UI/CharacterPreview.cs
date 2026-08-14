// Assets/Scripts/UI/CharacterPreview.cs
// Đồng bộ animation từ nhân vật thật sang nhân vật preview.
// Gắn lên PlayerPreviewChar hoặc HostagePreviewChar.

using UnityEngine;

public class CharacterPreview : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Source — nhân vật thật")]
    [Tooltip("Kéo Animator của Player hoặc Hostage vào đây")]
    public Animator sourceAnimator;

    [Header("Preview Layer")]
    [Tooltip("Đặt tất cả child SpriteRenderer sang layer này")]
    public string previewLayerName = "CharacterPreview";

    // ── Internal ──────────────────────────────────────────
    private Animator previewAnimator;

    // Parameter hashes — đồng bộ các parameter quan trọng
    private static readonly int HashFacingIndex = Animator.StringToHash("FacingIndex");
    private static readonly int HashWeaponType = Animator.StringToHash("WeaponType");
    private static readonly int HashFacingIndexF = Animator.StringToHash("FacingIndexF");
    private static readonly int HashWeaponTypeF = Animator.StringToHash("WeaponTypeF");
    private static readonly int HashIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int HashIsDashing = Animator.StringToHash("IsDashing");
    private static readonly int HashIsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int HashIsReloading = Animator.StringToHash("IsReloading");
    private static readonly int HashIsDead = Animator.StringToHash("IsDead");

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        previewAnimator = GetComponentInChildren<Animator>();
        SetPreviewLayer();
    }

    void LateUpdate()
    {
        if (sourceAnimator == null || previewAnimator == null) return;
        SyncAnimator();
    }

    // ── Sync ──────────────────────────────────────────────
    void SyncAnimator()
    {
        // Sync Int
        previewAnimator.SetInteger(HashFacingIndex, sourceAnimator.GetInteger(HashFacingIndex));
        previewAnimator.SetInteger(HashWeaponType, sourceAnimator.GetInteger(HashWeaponType));

        // Sync Float
        previewAnimator.SetFloat(HashFacingIndexF, sourceAnimator.GetFloat(HashFacingIndexF));
        previewAnimator.SetFloat(HashWeaponTypeF, sourceAnimator.GetFloat(HashWeaponTypeF));

        // Sync Bool
        previewAnimator.SetBool(HashIsMoving, sourceAnimator.GetBool(HashIsMoving));
        previewAnimator.SetBool(HashIsDashing, sourceAnimator.GetBool(HashIsDashing));
        previewAnimator.SetBool(HashIsAttacking, sourceAnimator.GetBool(HashIsAttacking));
        previewAnimator.SetBool(HashIsReloading, sourceAnimator.GetBool(HashIsReloading));
        previewAnimator.SetBool(HashIsDead, sourceAnimator.GetBool(HashIsDead));
    }

    // ── Setup layer ───────────────────────────────────────
    void SetPreviewLayer()
    {
        int layer = LayerMask.NameToLayer(previewLayerName);
        if (layer < 0) return;

        // Đặt tất cả object trong preview sang CharacterPreview layer
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = layer;
    }
}