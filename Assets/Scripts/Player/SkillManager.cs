using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillManager : MonoBehaviour
{
    [System.Serializable]
    public class SkillData
    {
        public string skillName;
        public Sprite icon;
    }

    [Header("HUD Slots (orden visual)")]
    [SerializeField] private Image[] hudSlots;

    [SerializeField] private Sprite testSprite1;
    [SerializeField] private Sprite testSprite2;

    private List<SkillData> obtainedSkills = new List<SkillData>();

    void Start()
    {
        // Test skills (remove in production)
        AddSkill(testSprite1, "Fireball");
        AddSkill(testSprite2, "Ice Shard");
    }
    public void AddSkill(Sprite icon, string name = "Skill")
    {
        if (obtainedSkills.Count >= hudSlots.Length)
        {
            Debug.LogWarning("No more HUD slots available");
            return;
        }

        SkillData newSkill = new SkillData
        {
            skillName = name,
            icon = icon
        };

        obtainedSkills.Add(newSkill);

        RefreshHUD();
    }

    private void RefreshHUD()
    {
        for (int i = 0; i < hudSlots.Length; i++)
        {
            if (i < obtainedSkills.Count)
            {
                hudSlots[i].sprite = obtainedSkills[i].icon;
                hudSlots[i].color = Color.white;
            }
            else
            {
                hudSlots[i].color = Color.clear;
            }
        }
    }
}