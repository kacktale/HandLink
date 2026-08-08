using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ShopUpgradeButton : MonoBehaviour
{
    private const string LanguagePreferenceKey = "GameLanguage";
    private const float FeedbackDuration = 0.22f;
    private const float FeedbackScale = 1.04f;

    private static readonly Color AvailableCostColor = new(0.65f, 1f, 0.72f);
    private static readonly Color UnavailableCostColor = new(1f, 0.45f, 0.45f);
    private static readonly Color MaxLevelColor = new(0.65f, 0.78f, 0.9f);
    private static readonly Color FeedbackValueColor = new(1f, 0.88f, 0.32f);

    [SerializeField] private UpgradeType upgradeType;
    [SerializeField] private UpgradeDefinition upgradeDefinition;
    [SerializeField] private string description;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private TextMeshProUGUI costText;

    private Button button;
    private Vector3 baseScale;
    private Color descriptionColor;
    private Color valueColor;
    private Coroutine feedbackRoutine;
    private bool initialized;
    private bool listenerRegistered;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void OnDestroy()
    {
        if (listenerRegistered && button != null)
        {
            button.onClick.RemoveListener(Purchase);
        }
    }

    private void OnDisable()
    {
        if (!initialized)
        {
            return;
        }

        transform.localScale = baseScale;
        valueText.color = valueColor;
        feedbackRoutine = null;
    }

    public void Refresh(Player player)
    {
        EnsureInitialized();
        if (player == null || upgradeDefinition == null)
        {
            button.interactable = false;
            return;
        }

        PlayerProgression progression = player.Progression;
        int level = progression.GetLevel(upgradeType);
        int maxLevel = upgradeDefinition.MaxLevel;

        descriptionText.gameObject.SetActive(true);
        valueText.gameObject.SetActive(true);
        costText.gameObject.SetActive(true);
        descriptionText.color = descriptionColor;
        valueText.color = valueColor;

        string effectName = GetEffectName();
        if (level >= maxLevel)
        {
            descriptionText.SetText(effectName);
            valueText.SetText(GetMaxValueText(maxLevel));
            costText.SetText(IsKorean() ? "\uCD5C\uB300 \uB808\uBCA8" : "MAX LEVEL");
            valueText.color = MaxLevelColor;
            costText.color = MaxLevelColor;
            button.interactable = false;
            return;
        }

        int cost = Mathf.Max(0, upgradeDefinition.GetCost(level));
        bool canPurchase = progression.Coin >= cost;
        float currentValue = upgradeDefinition.GetTotalValue(level);
        float nextValue = upgradeDefinition.GetTotalValue(level + 1);

        descriptionText.SetText(effectName);
        valueText.SetText($"{FormatValue(currentValue)} \u2192 {FormatValue(nextValue)}");

        if (canPurchase)
        {
            costText.SetText($"{cost:N0} C");
            costText.color = AvailableCostColor;
        }
        else
        {
            costText.SetText($"{cost:N0} C");
            costText.color = UnavailableCostColor;
        }

        button.interactable = canPurchase;
    }

    private void Purchase()
    {
        Player player = Player.Instance;
        if (player == null ||
            !player.Progression.TryPurchase(upgradeType, upgradeDefinition))
        {
            return;
        }

        player.ApplyUpgradeStats();
        GameAudio.Instance?.PlayUpgradePurchase();
        GetComponentInParent<GameOverFlow>()?.RefreshShop();
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
        }

        feedbackRoutine = StartCoroutine(PlayPurchaseFeedback());
    }

    private void EnsureInitialized()
    {
        if (!initialized)
        {
            button = GetComponent<Button>();
            baseScale = transform.localScale;
            descriptionColor = descriptionText.color;
            valueColor = valueText.color;
            initialized = true;
        }

        if (!listenerRegistered)
        {
            button.onClick.AddListener(Purchase);
            listenerRegistered = true;
        }
    }

    private IEnumerator PlayPurchaseFeedback()
    {
        float elapsed = 0f;
        while (elapsed < FeedbackDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / FeedbackDuration);
            float pulse = Mathf.Sin(progress * Mathf.PI);
            transform.localScale = baseScale * Mathf.Lerp(1f, FeedbackScale, pulse);
            valueText.color = Color.Lerp(valueColor, FeedbackValueColor, pulse);
            yield return null;
        }

        transform.localScale = baseScale;
        valueText.color = valueColor;
        feedbackRoutine = null;
    }

    private string GetEffectName()
    {
        bool korean = IsKorean();
        return upgradeType switch
        {
            UpgradeType.Judgement => korean
                ? "\uD310\uC815 \uBC94\uC704"
                : "JUDGEMENT RANGE",
            UpgradeType.ExtraLife => korean
                ? "\uCD94\uAC00 \uC0DD\uBA85"
                : "EXTRA LIFE",
            UpgradeType.Stamina => korean
                ? "\uCD5C\uB300 \uC2A4\uD0DC\uBBF8\uB098"
                : "MAX STAMINA",
            UpgradeType.CircleSize => korean
                ? "\uC6D0 \uD06C\uAE30"
                : "CIRCLE SIZE",
            _ => string.IsNullOrWhiteSpace(description)
                ? upgradeType.ToString()
                : description
        };
    }

    private string GetMaxValueText(int maxLevel)
    {
        return FormatValue(upgradeDefinition.GetTotalValue(maxLevel));
    }

    private string FormatValue(float value)
    {
        return upgradeType switch
        {
            UpgradeType.ExtraLife => $"+{Mathf.RoundToInt(value)}",
            UpgradeType.Stamina => $"+{value:0.#}",
            UpgradeType.Judgement => $"+{value:0.##}",
            UpgradeType.CircleSize => $"+{value:0.##}",
            _ => $"+{value:0.##}"
        };
    }

    private static bool IsKorean()
    {
        return PlayerPrefs.GetInt(
                   LanguagePreferenceKey,
                   (int)GameLanguage.Korean) ==
               (int)GameLanguage.Korean;
    }
}
