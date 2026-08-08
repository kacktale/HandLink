using UnityEngine;

[CreateAssetMenu(
    fileName = "DifficultyConfig",
    menuName = "HandLink/Difficulty Config")]
public sealed class DifficultyConfig : ScriptableObject
{
    [SerializeField] private AnimationCurve spawnInterval =
        AnimationCurve.Linear(0f, 2f, 120f, 0.3f);
    [SerializeField] private AnimationCurve enemyTravelTime =
        AnimationCurve.Constant(0f, 120f, 1.2f);
    [SerializeField] private AnimationCurve scoreMultiplier =
        AnimationCurve.Constant(0f, 120f, 1f);
    [SerializeField] private AnimationCurve pulseEnemyChance =
        AnimationCurve.Constant(0f, 120f, 0.15f);
    [SerializeField] private AnimationCurve healingSpawnInterval =
        AnimationCurve.Constant(0f, 120f, 20f);
    [SerializeField, Min(0f)] private float initialProtectionDuration = 2f;
    [SerializeField, Min(0.01f)] private float minimumSpawnInterval = 0.35f;
    [SerializeField, Min(1)] private int maxConcurrentEnemies = 12;

    public float InitialProtectionDuration =>
        Mathf.Max(0f, initialProtectionDuration);
    public int MaxConcurrentEnemies => Mathf.Max(1, maxConcurrentEnemies);

    public float EvaluateSpawnInterval(float elapsedTime)
    {
        return Mathf.Max(
            minimumSpawnInterval,
            Evaluate(spawnInterval, elapsedTime, 2f));
    }

    public float EvaluateEnemyTravelTime(float elapsedTime)
    {
        return Mathf.Max(
            0.01f,
            Evaluate(enemyTravelTime, elapsedTime, 1.2f));
    }

    public float EvaluateScoreMultiplier(float elapsedTime)
    {
        return Mathf.Max(
            0f,
            Evaluate(scoreMultiplier, elapsedTime, 1f));
    }

    public float EvaluatePulseEnemyChance(float elapsedTime)
    {
        return Mathf.Clamp01(
            Evaluate(pulseEnemyChance, elapsedTime, 0.15f));
    }

    public float EvaluateHealingSpawnInterval(float elapsedTime)
    {
        return Mathf.Max(
            1f,
            Evaluate(healingSpawnInterval, elapsedTime, 20f));
    }

    private static float Evaluate(
        AnimationCurve curve,
        float elapsedTime,
        float fallback)
    {
        return curve == null || curve.length == 0
            ? fallback
            : curve.Evaluate(Mathf.Max(0f, elapsedTime));
    }
}
