using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class PlayerVisual : MonoBehaviour
{
    [FormerlySerializedAs("hartCircle")]
    [SerializeField] private SpriteRenderer heartCircle;

    private SpriteRenderer coreRenderer;
    private Color baseHeartCircleColor;

    private void Awake()
    {
        coreRenderer = GetComponent<SpriteRenderer>();
        baseHeartCircleColor =
            heartCircle != null ? heartCircle.color : Color.white;
    }

    public void SetGameplayVisible(bool visible)
    {
        coreRenderer.enabled = visible;
    }

    public void ResetVisual()
    {
        SetHeartCircleColor(baseHeartCircleColor);
    }

    public void RefreshHeartCircleColor()
    {
        if (heartCircle == null)
        {
            return;
        }

        float heartDistance =
            Vector2.Distance(transform.position, heartCircle.transform.position);
        float heartRadius =
            Mathf.Min(heartCircle.bounds.extents.x, heartCircle.bounds.extents.y);

        if (heartDistance <= heartRadius)
        {
            SetHeartCircleColor(Color.white);
            return;
        }

        if (SpawnPivot.Instance != null &&
            SpawnPivot.Instance.TryGetHeartDistanceJudgementColor(
                heartDistance,
                out Color judgementColor))
        {
            SetHeartCircleColor(judgementColor);
            return;
        }

        SetHeartCircleColor(baseHeartCircleColor);
    }

    private void SetHeartCircleColor(Color color)
    {
        if (heartCircle == null)
        {
            return;
        }

        color.a = baseHeartCircleColor.a;
        heartCircle.color = color;
    }
}
