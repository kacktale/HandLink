using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class TutorialReplayHint : MonoBehaviour
{
    private const float TotalDuration = 3f;
    private const float FadeDuration = 0.35f;

    private CanvasGroup canvasGroup;
    private TextMeshProUGUI messageText;
    private float shownAt;

    public static void Show(
        Canvas parentCanvas,
        TMP_FontAsset font,
        GameLanguage language)
    {
        if (parentCanvas == null || font == null)
        {
            return;
        }

        Transform existingRoot = parentCanvas.transform.Find("TutorialReplayHintRoot");
        TutorialReplayHint hint = existingRoot != null
            ? existingRoot.GetComponent<TutorialReplayHint>()
            : null;
        if (hint == null)
        {
            GameObject root = new GameObject(
                "TutorialReplayHintRoot",
                typeof(RectTransform));
            root.transform.SetParent(parentCanvas.transform, false);
            hint = root.AddComponent<TutorialReplayHint>();
        }

        hint.ShowMessage(font, language);
    }

    private void ShowMessage(TMP_FontAsset font, GameLanguage language)
    {
        BuildUserInterface(font);
        messageText.SetText(language == GameLanguage.Korean
            ? "튜토리얼은 메뉴 > 설정 > 튜토리얼 다시 보기에서 재시작할 수 있어요."
            : "Replay the tutorial from Menu > Settings > Play Tutorial.");
        shownAt = Time.unscaledTime;
        canvasGroup.alpha = 0f;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        float elapsed = Time.unscaledTime - shownAt;
        if (elapsed >= TotalDuration)
        {
            gameObject.SetActive(false);
            return;
        }

        float fadeOutStart = TotalDuration - FadeDuration;
        canvasGroup.alpha = elapsed < FadeDuration
            ? elapsed / FadeDuration
            : elapsed >= fadeOutStart
                ? 1f - ((elapsed - fadeOutStart) / FadeDuration)
                : 1f;
    }

    private void BuildUserInterface(TMP_FontAsset font)
    {
        if (canvasGroup != null)
        {
            return;
        }

        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        GameObject panelObject = new GameObject(
            "TutorialReplayHintPanel",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.SetParent(transform, false);
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0f, -42f);
        panelRect.sizeDelta = new Vector2(880f, 104f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.035f, 0.065f, 0.12f, 0.96f);
        panelImage.raycastTarget = false;

        GameObject textObject = new GameObject(
            "Message",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(panelRect, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(24f, 12f);
        textRect.offsetMax = new Vector2(-24f, -12f);

        messageText = textObject.GetComponent<TextMeshProUGUI>();
        messageText.font = font;
        messageText.fontSize = 30f;
        messageText.enableAutoSizing = true;
        messageText.fontSizeMin = 22f;
        messageText.fontSizeMax = 30f;
        messageText.fontStyle = FontStyles.Bold;
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.color = new Color(1f, 0.9f, 0.58f, 1f);
        messageText.raycastTarget = false;
    }
}
