using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
        if (hud != null)          
            hud.SetActive(false);
        else
            Debug.LogError("Panel no asignado en PerkSelectionUI");

        Time.timeScale = 0f;
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

        // IMPORTANTE: el botón ES la imagen
        button.image.sprite = perk.iconNormal;
    }

    private void SetupHover(Button button, System.Func<PerkData> getPerk)
    {
        EventTrigger trigger = button.GetComponent<EventTrigger>();

        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        trigger.triggers = new List<EventTrigger.Entry>();

        // HOVER ENTER
        EventTrigger.Entry enter = new EventTrigger.Entry();
        enter.eventID = EventTriggerType.PointerEnter;
        enter.callback.AddListener((data) =>
        {
            var perk = getPerk();
            if (perk != null && perk.iconHover != null)
            {
                button.image.sprite = perk.iconHover;
                Debug.Log("Hover ON: " + perk.perkName);
            }
        });

        // HOVER EXIT
        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener((data) =>
        {
            var perk = getPerk();
            if (perk != null && perk.iconNormal != null)
            {
                button.image.sprite = perk.iconNormal;
                Debug.Log("Hover OFF: " + perk.perkName);
            }
        });

        trigger.triggers.Add(enter);
        trigger.triggers.Add(exit);
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

        if (panel != null)
            panel.SetActive(false);

        if (hud != null)
            hud.SetActive(true);
        Time.timeScale = 1f;
    }
}