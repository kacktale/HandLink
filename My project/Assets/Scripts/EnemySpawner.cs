using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemySpawner : MonoBehaviour
{
    private Transform[] spawnPositions;
    private Transform target;
    private EnemyPool enemyPool;
    private DifficultyController difficulty;
    private GameplayAspectController aspectController;
    private float spawnElapsed;
    private float healingElapsed;

    public void Configure(
        Transform[] enemySpawnPositions,
        Transform enemyTarget,
        EnemyPool pool,
        DifficultyController difficultyController,
        GameplayAspectController gameplayAspectController)
    {
        spawnPositions = enemySpawnPositions;
        target = enemyTarget;
        enemyPool = pool;
        difficulty = difficultyController;
        aspectController = gameplayAspectController;
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

        difficulty.Tick(deltaTime);
        spawnElapsed += deltaTime;
        healingElapsed += deltaTime;

        if (difficulty.IsInitialProtectionActive)
        {
            return;
        }

        if (healingElapsed >= difficulty.HealingSpawnInterval &&
            difficulty.CanSpawn(enemyPool.ActiveCount))
        {
            Spawn(enemyPool.RentSpecial(SpecialEnemyType.HeartHealer));
            healingElapsed = 0f;
        }

        if (spawnElapsed < difficulty.CurrentSpawnInterval)
        {
            return;
        }

        if (!difficulty.CanSpawn(enemyPool.ActiveCount))
        {
            return;
        }

        difficulty.RegisterSpawn(deltaTime);
        GameObject enemy = Random.value < difficulty.PulseEnemyChance
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
        return ConfigureTutorialEnemy(
            enemyPool.RentNormal(0),
            position,
            targetPosition,
            travelDuration);
    }

public GameObject SpawnTutorialSpecialEnemy(
        SpecialEnemyType type,
        Vector2 position,
        Vector2 targetPosition,
        float travelDuration)
    {
        return ConfigureTutorialEnemy(
            enemyPool.RentSpecial(type),
            position,
            targetPosition,
            travelDuration);
    }

private static GameObject ConfigureTutorialEnemy(
        GameObject enemy,
        Vector2 position,
        Vector2 targetPosition,
        float travelDuration)
    {
        if (enemy == null)
        {
            return null;
        }

        enemy.transform.position = position;
        Vector2 direction = targetPosition - position;
        if (direction.sqrMagnitude > 0.0001f)
        {
            float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            enemy.transform.rotation = Quaternion.Euler(0f, 0f, rotation);
        }

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
        Vector2 desiredSpawnPosition = spawnPosition.position;
        Vector2 spawnDirection = desiredSpawnPosition - (Vector2)target.position;
        if (aspectController != null &&
            aspectController.TryGetSpawnPosition(spawnDirection, out Vector2 aspectSpawnPosition))
        {
            desiredSpawnPosition = aspectSpawnPosition;
        }

        enemy.transform.position = desiredSpawnPosition;
        float travelDuration = aspectController == null
            ? difficulty.EnemyTravelDuration
            : aspectController.GetAspectAdjustedTravelDuration(
                desiredSpawnPosition,
                target.position,
                difficulty.EnemyTravelDuration);
        enemy.GetComponent<Enemy>().SetTravelDuration(target.position, travelDuration);

        Vector2 direction = target.position - enemy.transform.position;
        float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        enemy.transform.rotation = Quaternion.Euler(0f, 0f, rotation);
    }
}
