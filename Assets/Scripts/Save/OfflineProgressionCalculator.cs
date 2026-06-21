using System;
using System.Collections.Generic;
using UnityEngine;

public static class OfflineProgressionCalculator
{
    private const long MinAfkSeconds = 5 * 60;    // 5 minutes
    private const long MaxAfkSeconds = 3 * 3600;  // 3 hours

    // Skill parameters (from DESIGN §7)
    private const float SlashDamageMultiplier    = 2f;
    private const float SlashCooldown            = 8f;
    private const float FireballDamageMultiplier = 3f;
    private const float FireballCooldown         = 10f;
    // Barrier: ~20% of time fully blocked → multiply enemy DPS by 0.8
    private const float BarrierMitigation = 0.8f;

    public static OfflineProgressionResult Calculate(
        SaveData data,
        SaveRegistry registry,
        PlayerStatsData statsData,
        long currentTimestamp)
    {
        var result = new OfflineProgressionResult();

        if (data == null || registry == null || statsData == null)
            return result;
        if (data.lastTargetType != "enemy" && data.lastTargetType != "resource")
            return result;
        if (data.saveTimestamp <= 0)
            return result;

        long rawAway = currentTimestamp - data.saveTimestamp;
        result.TimeAwaySeconds = rawAway;

        if (rawAway < MinAfkSeconds)
            return result;

        long cappedAway = Math.Min(rawAway, MaxAfkSeconds);

        result.TargetDisplayName = data.lastTargetDisplayName;
        result.TargetType        = data.lastTargetType;
        result.TargetName        = data.lastTargetName;

        // Derive stats mirroring PlayerStats formulas
        ItemData weapon = registry.FindItem(data.equippedWeapon);
        ItemData shield = registry.FindItem(data.equippedShield);
        ItemData potion = string.IsNullOrEmpty(data.equippedPotion)
            ? null
            : registry.FindItem(data.equippedPotion);

        int weaponBonus = weapon != null ? weapon.AttackBonus  : 0;
        int shieldBonus = shield != null ? shield.DefenseBonus : 0;

        int totalAttack  = Mathf.RoundToInt(
            (statsData.BaseAttackDamage + weaponBonus) * (1f + data.strengthTier * 0.1f));
        int totalDefense = Mathf.RoundToInt(
            shieldBonus * (1f + data.resilienceTier * 0.1f));
        int maxHP = Mathf.RoundToInt(
            (statsData.BaseMaxHealth + (data.level - 1) * statsData.HealthPerLevel)
            * (1f + data.vitalityTier * 0.1f));
        int gatherDamage   = statsData.BaseGatheringDamage + Mathf.Min(data.yieldTier, 4);
        float gatherCooldown = statsData.GatherCooldown * (data.yieldTier >= 5 ? 0.8f : 1f);

        bool slashUnlocked    = weapon != null;
        bool barrierUnlocked  = shield != null;
        bool fireballUnlocked = data.fireballUnlocked;

        if (data.lastTargetType == "resource")
        {
            ResourceData node = registry.FindResource(data.lastTargetName);
            if (node == null)
                return result;

            CalculateGathering(result, node, gatherDamage, gatherCooldown, cappedAway);
        }
        else
        {
            EnemyData enemy = registry.FindEnemy(data.lastTargetName);
            if (enemy == null)
                return result;

            CalculateCombat(
                result, enemy,
                totalAttack, totalDefense, maxHP,
                statsData.AttackCooldown,
                slashUnlocked, barrierUnlocked, fireballUnlocked,
                potion, data.equippedPotionQty,
                cappedAway);
        }

        result.IsValid = true;
        return result;
    }

    #region Private Helpers

    private static void CalculateGathering(
        OfflineProgressionResult result,
        ResourceData node,
        int gatherDamage,
        float gatherCooldown,
        long timeAway)
    {
        int   hitsPerDrop = Mathf.CeilToInt((float)node.MaxHealth / gatherDamage);
        float timePerDrop = hitsPerDrop * gatherCooldown;
        int   drops       = Mathf.FloorToInt(timeAway / timePerDrop);

        result.XpGained = drops * node.XpReward;

        if (node.DropItem != null && node.DropQuantity > 0 && drops > 0)
            result.ItemsGained.Add(new ItemSaveEntry
            {
                itemName = node.DropItem.name,
                quantity = drops * node.DropQuantity,
            });
    }

    private static void CalculateCombat(
        OfflineProgressionResult result,
        EnemyData enemy,
        int totalAttack,
        int totalDefense,
        int maxHP,
        float attackCooldown,
        bool slashUnlocked,
        bool barrierUnlocked,
        bool fireballUnlocked,
        ItemData potion,
        int potionQty,
        long timeAway)
    {
        // Player DPS including skill contributions (DESIGN §10)
        float playerDPS = totalAttack / attackCooldown;
        if (slashUnlocked)    playerDPS += SlashDamageMultiplier    * totalAttack / SlashCooldown;
        if (fireballUnlocked) playerDPS += FireballDamageMultiplier * totalAttack / FireballCooldown;

        // Guard against zero DPS (no weapon, edge case)
        float tKill     = playerDPS > 0f ? enemy.MaxHealth / playerDPS : float.MaxValue;
        float cycleTime = tKill + enemy.RespawnCooldown;

        // Enemy DPS after defense, barrier mitigation
        float rawEnemyDPS = (enemy.AttackCooldown > 0f && enemy.AttackDamage > 0)
            ? Mathf.Max(0f, enemy.AttackDamage - totalDefense) / enemy.AttackCooldown
            : 0f;
        float enemyDPS = barrierUnlocked ? rawEnemyDPS * BarrierMitigation : rawEnemyDPS;

        float dmgPerCycle = enemyDPS * tKill;

        int  cycles        = 0;
        bool playerDied    = false;
        int  potionsConsumed = 0;

        if (dmgPerCycle <= 0f)
        {
            cycles = Mathf.FloorToInt(timeAway / cycleTime);
        }
        else
        {
            float lifeCyclesFromFullHp = Mathf.Floor(maxHP / dmgPerCycle);
            float lifeCyclesFromPotion = potion != null
                ? Mathf.Floor((float)potion.HealAmount / dmgPerCycle)
                : 0f;
            float maxSurvivableCycles = lifeCyclesFromFullHp
                                      + potionQty * lifeCyclesFromPotion;
            float timeToDeath   = maxSurvivableCycles * cycleTime;

            playerDied  = timeAway > timeToDeath;
            float effectiveTime = Mathf.Min(timeAway, timeToDeath);
            cycles = Mathf.FloorToInt(effectiveTime / cycleTime);

            if (playerDied)
            {
                potionsConsumed = potionQty;
            }
            else if (lifeCyclesFromFullHp > 0f)
            {
                // After the first free HP pool, every lifeCyclesFromFullHp kills costs 1 potion
                potionsConsumed = Mathf.Min(
                    Mathf.FloorToInt(cycles / lifeCyclesFromFullHp),
                    potionQty);
            }
        }

        result.PlayerDied      = playerDied;
        result.PotionsConsumed = potionsConsumed;
        result.KillsEarned     = cycles;
        result.XpGained        = cycles * enemy.XpReward;

        // Expected drops — no RNG, use expected value and round at collection
        if (cycles > 0)
        {
            foreach (DropTableEntry entry in enemy.DropTable)
            {
                if (entry.Item == null) continue;
                float expectedQty = (float)cycles * entry.Quantity / entry.Chance;
                int rounded = Mathf.RoundToInt(expectedQty);
                if (rounded > 0)
                    result.ItemsGained.Add(new ItemSaveEntry
                    {
                        itemName = entry.Item.name,
                        quantity = rounded,
                    });
            }
        }
    }

    #endregion
}
