using UnityEngine;

[DefaultExecutionOrder(-200)]
[DisallowMultipleComponent]
public sealed class MobileFrameRateController : MonoBehaviour
{
    private const int MinimumTargetFrameRate = 60;
    private const int FallbackTargetFrameRate = 120;

    private void Awake()
    {
        ApplyDisplayRefreshRate();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            ApplyDisplayRefreshRate();
        }
    }

    private static void ApplyDisplayRefreshRate()
    {
        QualitySettings.vSyncCount = 0;

        double refreshRate = Screen.currentResolution.refreshRateRatio.value;
        int targetFrameRate = refreshRate > 0d
            ? Mathf.RoundToInt((float)refreshRate)
            : FallbackTargetFrameRate;

        Application.targetFrameRate = Mathf.Max(MinimumTargetFrameRate, targetFrameRate);
    }
}
