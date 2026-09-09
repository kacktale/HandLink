using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerStamina : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float baseMaxStamina = 100f;
    [SerializeField, Min(0.01f)] private float recoveryDuration = 1.5f;

    private float recoveryTime;

    public float BaseMaxStamina => baseMaxStamina;
    public float CurrentStamina { get; private set; }
    public float MaxStamina { get; private set; }
    public float Normalized => MaxStamina <= 0f ? 0f : CurrentStamina / MaxStamina;
    public bool IsStunned { get; private set; }

    public event Action<float, float> StaminaChanged;

    private void Awake()
    {
        MaxStamina = Mathf.Max(0.01f, baseMaxStamina);
        CurrentStamina = MaxStamina;
    }

    public bool Tick(Vector2 distanceDelta, bool inputHeld, float deltaTime)
    {
        if (!IsStunned)
        {
            SetCurrent(CurrentStamina - distanceDelta.magnitude * 0.5f);
            if (CurrentStamina <= 0f)
            {
                recoveryTime = 0f;
                IsStunned = true;
            }

            return true;
        }

        if (!inputHeld)
        {
            return false;
        }

        recoveryTime += deltaTime;
        SetCurrent(MaxStamina * recoveryTime / recoveryDuration);
        if (recoveryTime >= recoveryDuration)
        {
            IsStunned = false;
            recoveryTime = 0f;
            SetCurrent(MaxStamina);
        }

        return true;
    }

    public void SetMaxStamina(float maxStamina)
    {
        MaxStamina = Mathf.Max(0.01f, maxStamina);
        SetCurrent(Mathf.Min(CurrentStamina, MaxStamina));
    }

    public void Restore(float amount)
    {
        if (amount > 0f)
        {
            SetCurrent(Mathf.Min(MaxStamina, CurrentStamina + amount));
        }
    }

    public void ResetStamina()
    {
        IsStunned = false;
        recoveryTime = 0f;
        SetCurrent(MaxStamina);
    }

    private void SetCurrent(float value)
    {
        float clampedValue = Mathf.Clamp(value, 0f, MaxStamina);
        if (Mathf.Approximately(CurrentStamina, clampedValue))
        {
            return;
        }

        CurrentStamina = clampedValue;
        StaminaChanged?.Invoke(CurrentStamina, MaxStamina);
    }
}
