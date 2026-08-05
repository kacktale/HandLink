using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ShopUpgradeButton : MonoBehaviour
{
    [SerializeField] private UpgradeType upgradeType;
    [SerializeField] private UpgradeDefinition upgradeDefinition;
    [SerializeField] private string description;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private TextMeshProUGUI costText;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(Purchase);
    }

    public void Refresh(Player player)
    {
        PlayerProgression progression = player.Progression;
        int level = progression.GetLevel(upgradeType);
        if (level >= upgradeDefinition.MaxLevel)
        {
            descriptionText.SetText("MAX LEVEL");
            valueText.gameObject.SetActive(false);
            costText.gameObject.SetActive(false);
            button.interactable = false;
            return;
        }

        descriptionText.gameObject.SetActive(true);
        valueText.gameObject.SetActive(true);
        costText.gameObject.SetActive(true);
        descriptionText.SetText(description);
        valueText.SetText($"+{upgradeDefinition.GetValue(level):0.##}");
        costText.SetText($"{upgradeDefinition.GetCost(level)} COIN");
        button.interactable = progression.Coin >= upgradeDefinition.GetCost(level);
    }

    private void Purchase()
    {
        Player player = Player.Instance;
        if (player == null || !player.Progression.TryPurchase(upgradeType, upgradeDefinition))
        {
            return;
        }

        player.ApplyUpgradeStats();
        GetComponentInParent<GameOverFlow>().RefreshShop();
    }
}
