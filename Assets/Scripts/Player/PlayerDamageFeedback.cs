using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDamageFeedback : MonoBehaviour
{
    [Header("Damage Levels")]
    [SerializeField] private RawImage[] bloodImages;

    [Header("Death Overlay")]
    [SerializeField] private RawImage deathOverlay;

    [Header("Flash")]
    [SerializeField] private bool damageFlash = true;
    [SerializeField] private Color damageFlashColor = Color.white;
    [SerializeField] private float damageFlashDuration = 0.1f;
    [SerializeField] private float damageFlashInterval = 0.03f;

    [Header("Smooth Fade")]
    [SerializeField] private float fadeSpeed = 4f;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private Renderer[] renderers;
    private MaterialPropertyBlock mpb;
    private Color[] originalColors;

    private Coroutine damageFlashRoutine;
    private Coroutine fadeRoutine;

    private float[] targetAlphas;

    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        mpb = new MaterialPropertyBlock();
        originalColors = new Color[renderers.Length];

        targetAlphas = new float[bloodImages.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];

            if (r == null || r.sharedMaterial == null || !r.sharedMaterial.HasProperty(ColorId))
                originalColors[i] = Color.white;
            else
                originalColors[i] = r.sharedMaterial.color;
        }

        // Inicializar overlays invisibles
        for (int i = 0; i < bloodImages.Length; i++)
        {
            if (bloodImages[i] != null)
            {
                SetAlpha(bloodImages[i], 0f);
                targetAlphas[i] = 0f;
            }
        }

        if (deathOverlay != null)
            SetAlpha(deathOverlay, 0f);
    }

    public void UpdateBlood(int currentLives, int maxLives)
    {
        if (maxLives <= 0) return;

        float healthPercent = (float)currentLives / maxLives;
        float damagePercent = 1f - healthPercent;

        int visibleBloodImages = Mathf.CeilToInt(damagePercent * bloodImages.Length);
        visibleBloodImages = Mathf.Clamp(visibleBloodImages, 0, bloodImages.Length);

        if (showDebug)
            Debug.Log("Health: " + currentLives + "/" + maxLives + " DamagePercent: " + damagePercent);

        for (int i = 0; i < bloodImages.Length; i++)
        {
            targetAlphas[i] = (i < visibleBloodImages) ? 1f : 0f;
        }

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        bool done = false;

        while (!done)
        {
            done = true;

            for (int i = 0; i < bloodImages.Length; i++)
            {
                if (bloodImages[i] == null) continue;

                float current = bloodImages[i].color.a;
                float target = targetAlphas[i];

                float newAlpha = Mathf.Lerp(current, target, Time.deltaTime * fadeSpeed);

                SetAlpha(bloodImages[i], newAlpha);

                if (Mathf.Abs(newAlpha - target) > 0.01f)
                    done = false;
            }

            yield return null;
        }

        fadeRoutine = null;
    }

    public void ClearDeathOverlaySmooth()
    {
        if (deathOverlay == null) return;

        StartCoroutine(FadeDeathOverlayRoutine(0f));
    }

    private IEnumerator FadeDeathOverlayRoutine(float targetAlpha)
    {
        float currentAlpha = deathOverlay.color.a;

        while (Mathf.Abs(currentAlpha - targetAlpha) > 0.01f)
        {
            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
            SetAlpha(deathOverlay, currentAlpha);
            yield return null;
        }

        SetAlpha(deathOverlay, targetAlpha);
    }

    public void ClearBloodSmooth()
    {
        for (int i = 0; i < targetAlphas.Length; i++)
            targetAlphas[i] = 0f;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine());
    }

    public void SetFullBlood()
    {
        for (int i = 0; i < targetAlphas.Length; i++)
            targetAlphas[i] = 1f;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine());

        if (deathOverlay != null)
            SetAlpha(deathOverlay, 1f);
    }

    public void PlayDamageFeedback()
    {
        if (!damageFlash) return;

        if (damageFlashRoutine != null)
            StopCoroutine(damageFlashRoutine);

        damageFlashRoutine = StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        float elapsed = 0f;
        bool on = true;

        while (elapsed < damageFlashDuration)
        {
            if (on) SetColorAll(damageFlashColor);
            else RestoreOriginalColors();

            on = !on;
            yield return new WaitForSeconds(damageFlashInterval);
            elapsed += damageFlashInterval;
        }

        RestoreOriginalColors();
        damageFlashRoutine = null;
    }

    private void SetAlpha(RawImage img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    private void SetColorAll(Color c)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r == null) continue;

            r.GetPropertyBlock(mpb);
            mpb.SetColor(ColorId, c);
            r.SetPropertyBlock(mpb);
        }
    }

    private void RestoreOriginalColors()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r == null) continue;

            r.GetPropertyBlock(mpb);
            mpb.SetColor(ColorId, originalColors[i]);
            r.SetPropertyBlock(mpb);
        }
    }
}