using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MenuButtons : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Botones")]
    [SerializeField] private Button[] buttons;

    [Header("Icono hover")]
    [SerializeField] private RectTransform hoverIcon;
    [SerializeField] private float iconOffsetX = -60f;

    private Button currentHovered;

    private void Start()
    {
        hoverIcon.gameObject.SetActive(false);

        foreach (Button btn in buttons)
        {
            EventTrigger trigger = btn.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => OnButtonHover(btn));
            trigger.triggers.Add(enterEntry);

            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => OnButtonExit());
            trigger.triggers.Add(exitEntry);
        }
    }

    private void OnButtonHover(Button btn)
    {
        hoverIcon.gameObject.SetActive(true);

        RectTransform btnRect = btn.GetComponent<RectTransform>();
        Vector2 iconPos = btnRect.anchoredPosition;
        iconPos.x += iconOffsetX;
        hoverIcon.anchoredPosition = iconPos;
    }

    private void OnButtonExit()
    {
        hoverIcon.gameObject.SetActive(false);
    }
    public void HideIcon()
    {
        hoverIcon.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData) { }
    public void OnPointerExit(PointerEventData eventData) { }
}