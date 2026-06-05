using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Footsteps")]
    [SerializeField] private AudioClip[] walkFootsteps;
    [SerializeField] private AudioClip[] runFootsteps;

    [Header("Damage")]
    //[SerializeField] private AudioClip damageClip;
    [SerializeField] private AudioClip deathClip;

    [Header("Footstep Settings")]
    [SerializeField] private float walkVolume = 0.55f;
    [SerializeField] private float runVolume = 0.75f;

    [Header("Damage Settings")]
    [SerializeField] private float deathVolume = 1f;

    [Header("Random")]
    [SerializeField] private float minPitch = 0.9f;
    [SerializeField] private float maxPitch = 1.1f;

    [Header("Movement")]
    [SerializeField] private AudioClip dashClip;
    [SerializeField] private float dashVolume = 0.8f;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlayFootstep(bool isRunning)
    {
        AudioClip[] clips = isRunning ? runFootsteps : walkFootsteps;
        float volume = isRunning ? runVolume : walkVolume;

        PlayRandom(clips, volume);
    }

    /*public void PlayDamage()
    {
        PlayOneShot(damageClip, damageVolume);
    }*/
    public void PlayDash()
    {
        PlayOneShot(dashClip, dashVolume);
    }

    public void PlayDeath()
    {
        PlayOneShot(deathClip, deathVolume);
    }

    private void PlayRandom(AudioClip[] clips, float volume)
    {
        if (audioSource == null) return;
        if (clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        PlayOneShot(clip, volume);
    }

    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (audioSource == null || clip == null) return;

        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(clip, volume);
    }

    // Weapons Audio
    public void PlayWeaponSound(WeaponData weapon)
    {
        if (weapon == null)
            return;

        if (weapon.attackSounds == null ||
            weapon.attackSounds.Length == 0)
            return;

        AudioClip clip =
            weapon.attackSounds[
                Random.Range(0, weapon.attackSounds.Length)
            ];

        audioSource.pitch =
            Random.Range(minPitch, maxPitch);

        audioSource.PlayOneShot(
            clip,
            weapon.attackVolume
        );
    }

    public void PlayReloadSound(WeaponData weapon)
    {
        if (weapon == null) return;
        PlayOneShot(weapon.reloadSound, weapon.reloadVolume);
    }

    public void PlayEquipSound(WeaponData weapon)
    {
        if (weapon == null) return;
        PlayOneShot(weapon.equipSound, weapon.equipVolume);
    }


}