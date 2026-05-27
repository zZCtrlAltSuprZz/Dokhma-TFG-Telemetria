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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        ingameCanvas.SetActive(true);
        Instance = this;
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
    }

    public bool TrySpendSouls(int amount)
    {
        if (currentSouls < amount)
            return false;

        currentSouls -= amount;
        UpdateUI();

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
        if (soulsText != null)
            soulsText.text = "Souls: " + currentSouls;
    }
}