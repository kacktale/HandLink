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
    private const float PulseInterval = 0.3f;
    private const float InitialPulseDelay = 0.05f;
    private const float PulseRadius = 1f;
    private const float PulseHitThickness = 0.14f;
    private const float PulseWarningDuration = 0.15f;
    private const float PulseWarningFlashInterval = 0.06f;
    private const float PulseVisualDuration = 0.28f;

    private static readonly Color PulseWarningColor = new Color(1f, 0.35f, 0.6f);

    private SpecialEnemyType type;
    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private LineRenderer pulseRenderer;
    private float nextPulseTime;
    private float pulseVisualTime;
    private float previousPulseRadius;
    private bool pulseDamageAvailable;
    private bool warningSoundPlayed;

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
        nextPulseTime = InitialPulseDelay;
        pulseVisualTime = 0f;
        previousPulseRadius = 0f;
        pulseDamageAvailable = false;
        warningSoundPlayed = false;
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
        if (type != SpecialEnemyType.Pulse ||
            Player.Instance == null ||
            GameManager.Instance == null ||
            !GameManager.Instance.IsGameplayActive)
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
        GameAudio.Instance?.PlayHeal();
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
        GameAudio.Instance?.PlayPulseBurst();
        warningSoundPlayed = false;

        pulseVisualTime = PulseVisualDuration;
        previousPulseRadius = 0f;
        pulseDamageAvailable = true;
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

        TryDamagePlayerAtWaveFront(radius);
        previousPulseRadius = radius;

        Color color = new Color(1f, 0f, 0f, 1f - progress);
        pulseRenderer.startColor = color;
        pulseRenderer.endColor = color;

        if (pulseVisualTime <= 0f)
        {
            SetPulseVisible(false);
        }
    }

    private void TryDamagePlayerAtWaveFront(float currentRadius)
    {
        if (!pulseDamageAvailable || Player.Instance == null)
        {
            return;
        }

        float playerDistance = Vector2.Distance(
            transform.position,
            Player.Instance.transform.position);
        if (playerDistance < previousPulseRadius - PulseHitThickness ||
            playerDistance > currentRadius + PulseHitThickness)
        {
            return;
        }

        pulseDamageAvailable = false;
        Player.Instance.Damage();
    }

private void UpdatePulseWarning()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (nextPulseTime > PulseWarningDuration)
        {
            warningSoundPlayed = false;
            spriteRenderer.color = Color.red;
            return;
        }

        if (!warningSoundPlayed)
        {
            GameAudio.Instance?.PlayPulseWarning();
            warningSoundPlayed = true;
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
        pulseRenderer.widthMultiplier = 0.12f;
        pulseRenderer.material = new Material(Shader.Find("Sprites/Default"));
        pulseRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;

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
