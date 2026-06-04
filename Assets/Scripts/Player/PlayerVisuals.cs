using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private Image weaponIcon;

    [Header("Weapon Sprites")]
    [SerializeField] private Sprite pistolSprite;
    [SerializeField] private Sprite daggerSprite;
    [SerializeField] private Sprite minigunSprite;
    [SerializeField] private Sprite scytheSprite;

    public void UpdateWeaponVisuals(WeaponData currentWeapon)
    {
        if (currentWeapon == null)
            return;

        bool usingPistol = currentWeapon == pistolData;
        bool usingDagger = currentWeapon == daggerData;
        bool usingMinigun = currentWeapon == minigunData;
        bool usingScythe = currentWeapon == scytheData;

        // Arma visible
        if (pistolObject != null)
            pistolObject.SetActive(usingPistol);

        if (daggerObject != null)
            daggerObject.SetActive(usingDagger);

        if (minigunObject != null)
            minigunObject.SetActive(usingMinigun);

        if (scytheObject != null)
            scytheObject.SetActive(usingScythe);

        // Icono HUD
        if (weaponIcon != null)
        {
            if (usingPistol)
                weaponIcon.sprite = pistolSprite;
            else if (usingDagger)
                weaponIcon.sprite = daggerSprite;
            else if (usingMinigun)
                weaponIcon.sprite = minigunSprite;
            else if (usingScythe)
                weaponIcon.sprite = scytheSprite;
        }
    }
}