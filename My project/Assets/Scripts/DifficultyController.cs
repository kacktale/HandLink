using UnityEngine;

[DisallowMultipleComponent]
public sealed class DifficultyController : MonoBehaviour
{
    private float initialSpawnInterval;
    private float minimumSpawnInterval;

    public float CurrentSpawnInterval { get; private set; }
    public float EnemyTravelDuration { get; private set; }

    public void Configure(
        float spawnInterval,
        float minSpawnInterval,
        float enemyTravelDuration)
    {
        initialSpawnInterval = Mathf.Max(0f, spawnInterval);
        minimumSpawnInterval = Mathf.Max(0f, minSpawnInterval);
        EnemyTravelDuration = Mathf.Max(0.01f, enemyTravelDuration);
        ResetDifficulty();
    }

    public void RegisterSpawn(float deltaTime)
    {
        CurrentSpawnInterval = Mathf.Max(
            minimumSpawnInterval,
            CurrentSpawnInterval - Mathf.Max(0f, deltaTime));
    }

    public void ResetDifficulty()
    {
        CurrentSpawnInterval = initialSpawnInterval;
    }
}
