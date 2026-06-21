using System.Collections.Generic;

public class OfflineProgressionResult
{
    public bool IsValid;

    public long   TimeAwaySeconds;
    public int    XpGained;
    public int    PotionsConsumed;
    public bool   PlayerDied;

    public List<ItemSaveEntry> ItemsGained = new();

    public string TargetDisplayName;
    public string TargetType; // "enemy" | "resource"
    public string TargetName; // SO asset name for registry lookup
    public int    KillsEarned;
}
