using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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
    private readonly Dictionary<Button, GameObject> openButtonOwners = new();
    private readonly Dictionary<Button, UnityAction> openButtonActions = new();
    private readonly Dictionary<string, string> koreanTexts = new()
    {
        { "MAIN", "메인" },
        { "START GAME", "게임 시작" },
        { "SHOP", "상점" },
        { "RESTART", "다시 시작" },
        { "BACK", "뒤로" },
        { "JUDGEMENT UPGRADE", "판정 강화" },
        { "EXTRA LIFE", "추가 생명" },
        { "STAMINA UPGRADE", "점수 배율" },
        { "SCORE MULTIPLIER", "점수 배율" },
        { "CIRCLE SIZE", "원 크기" },
        { "SETTINGS", "설정" },
        { "LANGUAGE", "언어" },
        { "KOREAN", "한국어" },
        { "ENGLISH", "영어" },
        { "SOUND", "사운드" },
        { "SOUND ON", "사운드: 켜짐" },
        { "SOUND OFF", "사운드: 꺼짐" },
        { "VIBRATION", "진동" },
        { "VIBRATION ON", "진동: 켜짐" },
        { "VIBRATION OFF", "진동: 꺼짐" },
        { "PLAY TUTORIAL", "튜토리얼 다시 보기" },
        { "CLOSE", "닫기" }
    };

    private GameObject mainPanel;
    private GameObject gameOverPanel;
    private GameObject shopPanel;
    private GameObject settingsPanel;
    private GameObject returnPanel;
    private Button openButton;
    private Button closeButton;
    private Button koreanButton;
    private Button englishButton;
    private Button soundButton;
    private Button vibrationButton;
    private Button tutorialButton;
    private TextMeshProUGUI soundButtonText;
    private TextMeshProUGUI vibrationButtonText;

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
        RegisterOpenButtons();
        closeButton?.onClick.AddListener(Close);
        koreanButton?.onClick.AddListener(SetKorean);
        englishButton?.onClick.AddListener(SetEnglish);
        soundButton?.onClick.AddListener(ToggleSound);
        vibrationButton?.onClick.AddListener(ToggleVibration);
        tutorialButton?.onClick.AddListener(ReplayTutorial);

        ApplySettings();
    }

    private void OnDestroy()
    {
        foreach (KeyValuePair<Button, UnityAction> entry in openButtonActions)
        {
            entry.Key?.onClick.RemoveListener(entry.Value);
        }

        openButtonActions.Clear();
        closeButton?.onClick.RemoveListener(Close);
        koreanButton?.onClick.RemoveListener(SetKorean);
        englishButton?.onClick.RemoveListener(SetEnglish);
        soundButton?.onClick.RemoveListener(ToggleSound);
        vibrationButton?.onClick.RemoveListener(ToggleVibration);
        tutorialButton?.onClick.RemoveListener(ReplayTutorial);
    }

private void Open(GameObject sourcePanel)
    {
        GameAudio.Instance?.PlayButton();
        returnPanel = sourcePanel;
        returnPanel?.SetActive(false);
        settingsPanel?.SetActive(true);
    }

private void Close()
    {
        GameAudio.Instance?.PlayButton();
        settingsPanel?.SetActive(false);
        (returnPanel != null ? returnPanel : mainPanel)?.SetActive(true);
        returnPanel = null;
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
        GameAudio.Instance?.PlayButton();
        PlayerPrefs.SetInt(SoundEnabledPreferenceKey, IsSoundEnabled ? 0 : 1);
        PlayerPrefs.Save();
        ApplySoundState();
    }

    private void ToggleVibration()
    {
        GameAudio.Instance?.PlayButton();
        GameHaptics.SetEnabled(!GameHaptics.IsEnabled);
        ApplyVibrationState();
    }

    private void ReplayTutorial()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null ||
            (gameManager.CurrentState != GameState.MainMenu &&
             !gameManager.ReturnToMainMenu()))
        {
            return;
        }

        GameAudio.Instance?.PlayButton();
        settingsPanel?.SetActive(false);
        mainPanel?.SetActive(true);

        FirstRunTutorial tutorial = GetComponent<FirstRunTutorial>();
        if (tutorial != null && tutorial.ReplayTutorial())
        {
            return;
        }

        mainPanel?.SetActive(false);
        settingsPanel?.SetActive(true);
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
        ApplyVibrationState();
    }

    private void ApplyVibrationState()
    {
        if (vibrationButtonText == null)
        {
            return;
        }

        string state = GameHaptics.IsEnabled
            ? "VIBRATION ON"
            : "VIBRATION OFF";
        vibrationButtonText.SetText(
            CurrentLanguage == GameLanguage.Korean
                ? koreanTexts[state]
                : state);
    }

    public void RefreshSettings()
    {
        ApplySettings();
    }

private void ApplySoundState()
    {
        bool muted = !IsSoundEnabled;
        if (soundSource != null)
        {
            soundSource.mute = muted;
        }

        GameAudio.Instance?.SetMuted(muted);

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

        gameOverPanel = transform.Find("GameOverPanel")?.gameObject;
        shopPanel = transform.Find("ShopPanel")?.gameObject;

        TMP_FontAsset font = GetComponentInChildren<TextMeshProUGUI>(true)?.font ?? TMP_Settings.defaultFontAsset;
        if (font == null)
        {
            Debug.LogError("InGameSettingsPanel could not find a TMP font asset.", this);
            return;
        }

        openButton = FindButton(mainPanel.transform, "SettingsButton");
        if (openButton == null)
        {
            openButton = CreateButton(
                "SettingsButton",
                mainPanel.transform,
                new Vector2(-70f, -172f),
                new Vector2(180f, 180f),
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
        }

        AddOpenButtonOwner(openButton, mainPanel);
        AddOpenButtonOwner(CloneOpenButton(gameOverPanel), gameOverPanel);
        AddOpenButtonOwner(CloneOpenButton(shopPanel), shopPanel);

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
        tutorialButton = CreateButton("TutorialButton", settingsPanel.transform, new Vector2(0f, -10f), new Vector2(720f, 90f), "PLAY TUTORIAL", new Color(0.2f, 0.4f, 0.38f, 1f), font);
        CreateText("SoundLabel", settingsPanel.transform, new Vector2(0f, -110f), new Vector2(700f, 70f), "SOUND", 38f, font);
        soundButton = CreateButton("SoundButton", settingsPanel.transform, new Vector2(0f, -205f), new Vector2(720f, 90f), "SOUND ON", new Color(0.13f, 0.29f, 0.52f, 1f), font);
        CreateText("VibrationLabel", settingsPanel.transform, new Vector2(0f, -330f), new Vector2(700f, 70f), "VIBRATION", 38f, font);
        vibrationButton = CreateButton("VibrationButton", settingsPanel.transform, new Vector2(0f, -425f), new Vector2(720f, 90f), "VIBRATION ON", new Color(0.13f, 0.29f, 0.52f, 1f), font);
        closeButton = CreateButton("CloseButton", settingsPanel.transform, new Vector2(0f, -570f), new Vector2(720f, 120f), "CLOSE", new Color(0.33f, 0.16f, 0.29f, 1f), font);
        soundButtonText = soundButton.GetComponentInChildren<TextMeshProUGUI>(true);
        vibrationButtonText = vibrationButton.GetComponentInChildren<TextMeshProUGUI>(true);
        settingsPanel.SetActive(false);
    }

    private Button CloneOpenButton(GameObject ownerPanel)
    {
        if (ownerPanel == null || openButton == null)
        {
            return null;
        }

        Button existingButton = FindButton(ownerPanel.transform, "SettingsButton");
        if (existingButton != null)
        {
            return existingButton;
        }

        Button clonedButton = Instantiate(openButton, ownerPanel.transform, false);
        clonedButton.name = "SettingsButton";
        return clonedButton;
    }

    private void AddOpenButtonOwner(Button button, GameObject ownerPanel)
    {
        if (button != null && ownerPanel != null)
        {
            openButtonOwners[button] = ownerPanel;
        }
    }

    private void RegisterOpenButtons()
    {
        foreach (KeyValuePair<Button, GameObject> entry in openButtonOwners)
        {
            Button button = entry.Key;
            GameObject ownerPanel = entry.Value;
            UnityAction action = () => Open(ownerPanel);
            openButtonActions.Add(button, action);
            button.onClick.AddListener(action);
        }
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

    private static Button FindButton(Transform root, string buttonName)
    {
        Button[] buttons = root.GetComponentsInChildren<Button>(true);
        for (int index = 0; index < buttons.Length; index++)
        {
            if (buttons[index].name == buttonName)
            {
                return buttons[index];
            }
        }

        return null;
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
