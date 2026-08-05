using UnityEngine;

public sealed class DamageFeedback : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private DamageVignette vignette;
    [SerializeField, Min(0.01f)] private float duration = 0.22f;
    [SerializeField, Min(0f)] private float shakeMagnitude = 0.12f;
    [SerializeField, Range(0f, 1f)] private float maxVignetteIntensity = 0.7f;

    private Vector3 baseCameraPosition;
    private float remainingTime;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }

        if (targetCamera != null)
        {
            baseCameraPosition = targetCamera.transform.localPosition;
        }

        vignette?.SetIntensity(0f);
    }

    private void OnDisable()
    {
        ResetFeedback();
    }

    public void Play()
    {
        if (targetCamera != null)
        {
            baseCameraPosition = targetCamera.transform.localPosition;
        }

        remainingTime = duration;
    }

    private void LateUpdate()
    {
        if (remainingTime <= 0f)
        {
            return;
        }

        remainingTime = Mathf.Max(0f, remainingTime - Time.unscaledDeltaTime);
        float normalizedTime = remainingTime / duration;
        float strength = Mathf.Sin(normalizedTime * Mathf.PI);

        if (targetCamera != null)
        {
            Vector2 offset = Random.insideUnitCircle * (shakeMagnitude * strength);
            targetCamera.transform.localPosition = baseCameraPosition + new Vector3(offset.x, offset.y, 0f);
        }

        vignette?.SetIntensity(maxVignetteIntensity * strength);

        if (remainingTime <= 0f)
        {
            ResetFeedback();
        }
    }

    private void ResetFeedback()
    {
        if (targetCamera != null)
        {
            targetCamera.transform.localPosition = baseCameraPosition;
        }

        vignette?.SetIntensity(0f);
    }
}
