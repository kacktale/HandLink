using System;
using System.Globalization;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerProgression : MonoBehaviour
{
    private const int CurrentSaveVersion = 1;
    private const int MaximumStoredUpgradeLevel = 1000;
    private const int MaximumUpgradeKeysToClear = 64;

    private const string SaveVersionKey = "PlayerProgression.SaveVersion";
    private const string CoinKey = "PlayerProgression.Coin";
    private const string BestScoreKey = "PlayerProgression.BestScore";
    private const string BestScoreStorageVersionKey =
        "PlayerProgression.BestScore.StorageVersion";
    private const int BestScoreStorageVersion = 1;
    private const string UpgradeCountKey = "PlayerProgression.UpgradeCount";
    private const string UpgradeLevelKeyPrefix =
        "PlayerProgression.UpgradeLevel.";

    [SerializeField] private int coin;
    [SerializeField] private long bestScore;
    [SerializeField, HideInInspector] private int[] upgradeLevels = new int[4];

    private int loadedSaveVersion;

    public int Coin => coin;
    public long BestScore => bestScore;
    public int SaveVersion => loadedSaveVersion;

    public event Action Changed;

    private void Awake()
    {
        Load();
    }

    public int GetLevel(UpgradeType type)
    {
        EnsureUpgradeLevelArray();
        int index = (int)type;
        return index >= 0 && index < upgradeLevels.Length
            ? upgradeLevels[index]
            : 0;
    }

    public bool TryPurchase(UpgradeType type, UpgradeDefinition definition)
    {
        if (definition == null)
        {
            return false;
        }

        int index = (int)type;
        if (index < 0 || index >= upgradeLevels.Length)
        {
            return false;
        }

        int level = GetLevel(type);
        if (level >= definition.MaxLevel)
        {
            return false;
        }

        int requiredCoin = Mathf.Max(0, definition.GetCost(level));
        if (coin < requiredCoin)
        {
            return false;
        }

        coin -= requiredCoin;
        upgradeLevels[index]++;
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

        coin = amount > int.MaxValue - coin
            ? int.MaxValue
            : coin + amount;
        Save();
        Changed?.Invoke();
    }

    public void SaveBestScore(long score)
    {
        if (score <= bestScore)
        {
            return;
        }

        bestScore = Math.Max(0L, score);
        Save();
        Changed?.Invoke();
    }

    public void ClampUpgradeLevels(Func<UpgradeType, int> getMaxLevel)
    {
        if (getMaxLevel == null)
        {
            return;
        }

        EnsureUpgradeLevelArray();
        bool changed = false;
        for (int index = 0; index < upgradeLevels.Length; index++)
        {
            UpgradeType type = (UpgradeType)index;
            int clampedLevel = Mathf.Clamp(
                upgradeLevels[index],
                0,
                Mathf.Max(0, getMaxLevel(type)));
            if (upgradeLevels[index] == clampedLevel)
            {
                continue;
            }

            upgradeLevels[index] = clampedLevel;
            changed = true;
        }

        if (!changed)
        {
            return;
        }

        Save();
        Changed?.Invoke();
    }

    public void ResetProgressionData()
    {
        DeleteSavedData();
        coin = 0;
        bestScore = 0L;
        EnsureUpgradeLevelArray();
        Array.Clear(upgradeLevels, 0, upgradeLevels.Length);
        loadedSaveVersion = CurrentSaveVersion;
        Changed?.Invoke();
    }

    public static void DeleteSavedData()
    {
        PlayerPrefs.DeleteKey(SaveVersionKey);
        PlayerPrefs.DeleteKey(CoinKey);
        PlayerPrefs.DeleteKey(BestScoreKey);
        PlayerPrefs.DeleteKey(BestScoreStorageVersionKey);
        PlayerPrefs.DeleteKey(UpgradeCountKey);
        for (int index = 0; index < MaximumUpgradeKeysToClear; index++)
        {
            PlayerPrefs.DeleteKey(UpgradeLevelKeyPrefix + index);
        }

        PlayerPrefs.Save();
    }

    private void Load()
    {
        EnsureUpgradeLevelArray();
        loadedSaveVersion = Mathf.Max(
            0,
            PlayerPrefs.GetInt(SaveVersionKey, 0));
        bool needsSave = loadedSaveVersion < CurrentSaveVersion;

        int storedCoin = PlayerPrefs.GetInt(CoinKey, coin);
        coin = Mathf.Max(0, storedCoin);
        needsSave |= coin != storedCoin;
        needsSave |= LoadBestScore();

        int storedUpgradeCount = Mathf.Clamp(
            PlayerPrefs.GetInt(UpgradeCountKey, upgradeLevels.Length),
            0,
            MaximumUpgradeKeysToClear);
        needsSave |= storedUpgradeCount != upgradeLevels.Length;

        for (int index = 0; index < upgradeLevels.Length; index++)
        {
            int storedLevel = PlayerPrefs.GetInt(
                UpgradeLevelKeyPrefix + index,
                upgradeLevels[index]);
            int clampedLevel = Mathf.Clamp(
                storedLevel,
                0,
                MaximumStoredUpgradeLevel);
            upgradeLevels[index] = clampedLevel;
            needsSave |= storedLevel != clampedLevel;
        }

        if (needsSave && loadedSaveVersion <= CurrentSaveVersion)
        {
            Save();
        }
    }

    private void Save()
    {
        if (loadedSaveVersion > CurrentSaveVersion)
        {
            return;
        }

        EnsureUpgradeLevelArray();
        loadedSaveVersion = Math.Max(
            loadedSaveVersion,
            CurrentSaveVersion);
        coin = Mathf.Max(0, coin);
        bestScore = Math.Max(0L, bestScore);

        PlayerPrefs.SetInt(SaveVersionKey, loadedSaveVersion);
        PlayerPrefs.SetInt(CoinKey, coin);
        PlayerPrefs.SetString(
            BestScoreKey,
            bestScore.ToString(CultureInfo.InvariantCulture));
        PlayerPrefs.SetInt(
            BestScoreStorageVersionKey,
            BestScoreStorageVersion);
        PlayerPrefs.SetInt(UpgradeCountKey, upgradeLevels.Length);
        for (int index = 0; index < upgradeLevels.Length; index++)
        {
            PlayerPrefs.SetInt(
                UpgradeLevelKeyPrefix + index,
                Mathf.Clamp(
                    upgradeLevels[index],
                    0,
                    MaximumStoredUpgradeLevel));
        }

        PlayerPrefs.Save();
    }

    private void EnsureUpgradeLevelArray()
    {
        int requiredLength = Enum.GetValues(typeof(UpgradeType)).Length;
        if (upgradeLevels == null)
        {
            upgradeLevels = new int[requiredLength];
            return;
        }

        if (upgradeLevels.Length != requiredLength)
        {
            Array.Resize(ref upgradeLevels, requiredLength);
        }
    }

    private bool LoadBestScore()
    {
        if (PlayerPrefs.GetInt(BestScoreStorageVersionKey, 0) >=
            BestScoreStorageVersion)
        {
            string storedValue = PlayerPrefs.GetString(BestScoreKey, "0");
            bool parsed = long.TryParse(
                storedValue,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out long parsedValue);
            bestScore = parsed ? Math.Max(0L, parsedValue) : 0L;
            return !parsed || parsedValue != bestScore;
        }

        float legacyBestScore = PlayerPrefs.GetFloat(
            BestScoreKey,
            (float)bestScore);
        bestScore = Math.Max(0L, (long)Math.Floor(legacyBestScore));
        return true;
    }
}
