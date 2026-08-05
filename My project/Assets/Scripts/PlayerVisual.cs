using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class PlayerVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer hartCircle;

    private SpriteRenderer coreRenderer;
    private Color baseHartCircleColor;

    private void Awake()
    {
        coreRenderer = GetComponent<SpriteRenderer>();
        baseHartCircleColor =
            hartCircle != null ? hartCircle.color : Color.white;
    }

    public void SetGameplayVisible(bool visible)
    {
        coreRenderer.enabled = visible;
    }

    public void ResetVisual()
    {
        SetHartCircleColor(baseHartCircleColor);
    }

    public void RefreshHeartCircleColor()
    {
        if (hartCircle == null)
        {
            return;
        }

        float heartDistance =
            Vector2.Distance(transform.position, hartCircle.transform.position);
        float heartRadius =
            Mathf.Min(hartCircle.bounds.extents.x, hartCircle.bounds.extents.y);

        if (heartDistance <= heartRadius)
        {
            SetHartCircleColor(Color.white);
            return;
        }

        if (SpawnPivot.Instance != null &&
            SpawnPivot.Instance.TryGetHeartDistanceJudgementColor(
                heartDistance,
                out Color judgementColor))
        {
            SetHartCircleColor(judgementColor);
            return;
        }

        SetHartCircleColor(baseHartCircleColor);
    }

    private void SetHartCircleColor(Color color)
    {
        if (hartCircle == null)
        {
            return;
        }

        color.a = baseHartCircleColor.a;
        hartCircle.color = color;
    }
}
