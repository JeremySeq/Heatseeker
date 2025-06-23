using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public RectTransform background;
    public RectTransform handle;
    public Vector2 inputDirection;
    private CanvasGroup _canvasGroup;

    private void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        
        if (PlayerController.IsMobile())
        {
            Debug.Log("Joystick enabled for mobile");
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
        }
        else
        {
            Debug.Log("Joystick disabled for desktop");
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, eventData.pressEventCamera, out pos))
        {
            pos.x = (pos.x / background.sizeDelta.x) * 2;
            pos.y = (pos.y / background.sizeDelta.y) * 2;

            inputDirection = new Vector2(pos.x, pos.y);
            inputDirection = (inputDirection.magnitude > 1) ? inputDirection.normalized : inputDirection;

            handle.anchoredPosition = new Vector2(
                inputDirection.x * (background.sizeDelta.x / 2),
                inputDirection.y * (background.sizeDelta.y / 2));
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputDirection = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}