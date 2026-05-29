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
        if (data.playerDeaths > 0 ||
            data.PressureScore > 80)
        {
            return PlayerProfile.Overwhelmed;
        }

        if (data.MeleeUseRatio > 0.5f)
        {
            return PlayerProfile.Aggressive;
        }

        if (data.KillRate > 0.35f &&
            data.Accuracy > 0.60f)
        {
            return PlayerProfile.Dominant;
        }

        return PlayerProfile.Defensive;
    }
}
