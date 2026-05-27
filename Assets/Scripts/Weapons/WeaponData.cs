using UnityEngine;

public enum WeaponAttackMode
{
    Ranged,
    Melee
}

[CreateAssetMenu(menuName = "Juego/Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public WeaponType weaponType;
    public string weaponName;

    [Header("HUD")]
    public string hudText;

    [Header("Attack")]
    public WeaponAttackMode attackMode;
    public bool automatic;

    [Header("Ranged")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 15f;
    public float fireRate = 0.2f;

    [Header("Ammo")]
    public int magazineSize = 8;
    public int reserveAmmo = 70;
    public float reloadTime = 1f;

    [Header("Melee")]
    public float attackCooldown = 0.5f;
    public float attackRange = 1.5f;
    public int damage = 1;
    public float forwardOffset = 1f;
    public LayerMask enemyLayer;

    [Header("VFX")]
    public GameObject useVFXPrefab;
    public float vfxLifetime = 2f; 
    public bool attachVFXToFirePoint = false;


    [Header("Animator")]
    public bool usesPistolPose;
}