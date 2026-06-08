using TMPro;
using UnityEngine;

public class SoulManager : MonoBehaviour
{
    public static SoulManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text soulsText;

    [Header("Points")]
    [SerializeField] public int currentSouls = 0;

    [Header("Animator")]
    [SerializeField] private Animator scoreAnimator;

    [SerializeField] private GameObject ingameCanvas;

    private void Awake()
    {
        Debug.Log("SOULMANAGER AWAKE EN: " + gameObject.name);


        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("SoulManager duplicado destruido: " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (ingameCanvas != null)
            ingameCanvas.SetActive(true);

        UpdateUI();
    }

    public void AddSouls(int amount)
    {
        currentSouls += amount;

        if (currentSouls < 0)
            currentSouls = 0;

        UpdateUI();

        if (scoreAnimator != null)
            scoreAnimator.SetTrigger("Pulse");

        GameTelemetryEvents.SoulsGained(amount);
    }

    public bool TrySpendSouls(int amount)
    {
        if (currentSouls < amount)
            return false;

        currentSouls -= amount;

        UpdateUI();

        GameTelemetryEvents.SoulsSpent(amount);

        if (scoreAnimator != null)
            scoreAnimator.SetTrigger("Pulse");

        return true;
    }

    public int GetSouls()
    {
        return currentSouls;
    }

    private void UpdateUI()
    {
        if (soulsText == null)
        {
            Debug.LogWarning("SoulManager: soulsText no está asignado.");
            return;
        }

        soulsText.text = "Souls: " + currentSouls;
    }
    public void DebugAdd100Souls()
    {
        AddSouls(100);
    }

}