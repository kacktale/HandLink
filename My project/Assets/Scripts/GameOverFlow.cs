using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameOverFlow : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private Image gameOverBackground;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI maxScoreText;
    [SerializeField] private Button startGameButton;
    [SerializeField] private float gameOverPresentationDuration = 1.2f;

    private Color backgroundColor;
    private Color textColor;
    private Color currentScoreColor;
    private Color maxScoreColor;
    private GameManager gameManager;

    public bool IsShowingGameOver { get; private set; }

    private void Awake()
    {
        backgroundColor = gameOverBackground.color;
        textColor = gameOverText.color;
        currentScoreColor = currentScoreText.color;
        maxScoreColor = maxScoreText.color;
        startGameButton.onClick.AddListener(StartGame);
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.StateChanged += HandleStateChanged;
            ApplyState(gameManager.CurrentState);
        }

        RefreshShop();
    }

    private void OnDestroy()
    {
        startGameButton.onClick.RemoveListener(StartGame);
        if (gameManager != null)
        {
            gameManager.StateChanged -= HandleStateChanged;
        }
    }

    private void Play()
    {
        if (IsShowingGameOver)
        {
            return;
        }

        IsShowingGameOver = true;

        gameOverPanel.SetActive(true);
        SetScoreTexts();
        SetAlpha(1f);
        gameOverText.rectTransform.localScale = Vector3.one;
        mainPanel.SetActive(false);
        StartCoroutine(ShowGameOverThenMainPanel());
    }

    private IEnumerator ShowGameOverThenMainPanel()
    {
        float elapsed = 0f;
        while (elapsed < gameOverPresentationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / gameOverPresentationDuration);
            float alpha = Mathf.Sin(progress * Mathf.PI);
            SetAlpha(alpha);
            gameOverText.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.78f, 1.08f, Mathf.SmoothStep(0f, 1f, progress));
            yield return null;
        }

        gameManager?.ReturnToMainMenu();
    }

    private void SetAlpha(float alpha)
    {
        Color currentBackgroundColor = backgroundColor;
        Color currentTextColor = textColor;
        currentBackgroundColor.a *= alpha;
        currentTextColor.a *= alpha;
        gameOverBackground.color = currentBackgroundColor;
        gameOverText.color = currentTextColor;
        Color currentScoreTextColor = currentScoreColor;
        Color maxScoreTextColor = maxScoreColor;
        currentScoreTextColor.a *= alpha;
        maxScoreTextColor.a *= alpha;
        currentScoreText.color = currentScoreTextColor;
        maxScoreText.color = maxScoreTextColor;
    }

    private void SetScoreTexts()
    {
        Player player = Player.Instance;
        if (player == null)
        {
            return;
        }

        currentScoreText.SetText($"SCORE  {player.score:N0}");
        maxScoreText.SetText($"BEST  {player.Progression.BestScore:N0}");
    }

    public void RefreshShop()
    {
        if (Player.Instance == null)
        {
            return;
        }

        foreach (ShopUpgradeButton upgradeButton in mainPanel.GetComponentsInChildren<ShopUpgradeButton>(true))
        {
            upgradeButton.Refresh(Player.Instance);
        }
    }

    private void StartGame()
    {
        gameManager?.StartGame();
    }

    private void HandleStateChanged(GameState previousState, GameState nextState)
    {
        ApplyState(nextState);
    }

    private void ApplyState(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
            case GameState.Shop:
                IsShowingGameOver = false;
                gameOverPanel.SetActive(false);
                mainPanel.SetActive(true);
                startGameButton.interactable = true;
                RefreshShop();
                break;

            case GameState.GameOver:
                startGameButton.interactable = false;
                Play();
                break;

            default:
                IsShowingGameOver = false;
                gameOverPanel.SetActive(false);
                mainPanel.SetActive(false);
                startGameButton.interactable = false;
                break;
        }
    }
}
