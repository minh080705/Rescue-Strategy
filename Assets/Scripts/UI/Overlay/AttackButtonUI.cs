using UnityEngine;
using UnityEngine.EventSystems;

public class AttackButtonUI : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler
{
    public PlayerCombat combat;

    [Header("Button Effect")]
    public float pressedScale = 0.9f;

    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.localScale = originalScale * pressedScale;

        combat?.RequestAttack();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = originalScale;
    }
}