using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ScoreController : MonoBehaviour
{
    public float CurrentScore { get; private set; }

    public event Action<float> ScoreChanged;

    public void AddScore(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        CurrentScore += amount;
        ScoreChanged?.Invoke(CurrentScore);
    }

    public void ResetScore()
    {
        CurrentScore = 0f;
        ScoreChanged?.Invoke(CurrentScore);
    }
}
