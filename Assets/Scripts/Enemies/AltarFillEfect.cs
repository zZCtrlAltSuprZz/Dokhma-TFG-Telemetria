using UnityEngine;

public class AltarFillEffect : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private RitualAltar altar;
    [SerializeField] private Renderer altarRenderer;

    [Header("Fill")]
    [SerializeField] private string fillProperty = "_FillAmount";
    [SerializeField] private float fillSpeed = 2f;

    [Header("Color del líquido")]
    [SerializeField] private Color fillColor = new Color(0.2f, 0.6f, 1f, 1f);
    [SerializeField] private float emissionIntensity = 2f;

    private Material mat;
    private float targetFill;
    private float currentFill;

    private void Start()
    {
        if (altarRenderer != null)
            mat = altarRenderer.material; // instancia propia, no afecta otros altares

        if (mat != null)
        {
            mat.SetFloat(fillProperty, 0f);
            mat.SetColor("_FillColor", fillColor * emissionIntensity);
            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
    }

    private void Update()
    {
        if (altar == null || mat == null) return;

        // Calcular el fill objetivo
        if (altar.RitualCompleted)
            targetFill = 1f;
        else if (altar.RitualActive && altar.KillsRequired > 0)
            targetFill = (float)altar.CurrentKills / altar.KillsRequired;
        else
            targetFill = 0f;

        // Animar suavemente hacia el objetivo
        currentFill = Mathf.MoveTowards(currentFill, targetFill, fillSpeed * Time.deltaTime);
        mat.SetFloat(fillProperty, currentFill);
        mat.SetColor("_GlowColor", fillColor * emissionIntensity * currentFill);
        DynamicGI.SetEmissive(altarRenderer, fillColor * emissionIntensity * currentFill);
    }
}