using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyPool : MonoBehaviour
{
    private readonly List<List<GameObject>> normalPools = new();
    private readonly List<GameObject> pulsePool = new();
    private readonly List<GameObject> healingPool = new();

    private GameObject[] prefabs;
    private Transform poolParent;
    private Vector3 creationPosition;

    public int NormalTypeCount => normalPools.Count;

    public int TotalCount
    {
        get
        {
            int count = pulsePool.Count + healingPool.Count;
            foreach (List<GameObject> pool in normalPools)
            {
                count += pool.Count;
            }

            return count;
        }
    }

    public int ActiveCount
    {
        get
        {
            int count = CountActive(pulsePool) + CountActive(healingPool);
            foreach (List<GameObject> pool in normalPools)
            {
                count += CountActive(pool);
            }

            return count;
        }
    }

    public void Initialize(
        GameObject[] enemyPrefabs,
        Transform parent,
        Vector3 spawnPosition,
        int normalPoolSize,
        int pulsePoolSize,
        int healingPoolSize)
    {
        prefabs = enemyPrefabs;
        poolParent = parent;
        creationPosition = spawnPosition;
        normalPools.Clear();
        pulsePool.Clear();
        healingPool.Clear();

        if (prefabs == null)
        {
            return;
        }

        for (int prefabIndex = 0; prefabIndex < prefabs.Length; prefabIndex++)
        {
            List<GameObject> pool = new();
            normalPools.Add(pool);
            for (int index = 0; index < normalPoolSize; index++)
            {
                pool.Add(CreateNormal(prefabIndex));
            }
        }

        PrewarmSpecial(pulsePool, SpecialEnemyType.Pulse, pulsePoolSize);
        PrewarmSpecial(
            healingPool,
            SpecialEnemyType.HeartHealer,
            healingPoolSize);
    }

    public GameObject RentNormal(int prefabIndex)
    {
        if (prefabIndex < 0 || prefabIndex >= normalPools.Count)
        {
            return null;
        }

        return Rent(normalPools[prefabIndex], () => CreateNormal(prefabIndex));
    }

    public GameObject RentSpecial(SpecialEnemyType type)
    {
        return type switch
        {
            SpecialEnemyType.Pulse =>
                Rent(pulsePool, () => CreateSpecial(type)),
            SpecialEnemyType.HeartHealer =>
                Rent(healingPool, () => CreateSpecial(type)),
            _ => null
        };
    }

    public void ReturnAll()
    {
        foreach (List<GameObject> pool in normalPools)
        {
            SetActive(pool, false);
        }

        SetActive(pulsePool, false);
        SetActive(healingPool, false);
    }

    public void RefreshJudgementDistances()
    {
        foreach (List<GameObject> pool in normalPools)
        {
            foreach (GameObject enemyObject in pool)
            {
                enemyObject.GetComponent<Enemy>()?.RefreshJudgeDistance();
            }
        }

        RefreshJudgementDistances(pulsePool);
        RefreshJudgementDistances(healingPool);
    }

    public Enemy GetJudgementSettings()
    {
        return prefabs != null && prefabs.Length > 0
            ? prefabs[0].GetComponent<Enemy>()
            : null;
    }

    private GameObject CreateNormal(int prefabIndex)
    {
        GameObject enemy = Instantiate(
            prefabs[prefabIndex],
            creationPosition,
            Quaternion.identity,
            poolParent);
        enemy.SetActive(false);
        return enemy;
    }

    private GameObject CreateSpecial(SpecialEnemyType type)
    {
        GameObject enemy = Instantiate(
            prefabs[0],
            creationPosition,
            Quaternion.identity,
            poolParent);
        SpecialEnemy specialEnemy = enemy.GetComponent<SpecialEnemy>();
        if (specialEnemy == null)
        {
            specialEnemy = enemy.AddComponent<SpecialEnemy>();
        }

        specialEnemy.Configure(type);
        enemy.SetActive(false);
        return enemy;
    }

    private void PrewarmSpecial(
        List<GameObject> pool,
        SpecialEnemyType type,
        int poolSize)
    {
        for (int index = 0; index < poolSize; index++)
        {
            pool.Add(CreateSpecial(type));
        }
    }

    private static GameObject Rent(
        List<GameObject> pool,
        System.Func<GameObject> create)
    {
        foreach (GameObject pooledObject in pool)
        {
            if (!pooledObject.activeSelf)
            {
                pooledObject.SetActive(true);
                return pooledObject;
            }
        }

        GameObject expandedObject = create();
        pool.Add(expandedObject);
        expandedObject.SetActive(true);
        return expandedObject;
    }

    private static int CountActive(List<GameObject> pool)
    {
        int count = 0;
        foreach (GameObject pooledObject in pool)
        {
            if (pooledObject.activeSelf)
            {
                count++;
            }
        }

        return count;
    }

    private static void SetActive(List<GameObject> pool, bool active)
    {
        foreach (GameObject pooledObject in pool)
        {
            pooledObject.SetActive(active);
        }
    }

    private static void RefreshJudgementDistances(List<GameObject> pool)
    {
        foreach (GameObject enemyObject in pool)
        {
            enemyObject.GetComponent<Enemy>()?.RefreshJudgeDistance();
        }
    }
}
