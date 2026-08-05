using UnityEngine;

public enum SpecialEnemyType
{
    None,
    Pulse,
    HeartHealer
}

[DisallowMultipleComponent]
public sealed class SpecialEnemy : MonoBehaviour
{
    private const int PulseSegmentCount = 32;
    private const float PulseInterval = 0.5f;
    private const float PulseRadius = 0.5f;
    private const float PulseWarningDuration = 0.25f;
    private const float PulseWarningFlashInterval = 0.1f;
    private const float PulseVisualDuration = 0.35f;

    private static readonly Color PulseWarningColor = new Color(1f, 0.35f, 0.6f);

    private SpecialEnemyType type;
    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private LineRenderer pulseRenderer;
    private float nextPulseTime;
    private float pulseVisualTime;

    public void Configure(SpecialEnemyType enemyType)
    {
        type = enemyType;
        spriteRenderer ??= GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            return;
        }

        baseColor = spriteRenderer.color;
        spriteRenderer.color = type switch
        {
            SpecialEnemyType.Pulse => Color.red,
            SpecialEnemyType.HeartHealer => Color.green,
            _ => baseColor
        };

        if (type == SpecialEnemyType.Pulse)
        {
            CreatePulseRenderer();
        }
    }

    private void OnEnable()
    {
        nextPulseTime = PulseInterval;
        pulseVisualTime = 0f;
        if (type == SpecialEnemyType.Pulse && spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }
        SetPulseVisible(false);
    }

    private void OnDisable()
    {
        SetPulseVisible(false);
    }

    private void Update()
    {
        if (type != SpecialEnemyType.Pulse || Player.Instance == null || !Player.Instance.gameStarted)
        {
            return;
        }

        nextPulseTime -= Time.deltaTime;
        if (nextPulseTime <= 0f)
        {
            nextPulseTime += PulseInterval;
            EmitPulse();
        }

        UpdatePulseWarning();
        UpdatePulseVisual();
    }

    public bool TryHandleHeartArrival()
    {
        if (type != SpecialEnemyType.HeartHealer || Player.Instance == null)
        {
            return false;
        }

        Player.Instance.Heal(1);
        gameObject.SetActive(false);
        return true;
    }

    public bool TryHandlePlayerContact()
    {
        if (type != SpecialEnemyType.HeartHealer)
        {
            return false;
        }

        gameObject.SetActive(false);
        return true;
    }

    private void EmitPulse()
    {
        Player player = Player.Instance;
        if (Vector2.Distance(transform.position, player.transform.position) <= PulseRadius)
        {
            player.Damage();
        }

        pulseVisualTime = PulseVisualDuration;
        SetPulseVisible(true);
    }

    private void UpdatePulseVisual()
    {
        if (pulseRenderer == null || pulseVisualTime <= 0f)
        {
            return;
        }

        pulseVisualTime = Mathf.Max(0f, pulseVisualTime - Time.deltaTime);
        float progress = 1f - (pulseVisualTime / PulseVisualDuration);
        float radius = Mathf.Lerp(0.05f, PulseRadius, progress);
        float parentScale = Mathf.Max(0.01f, transform.lossyScale.x);
        pulseRenderer.transform.localScale = Vector3.one * (radius / parentScale);

        Color color = new Color(1f, 0f, 0f, 1f - progress);
        pulseRenderer.startColor = color;
        pulseRenderer.endColor = color;

        if (pulseVisualTime <= 0f)
        {
            SetPulseVisible(false);
        }
    }

    private void UpdatePulseWarning()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (nextPulseTime > PulseWarningDuration)
        {
            spriteRenderer.color = Color.red;
            return;
        }

        float warningElapsed = PulseWarningDuration - nextPulseTime;
        int flashIndex = Mathf.FloorToInt(warningElapsed / PulseWarningFlashInterval);
        spriteRenderer.color = flashIndex % 2 == 0 ? PulseWarningColor : Color.red;
    }

    private void CreatePulseRenderer()
    {
        if (pulseRenderer != null)
        {
            return;
        }

        GameObject waveObject = new GameObject("PulseWave");
        waveObject.transform.SetParent(transform, false);
        pulseRenderer = waveObject.AddComponent<LineRenderer>();
        pulseRenderer.useWorldSpace = false;
        pulseRenderer.loop = true;
        pulseRenderer.positionCount = PulseSegmentCount;
        pulseRenderer.widthMultiplier = 0.08f;
        pulseRenderer.material = new Material(Shader.Find("Sprites/Default"));
        pulseRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;

        for (int index = 0; index < PulseSegmentCount; index++)
        {
            float angle = index * Mathf.PI * 2f / (PulseSegmentCount - 1);
            pulseRenderer.SetPosition(index, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f));
        }
    }

    private void SetPulseVisible(bool isVisible)
    {
        if (pulseRenderer != null)
        {
            pulseRenderer.enabled = isVisible;
        }
    }
}
