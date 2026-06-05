using UnityEngine;
using UnityEngine.EventSystems;

public class FloatingJoystick : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    [Header("References")]
    public RectTransform background;
    public RectTransform handle;
    public Canvas canvas;
    public PlayerController player;

    [Header("Settings")]
    public float radius = 80f;

    private Camera uiCamera;

    void Start()
    {
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            uiCamera = null;
        else
            uiCamera = canvas.worldCamera;

        background.gameObject.SetActive(false);
        handle.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Pointer Down");
        background.gameObject.SetActive(true);
        handle.gameObject.SetActive(true);

        background.position = eventData.position;
        handle.position = eventData.position;

        player.SetJoystickInput(Vector2.zero);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 bgPos = background.position;

        Vector2 delta =
            eventData.position - bgPos;

        delta = Vector2.ClampMagnitude(delta, radius);

        handle.position = bgPos + delta;

        Vector2 direction = delta / radius;

        player.SetJoystickInput(direction);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        background.gameObject.SetActive(false);
        handle.gameObject.SetActive(false);

        player.SetJoystickInput(Vector2.zero);
    }
}