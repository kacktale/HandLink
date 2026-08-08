using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class GameScreenController : MonoBehaviour
{
    [Header("State Screens")]
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject inGameScreen;
    [SerializeField] private GameObject staminaScreen;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject shopScreen;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button openShopButton;
    [SerializeField] private Button shopBackButton;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("GameScreenController requires an active GameManager.", this);
            enabled = false;
            return;
        }

        gameManager.StateChanged += HandleStateChanged;
        startGameButton?.onClick.AddListener(StartGame);
        openShopButton?.onClick.AddListener(OpenShop);
        shopBackButton?.onClick.AddListener(ReturnToMainMenu);
        ApplyState(gameManager.CurrentState);
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.StateChanged -= HandleStateChanged;
        }

        startGameButton?.onClick.RemoveListener(StartGame);
        openShopButton?.onClick.RemoveListener(OpenShop);
        shopBackButton?.onClick.RemoveListener(ReturnToMainMenu);
    }

    private void HandleStateChanged(GameState previousState, GameState nextState)
    {
        ApplyState(nextState);
    }

    private void OpenShop()
    {
        gameManager?.OpenShop();
    }

    private void StartGame()
    {
        gameManager?.StartGame();
    }

    private void ReturnToMainMenu()
    {
        gameManager?.ReturnToMainMenu();
    }

    private void ApplyState(GameState state)
    {
        bool isMainMenu = state == GameState.MainMenu;
        bool isInGame = state == GameState.Ready ||
                        state == GameState.Playing ||
                        state == GameState.Paused;

        SetScreenActive(mainScreen, isMainMenu);
        SetScreenActive(inGameScreen, isInGame);
        SetScreenActive(staminaScreen, isInGame);
        SetScreenActive(gameOverScreen, state == GameState.GameOver);
        SetScreenActive(shopScreen, state == GameState.Shop);
    }

    private static void SetScreenActive(GameObject screen, bool active)
    {
        if (screen != null && screen.activeSelf != active)
        {
            screen.SetActive(active);
        }
    }
}
