using UnityEngine;
using UnityEngine.UI;

public class NailsHUD : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite nailObtained;
    [SerializeField] private Sprite nailNotObtained;

    [Header("Slots")]
    [SerializeField] private Image[] nailSlots; // 4 slots

    private int nailsCollected = 3;

    private void Start()
    {

        RefreshHUD();
    }

    public void CollectNail()
    {
        if (nailsCollected >= nailSlots.Length) return;

        nailsCollected++;
        RefreshHUD();
    }

    private void RefreshHUD()
    {
        for (int i = 0; i < nailSlots.Length; i++)
        {
            if (nailSlots[i] == null) continue;

            nailSlots[i].sprite = i < nailsCollected ? nailObtained : nailNotObtained;
            nailSlots[i].color = Color.white; // asegura que son visibles
        }
    }
}