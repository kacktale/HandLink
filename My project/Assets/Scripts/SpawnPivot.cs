using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public sealed class SpawnPivot : MonoBehaviour
{
    private const int InitialNormalPoolSize = 80;
    private const int InitialJudgementPoolSize = 80;
    private const float MinimumSpawnInterval = 0.35f;

    public static SpawnPivot Instance { get; private set; }

    [SerializeField] private Transform[] enemySummonPos;
    [FormerlySerializedAs("enemys")]
    [SerializeField] private GameObject[] enemies;
    [SerializeField] private GameObject judgeObj;
    [SerializeField] private GameObject target;
    [SerializeField] private DifficultyConfig difficultyConfig;
    [SerializeField] private float spawnDelay;
    [SerializeField, Min(0.01f)] private float enemyTravelDuration = 1.2f;
    [FormerlySerializedAs("poolParant")]
    [SerializeField] private Transform poolParent;
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
    private GameplayAspectController aspectController;
    private EnemyDefeatEffects enemyDefeatEffects;
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

    public GameObject SpawnTutorialSpecialEnemy(
        SpecialEnemyType type,
        Vector2 position,
        Vector2 targetPosition,
        float travelDuration)
    {
        return enemySpawner.SpawnTutorialSpecialEnemy(
            type,
            position,
            targetPosition,
            travelDuration);
    }

    public void ResetGame()
    {
        enemySpawner.ResetSpawner();
        difficultyController.ResetDifficulty();
        enemyPool.ReturnAll();
        RefreshJudgementDistances();
        judgementPool.ReturnAll();
    }

    public void RefreshJudgementDistances()
    {
        enemyPool?.RefreshJudgementDistances();
    }

    public void PlayEnemyDefeatEffect(Vector3 position, Color color)
    {
        enemyDefeatEffects?.Play(position, color);
    }

    public long ApplyScoreMultiplier(long score)
    {
        return difficultyController == null
            ? score
            : difficultyController.ApplyScoreMultiplier(score);
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

        return judgementSettings.TryGetJudgementColor(
            distance,
            out judgementColor);
    }

    private void ResolveComponents()
    {
        enemySpawner = GetOrAdd<EnemySpawner>();
        enemyPool = GetOrAdd<EnemyPool>();
        judgementPool = GetOrAdd<JudgementPool>();
        difficultyController = GetOrAdd<DifficultyController>();
        aspectController = GetOrAdd<GameplayAspectController>();
        enemyDefeatEffects = GetOrAdd<EnemyDefeatEffects>();
    }

    private void ConfigureComponents()
    {
        difficultyController.Configure(
            difficultyConfig,
            spawnDelay,
            MinimumSpawnInterval,
            enemyTravelDuration,
            pulseEnemyChance,
            healingSpawnInterval);
        enemyPool.Initialize(
            enemies,
            poolParent,
            transform.position,
            InitialNormalPoolSize,
            pulsePoolSize,
            healingPoolSize);
        judgementPool.Initialize(
            judgeObj,
            poolParent,
            transform.position,
            InitialJudgementPoolSize);
        aspectController.Configure(Camera.main, target != null ? target.transform : transform);
        enemySpawner.Configure(
            enemySummonPos,
            target != null ? target.transform : transform,
            enemyPool,
            difficultyController,
            aspectController);
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
