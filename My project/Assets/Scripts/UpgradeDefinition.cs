using UnityEngine;

public enum UpgradeType
{
    Judgement = 0,
    ExtraLife = 1,
    Score = 2, // Keep the former stamina upgrade's saved slot.
    CircleSize = 3
}

[CreateAssetMenu(fileName = "UpgradeDefinition", menuName = "HandLink/Upgrade Definition")]
public class UpgradeDefinition : ScriptableObject
{
    [SerializeField] private float[] value;
    [SerializeField] private int[] cost;

    public int MaxLevel => Mathf.Min(value?.Length ?? 0, cost?.Length ?? 0);

    public float GetValue(int level) => value[level];
    public int GetCost(int level) => cost[level];

    public float GetTotalValue(int purchasedLevel)
    {
        float total = 0f;
        int count = Mathf.Min(purchasedLevel, MaxLevel);
        for (int index = 0; index < count; index++)
        {
            total += value[index];
        }

        return total;
    }
}
