using System.Collections.Generic;
using UnityEngine;

public class PlayerPerkManager : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private PlayerMovement playerMovement;

    private readonly HashSet<PerkType> ownedPerks = new HashSet<PerkType>();

    private void Awake()
    {
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        if (playerCombat == null)
            playerCombat = GetComponent<PlayerCombat>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
    }

    public void AddPerk(PerkData perk)
    {
        if (perk == null) return;

        if (ownedPerks.Contains(perk.perkType))
        {
            Debug.Log("Ya tienes este perk: " + perk.perkName);
            return;
        }

        ownedPerks.Add(perk.perkType);

        switch (perk.perkType)
        {
            case PerkType.Jugg:
                if (playerHealth != null)
                    playerHealth.ApplyJugger();
                break;

            case PerkType.QuickRev:
                if (playerHealth != null)
                    playerHealth.ApplyQuickRevive();
                break;

            case PerkType.DoubleTap:
                if (playerCombat != null)
                    playerCombat.ApplyDoubleTap();
                break;

            case PerkType.StrongKnockback:
                if (playerCombat != null)
                    playerCombat.ApplyStrongKnockback();
                break;

            case PerkType.StaminUp:
                if (playerMovement != null)
                    playerMovement.ApplyStaminUp();
                break;

            case PerkType.Dash:
                if (playerMovement != null)
                    playerMovement.ApplyDash();
                break;
            case PerkType.MeleeAttackSpeed:
                if (playerCombat != null)
                    playerCombat.ApplyMeleeAttackSpeed();
                break;

            case PerkType.ReloadSpeed:
                if (playerCombat != null)
                    playerCombat.ApplyReloadSpeed();
                break;
        }

        Debug.Log("Perk aplicado: " + perk.perkName);
    }

    public bool HasPerk(PerkType perkType)
    {
        return ownedPerks.Contains(perkType);
    }
}