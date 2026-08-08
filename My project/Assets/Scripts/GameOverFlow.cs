using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameOverFlow : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Image gameOverBackground;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI maxScoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button shopButton;
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
        restartButton.onClick.AddListener(RestartGame);
        shopButton.onClick.AddListener(OpenShop);
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
        restartButton.onClick.RemoveListener(RestartGame);
        shopButton.onClick.RemoveListener(OpenShop);
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
        SetScoreTexts();
        SetAlpha(0f);
        gameOverText.rectTransform.localScale = Vector3.one * 0.78f;
        restartButton.interactable = true;
        shopButton.interactable = true;
        StartCoroutine(ShowGameOverActions());
    }

    private IEnumerator ShowGameOverActions()
    {
        float elapsed = 0f;
        while (elapsed < gameOverPresentationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / gameOverPresentationDuration);
            float alpha = Mathf.SmoothStep(0f, 1f, progress);
            SetAlpha(alpha);
            gameOverText.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.78f, 1f, alpha);
            yield return null;
        }

        SetAlpha(1f);
        gameOverText.rectTransform.localScale = Vector3.one;
        restartButton.interactable = true;
        shopButton.interactable = true;
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

        foreach (ShopUpgradeButton upgradeButton in shopPanel.GetComponentsInChildren<ShopUpgradeButton>(true))
        {
            upgradeButton.Refresh(Player.Instance);
        }
    }

    private void RestartGame()
    {
        gameManager?.RestartGame();
    }

    private void OpenShop()
    {
        gameManager?.OpenShop();
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
                IsShowingGameOver = false;
                StopAllCoroutines();
                restartButton.interactable = false;
                shopButton.interactable = false;
                break;

            case GameState.GameOver:
                Play();
                break;

            case GameState.Shop:
                IsShowingGameOver = false;
                StopAllCoroutines();
                restartButton.interactable = false;
                shopButton.interactable = false;
                RefreshShop();
                break;

            default:
                IsShowingGameOver = false;
                StopAllCoroutines();
                restartButton.interactable = false;
                shopButton.interactable = false;
                break;
        }
    }
}
