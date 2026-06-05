using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class PerkSelectionUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("HUD")]
    [SerializeField] private PerkHUD perkHUD;
    [SerializeField] private GameObject hud;

    [Header("Option A")]
    [SerializeField] private Button optionAButton;
    [SerializeField] private TMP_Text optionAName;
    [SerializeField] private TMP_Text optionADescription;

    [Header("Option B")]
    [SerializeField] private Button optionBButton;
    [SerializeField] private TMP_Text optionBName;
    [SerializeField] private TMP_Text optionBDescription;

    [Header("Player")]
    [SerializeField] private PlayerPerkManager playerPerkManager;

    private PerkData currentA;
    private PerkData currentB;

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false);

        optionAButton.onClick.AddListener(() => SelectPerk(currentA));
        optionBButton.onClick.AddListener(() => SelectPerk(currentB));

        SetupHover(optionAButton, () => currentA);
        SetupHover(optionBButton, () => currentB);
    }

    public void OpenSelection(PerkData optionA, PerkData optionB)
    {
        Debug.Log("OpenSelection llamado");

        currentA = optionA;
        currentB = optionB;

        SetOption(optionAName, optionADescription, optionAButton, currentA);
        SetOption(optionBName, optionBDescription, optionBButton, currentB);

        if (panel != null)
            panel.SetActive(true);
        else
            Debug.LogError("Panel no asignado en PerkSelectionUI");

        if (hud != null)
            hud.SetActive(false);

        Time.timeScale = 0f;

        StartCoroutine(SelectFirstOptionNextFrame());
    }

    private IEnumerator SelectFirstOptionNextFrame()
    {
        yield return null;

        if (EventSystem.current == null || optionAButton == null)
            yield break;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionAButton.gameObject);

        SetHover(optionAButton, currentA, true);
        SetHover(optionBButton, currentB, false);
    }

    private void SetOption(TMP_Text nameText, TMP_Text descriptionText, Button button, PerkData perk)
    {
        if (perk == null)
        {
            Debug.LogError("PerkData no asignado");
            return;
        }

        nameText.text = perk.perkName;
        descriptionText.text = perk.description;

        button.image.sprite = perk.iconNormal;
    }

    private void SetupHover(Button button, System.Func<PerkData> getPerk)
    {
        EventTrigger trigger = button.GetComponent<EventTrigger>();

        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        trigger.triggers = new List<EventTrigger.Entry>();

        AddEvent(trigger, EventTriggerType.PointerEnter, () =>
        {
            SetHover(button, getPerk(), true);
        });

        AddEvent(trigger, EventTriggerType.PointerExit, () =>
        {
            if (EventSystem.current != null &&
                EventSystem.current.currentSelectedGameObject == button.gameObject)
                return;

            SetHover(button, getPerk(), false);
        });

        AddEvent(trigger, EventTriggerType.Select, () =>
        {
            SetHover(button, getPerk(), true);
        });

        AddEvent(trigger, EventTriggerType.Deselect, () =>
        {
            SetHover(button, getPerk(), false);
        });
    }

    private void AddEvent(EventTrigger trigger, EventTriggerType type, System.Action action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener((data) => action.Invoke());
        trigger.triggers.Add(entry);
    }

    private void SetHover(Button button, PerkData perk, bool active)
    {
        if (button == null || perk == null)
            return;

        if (active && perk.iconHover != null)
        {
            button.image.sprite = perk.iconHover;
        }
        else if (!active && perk.iconNormal != null)
        {
            button.image.sprite = perk.iconNormal;
        }
    }

    public void SelectPerk(PerkData perk)
    {
        if (perk == null) return;

        Debug.Log("Perk seleccionado: " + perk.perkName);

        if (playerPerkManager != null)
            playerPerkManager.AddPerk(perk);
        else
            Debug.LogError("PlayerPerkManager no asignado en PerkSelectionUI");

        if (perkHUD != null)
            perkHUD.ActivatePerkIcon(perk);
        else
            Debug.LogError("PerkHUD no asignado");

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        if (panel != null)
            panel.SetActive(false);

        if (hud != null)
            hud.SetActive(true);

        Time.timeScale = 1f;
    }
}