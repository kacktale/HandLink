using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ScoreController : MonoBehaviour
{
    public long CurrentScore { get; private set; }
    public int Combo { get; private set; }
    public float ComboBonus => GetComboBonus(Combo);
    public float ShopBonus { get; private set; }
    public float BonusMultiplier => 1f + ShopBonus + ComboBonus;

    public event Action<long> ScoreChanged;
    public event Action ComboChanged;

    public static float GetComboBonus(int combo)
    {
        int milestone = Math.Max(0, combo) / 10;
        if (milestone == 0) return 0f;
        if (milestone == 1) return 0.01f;
        return Math.Min(20, milestone + 2) / 100f;
    }

    public void SetShopBonus(float bonus)
    {
        ShopBonus = Mathf.Clamp(bonus, 0f, 0.5f);
        ComboChanged?.Invoke();
    }

    public void RegisterDefeat(long baseScore, bool isPerfect)
    {
        if (baseScore <= 0L)
        {
            return;
        }

        if (isPerfect)
        {
            Combo = Combo < int.MaxValue ? Combo + 1 : int.MaxValue;
        }

        // Combine bonuses before rounding the award.
        double scaledScore = Math.Round(baseScore * (double)BonusMultiplier,
            MidpointRounding.AwayFromZero);
        AddScore(scaledScore >= long.MaxValue ? long.MaxValue : (long)scaledScore);
        if (isPerfect)
        {
            ComboChanged?.Invoke();
        }
    }

    public void ResetCombo()
    {
        if (Combo == 0) return;
        Combo = 0;
        ComboChanged?.Invoke();
    }

    public void AddScore(long amount)
    {
        if (amount <= 0L)
        {
            return;
        }

        CurrentScore = amount > long.MaxValue - CurrentScore
            ? long.MaxValue
            : CurrentScore + amount;
        ScoreChanged?.Invoke(CurrentScore);
    }

    public void ResetScore()
    {
        ResetCombo();
        CurrentScore = 0L;
        ScoreChanged?.Invoke(CurrentScore);
    }
}
