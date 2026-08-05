using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum GameLanguage
{
    Korean,
    English
}

[DisallowMultipleComponent]
public sealed class InGameSettingsPanel : MonoBehaviour
{
    private const string LanguagePreferenceKey = "GameLanguage";
    private const string SoundEnabledPreferenceKey = "SoundEnabled";

    [SerializeField] private AudioSource soundSource;

    private readonly Dictionary<TextMeshProUGUI, string> originalTexts = new();
    private readonly Dictionary<string, string> koreanTexts = new()
    {
        { "MAIN", "메인" },
        { "START GAME", "게임 시작" },
        { "JUDGEMENT UPGRADE", "판정 강화" },
        { "EXTRA LIFE", "추가 생명" },
        { "STAMINA UPGRADE", "스태미나 강화" },
        { "CIRCLE SIZE", "원 크기" },
        { "SETTINGS", "설정" },
        { "LANGUAGE", "언어" },
        { "KOREAN", "한국어" },
        { "ENGLISH", "영어" },
        { "SOUND", "사운드" },
        { "SOUND ON", "사운드: 켜짐" },
        { "SOUND OFF", "사운드: 꺼짐" },
        { "CLOSE", "닫기" }
    };

    private GameObject mainPanel;
    private GameObject settingsPanel;
    private Button openButton;
    private Button closeButton;
    private Button koreanButton;
    private Button englishButton;
    private Button soundButton;
    private TextMeshProUGUI soundButtonText;

    private GameLanguage CurrentLanguage => (GameLanguage)PlayerPrefs.GetInt(LanguagePreferenceKey, (int)GameLanguage.Korean);
    private bool IsSoundEnabled => PlayerPrefs.GetInt(SoundEnabledPreferenceKey, 1) == 1;

    private void Awake()
    {
        BuildUserInterface();
        if (mainPanel == null || settingsPanel == null)
        {
            enabled = false;
            return;
        }

        CacheOriginalTexts();
        openButton?.onClick.AddListener(Open);
        closeButton?.onClick.AddListener(Close);
        koreanButton?.onClick.AddListener(SetKorean);
        englishButton?.onClick.AddListener(SetEnglish);
        soundButton?.onClick.AddListener(ToggleSound);

        ApplySettings();
    }

    private void OnDestroy()
    {
        openButton?.onClick.RemoveListener(Open);
        closeButton?.onClick.RemoveListener(Close);
        koreanButton?.onClick.RemoveListener(SetKorean);
        englishButton?.onClick.RemoveListener(SetEnglish);
        soundButton?.onClick.RemoveListener(ToggleSound);
    }

    private void Open()
    {
        mainPanel?.SetActive(false);
        settingsPanel?.SetActive(true);
    }

    private void Close()
    {
        settingsPanel?.SetActive(false);
        mainPanel?.SetActive(true);
    }

    private void SetLanguage(GameLanguage language)
    {
        PlayerPrefs.SetInt(LanguagePreferenceKey, (int)language);
        PlayerPrefs.Save();
        ApplySettings();
    }

    private void SetKorean()
    {
        SetLanguage(GameLanguage.Korean);
    }

    private void SetEnglish()
    {
        SetLanguage(GameLanguage.English);
    }

    private void ToggleSound()
    {
        PlayerPrefs.SetInt(SoundEnabledPreferenceKey, IsSoundEnabled ? 0 : 1);
        PlayerPrefs.Save();
        ApplySoundState();
    }

    private void ApplySettings()
    {
        foreach (KeyValuePair<TextMeshProUGUI, string> entry in originalTexts)
        {
            if (entry.Key == null)
            {
                continue;
            }

            entry.Key.SetText(CurrentLanguage == GameLanguage.Korean && koreanTexts.TryGetValue(entry.Value, out string korean)
                ? korean
                : entry.Value);
        }

        ApplySoundState();
    }

    public void RefreshSettings()
    {
        ApplySettings();
    }

    private void ApplySoundState()
    {
        if (soundSource != null)
        {
            soundSource.mute = !IsSoundEnabled;
        }

        if (soundButtonText != null)
        {
            string state = IsSoundEnabled ? "SOUND ON" : "SOUND OFF";
            soundButtonText.SetText(CurrentLanguage == GameLanguage.Korean ? koreanTexts[state] : state);
        }
    }

    private void CacheOriginalTexts()
    {
        foreach (TextMeshProUGUI text in GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (text != null && !originalTexts.ContainsKey(text))
            {
                originalTexts.Add(text, NormalizeToEnglish(text.text));
            }
        }
    }

    private string NormalizeToEnglish(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (koreanTexts.ContainsKey(value))
        {
            return value;
        }

        foreach (KeyValuePair<string, string> entry in koreanTexts)
        {
            if (entry.Value == value)
            {
                return entry.Key;
            }
        }

        return value;
    }

    private void BuildUserInterface()
    {
        mainPanel = transform.Find("MainPanel")?.gameObject;
        if (mainPanel == null)
        {
            Debug.LogError("InGameSettingsPanel requires a MainPanel child.", this);
            return;
        }

        TMP_FontAsset font = GetComponentInChildren<TextMeshProUGUI>(true)?.font ?? TMP_Settings.defaultFontAsset;
        if (font == null)
        {
            Debug.LogError("InGameSettingsPanel could not find a TMP font asset.", this);
            return;
        }

        openButton = CreateButton(
            "SettingsButton",
            mainPanel.transform,
            new Vector2(-30f, -30f),
            new Vector2(200f, 200f),
            "SETTINGS",
            new Color(0.13f, 0.29f, 0.52f, 1f),
            font);
        RectTransform openButtonRect = openButton.GetComponent<RectTransform>();
        openButtonRect.anchorMin = Vector2.one;
        openButtonRect.anchorMax = Vector2.one;
        openButtonRect.pivot = Vector2.one;
        TextMeshProUGUI openButtonText = openButton.GetComponentInChildren<TextMeshProUGUI>(true);
        openButtonText.fontSize = 32f;
        openButtonText.enableWordWrapping = false;

        settingsPanel = CreatePanel(
            "SettingsPanel",
            transform,
            Vector2.zero,
            new Vector2(860f, 1320f),
            new Color(0.025f, 0.045f, 0.09f, 0.98f));

        CreateText("SettingsTitle", settingsPanel.transform, new Vector2(0f, 500f), new Vector2(760f, 110f), "SETTINGS", 64f, font);
        CreateText("LanguageLabel", settingsPanel.transform, new Vector2(0f, 280f), new Vector2(700f, 80f), "LANGUAGE", 42f, font);
        koreanButton = CreateButton("KoreanButton", settingsPanel.transform, new Vector2(-205f, 145f), new Vector2(360f, 120f), "KOREAN", new Color(0.13f, 0.29f, 0.52f, 1f), font);
        englishButton = CreateButton("EnglishButton", settingsPanel.transform, new Vector2(205f, 145f), new Vector2(360f, 120f), "ENGLISH", new Color(0.13f, 0.29f, 0.52f, 1f), font);
        CreateText("SoundLabel", settingsPanel.transform, new Vector2(0f, -70f), new Vector2(700f, 80f), "SOUND", 42f, font);
        soundButton = CreateButton("SoundButton", settingsPanel.transform, new Vector2(0f, -205f), new Vector2(720f, 120f), "SOUND ON", new Color(0.13f, 0.29f, 0.52f, 1f), font);
        closeButton = CreateButton("CloseButton", settingsPanel.transform, new Vector2(0f, -505f), new Vector2(720f, 130f), "CLOSE", new Color(0.33f, 0.16f, 0.29f, 1f), font);
        soundButtonText = soundButton.GetComponentInChildren<TextMeshProUGUI>(true);
        settingsPanel.SetActive(false);
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

    private static Button CreateButton(string name, Transform parent, Vector2 position, Vector2 size, string label, Color color, TMP_FontAsset font)
    {
        GameObject buttonObject = CreatePanel(name, parent, position, size, color);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.SetParent(buttonObject.transform, false);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI labelText = labelObject.GetComponent<TextMeshProUGUI>();
        labelText.font = font;
        labelText.fontSize = 42f;
        labelText.color = Color.white;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.SetText(label);
        return button;
    }

    private static void CreateText(string name, Transform parent, Vector2 position, Vector2 size, string value, float fontSize, TMP_FontAsset font)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.SetText(value);
    }
}
