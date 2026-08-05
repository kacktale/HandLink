using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*ToDo : 
 * Rotate
 * Enemy Spawn
 * Enemy LookAt Target
 * SpawnDelay
 */
public class SpawnPivot : MonoBehaviour
{
    public static SpawnPivot Instance { get; private set; }

    [SerializeField] private Transform[] enemySummonPos;
    [SerializeField] private GameObject[] enemys;
    [SerializeField] private GameObject judgeObj;
    [SerializeField] private GameObject target;
    [SerializeField] private float spawnDelay;
    [SerializeField, Min(0.01f)] private float enemyTravelDuration = 1.2f;
    [SerializeField] private Transform poolParant;
    [Header("Special Enemies")]
    [SerializeField, UnityEngine.Range(0f, 1f)] private float pulseEnemyChance = 0.15f;
    [SerializeField, Min(1)] private int pulsePoolSize = 20;
    [SerializeField, Min(1)] private int healingPoolSize = 4;
    [SerializeField, Min(1f)] private float healingSpawnInterval = 20f;

    private Player player;
    private float currentTime;
    private float healingSpawnTime;
    private float minSpawnTime = 0.3f;
    private float initialSpawnDelay;
    private bool isTutorialMode;

    private List<List<GameObject>> enemyList = new List<List<GameObject>>();
    private List<SpriteRenderer> judgeObjs = new List<SpriteRenderer>();
    private List<GameObject> pulseEnemies = new List<GameObject>();
    private List<GameObject> healingEnemies = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        player = Player.Instance;
        initialSpawnDelay = spawnDelay;
        for (int i = 0; i < enemys.Length; i++)
        {
            enemyList.Add(new List<GameObject>());
        }
        SummonDummyEnemy();
    }

    void SummonDummyEnemy()
    {
        for (int i = 0; i < enemys.Length; i++)
        {
            for(int j = 0; j < 80; j++)
            {
                GameObject enemy = Instantiate(enemys[i],transform.position,Quaternion.identity, poolParant);
                enemyList[i].Add(enemy);
                enemy.SetActive(false);
            }
        }
        CreateSpecialEnemyPool(pulseEnemies, SpecialEnemyType.Pulse, pulsePoolSize);
        CreateSpecialEnemyPool(healingEnemies, SpecialEnemyType.HeartHealer, healingPoolSize);

        for (int i = 0;i < 80; i++)
        {
            SpriteRenderer judge = Instantiate(judgeObj, transform.position, Quaternion.identity, poolParant).GetComponent<SpriteRenderer>();
            judgeObjs.Add(judge);
            judge.gameObject.SetActive(false);
        }
    }

    private void CreateSpecialEnemyPool(List<GameObject> pool, SpecialEnemyType type, int poolSize)
    {
        for (int index = 0; index < poolSize; index++)
        {
            GameObject enemy = Instantiate(enemys[0], transform.position, Quaternion.identity, poolParant);
            SpecialEnemy specialEnemy = enemy.AddComponent<SpecialEnemy>();
            specialEnemy.Configure(type);
            pool.Add(enemy);
            enemy.SetActive(false);
        }
    }

    public GameObject FindObj(int tag)
    {
        GameObject enemy;
        for (int i = 0; i < enemyList[tag].Count; i++)
        {
            if (!enemyList[tag][i].activeInHierarchy)
            {
                enemy = enemyList[tag][i];
                enemy.SetActive(true);
                return enemy;
            }
        }
        enemy = Instantiate(enemys[tag], transform.position, Quaternion.identity);
        enemyList[tag].Add(enemy);
        return enemy;
    }

    private GameObject FindSpecialObj(List<GameObject> pool, SpecialEnemyType type)
    {
        for (int index = 0; index < pool.Count; index++)
        {
            if (!pool[index].activeInHierarchy)
            {
                pool[index].SetActive(true);
                return pool[index];
            }
        }

        GameObject enemy = Instantiate(enemys[0], transform.position, Quaternion.identity, poolParant);
        SpecialEnemy specialEnemy = enemy.AddComponent<SpecialEnemy>();
        specialEnemy.Configure(type);
        pool.Add(enemy);
        return enemy;
    }

    public SpriteRenderer FindJudge()
    {
        SpriteRenderer judge;
        for (int i = 0; i < judgeObjs.Count; i++)
        {
            if (!judgeObjs[i].gameObject.activeInHierarchy)
            {
                judge = judgeObjs[i];
                judge.gameObject.SetActive(true);
                return judge;
            }
        }
        judge = Instantiate(judgeObj, transform.position, Quaternion.identity, poolParant).GetComponent<SpriteRenderer>();
        judgeObjs.Add(judge);
        return judge;
    }

    public Vector2 TutorialTargetPosition => target != null ? target.transform.position : transform.position;
    public Transform TutorialTargetTransform => target != null ? target.transform : transform;

    public void SetTutorialMode(bool enabled)
    {
        isTutorialMode = enabled;
        if (!enabled)
        {
            return;
        }

        currentTime = 0f;
        healingSpawnTime = 0f;
        SetPoolActive(pulseEnemies, false);
        SetPoolActive(healingEnemies, false);
        foreach (List<GameObject> enemies in enemyList)
        {
            foreach (GameObject enemy in enemies)
            {
                enemy.SetActive(false);
            }
        }
    }

    public GameObject SpawnTutorialEnemy(Vector2 position, Vector2 targetPosition, float travelDuration)
    {
        if (enemyList.Count == 0)
        {
            return null;
        }

        GameObject enemy = FindObj(0);
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

    public void ResetGame()
    {
        currentTime = 0f;
        healingSpawnTime = 0f;
        spawnDelay = initialSpawnDelay;
        foreach (List<GameObject> enemies in enemyList)
        {
            foreach (GameObject enemy in enemies)
            {
                enemy.SetActive(false);
                enemy.GetComponent<Enemy>().RefreshJudgeDistance();
            }
        }

        SetPoolActive(pulseEnemies, false);
        SetPoolActive(healingEnemies, false);

        foreach (SpriteRenderer judge in judgeObjs)
        {
            judge.gameObject.SetActive(false);
        }
    }

    private static void SetPoolActive(List<GameObject> pool, bool isActive)
    {
        foreach (GameObject enemy in pool)
        {
            enemy.SetActive(isActive);
        }
    }

    public bool TryGetHeartDistanceJudgementColor(float distance, out Color judgementColor)
    {
        judgementColor = Color.white;
        if (enemys == null || enemys.Length == 0)
        {
            return false;
        }

        Enemy judgementSettings = enemys[0].GetComponent<Enemy>();
        if (judgementSettings == null)
        {
            return false;
        }

        float upgradeValue = player == null ? 0f : player.GetUpgradeValue(UpgradeType.Judgement);
        return judgementSettings.TryGetJudgementColor(distance, upgradeValue, out judgementColor);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance == null ||
            !GameManager.Instance.IsGameplayActive ||
            !player.gameStarted)
        {
            return;
        }

        gameObject.transform.Rotate(new Vector3(0,0,1)); //Rotate Pivot

        if (isTutorialMode)
        {
            return;
        }

        currentTime += Time.deltaTime;
        healingSpawnTime += Time.deltaTime;

        if (healingSpawnTime >= healingSpawnInterval)
        {
            SpawnEnemy(FindSpecialObj(healingEnemies, SpecialEnemyType.HeartHealer));
            healingSpawnTime = 0f;
        }

        if (currentTime >= spawnDelay)
        {
            spawnDelay = Mathf.Max(minSpawnTime,spawnDelay - Time.deltaTime);
            GameObject enemy = Random.value < pulseEnemyChance
                ? FindSpecialObj(pulseEnemies, SpecialEnemyType.Pulse)
                : FindObj(Random.Range(0, enemys.Length));
            SpawnEnemy(enemy);

            currentTime = 0;
        }
    }

    private void SpawnEnemy(GameObject enemy)
    {
        int summonPos = Random.Range(0, enemySummonPos.Length);
        enemy.transform.position = enemySummonPos[summonPos].position;
        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        enemyComponent.SetTravelDuration(target.transform.position, enemyTravelDuration);

        Vector2 newPos = target.transform.position - enemy.transform.position;
        float rotZ = Mathf.Atan2(newPos.y, newPos.x) * Mathf.Rad2Deg;
        enemy.transform.rotation = Quaternion.Euler(0, 0, rotZ);
    }
}
