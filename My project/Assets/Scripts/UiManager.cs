using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager instance;

    public Transform hartUI;
    public GameObject hartObj;
    public TextMeshProUGUI bestScoreTxt;
    public TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI heldCoinsText;
    public long bestScore;
    [SerializeField] private DamageFeedback damageFeedback;
    [SerializeField] private CoinRewardFeedback coinRewardFeedback;

    public List<GameObject> harts = new List<GameObject>();
    private Player boundPlayer;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        BindPlayer(Player.Instance);
    }

    private void OnDestroy()
    {
        if (boundPlayer == null)
        {
            return;
        }

        boundPlayer.Progression.Changed -= RefreshProgressionUi;
        boundPlayer.Health.HealthChanged -= RefreshHealthUi;
        boundPlayer.Health.Damaged -= PlayDamageFeedback;
        boundPlayer.Score.ScoreChanged -= RefreshScoreUi;
        boundPlayer.CoinRewarded -= ShowCoinReward;
    }

    public void ShowGameOver()
    {
        GameManager.Instance?.EndGame();
    }

    public void SaveGameResult(Player player)
    {
        if (player == null)
        {
            return;
        }

        player.Progression.SaveBestScore(player.score);
        SetBestScore(player.Progression.BestScore);
        bestScoreTxt.SetText($"{bestScore:N0}");
    }

    private void SetBestScore(long value)
    {
        bestScore = value;
        bestScoreTxt.SetText($"{bestScore:N0}");
    }

    public void ResetGameUI(int hp)
    {
        RefreshHealthUi(hp, hp);
        RefreshScoreUi(0L);
    }

public void PlayDamageFeedback()
    {
        damageFeedback?.Play();
        GameAudio.Instance?.PlayDamage();
    }

    public void ShowCoinReward(Vector3 worldPosition, int amount)
    {
        coinRewardFeedback?.Show(worldPosition, amount);
    }

    private void BindPlayer(Player player)
    {
        if (player == null)
        {
            return;
        }

        boundPlayer = player;
        boundPlayer.Progression.Changed += RefreshProgressionUi;
        boundPlayer.Health.HealthChanged += RefreshHealthUi;
        boundPlayer.Health.Damaged += PlayDamageFeedback;
        boundPlayer.Score.ScoreChanged += RefreshScoreUi;
        boundPlayer.CoinRewarded += ShowCoinReward;

        RefreshProgressionUi();
        RefreshHealthUi(
            boundPlayer.Health.CurrentHealth,
            boundPlayer.Health.MaxHealth);
        RefreshScoreUi(boundPlayer.Score.CurrentScore);
    }

    private void RefreshHealthUi(int currentHealth, int maxHealth)
    {
        if (harts.Count != maxHealth)
        {
            RebuildHeartUi(maxHealth);
        }

        for (int index = 0; index < harts.Count; index++)
        {
            harts[index].SetActive(index < currentHealth);
        }
    }

    private void RebuildHeartUi(int maxHealth)
    {
        foreach (GameObject heart in harts)
        {
            Destroy(heart);
        }

        harts.Clear();
        for (int index = 0; index < maxHealth; index++)
        {
            GameObject heart = Instantiate(hartObj, hartUI.position, Quaternion.identity, hartUI);
            harts.Add(heart);
        }
    }

    private void RefreshScoreUi(long score)
    {
        currentScoreText.SetText($"{score:N0}");
    }

    private void RefreshProgressionUi()
    {
        if (boundPlayer == null)
        {
            return;
        }

        SetBestScore(boundPlayer.Progression.BestScore);
        if (heldCoinsText != null)
        {
            heldCoinsText.SetText($"{boundPlayer.Progression.Coin:N0}C");
        }
    }
}
