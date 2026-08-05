using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerHealth : MonoBehaviour
{
    [SerializeField, Min(1)] private int baseHealth = 3;

    public int BaseHealth => baseHealth;
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

    public event Action<int, int> HealthChanged;
    public event Action Damaged;
    public event Action Died;

    private void Awake()
    {
        MaxHealth = Mathf.Max(1, baseHealth);
        CurrentHealth = MaxHealth;
    }

    public void SetMaxHealth(int maxHealth, bool refill)
    {
        MaxHealth = Mathf.Max(1, maxHealth);
        CurrentHealth = refill
            ? MaxHealth
            : Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public bool TakeDamage()
    {
        if (IsDead)
        {
            return false;
        }

        CurrentHealth--;
        Damaged?.Invoke();
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (IsDead)
        {
            Died?.Invoke();
        }

        return true;
    }

    public bool Heal(int amount)
    {
        if (amount <= 0 || IsDead || CurrentHealth >= MaxHealth)
        {
            return false;
        }

        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        return true;
    }

    public void ResetHealth()
    {
        CurrentHealth = MaxHealth;
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }
}
