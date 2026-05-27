using TMPro;
using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [Header("Weapon Data")]
    [SerializeField] private WeaponData pistolData;
    [SerializeField] private WeaponData daggerData;
    [SerializeField] private WeaponData minigunData;
    [SerializeField] private WeaponData scytheData;

    [Header("Weapon Objects")]
    [SerializeField] private GameObject pistolObject;
    [SerializeField] private GameObject daggerObject;
    [SerializeField] private GameObject minigunObject;
    [SerializeField] private GameObject scytheObject;

    [Header("HUD")]
    [SerializeField] private TMP_Text weaponText;

    public void UpdateWeaponVisuals(WeaponData currentWeapon)
    {
        if (currentWeapon == null)
            return;

        bool usingPistol = currentWeapon == pistolData;
        bool usingDagger = currentWeapon == daggerData;
        bool usingMinigun = currentWeapon == minigunData;
        bool usingScythe = currentWeapon == scytheData;

        if (pistolObject != null)
            pistolObject.SetActive(usingPistol);

        if (daggerObject != null)
            daggerObject.SetActive(usingDagger);

        if (minigunObject != null)
            minigunObject.SetActive(usingMinigun);

        if (scytheObject != null)
            scytheObject.SetActive(usingScythe);

        if (weaponText != null)
            weaponText.text = currentWeapon.hudText;
    }
}