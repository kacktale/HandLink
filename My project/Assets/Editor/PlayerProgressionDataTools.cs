using UnityEditor;
using UnityEngine;

public static class PlayerProgressionDataTools
{
    [MenuItem("Tools/Komint/Reset Player Progression Data")]
    private static void ResetPlayerProgressionData()
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Reset Player Progression Data",
            "코인, 최고 점수, 업그레이드 레벨을 초기화합니다. 계속할까요?",
            "Reset",
            "Cancel");
        if (!confirmed)
        {
            return;
        }

        PlayerProgression[] progressions =
            Object.FindObjectsByType<PlayerProgression>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
        if (progressions.Length == 0)
        {
            PlayerProgression.DeleteSavedData();
        }
        else
        {
            foreach (PlayerProgression progression in progressions)
            {
                progression.ResetProgressionData();
            }
        }

        Debug.Log("Player progression data was reset.");
    }
}
