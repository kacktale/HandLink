using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ScoreController : MonoBehaviour
{
    public long CurrentScore { get; private set; }

    public event Action<long> ScoreChanged;

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
        CurrentScore = 0L;
        ScoreChanged?.Invoke(CurrentScore);
    }
}
