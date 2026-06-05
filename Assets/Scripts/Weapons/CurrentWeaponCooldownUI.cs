using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrentWeaponCooldownUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private PlayerAttack playerAttack;

    [Header("UI")]
    [SerializeField] private Image weaponIconImage;
    [SerializeField] private Image cooldownFillImage;

    private void Awake()
    {
        if (playerCombat == null)
            playerCombat = FindFirstObjectByType<PlayerCombat>();

        if (playerAttack == null)
            playerAttack = FindFirstObjectByType<PlayerAttack>();
    }

    private void Update()
    {
        if (playerCombat == null)
            return;

        WeaponData weapon = playerCombat.CurrentWeapon;

        if (weapon == null)
        {
            HideCooldown();
            return;
        }

        UpdateWeaponIcon(weapon);
        UpdateCooldown(weapon);
    }

    private void UpdateWeaponIcon(WeaponData weapon)
    {
        if (weaponIconImage == null)
            return;

        weaponIconImage.sprite = weapon.weaponIcon;
        weaponIconImage.enabled = weapon.weaponIcon != null;
    }

    private void UpdateCooldown(WeaponData weapon)
    {
        float remaining = 0f;
        float total = 1f;

        if (weapon.attackMode == WeaponAttackMode.Melee)
        {
            if (playerAttack == null)
            {
                HideCooldown();
                return;
            }

            remaining = playerAttack.GetMeleeCooldownRemaining();
            total = playerAttack.GetMeleeCooldownTotal(weapon);
        }
        else
        {
            if (playerCombat.IsCurrentWeaponReloading())
            {
                remaining = playerCombat.GetReloadRemainingSeconds();
                total = playerCombat.GetFinalReloadTimePublic(weapon);
            }
            else
            {
                remaining = 0f;
                total = 1f;
            }
        }

        float normalized = total > 0f ? remaining / total : 0f;
        normalized = Mathf.Clamp01(normalized);

        if (cooldownFillImage != null)
        {
            cooldownFillImage.enabled = normalized > 0f;
            cooldownFillImage.fillAmount = normalized;
        }      
    }

    private void HideCooldown()
    {
        if (cooldownFillImage != null)
        {
            cooldownFillImage.fillAmount = 0f;
            cooldownFillImage.enabled = false;
        }
    }
}