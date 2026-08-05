using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerProgression : MonoBehaviour
{
    private const string CoinKey = "PlayerProgression.Coin";
    private const string BestScoreKey = "PlayerProgression.BestScore";
    private const string UpgradeLevelKeyPrefix = "PlayerProgression.UpgradeLevel.";

    [SerializeField] private int coin;
    [SerializeField] private float bestScore;
    [SerializeField, HideInInspector] private int[] upgradeLevels = new int[4];

    public int Coin => coin;
    public float BestScore => bestScore;

    public event Action Changed;

    private void Awake()
    {
        Load();
    }

    public int GetLevel(UpgradeType type)
    {
        EnsureUpgradeLevelArray();
        return upgradeLevels[(int)type];
    }

    public bool TryPurchase(UpgradeType type, UpgradeDefinition definition)
    {
        int level = GetLevel(type);
        if (level >= definition.MaxLevel)
        {
            return false;
        }

        int requiredCoin = definition.GetCost(level);
        if (coin < requiredCoin)
        {
            return false;
        }

        coin -= requiredCoin;
        upgradeLevels[(int)type]++;
        Save();
        Changed?.Invoke();
        return true;
    }

    public void AddCoin(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        coin += amount;
        Save();
        Changed?.Invoke();
    }

    public void SaveBestScore(float score)
    {
        if (score <= bestScore)
        {
            return;
        }

        bestScore = score;
        Save();
        Changed?.Invoke();
    }

    private void Load()
    {
        EnsureUpgradeLevelArray();
        coin = PlayerPrefs.GetInt(CoinKey, coin);
        bestScore = PlayerPrefs.GetFloat(BestScoreKey, bestScore);
        for (int index = 0; index < upgradeLevels.Length; index++)
        {
            upgradeLevels[index] = PlayerPrefs.GetInt(UpgradeLevelKeyPrefix + index, upgradeLevels[index]);
        }
    }

    private void Save()
    {
        PlayerPrefs.SetInt(CoinKey, coin);
        PlayerPrefs.SetFloat(BestScoreKey, bestScore);
        for (int index = 0; index < upgradeLevels.Length; index++)
        {
            PlayerPrefs.SetInt(UpgradeLevelKeyPrefix + index, upgradeLevels[index]);
        }

        PlayerPrefs.Save();
    }

    private void EnsureUpgradeLevelArray()
    {
        if (upgradeLevels == null || upgradeLevels.Length != Enum.GetValues(typeof(UpgradeType)).Length)
        {
            upgradeLevels = new int[Enum.GetValues(typeof(UpgradeType)).Length];
        }
    }
}
