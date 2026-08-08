using System;
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerStamina))]
[RequireComponent(typeof(ScoreController))]
[RequireComponent(typeof(PlayerUpgradeApplier))]
[RequireComponent(typeof(PlayerVisual))]
public class Player : InputAxis
{
    public static Player Instance;

    public bool invincible = false;

    private bool isGameOver;
    private bool isReadyToPlay;
    private bool tutorialMovementLocked;
    private Vector3 basePosition;
    private PlayerProgression progression;
    private PlayerHealth health;
    private PlayerStamina staminaController;
    private ScoreController scoreController;
    private PlayerUpgradeApplier upgradeApplier;
    private PlayerVisual playerVisual;

    public int hp => health == null ? 0 : health.CurrentHealth;
    public long score => scoreController == null ? 0L : scoreController.CurrentScore;
    public float stamina => staminaController == null ? 0f : staminaController.CurrentStamina;
    public PlayerProgression Progression => progression;
    public PlayerHealth Health => health;
    public PlayerStamina Stamina => staminaController;
    public ScoreController Score => scoreController;
    public float StaminaNormalized =>
        staminaController == null ? 0f : staminaController.Normalized;

    public event Action<bool> EnemyJudged;
    public event Func<Vector3, bool> PerfectCoinDropRequested;
    public event Action<Vector3, int> CoinRewarded;

    public void Awake()
    {
        Instance = this;
        progression = GetComponent<PlayerProgression>();
        health = GetComponent<PlayerHealth>();
        staminaController = GetComponent<PlayerStamina>();
        scoreController = GetComponent<ScoreController>();
        upgradeApplier = GetComponent<PlayerUpgradeApplier>();
        playerVisual = GetComponent<PlayerVisual>();
        basePosition = transform.position;
    }

    private void Start()
    {
        upgradeApplier.Apply();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public override void Update()
    {
        if (isGameOver || !isReadyToPlay)
        {
            return;
        }

        if (tutorialMovementLocked)
        {
            gameStarted = true;
            distanceValue = Vector2.zero;
            return;
        }

        base.Update();

        if (!staminaController.IsStunned)
        {
            transform.position = pointerWorldPosition;
        }

        if (!staminaController.Tick(distanceValue, gameStarted, Time.deltaTime))
        {
            return;
        }

        playerVisual.RefreshHeartCircleColor();
    }

    public void Damage()
    {
        if (!isGameOver)
        {
            health.TakeDamage();
        }
    }

    public void Heal(int amount)
    {
        if (!isGameOver)
        {
            health.Heal(amount);
        }
    }

    public void EndGame()
    {
        isGameOver = true;
        gameStarted = false;
        ResetPointerInputTracking();
        playerVisual.SetGameplayVisible(false);
    }

    public void BeginGame()
    {
        isGameOver = false;
        isReadyToPlay = true;
        gameStarted = true;
        ResetPointerInputTracking();
        scoreController.ResetScore();
        transform.position = basePosition;
        playerVisual.SetGameplayVisible(true);
        playerVisual.ResetVisual();
        upgradeApplier.Apply();
        staminaController.ResetStamina();
    }

    public void SetTutorialMovementLocked(bool locked)
    {
        tutorialMovementLocked = locked;
        if (locked)
        {
            distanceValue = Vector2.zero;
        }
    }

    public float GetUpgradeValue(UpgradeType type)
    {
        return upgradeApplier.GetUpgradeValue(type);
    }

    public void ApplyUpgradeStats()
    {
        upgradeApplier.Apply();
        SpawnPivot.Instance?.RefreshJudgementDistances();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isGameOver || !collision.gameObject.CompareTag("Enemy"))
        {
            return;
        }

        SpecialEnemy specialEnemy =
            collision.gameObject.GetComponent<SpecialEnemy>();
        if (specialEnemy != null && specialEnemy.TryHandlePlayerContact())
        {
            return;
        }

        SpriteRenderer judge = SpawnPivot.Instance.FindJudge();
        judge.transform.position = collision.transform.position;

        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        long baseAwardedScore =
            enemy.Calculate(out Color judgementColor, out bool isPerfect);
        long awardedScore = SpawnPivot.Instance == null
            ? baseAwardedScore
            : SpawnPivot.Instance.ApplyScoreMultiplier(baseAwardedScore);
        scoreController.AddScore(awardedScore);
        
        GameAudio.Instance?.PlayHit(isPerfect);
        if (isPerfect)
        {
            GameHaptics.PlayPerfect();
        }

        EnemyJudged?.Invoke(isPerfect);
        judge.color = judgementColor;

        if (isPerfect && !TryHandlePerfectCoinDrop(collision.transform.position))
        {
            const int perfectCoinReward = 1;
            progression.AddCoin(perfectCoinReward);
            CoinRewarded?.Invoke(collision.transform.position, perfectCoinReward);
        }

        if (isPerfect)
        {
            GameAudio.Instance?.PlayCoin();
        }

        if (scoreController.CurrentScore > 0L)
        {
            staminaController.Restore(30f);
        }

        collision.gameObject.SetActive(false);
    }

    private bool TryHandlePerfectCoinDrop(Vector3 position)
    {
        if (PerfectCoinDropRequested == null)
        {
            return false;
        }

        foreach (Func<Vector3, bool> handler
                 in PerfectCoinDropRequested.GetInvocationList())
        {
            if (handler(position))
            {
                return true;
            }
        }

        return false;
    }

    private void TurnOffInvincible()
    {
        invincible = false;
    }
}
