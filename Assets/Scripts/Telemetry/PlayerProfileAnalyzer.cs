using UnityEngine;

public enum PlayerProfile
{
    Dominant,
    Aggressive,
    Defensive,
    Overwhelmed
}

public static class PlayerProfileAnalyzer
{
    public static PlayerProfile Analyze(WaveTelemetryData data)
    {
        float killRate = Mathf.Clamp01(data.KillRate/ 0.5f);
        float pressure = Mathf.Clamp01(data.PressureScore / 100f);
        float accuracy = Mathf.Clamp01(data.Accuracy);
        float meleeUse = Mathf.Clamp01(data.MeleeUseRatio);
        float meleeAccuracy = Mathf.Clamp01(data.MeleeAccuracy);

        float damageEvents = Mathf.Clamp01(data.damageEvents / 8f);
        float damageStreak = Mathf.Clamp01(data.maxDamageStreak / 4f);
        float autoHeals = Mathf.Clamp01(data.automaticHeals / 4f);
        float reviveUsed = data.playerRevives > 0 ? 1f : 0f;

        //Player Profiles 
        float dominantScore =       killRate * 0.3f +
                                    accuracy * 0.25f +
                                    (1f - pressure) * 0.25f +
                                    (1f - damageStreak) * 0.1f +
                                    (1f - autoHeals) * 0.1f;

        float overwhelmedScore =    pressure * 0.35f +
                                    damageEvents * 0.2f +
                                    damageStreak * 0.2f +
                                    autoHeals * 0.15f + 
                                    reviveUsed * 0.1f;

        float aggressiveScore =     meleeUse * 0.4f +
                                    meleeAccuracy * 0.2f +
                                    killRate * 0.2f +
                                    pressure * 0.2f;

        float defensiveScore =      (1f - pressure) * 0.35f +
                                    (1f - meleeUse) * 0.25f +
                                    (1f - killRate) * 0.20f +
                                    accuracy * 0.2f;

        Debug.Log(  $"[PROFILE SCORES]\n" +
                    $"Dominant: {dominantScore:F2}\n" +
                    $"Overwhelmed: {overwhelmedScore:F2}\n" +
                    $"Aggressive: {aggressiveScore:F2}\n" +
                    $"Defensive: {defensiveScore:F2}");

        float maxScore = dominantScore;
        PlayerProfile selectedProfile = PlayerProfile.Dominant;

        if (overwhelmedScore > maxScore)
        {
            maxScore = overwhelmedScore;
            selectedProfile = PlayerProfile.Overwhelmed;
        }

        if (aggressiveScore > maxScore)
        {
            maxScore = aggressiveScore;
            selectedProfile = PlayerProfile.Aggressive;
        }

        if (defensiveScore > maxScore)
        {
            maxScore = defensiveScore;
            selectedProfile = PlayerProfile.Defensive;
        }

        return selectedProfile;

    }
}
