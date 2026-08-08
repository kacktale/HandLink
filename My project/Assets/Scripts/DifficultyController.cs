using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class DifficultyController : MonoBehaviour
{
    private DifficultyConfig config;
    private float elapsedTime;
    private float legacyInitialSpawnInterval;
    private float legacyMinimumSpawnInterval;

    public float ElapsedTime => elapsedTime;
    public float CurrentSpawnInterval { get; private set; }
    public float EnemyTravelDuration { get; private set; }
    public float ScoreMultiplier { get; private set; } = 1f;
    public float PulseEnemyChance { get; private set; }
    public float HealingSpawnInterval { get; private set; } = 20f;
    public int MaxConcurrentEnemies =>
        config == null ? int.MaxValue : config.MaxConcurrentEnemies;
    public bool IsInitialProtectionActive =>
        config != null && elapsedTime < config.InitialProtectionDuration;

    public void Configure(
        DifficultyConfig difficultyConfig,
        float legacySpawnInterval,
        float legacyMinSpawnInterval,
        float legacyEnemyTravelDuration,
        float legacyPulseEnemyChance,
        float legacyHealingSpawnInterval)
    {
        config = difficultyConfig;
        legacyInitialSpawnInterval = Mathf.Max(0f, legacySpawnInterval);
        CurrentSpawnInterval = legacyInitialSpawnInterval;
        legacyMinimumSpawnInterval = Mathf.Max(0f, legacyMinSpawnInterval);
        EnemyTravelDuration = Mathf.Max(0.01f, legacyEnemyTravelDuration);
        PulseEnemyChance = Mathf.Clamp01(legacyPulseEnemyChance);
        HealingSpawnInterval = Mathf.Max(1f, legacyHealingSpawnInterval);
        ResetDifficulty();
    }

    public void Tick(float deltaTime)
    {
        if (config == null)
        {
            return;
        }

        elapsedTime += Mathf.Max(0f, deltaTime);
        RefreshEvaluatedValues();
    }

    public void RegisterSpawn(float deltaTime)
    {
        if (config != null)
        {
            return;
        }

        CurrentSpawnInterval = Mathf.Max(
            legacyMinimumSpawnInterval,
            CurrentSpawnInterval - Mathf.Max(0f, deltaTime));
    }

    public bool CanSpawn(int activeEnemyCount)
    {
        return activeEnemyCount < MaxConcurrentEnemies;
    }

    public long ApplyScoreMultiplier(long score)
    {
        if (score <= 0L || ScoreMultiplier <= 0f)
        {
            return 0L;
        }

        double scaledScore = score * (double)ScoreMultiplier;
        if (scaledScore >= long.MaxValue)
        {
            return long.MaxValue;
        }

        return Math.Max(
            0L,
            (long)Math.Round(
                scaledScore,
                MidpointRounding.AwayFromZero));
    }

    public void ResetDifficulty()
    {
        elapsedTime = 0f;
        if (config == null)
        {
            CurrentSpawnInterval = legacyInitialSpawnInterval;
            return;
        }

        RefreshEvaluatedValues();
    }

    private void RefreshEvaluatedValues()
    {
        if (config == null)
        {
            return;
        }

        CurrentSpawnInterval = config.EvaluateSpawnInterval(elapsedTime);
        EnemyTravelDuration = config.EvaluateEnemyTravelTime(elapsedTime);
        ScoreMultiplier = config.EvaluateScoreMultiplier(elapsedTime);
        PulseEnemyChance = config.EvaluatePulseEnemyChance(elapsedTime);
        HealingSpawnInterval =
            config.EvaluateHealingSpawnInterval(elapsedTime);
    }
}
