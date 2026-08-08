using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class KomintResponsiveText : MonoBehaviour
{
    private const float MinimumFontSize = 12f;
    private const float MinimumScale = 0.5f;
    private const float ButtonHorizontalMargin = 8f;
    private const float ButtonVerticalMargin = 4f;

    private void OnEnable()
    {
        ApplyAll();
    }

    private void Start()
    {
        // Runtime settings and tutorial UI are created during Awake.
        ApplyAll();
    }

    private void OnTransformChildrenChanged()
    {
        ApplyAll();
    }

    public void ApplyAll()
    {
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int index = 0; index < texts.Length; index++)
        {
            Apply(texts[index]);
        }
    }

    private static void Apply(TextMeshProUGUI text)
    {
        if (text == null)
        {
            return;
        }

        if (!text.enableAutoSizing)
        {
            float maximumSize = Mathf.Max(MinimumFontSize, text.fontSize);
            text.fontSizeMax = maximumSize;
            text.fontSizeMin = Mathf.Min(
                maximumSize,
                Mathf.Max(MinimumFontSize, maximumSize * MinimumScale));
            text.enableAutoSizing = true;
        }

        if (text.GetComponentInParent<Button>() == null)
        {
            return;
        }

        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        Vector4 margin = text.margin;
        margin.x = Mathf.Max(margin.x, ButtonHorizontalMargin);
        margin.y = Mathf.Max(margin.y, ButtonVerticalMargin);
        margin.z = Mathf.Max(margin.z, ButtonHorizontalMargin);
        margin.w = Mathf.Max(margin.w, ButtonVerticalMargin);
        text.margin = margin;
    }
}
