using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerProgression))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerStamina))]
public sealed class PlayerUpgradeApplier : MonoBehaviour
{
    [SerializeField] private UpgradeDefinition judgementUpgrade;
    [SerializeField] private UpgradeDefinition extraLifeUpgrade;
    [SerializeField] private UpgradeDefinition staminaUpgrade;
    [SerializeField] private UpgradeDefinition circleSizeUpgrade;

    private PlayerProgression progression;
    private PlayerHealth health;
    private PlayerStamina stamina;
    private Vector3 baseScale;

    private void Awake()
    {
        progression = GetComponent<PlayerProgression>();
        health = GetComponent<PlayerHealth>();
        stamina = GetComponent<PlayerStamina>();
        baseScale = transform.localScale;
    }

    public float GetUpgradeValue(UpgradeType type)
    {
        UpgradeDefinition definition = GetUpgradeDefinition(type);
        return definition == null
            ? 0f
            : definition.GetTotalValue(progression.GetLevel(type));
    }

    public void Apply()
    {
        int maxHealth = health.BaseHealth +
                        Mathf.RoundToInt(GetUpgradeValue(UpgradeType.ExtraLife));
        health.SetMaxHealth(maxHealth, refill: true);
        stamina.SetMaxStamina(
            stamina.BaseMaxStamina + GetUpgradeValue(UpgradeType.Stamina));
        transform.localScale =
            baseScale + Vector3.one * GetUpgradeValue(UpgradeType.CircleSize);
    }

    private UpgradeDefinition GetUpgradeDefinition(UpgradeType type)
    {
        return type switch
        {
            UpgradeType.Judgement => judgementUpgrade,
            UpgradeType.ExtraLife => extraLifeUpgrade,
            UpgradeType.Stamina => staminaUpgrade,
            UpgradeType.CircleSize => circleSizeUpgrade,
            _ => null
        };
    }
}
