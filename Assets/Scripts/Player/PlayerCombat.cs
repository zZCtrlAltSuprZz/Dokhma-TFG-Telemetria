using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] private WeaponData[] startingWeapons;
    [SerializeField] private List<WeaponData> ownedWeapons = new List<WeaponData>();
    [SerializeField] private int currentWeaponIndex = 0;
    [SerializeField] private int maxOwnedWeapons = 2;
    [SerializeField] private WeaponRecoil weaponRecoil;

    [Header("Ammo HUD")]
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private string reloadTrigger = "reload";

    [Header("Camera Shake")]
    [SerializeField] private CinemachineCameraShake cameraShake;

    [Header("References")]
    [SerializeField] public WeaponPointSlot[] weaponPoints;
    [SerializeField] private PlayerAttack meleeAttack;
    [SerializeField] private Animator animator;

    [Header("Animator")]
    [SerializeField] private string pistolBool = "isPistol";
    [SerializeField] private string reloadSpeedParam = "reloadSpeed";
    [SerializeField] private string reloadClipNameContains = "Reload";

    [Header("Perks - Reload Speed")]
    [SerializeField] private float reloadSpeedMultiplier = 1.35f;

    [Header("Perks - Melee Cooldown")]
    [SerializeField] private float meleeCooldownMultiplier = 1.35f;

    [Header("Perks - Double Tap")]
    [SerializeField] private float doubleTapDamageMultiplier = 2f;

    [Header("Perks - Strong Knockback")]
    [SerializeField] private float strongKnockbackDistanceMultiplier = 3f;
    [SerializeField] private float strongKnockbackTimeMultiplier = 1.5f;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private readonly Dictionary<WeaponData, int> ammoInMagazine = new Dictionary<WeaponData, int>();
    private readonly Dictionary<WeaponData, int> reserveAmmo = new Dictionary<WeaponData, int>();

    private WeaponData reloadingWeapon;

    private bool isReloading;
    private bool hasReloadSpeedPerk;
    private bool hasMeleeCooldownPerk;
    private bool hasDoubleTapPerk;
    private bool hasStrongKnockbackPerk;

    private float nextFireTime;

    public System.Action<WeaponData> OnWeaponChanged;

    public float KnockbackMultiplier => hasStrongKnockbackPerk ? strongKnockbackDistanceMultiplier : 1f;
    public float KnockbackTimeMultiplier => hasStrongKnockbackPerk ? strongKnockbackTimeMultiplier : 1f;

    public WeaponData CurrentWeapon
    {
        get
        {
            if (ownedWeapons == null || ownedWeapons.Count == 0)
            {
                return null;
            }

            if (currentWeaponIndex < 0 || currentWeaponIndex >= ownedWeapons.Count)
            {
                currentWeaponIndex = 0;
            }

            return ownedWeapons[currentWeaponIndex];
        }
    }

    private void Awake()
    {
        if (meleeAttack == null)
        {
            meleeAttack = GetComponent<PlayerAttack>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Start()
    {
        ownedWeapons.Clear();

        foreach (WeaponData weapon in startingWeapons)
        {
            if (weapon != null && !ownedWeapons.Contains(weapon))
            {
                ownedWeapons.Add(weapon);
            }
        }

        foreach (WeaponData weapon in ownedWeapons)
        {
            InitAmmo(weapon);
        }

        ApplyWeapon(CurrentWeapon);
        UpdateAmmoHUD();
    }

    public void UseCurrentWeapon()
    {
        if (isReloading)
        {
            return;
        }

        WeaponData weapon = CurrentWeapon;

        if (weapon == null)
        {
            return;
        }

        if (weapon.attackMode == WeaponAttackMode.Ranged)
        {
            Shoot(weapon);
        }
        else if (weapon.attackMode == WeaponAttackMode.Melee)
        {
            UseMelee(weapon);
        }
    }

    public bool IsReloading()
    {
        return isReloading;
    }

    public void SwitchWeapon()
    {
        if (isReloading || ownedWeapons == null || ownedWeapons.Count == 0)
        {
            return;
        }

        currentWeaponIndex++;

        if (currentWeaponIndex >= ownedWeapons.Count)
        {
            currentWeaponIndex = 0;
        }

        ApplyWeapon(CurrentWeapon);
    }

    private void ApplyWeapon(WeaponData weapon)
    {
        if (weapon == null)
        {
            return;
        }

        if (animator != null)
        {
            animator.SetBool(pistolBool, weapon.usesPistolPose);
        }

        OnWeaponChanged?.Invoke(weapon);
        InitAmmo(weapon);
        UpdateAmmoHUD();
    }

    private void InitAmmo(WeaponData weapon)
    {
        if (weapon == null || weapon.attackMode != WeaponAttackMode.Ranged)
        {
            return;
        }

        if (!ammoInMagazine.ContainsKey(weapon))
        {
            ammoInMagazine.Add(weapon, weapon.magazineSize);
        }

        if (!reserveAmmo.ContainsKey(weapon))
        {
            reserveAmmo.Add(weapon, weapon.reserveAmmo);
        }
    }

    private void UpdateAmmoHUD()
    {
        WeaponData weapon = CurrentWeapon;

        if (ammoText == null || weapon == null)
        {
            return;
        }

        if (weapon.attackMode != WeaponAttackMode.Ranged)
        {
            ammoText.text = "";
            return;
        }

        InitAmmo(weapon);
        ammoText.text = ammoInMagazine[weapon] + " / " + reserveAmmo[weapon];
    }

    public void TryReload()
    {
        WeaponData weapon = CurrentWeapon;

        if (weapon == null || weapon.attackMode != WeaponAttackMode.Ranged || isReloading)
        {
            return;
        }

        InitAmmo(weapon);

        if (ammoInMagazine[weapon] >= weapon.magazineSize || reserveAmmo[weapon] <= 0)
        {
            return;
        }

        isReloading = true;
        reloadingWeapon = weapon;

        if (animator == null)
        {
            return;
        }

        animator.ResetTrigger(reloadTrigger);
        animator.SetFloat(reloadSpeedParam, GetReloadAnimatorSpeed(weapon));
        animator.SetTrigger(reloadTrigger);
    }

    public void FinishReload()
    {
        if (!isReloading || reloadingWeapon == null)
        {
            return;
        }

        int neededAmmo = reloadingWeapon.magazineSize - ammoInMagazine[reloadingWeapon];
        int ammoToLoad = Mathf.Min(neededAmmo, reserveAmmo[reloadingWeapon]);

        ammoInMagazine[reloadingWeapon] += ammoToLoad;
        reserveAmmo[reloadingWeapon] -= ammoToLoad;

        isReloading = false;
        reloadingWeapon = null;

        UpdateAmmoHUD();
    }

    private void Shoot(WeaponData weapon)
    {
        if (isReloading || Time.time < nextFireTime)
        {
            return;
        }

        InitAmmo(weapon);

        if (ammoInMagazine[weapon] <= 0)
        {
            TryReload();
            return;
        }

        if (weapon.bulletPrefab == null)
        {
            Debug.LogError("Este WeaponData no tiene bulletPrefab: " + weapon.weaponName);
            return;
        }

        ammoInMagazine[weapon]--;
        UpdateAmmoHUD();

        //Telemetry
        GameTelemetryEvents.ShotFired(weapon.weaponName);

        weaponRecoil?.PlayRecoil();

        Transform shootPoint = GetFirePoint(weapon);
        GameObject bullet = Instantiate(weapon.bulletPrefab, shootPoint.position, shootPoint.rotation);

        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.SetStats(weapon.bulletSpeed, GetFinalDamage(weapon.damage), KnockbackMultiplier, KnockbackTimeMultiplier, weapon);
        }

        cameraShake?.Shake(0.06f, 0.8f, 10f);
        PlayWeaponVFX(weapon);

        nextFireTime = Time.time + weapon.fireRate;
    }

    private void UseMelee(WeaponData weapon)
    {
        if (meleeAttack != null)
        {
            meleeAttack.TryMeleeAttack(weapon);
        }

        //Telemetry
        GameTelemetryEvents.MeleeAttackUsed(weapon.weaponName);
    }

    private Transform GetFirePoint(WeaponData weapon)
    {
        if (weaponPoints != null)
        {
            foreach (WeaponPointSlot slot in weaponPoints)
            {
                if (slot != null && slot.weaponData == weapon && slot.firePoint != null)
                {
                    return slot.firePoint;
                }
            }
        }

        return transform;
    }

    private Transform GetVFXPoint(WeaponData weapon)
    {
        if (weaponPoints != null)
        {
            foreach (WeaponPointSlot slot in weaponPoints)
            {
                if (slot != null && slot.weaponData == weapon && slot.vfxPoint != null)
                {
                    return slot.vfxPoint;
                }
            }
        }

        return transform;
    }

    public void PlayWeaponVFX(WeaponData weapon)
    {
        if (weapon == null || weapon.useVFXPrefab == null)
        {
            return;
        }

        Transform vfxPoint = GetVFXPoint(weapon);
        GameObject vfx = Instantiate(weapon.useVFXPrefab, vfxPoint.position, vfxPoint.rotation);

        if (weapon.attachVFXToFirePoint)
        {
            vfx.transform.SetParent(vfxPoint);
            vfx.transform.localPosition = Vector3.zero;
            vfx.transform.localRotation = Quaternion.identity;
        }

        Destroy(vfx, weapon.vfxLifetime);
    }

    private void ResetAmmo(WeaponData weapon)
    {
        if (weapon == null || weapon.attackMode != WeaponAttackMode.Ranged)
        {
            return;
        }

        ammoInMagazine[weapon] = weapon.magazineSize;
        reserveAmmo[weapon] = weapon.reserveAmmo;

        UpdateAmmoHUD();
    }

    public bool HasWeapon(WeaponData weapon)
    {
        return weapon != null && ownedWeapons.Contains(weapon);
    }

    public bool AddWeapon(WeaponData weapon)
    {
        if (weapon == null)
        {
            return false;
        }

        if (ownedWeapons.Contains(weapon))
        {
            Debug.Log("Ya tienes esta arma.");
            return false;
        }

        if (ownedWeapons.Count < maxOwnedWeapons)
        {
            ownedWeapons.Add(weapon);
            currentWeaponIndex = ownedWeapons.Count - 1;
            ApplyWeapon(CurrentWeapon);
            return true;
        }

        ownedWeapons[currentWeaponIndex] = weapon;
        ResetAmmo(weapon);
        InitAmmo(weapon);
        ApplyWeapon(CurrentWeapon);

        Debug.Log("Arma actual sustituida por: " + weapon.weaponName);

        return true;
    }

    public List<WeaponData> GetOwnedWeapons()
    {
        return ownedWeapons;
    }

    public bool CurrentWeaponIsAutomatic()
    {
        WeaponData weapon = CurrentWeapon;
        return weapon != null && weapon.automatic;
    }

    public bool CurrentWeaponIsMelee()
    {
        WeaponData weapon = CurrentWeapon;
        return weapon != null && weapon.attackMode == WeaponAttackMode.Melee;
    }

    public int GetFinalDamage(int baseDamage)
    {
        float multiplier = hasDoubleTapPerk ? doubleTapDamageMultiplier : 1f;
        return Mathf.CeilToInt(baseDamage * multiplier);
    }

    public float GetFinalMeleeCooldown(WeaponData weapon)
    {
        if (weapon == null)
        {
            return 1f;
        }

        float multiplier = hasMeleeCooldownPerk ? meleeCooldownMultiplier : 1f;
        return weapon.attackCooldown / multiplier;
    }

    private float GetFinalReloadTime(WeaponData weapon)
    {
        if (weapon == null)
        {
            return 1f;
        }

        float multiplier = hasReloadSpeedPerk ? reloadSpeedMultiplier : 1f;
        return weapon.reloadTime / multiplier;
    }

    private float GetReloadAnimatorSpeed(WeaponData weapon)
    {
        float finalReloadTime = GetFinalReloadTime(weapon);
        AnimationClip reloadClip = GetReloadClip();

        if (reloadClip != null)
        {
            return Mathf.Max(0.05f, reloadClip.length / finalReloadTime);
        }

        return Mathf.Max(0.05f, 1f / finalReloadTime);
    }

    private AnimationClip GetReloadClip()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return null;
        }

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip != null && clip.name.Contains(reloadClipNameContains))
            {
                return clip;
            }
        }

        return null;
    }

    public void ApplyDoubleTap()
    {
        if (hasDoubleTapPerk)
        {
            return;
        }

        hasDoubleTapPerk = true;

        if (showDebug)
        {
            Debug.Log("Double Tap aplicado. Daño x" + doubleTapDamageMultiplier);
        }
    }

    public void ApplyStrongKnockback()
    {
        if (hasStrongKnockbackPerk)
        {
            return;
        }

        hasStrongKnockbackPerk = true;

        if (showDebug)
        {
            Debug.Log("Strong Knockback aplicado. Distancia x" + strongKnockbackDistanceMultiplier + " Tiempo x" + strongKnockbackTimeMultiplier);
        }
    }

    public void ApplyReloadSpeed()
    {
        if (hasReloadSpeedPerk)
        {
            return;
        }

        hasReloadSpeedPerk = true;

        if (showDebug)
        {
            Debug.Log("Reload Speed aplicado x" + reloadSpeedMultiplier);
        }
    }

    public void ApplyMeleeAttackSpeed()
    {
        if (hasMeleeCooldownPerk)
        {
            return;
        }

        hasMeleeCooldownPerk = true;

        if (showDebug)
        {
            Debug.Log("Melee Cooldown aplicado x" + meleeCooldownMultiplier);
        }
    }
}