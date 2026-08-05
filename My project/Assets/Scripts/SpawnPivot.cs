using UnityEngine;

[DisallowMultipleComponent]
public sealed class SpawnPivot : MonoBehaviour
{
    private const int InitialNormalPoolSize = 80;
    private const int InitialJudgementPoolSize = 80;
    private const float MinimumSpawnInterval = 0.3f;

    public static SpawnPivot Instance { get; private set; }

    [SerializeField] private Transform[] enemySummonPos;
    [SerializeField] private GameObject[] enemys;
    [SerializeField] private GameObject judgeObj;
    [SerializeField] private GameObject target;
    [SerializeField] private float spawnDelay;
    [SerializeField, Min(0.01f)] private float enemyTravelDuration = 1.2f;
    [SerializeField] private Transform poolParant;
    [Header("Special Enemies")]
    [SerializeField, Range(0f, 1f)] private float pulseEnemyChance = 0.15f;
    [SerializeField, Min(1)] private int pulsePoolSize = 20;
    [SerializeField, Min(1)] private int healingPoolSize = 4;
    [SerializeField, Min(1f)] private float healingSpawnInterval = 20f;

    private Player player;
    private EnemySpawner enemySpawner;
    private EnemyPool enemyPool;
    private JudgementPool judgementPool;
    private DifficultyController difficultyController;
    private bool isTutorialMode;

    public Vector2 TutorialTargetPosition =>
        target != null ? target.transform.position : transform.position;
    public Transform TutorialTargetTransform =>
        target != null ? target.transform : transform;

    private void Awake()
    {
        Instance = this;
        ResolveComponents();
    }

    private void Start()
    {
        player = Player.Instance;
        ConfigureComponents();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (player == null)
        {
            player = Player.Instance;
        }

        enemySpawner.Tick(
            Time.deltaTime,
            player != null && player.gameStarted,
            isTutorialMode);
    }

    public GameObject FindObj(int prefabIndex)
    {
        return enemyPool.RentNormal(prefabIndex);
    }

    public SpriteRenderer FindJudge()
    {
        return judgementPool.Rent();
    }

    public void SetTutorialMode(bool enabled)
    {
        isTutorialMode = enabled;
        if (enabled)
        {
            enemySpawner.ResetSpawner();
            enemyPool.ReturnAll();
        }
    }

    public GameObject SpawnTutorialEnemy(
        Vector2 position,
        Vector2 targetPosition,
        float travelDuration)
    {
        return enemySpawner.SpawnTutorialEnemy(
            position,
            targetPosition,
            travelDuration);
    }

    public void ResetGame()
    {
        enemySpawner.ResetSpawner();
        difficultyController.ResetDifficulty();
        enemyPool.ReturnAll();
        enemyPool.RefreshJudgementDistances();
        judgementPool.ReturnAll();
    }

    public bool TryGetHeartDistanceJudgementColor(
        float distance,
        out Color judgementColor)
    {
        judgementColor = Color.white;
        Enemy judgementSettings = enemyPool.GetJudgementSettings();
        if (judgementSettings == null)
        {
            return false;
        }

        float upgradeValue = player == null
            ? 0f
            : player.GetUpgradeValue(UpgradeType.Judgement);
        return judgementSettings.TryGetJudgementColor(
            distance,
            upgradeValue,
            out judgementColor);
    }

    private void ResolveComponents()
    {
        enemySpawner = GetOrAdd<EnemySpawner>();
        enemyPool = GetOrAdd<EnemyPool>();
        judgementPool = GetOrAdd<JudgementPool>();
        difficultyController = GetOrAdd<DifficultyController>();
    }

    private void ConfigureComponents()
    {
        difficultyController.Configure(
            spawnDelay,
            MinimumSpawnInterval,
            enemyTravelDuration);
        enemyPool.Initialize(
            enemys,
            poolParant,
            transform.position,
            InitialNormalPoolSize,
            pulsePoolSize,
            healingPoolSize);
        judgementPool.Initialize(
            judgeObj,
            poolParant,
            transform.position,
            InitialJudgementPoolSize);
        enemySpawner.Configure(
            enemySummonPos,
            target != null ? target.transform : transform,
            enemyPool,
            difficultyController,
            pulseEnemyChance,
            healingSpawnInterval);
    }

    private T GetOrAdd<T>() where T : Component
    {
        if (TryGetComponent(out T component))
        {
            return component;
        }

        return gameObject.AddComponent<T>();
    }
}
