using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerProgression))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerStamina))]
public sealed class PlayerUpgradeApplier : MonoBehaviour
{
    [SerializeField] private UpgradeDefinition judgementUpgrade;
    [SerializeField] private UpgradeDefinition extraLifeUpgrade;
    [FormerlySerializedAs("staminaUpgrade")]
    [SerializeField] private UpgradeDefinition scoreUpgrade;
    [SerializeField] private UpgradeDefinition circleSizeUpgrade;

    private PlayerProgression progression;
    private PlayerHealth health;
    private PlayerStamina stamina;
    private Vector3 baseScale;
    public float PulseHitboxScale { get; private set; } = 1f;

    private void Awake()
    {
        progression = GetComponent<PlayerProgression>();
        health = GetComponent<PlayerHealth>();
        stamina = GetComponent<PlayerStamina>();
        baseScale = transform.localScale;
    }

    private void Start()
    {
        progression.ClampUpgradeLevels(GetMaximumLevel);
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
        stamina.SetMaxStamina(stamina.BaseMaxStamina);
        transform.localScale =
            baseScale + Vector3.one * GetUpgradeValue(UpgradeType.CircleSize);
        PulseHitboxScale = Mathf.Clamp01(
            Mathf.Abs(baseScale.x) / Mathf.Max(0.01f, Mathf.Abs(transform.localScale.x)));
    }

    private UpgradeDefinition GetUpgradeDefinition(UpgradeType type)
    {
        return type switch
        {
            UpgradeType.Judgement => judgementUpgrade,
            UpgradeType.ExtraLife => extraLifeUpgrade,
            UpgradeType.Score => scoreUpgrade,
            UpgradeType.CircleSize => circleSizeUpgrade,
            _ => null
        };
    }

    private int GetMaximumLevel(UpgradeType type)
    {
        return GetUpgradeDefinition(type)?.MaxLevel ?? 0;
    }
}
