using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemySpawner : MonoBehaviour
{
    private Transform[] spawnPositions;
    private Transform target;
    private EnemyPool enemyPool;
    private DifficultyController difficulty;
    private float pulseEnemyChance;
    private float healingSpawnInterval;
    private float spawnElapsed;
    private float healingElapsed;

    public void Configure(
        Transform[] enemySpawnPositions,
        Transform enemyTarget,
        EnemyPool pool,
        DifficultyController difficultyController,
        float pulseChance,
        float healingInterval)
    {
        spawnPositions = enemySpawnPositions;
        target = enemyTarget;
        enemyPool = pool;
        difficulty = difficultyController;
        pulseEnemyChance = Mathf.Clamp01(pulseChance);
        healingSpawnInterval = Mathf.Max(1f, healingInterval);
        ResetSpawner();
    }

    public void Tick(float deltaTime, bool inputHeld, bool tutorialMode)
    {
        if (GameManager.Instance == null ||
            !GameManager.Instance.IsGameplayActive ||
            !inputHeld)
        {
            return;
        }

        transform.Rotate(0f, 0f, 1f);
        if (tutorialMode)
        {
            return;
        }

        spawnElapsed += deltaTime;
        healingElapsed += deltaTime;

        if (healingElapsed >= healingSpawnInterval)
        {
            Spawn(enemyPool.RentSpecial(SpecialEnemyType.HeartHealer));
            healingElapsed = 0f;
        }

        if (spawnElapsed < difficulty.CurrentSpawnInterval)
        {
            return;
        }

        difficulty.RegisterSpawn(deltaTime);
        GameObject enemy = Random.value < pulseEnemyChance
            ? enemyPool.RentSpecial(SpecialEnemyType.Pulse)
            : enemyPool.RentNormal(Random.Range(0, enemyPool.NormalTypeCount));
        Spawn(enemy);
        spawnElapsed = 0f;
    }

    public void ResetSpawner()
    {
        spawnElapsed = 0f;
        healingElapsed = 0f;
    }

    public GameObject SpawnTutorialEnemy(
        Vector2 position,
        Vector2 targetPosition,
        float travelDuration)
    {
        GameObject enemy = enemyPool.RentNormal(0);
        if (enemy == null)
        {
            return null;
        }

        enemy.transform.position = position;
        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if (travelDuration <= 0f)
        {
            enemyComponent.targetPos = targetPosition;
            enemyComponent.speed = 0f;
        }
        else
        {
            enemyComponent.SetTravelDuration(targetPosition, travelDuration);
        }

        return enemy;
    }

    private void Spawn(GameObject enemy)
    {
        if (enemy == null || spawnPositions == null || spawnPositions.Length == 0)
        {
            return;
        }

        Transform spawnPosition = spawnPositions[Random.Range(0, spawnPositions.Length)];
        enemy.transform.position = spawnPosition.position;
        enemy.GetComponent<Enemy>().SetTravelDuration(
            target.position,
            difficulty.EnemyTravelDuration);

        Vector2 direction = target.position - enemy.transform.position;
        float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        enemy.transform.rotation = Quaternion.Euler(0f, 0f, rotation);
    }
}
