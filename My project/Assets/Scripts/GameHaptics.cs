using UnityEngine;

public static class GameHaptics
{
    private const string HapticsEnabledPreferenceKey = "HapticsEnabled";
    private const int PerfectDurationMilliseconds = 24;
    private const int PerfectAmplitude = 90;
    private const int DamageDurationMilliseconds = 75;
    private const int DamageAmplitude = 220;

    public static bool IsEnabled =>
        PlayerPrefs.GetInt(HapticsEnabledPreferenceKey, 1) == 1;

    public static void SetEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(HapticsEnabledPreferenceKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void PlayPerfect()
    {
        Vibrate(PerfectDurationMilliseconds, PerfectAmplitude);
    }

    public static void PlayDamage()
    {
        Vibrate(DamageDurationMilliseconds, DamageAmplitude);
    }

    private static void Vibrate(int durationMilliseconds, int amplitude)
    {
        if (!IsEnabled)
        {
            return;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using AndroidJavaClass unityPlayer =
                new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using AndroidJavaObject activity =
                unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using AndroidJavaObject vibrator =
                activity.Call<AndroidJavaObject>("getSystemService", "vibrator");

            if (vibrator == null || !vibrator.Call<bool>("hasVibrator"))
            {
                return;
            }

            using AndroidJavaClass version =
                new AndroidJavaClass("android.os.Build$VERSION");
            int sdkLevel = version.GetStatic<int>("SDK_INT");
            if (sdkLevel >= 26)
            {
                using AndroidJavaClass vibrationEffect =
                    new AndroidJavaClass("android.os.VibrationEffect");
                using AndroidJavaObject effect =
                    vibrationEffect.CallStatic<AndroidJavaObject>(
                        "createOneShot",
                        (long)durationMilliseconds,
                        Mathf.Clamp(amplitude, 1, 255));
                vibrator.Call("vibrate", effect);
            }
            else
            {
                vibrator.Call("vibrate", (long)durationMilliseconds);
            }
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"Haptic feedback failed: {exception.Message}");
        }
#endif
    }
}
