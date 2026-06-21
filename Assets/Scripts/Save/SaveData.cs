using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int level          = 1;
    public int currentXp      = 0;
    public int currentHealth  = 0;
    public int playerClass    = 0;

    public int strengthTier   = 0;
    public int resilienceTier = 0;
    public int vitalityTier   = 0;
    public int yieldTier      = 0;

    public bool fireballUnlocked = false;

    public string equippedWeapon    = "";
    public string equippedShield    = "";
    public string equippedPotion    = "";
    public int    equippedPotionQty = 0;

    public List<ItemSaveEntry>  inventory  = new();
    public List<QuestSaveEntry> quests     = new();
    public List<EnemyKillEntry> enemyKills = new();

    // AFK / offline progression
    public string lastScene             = "";
    public string lastTargetType        = "none"; // "none" | "enemy" | "resource"
    public string lastTargetName        = "";     // SO asset name for registry lookup
    public string lastTargetDisplayName = "";     // human-readable display name
    public long   saveTimestamp         = 0L;     // UTC Unix seconds
}

[Serializable]
public class ItemSaveEntry
{
    public string itemName;
    public int    quantity;
}

[Serializable]
public class QuestSaveEntry
{
    public string questName;
    public int    state;     // QuestState enum value
    public int    killCount; // only meaningful for Kill-type quests
}

[Serializable]
public class EnemyKillEntry
{
    public string enemyName;
    public int    killCount;
}
