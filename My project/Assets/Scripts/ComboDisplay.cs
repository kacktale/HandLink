using TMPro;
using UnityEngine;

// Combo feedback only colors the existing score; no additional text or per-frame polling.
public sealed class ComboDisplay : MonoBehaviour
{
    private ScoreController score;
    private TextMeshProUGUI label;

    public void Initialize(ScoreController source, TextMeshProUGUI scoreText)
    {
        score = source;
        label = scoreText;
        score.ComboChanged += Refresh;
        Refresh();
    }

    private void Refresh()
    {
        label.color = GetComboColor(score.Combo);
    }

    public static Color GetComboColor(int combo)
    {
        if (combo < 10) return Color.white;
        if (combo < 30) return new Color(0.55f, 0.9f, 1f);
        if (combo < 60) return new Color(0.3f, 1f, 0.85f);
        if (combo < 100) return new Color(0.75f, 0.6f, 1f);
        if (combo < 180) return new Color(1f, 0.55f, 0.8f);
        return new Color(1f, 0.82f, 0.25f);
    }

    private void OnDestroy()
    {
        if (score != null) score.ComboChanged -= Refresh;
    }
}
