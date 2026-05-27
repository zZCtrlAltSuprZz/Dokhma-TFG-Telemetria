using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PerkHUD : MonoBehaviour
{
    [System.Serializable]
    public class HUDSlot
    {
        public GameObject rootObject;
        public Image icon;
    }

    [Header("HUD Slots")]
    [SerializeField] private HUDSlot[] hudSlots;

    private readonly List<PerkData> activePerks = new List<PerkData>();

    private void Start()
    {
        RefreshHUD();
    }

    public void ActivatePerkIcon(PerkData perkData)
    {
        if (perkData == null) return;

        if (activePerks.Contains(perkData))
        {
            Debug.LogWarning("Perk ya activado: " + perkData.perkName);
            return;
        }

        if (activePerks.Count >= hudSlots.Length)
        {
            Debug.LogWarning("No hay más espacio en el HUD");
            return;
        }

        activePerks.Add(perkData);
        RefreshHUD();

        Debug.Log("Perk añadido al HUD: " + perkData.perkName);
    }

    public void DeactivatePerkIconByType(PerkType perkType)
    {
        for (int i = activePerks.Count - 1; i >= 0; i--)
        {
            if (activePerks[i] != null && activePerks[i].perkType == perkType)
            {
                Debug.Log("Perk eliminado del HUD: " + activePerks[i].perkName);
                activePerks.RemoveAt(i);
                RefreshHUD();
                return;
            }
        }

        Debug.LogWarning("No se encontró el perk en el HUD: " + perkType);
    }

    private void RefreshHUD()
    {
        for (int i = 0; i < hudSlots.Length; i++)
        {
            if (hudSlots[i] == null) continue;

            if (i < activePerks.Count)
            {
                if (hudSlots[i].rootObject != null)
                    hudSlots[i].rootObject.SetActive(true);

                if (hudSlots[i].icon != null)
                    hudSlots[i].icon.sprite = activePerks[i].iconNormal;
            }
            else
            {
                if (hudSlots[i].rootObject != null)
                    hudSlots[i].rootObject.SetActive(false);

                if (hudSlots[i].icon != null)
                    hudSlots[i].icon.sprite = null;
            }
        }
    }
}