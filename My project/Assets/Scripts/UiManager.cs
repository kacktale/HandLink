using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public static UiManager instance;

    public Transform hartUI;
    public GameObject hartObj;
    public TextMeshProUGUI bestScoreTxt;
    public TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI heldCoinsText;
    public long bestScore;
    [SerializeField] private DamageFeedback damageFeedback;
    [SerializeField] private CoinRewardFeedback coinRewardFeedback;

    public List<GameObject> harts = new List<GameObject>();
    private Player boundPlayer;
    private RectTransform healthFill;
    private TextMeshProUGUI healthText;
    private TextMeshProUGUI heldCoinsLabel;
    private ComboDisplay comboDisplay;

    private void Awake()
    {
        instance = this;
        BuildHealthGauge();
        BuildHeldCoinsDisplay();
    }

    private void Start()
    {
        BindPlayer(Player.Instance);
    }

    private void OnDestroy()
    {
        if (boundPlayer == null)
        {
            return;
        }

        boundPlayer.Progression.Changed -= RefreshProgressionUi;
        boundPlayer.Health.HealthChanged -= RefreshHealthUi;
        boundPlayer.Health.Damaged -= PlayDamageFeedback;
        boundPlayer.Health.Healed -= PlayHealingFeedback;
        boundPlayer.Score.ScoreChanged -= RefreshScoreUi;
        boundPlayer.CoinRewarded -= ShowCoinReward;
    }

    public void ShowGameOver()
    {
        GameManager.Instance?.EndGame();
    }

    public void SaveGameResult(Player player)
    {
        if (player == null || player.IsPracticeMode)
        {
            return;
        }

        player.Progression.SaveBestScore(player.score);
        SetBestScore(player.Progression.BestScore);
        bestScoreTxt.SetText($"{bestScore:N0}");
    }

    private void SetBestScore(long value)
    {
        bestScore = value;
        bestScoreTxt.SetText($"{bestScore:N0}");
    }

    public void ResetGameUI(int hp)
    {
        RefreshHealthUi(hp, hp);
        RefreshScoreUi(0L);
    }

    public void PlayDamageFeedback()
    {
        damageFeedback?.Play();
        GameAudio.Instance?.PlayDamage();
        GameHaptics.PlayDamage();
    }

    public void ShowCoinReward(Vector3 worldPosition, int amount)
    {
        coinRewardFeedback?.Show(worldPosition, amount);
    }

    private void PlayHealingFeedback()
    {
        damageFeedback?.PlayHeal();
    }

    private void BindPlayer(Player player)
    {
        if (player == null)
        {
            return;
        }

        boundPlayer = player;
        if (comboDisplay == null && currentScoreText != null)
        {
            comboDisplay = currentScoreText.gameObject.AddComponent<ComboDisplay>();
            comboDisplay.Initialize(player.Score, currentScoreText);
        }
        boundPlayer.Progression.Changed += RefreshProgressionUi;
        boundPlayer.Health.HealthChanged += RefreshHealthUi;
        boundPlayer.Health.Damaged += PlayDamageFeedback;
        boundPlayer.Health.Healed += PlayHealingFeedback;
        boundPlayer.Score.ScoreChanged += RefreshScoreUi;
        boundPlayer.CoinRewarded += ShowCoinReward;

        RefreshProgressionUi();
        RefreshHealthUi(
            boundPlayer.Health.CurrentHealth,
            boundPlayer.Health.MaxHealth);
        RefreshScoreUi(boundPlayer.Score.CurrentScore);
    }

    private void RefreshHealthUi(int currentHealth, int maxHealth)
    {
        if (healthFill == null)
        {
            BuildHealthGauge();
        }

        if (healthFill != null)
        {
            float normalizedHealth = maxHealth > 0
                ? Mathf.Clamp01((float)currentHealth / maxHealth)
                : 0f;
            healthFill.anchorMax = new Vector2(normalizedHealth, 1f);
        }

        if (healthText != null)
        {
            healthText.SetText("HP  {0} / {1}", currentHealth, maxHealth);
        }
    }

    private void BuildHealthGauge()
    {
        if (hartUI == null || healthFill != null)
        {
            return;
        }

        HorizontalLayoutGroup layout = hartUI.GetComponent<HorizontalLayoutGroup>();
        if (layout != null)
        {
            layout.enabled = false;
        }

        RectTransform gaugeRoot = hartUI as RectTransform;
        if (gaugeRoot != null)
        {
            gaugeRoot.sizeDelta = new Vector2(760f, 64f);
        }

        foreach (GameObject heart in harts)
        {
            Destroy(heart);
        }
        harts.Clear();

        Image background = CreateGaugeImage(
            "HealthGaugeBackground",
            hartUI,
            new Color(0.12f, 0.02f, 0.025f, 0.95f));

        Image fill = CreateGaugeImage(
            "HealthGaugeFill",
            background.transform,
            new Color(0.9f, 0.035f, 0.055f, 1f));
        healthFill = fill.rectTransform;
        healthFill.anchorMax = Vector2.one;

        GameObject textObject = new GameObject(
            "HealthGaugeText",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(background.transform, false);
        SetStretch(textRect);

        healthText = textObject.GetComponent<TextMeshProUGUI>();
        healthText.font = currentScoreText != null ? currentScoreText.font : TMP_Settings.defaultFontAsset;
        healthText.fontSize = 28f;
        healthText.fontStyle = FontStyles.Bold;
        healthText.alignment = TextAlignmentOptions.Center;
        healthText.color = Color.white;
        healthText.raycastTarget = false;
    }

    private void BuildHeldCoinsDisplay()
    {
        if (heldCoinsText == null || heldCoinsLabel != null)
        {
            return;
        }

        RectTransform coinsRect = heldCoinsText.rectTransform;
        Transform parent = coinsRect.parent;
        if (parent == null)
        {
            return;
        }

        GameObject backgroundObject = new GameObject(
            "HeldCoinsBackground",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Outline));
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.SetParent(parent, false);
        backgroundRect.anchorMin = coinsRect.anchorMin;
        backgroundRect.anchorMax = coinsRect.anchorMax;
        backgroundRect.pivot = coinsRect.pivot;
        Vector2 cardPosition = coinsRect.anchoredPosition + Vector2.up * 80f;
        backgroundRect.anchoredPosition = cardPosition;
        backgroundRect.sizeDelta = new Vector2(760f, 118f);
        backgroundRect.SetSiblingIndex(coinsRect.GetSiblingIndex());

        Image background = backgroundObject.GetComponent<Image>();
        background.color = new Color(0.11f, 0.075f, 0.015f, 0.96f);
        background.raycastTarget = false;

        Outline outline = backgroundObject.GetComponent<Outline>();
        outline.effectColor = new Color(1f, 0.72f, 0.12f, 0.92f);
        outline.effectDistance = new Vector2(3f, -3f);

        GameObject labelObject = new GameObject(
            "HeldCoinsLabel",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.SetParent(backgroundRect, false);
        labelRect.anchorMin = new Vector2(0.5f, 1f);
        labelRect.anchorMax = new Vector2(0.5f, 1f);
        labelRect.pivot = new Vector2(0.5f, 1f);
        labelRect.anchoredPosition = new Vector2(0f, -10f);
        labelRect.sizeDelta = new Vector2(680f, 34f);

        heldCoinsLabel = labelObject.GetComponent<TextMeshProUGUI>();
        heldCoinsLabel.font = heldCoinsText.font;
        heldCoinsLabel.fontSize = 24f;
        heldCoinsLabel.fontStyle = FontStyles.Bold;
        heldCoinsLabel.alignment = TextAlignmentOptions.Center;
        heldCoinsLabel.color = new Color(1f, 0.84f, 0.38f, 1f);
        heldCoinsLabel.raycastTarget = false;
        heldCoinsLabel.SetText("COINS");

        coinsRect.sizeDelta = new Vector2(700f, 64f);
        coinsRect.anchoredPosition = cardPosition + Vector2.down * 15f;
        heldCoinsText.fontSize = 64f;
        heldCoinsText.fontStyle = FontStyles.Bold;
        heldCoinsText.enableAutoSizing = true;
        heldCoinsText.fontSizeMin = 40f;
        heldCoinsText.fontSizeMax = 64f;
        heldCoinsText.alignment = TextAlignmentOptions.Center;
        heldCoinsText.color = new Color(1f, 0.82f, 0.15f, 1f);
        heldCoinsText.raycastTarget = false;
    }

    private static Image CreateGaugeImage(string objectName, Transform parent, Color color)
    {
        GameObject imageObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        SetStretch(rect);

        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static void SetStretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private void RefreshScoreUi(long score)
    {
        currentScoreText.SetText($"{score:N0}");
    }

    private void RefreshProgressionUi()
    {
        if (boundPlayer == null)
        {
            return;
        }

        SetBestScore(boundPlayer.Progression.BestScore);
        if (heldCoinsText != null)
        {
            heldCoinsText.SetText($"{boundPlayer.Progression.Coin:N0}C");
        }
    }
}
