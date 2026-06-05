// Assets/Scripts/Player/BoneThrower.cs

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class BoneThrower : MonoBehaviour
{
    public GameObject bonePrefab;
    public KeyCode throwKey = KeyCode.Q;
    public KeyCode cancelKey = KeyCode.Escape;
    public float throwDistance = 3f;
    public float throwCooldown = 1f;
    public int poolSize = 5;

    [Header("Preview")]
    public float previewAlpha = 0.4f;

    [Header("Aiming")]
    [Tooltip("Hệ số tốc độ khi đang giữ xương để ném (0.5 = 50%)")]
    public float aimSpeedMultiplier = 0.5f;

    private PlayerController controller;
    private Queue<GameObject> pool = new Queue<GameObject>();
    private float cooldown = 0f;

    public bool IsAiming => isAiming;
    private bool isAiming = false;
    private GameObject previewObject = null;
    private SpriteRenderer previewRenderer = null;

    void Awake()
    {
        controller = GetComponent<PlayerController>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(bonePrefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }

        CreatePreview();
    }

    void CreatePreview()
    {
        previewObject = Instantiate(bonePrefab);

        BoneItem boneItem = previewObject.GetComponent<BoneItem>();
        if (boneItem != null) boneItem.enabled = false;

        Collider2D col = previewObject.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        previewRenderer = previewObject.GetComponent<SpriteRenderer>();
        if (previewRenderer != null)
        {
            Color c = previewRenderer.color;
            c.a = previewAlpha;
            previewRenderer.color = c;
        }

        previewObject.SetActive(false);
    }

    void Update()
    {
        cooldown -= Time.deltaTime;

        if (isAiming)
        {
            UpdatePreviewPosition();
            HandleAimingInput();
        }
        else
        {
            if (Input.GetKeyDown(throwKey) && cooldown <= 0f && pool.Count > 0)
                EnterAimMode();
        }
    }

    void EnterAimMode()
    {
        isAiming = true;
        previewObject.SetActive(true);
        UpdatePreviewPosition();

        // Giảm tốc độ player khi đang aim
        controller.SetSpeedMultiplier(aimSpeedMultiplier);
    }

    void UpdatePreviewPosition()
    {
        Vector2 pos = (Vector2)transform.position + controller.LastFacing * throwDistance;
        previewObject.transform.position = pos;
    }

    void HandleAimingInput()
    {
        if (Input.GetKeyDown(throwKey))
        {
            ThrowBone();
            ExitAimMode();
            return;
        }

        if (Input.GetKeyDown(cancelKey))
            ExitAimMode();
    }

    void ExitAimMode()
    {
        isAiming = false;
        previewObject.SetActive(false);

        // Khôi phục tốc độ player
        controller.SetSpeedMultiplier(1f);
    }

    void ThrowBone()
    {
        if (pool.Count == 0) return;

        cooldown = throwCooldown;

        Vector2 spawnPos = (Vector2)transform.position + controller.LastFacing * throwDistance;
        GameObject bone = pool.Dequeue();

        bone.transform.position = spawnPos;
        bone.GetComponent<BoneItem>().thrower = this;
        bone.SetActive(true);
    }

    public void ReturnToPool(GameObject bone)
    {
        bone.SetActive(false);
        pool.Enqueue(bone);
    }

    void OnDestroy()
    {
        // Đảm bảo tốc độ được khôi phục nếu object bị destroy khi đang aim
        if (isAiming) controller?.SetSpeedMultiplier(1f);
        if (previewObject != null) Destroy(previewObject);
    }
}