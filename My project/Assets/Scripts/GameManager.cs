using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
[DisallowMultipleComponent]
public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Player player;
    [SerializeField] private SpawnPivot enemySpawner;
    [SerializeField] private UiManager uiManager;

    private bool isTransitioning;
    private PlayerHealth subscribedHealth;

    public GameState CurrentState { get; private set; } = GameState.MainMenu;
    public bool IsGameplayActive => CurrentState == GameState.Playing;

    public event Action<GameState, GameState> StateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Only one GameManager may exist in a scene.", this);
            enabled = false;
            return;
        }

        Instance = this;
        if (uiManager == null)
        {
            TryGetComponent(out uiManager);
        }
    }

    private void Start()
    {
        if (ResolveDependencies())
        {
            subscribedHealth = player.Health;
            subscribedHealth.Died += HandlePlayerDied;
        }

        ChangeState(GameState.MainMenu, force: true);
    }

    private void OnDestroy()
    {
        if (subscribedHealth != null)
        {
            subscribedHealth.Died -= HandlePlayerDied;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool StartGame()
    {
        if (isTransitioning || CurrentState != GameState.MainMenu || !ResolveDependencies())
        {
            return false;
        }

        isTransitioning = true;
        enemySpawner.ResetGame();
        player.BeginGame();
        ChangeState(GameState.Playing);
        isTransitioning = false;
        return true;
    }

    public bool EndGame()
    {
        if (isTransitioning ||
            (CurrentState != GameState.Ready &&
             CurrentState != GameState.Playing &&
             CurrentState != GameState.Paused) ||
            !ResolveDependencies())
        {
            return false;
        }

        isTransitioning = true;
        player.EndGame();
        uiManager.SaveGameResult(player);
        ChangeState(GameState.GameOver);
        isTransitioning = false;
        return true;
    }

    public bool ReturnToMainMenu()
    {
        if (isTransitioning || CurrentState != GameState.GameOver)
        {
            return false;
        }

        ChangeState(GameState.MainMenu);
        return true;
    }

    private bool ResolveDependencies()
    {
        if (player == null)
        {
            player = Player.Instance;
        }

        if (enemySpawner == null)
        {
            enemySpawner = SpawnPivot.Instance;
        }

        if (uiManager == null)
        {
            uiManager = UiManager.instance;
        }

        if (player != null && enemySpawner != null && uiManager != null)
        {
            return true;
        }

        Debug.LogError("GameManager requires Player, SpawnPivot, and UiManager references.", this);
        return false;
    }

    private void HandlePlayerDied()
    {
        EndGame();
    }

    private void ChangeState(GameState nextState, bool force = false)
    {
        if (!force && CurrentState == nextState)
        {
            return;
        }

        GameState previousState = CurrentState;
        CurrentState = nextState;
        StateChanged?.Invoke(previousState, nextState);
    }
}
