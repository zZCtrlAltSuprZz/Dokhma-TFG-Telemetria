using UnityEngine;

[CreateAssetMenu(menuName = "Perks/Perk")]
public class PerkData : ScriptableObject
{
    public string perkName;
    public string description;

    public Sprite iconNormal;
    public Sprite iconHover;

    public PerkType perkType;
}