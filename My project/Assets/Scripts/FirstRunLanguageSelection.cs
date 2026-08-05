using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class FirstRunLanguageSelection : MonoBehaviour
{
    private const string LanguagePreferenceKey = "GameLanguage";

    private GameObject selectionPanel;
    private Button koreanButton;
    private Button englishButton;

    private void Awake()
    {
        BuildUserInterface();
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey(LanguagePreferenceKey))
        {
            selectionPanel.SetActive(false);
            enabled = false;
            return;
        }

        selectionPanel.SetActive(true);
        selectionPanel.transform.SetAsLastSibling();
    }

    private void OnDestroy()
    {
        koreanButton?.onClick.RemoveListener(SelectKorean);
        englishButton?.onClick.RemoveListener(SelectEnglish);
    }

    private void SelectKorean()
    {
        SelectLanguage(GameLanguage.Korean);
    }

    private void SelectEnglish()
    {
        SelectLanguage(GameLanguage.English);
    }

    private void SelectLanguage(GameLanguage language)
    {
        PlayerPrefs.SetInt(LanguagePreferenceKey, (int)language);
        PlayerPrefs.Save();
        GetComponent<InGameSettingsPanel>()?.RefreshSettings();
        selectionPanel.SetActive(false);
        GameManager.Instance?.StartGame();
        enabled = false;
    }

    private void BuildUserInterface()
    {
        TMP_FontAsset font = GetComponentInChildren<TextMeshProUGUI>(true)?.font ?? TMP_Settings.defaultFontAsset;
        if (font == null)
        {
            Debug.LogError("FirstRunLanguageSelection could not find a TMP font asset.", this);
            enabled = false;
            return;
        }

        selectionPanel = CreatePanel(
            "FirstRunLanguageSelection",
            transform,
            Vector2.zero,
            Vector2.zero,
            new Color(0.01f, 0.025f, 0.05f, 1f));
        RectTransform panelRect = selectionPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        CreateText("Title", selectionPanel.transform, new Vector2(0f, 270f), new Vector2(900f, 110f), "언어 선택", 64f, font);
        CreateText("Subtitle", selectionPanel.transform, new Vector2(0f, 145f), new Vector2(900f, 70f), "LANGUAGE", 38f, font);
        koreanButton = CreateButton("Korean", selectionPanel.transform, new Vector2(0f, -45f), new Vector2(760f, 130f), "한국어", font);
        englishButton = CreateButton("English", selectionPanel.transform, new Vector2(0f, -220f), new Vector2(760f, 130f), "ENGLISH", font);
        koreanButton.onClick.AddListener(SelectKorean);
        englishButton.onClick.AddListener(SelectEnglish);
    }

    private static GameObject CreatePanel(string name, Transform parent, Vector2 position, Vector2 size, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        panel.GetComponent<Image>().color = color;
        return panel;
    }

    private static Button CreateButton(string name, Transform parent, Vector2 position, Vector2 size, string label, TMP_FontAsset font)
    {
        GameObject buttonObject = CreatePanel(name, parent, position, size, new Color(0.13f, 0.29f, 0.52f, 1f));
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();
        CreateText("Label", buttonObject.transform, Vector2.zero, Vector2.zero, label, 46f, font, stretch: true);
        return button;
    }

    private static void CreateText(string name, Transform parent, Vector2 position, Vector2 size, string value, float fontSize, TMP_FontAsset font, bool stretch = false)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        if (stretch)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        else
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        text.SetText(value);
    }
}
