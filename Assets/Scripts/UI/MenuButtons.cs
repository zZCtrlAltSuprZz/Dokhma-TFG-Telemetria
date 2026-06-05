using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuButtons : MonoBehaviour
{
    [Header("Botones")]
    [SerializeField] private Button[] buttons;

    [Header("Icono hover / selección")]
    [SerializeField] private RectTransform hoverIcon;
    [SerializeField] private float iconOffsetX = -60f;

    private void Start()
    {
        hoverIcon.gameObject.SetActive(false);

        foreach (Button btn in buttons)
        {
            EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();

            if (trigger == null)
                trigger = btn.gameObject.AddComponent<EventTrigger>();

            AddEvent(trigger, EventTriggerType.PointerEnter, () => ShowIconOnButton(btn));
            AddEvent(trigger, EventTriggerType.PointerExit, HideIcon);

            AddEvent(trigger, EventTriggerType.Select, () => ShowIconOnButton(btn));
            AddEvent(trigger, EventTriggerType.Deselect, HideIcon);
        }
    }

    private void AddEvent(EventTrigger trigger, EventTriggerType type, System.Action action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener((data) => action.Invoke());
        trigger.triggers.Add(entry);
    }

    public void ShowIconOnButton(Button btn)
    {
        if (btn == null || hoverIcon == null) return;

        hoverIcon.gameObject.SetActive(true);

        RectTransform btnRect = btn.GetComponent<RectTransform>();

        Vector2 iconPos = btnRect.anchoredPosition;
        iconPos.x += iconOffsetX;

        hoverIcon.anchoredPosition = iconPos;
    }

    public void HideIcon()
    {
        if (hoverIcon != null)
            hoverIcon.gameObject.SetActive(false);
    }
}