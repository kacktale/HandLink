using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class KomintButtonTheme : MonoBehaviour
{
    private static readonly Color PrimaryColor = new(0.035f, 0.24f, 0.72f, 1f);
    private static readonly Color AccentColor = new(0.02f, 0.42f, 0.68f, 1f);
    private static readonly Color SecondaryColor = new(0.025f, 0.075f, 0.16f, 1f);
    private static readonly Color DangerColor = new(0.24f, 0.055f, 0.08f, 1f);
    private static readonly Color DisabledColor = new(0.08f, 0.11f, 0.16f, 0.72f);
    private static readonly Color BlueOutline = new(0.08f, 0.42f, 1f, 1f);
    private static readonly Color CyanOutline = new(0f, 0.9f, 1f, 1f);
    private static readonly Color DangerOutline = new(0.95f, 0.2f, 0.28f, 1f);
    private static readonly Color CyanText = new(0f, 0.88f, 0.96f, 1f);

    private static Sprite roundedSprite;

    private void OnEnable()
    {
        ApplyAll();
    }

    private void Start()
    {
        // Runtime-created settings and tutorial buttons are available after Awake.
        ApplyAll();
    }

    public void ApplyAll()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Apply(buttons[i]);
        }
    }

    private static void Apply(Button button)
    {
        if (button == null || button.targetGraphic is not Image image)
        {
            return;
        }

        string buttonName = button.name;
        bool isUpgradeCard = button.GetComponent<ShopUpgradeButton>() != null;
        bool isDanger = buttonName == "CloseButton";
        bool isSkip = buttonName == "Skip" || buttonName == "SkipButton";
        bool isPrimary = buttonName == "StartButton";
        bool isAccent = buttonName == "RestartButton" || IsSelectedToggle(buttonName);

        Color baseColor = isDanger
            ? DangerColor
            : isPrimary
                ? PrimaryColor
                : isAccent
                    ? AccentColor
                    : SecondaryColor;

        image.color = isSkip ? new Color(0f, 0f, 0f, 0f) : baseColor;
        ApplyRoundedSprite(image);
        ApplyTransition(button);

        if (!isSkip)
        {
            Outline outline = button.GetComponent<Outline>();
            if (outline == null)
            {
                outline = button.gameObject.AddComponent<Outline>();
            }

            outline.effectColor = isDanger ? DangerOutline : (isPrimary || isAccent ? CyanOutline : BlueOutline);
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;
        }

        if (isUpgradeCard)
        {
            return;
        }

        TextMeshProUGUI[] labels = button.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < labels.Length; i++)
        {
            labels[i].color = isSkip ? CyanText : Color.white;
            labels[i].enableWordWrapping = false;
        }
    }

    private static void ApplyRoundedSprite(Image image)
    {
        if (roundedSprite == null)
        {
            roundedSprite = Resources.Load<Sprite>("UI/KomintRoundedButton");
        }

        if (roundedSprite == null)
        {
            return;
        }

        image.sprite = roundedSprite;
        image.type = Image.Type.Sliced;
    }

    private static void ApplyTransition(Button button)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
        colors.selectedColor = new Color(1f, 1f, 1f, 1f);
        colors.pressedColor = new Color(0.72f, 0.82f, 1f, 1f);
        colors.disabledColor = DisabledColor;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.transition = Selectable.Transition.ColorTint;
        button.colors = colors;
    }

    private static bool IsSelectedToggle(string buttonName)
    {
        if (buttonName == "KoreanButton" || buttonName == "Korean")
        {
            return PlayerPrefs.GetInt("GameLanguage", (int)GameLanguage.Korean) == (int)GameLanguage.Korean;
        }

        if (buttonName == "EnglishButton" || buttonName == "English")
        {
            return PlayerPrefs.GetInt("GameLanguage", (int)GameLanguage.Korean) == (int)GameLanguage.English;
        }

        if (buttonName == "SoundButton")
        {
            return PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        }

        return buttonName == "VibrationButton" && GameHaptics.IsEnabled;
    }
}
