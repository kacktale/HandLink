using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class CoinRewardFeedback : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI popupTemplate;
    [SerializeField] private RectTransform popupRoot;
    [SerializeField, Min(1)] private int initialPoolSize = 12;
    [SerializeField, Min(0.01f)] private float duration = 0.55f;
    [SerializeField, Min(0f)] private float riseDistance = 80f;
    [SerializeField] private Color coinTextColor = new Color(1f, 0.82f, 0.15f, 1f);

    private readonly List<PopupState> popupPool = new List<PopupState>();

    private void Awake()
    {
        for (int index = 0; index < initialPoolSize; index++)
        {
            CreatePopup();
        }
    }

    public void Show(Vector3 worldPosition, int amount)
    {
        if (amount <= 0 || popupTemplate == null || popupRoot == null)
        {
            return;
        }

        PopupState popup = FindAvailablePopup();
        if (popup == null)
        {
            return;
        }

        popup.Time = 0f;
        popup.Active = true;
        popup.Text.SetText($"+{amount}C");
        popup.Text.color = coinTextColor;
        popup.RectTransform.position = popupTemplate.rectTransform.position;
        popup.Text.transform.SetAsLastSibling();
        popup.Text.gameObject.SetActive(true);
    }

    private void Update()
    {
        for (int index = 0; index < popupPool.Count; index++)
        {
            PopupState popup = popupPool[index];
            if (!popup.Active)
            {
                continue;
            }

            popup.Time += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(popup.Time / duration);
            Color color = coinTextColor;
            color.a *= 1f - progress;
            popup.Text.color = color;
            // 코인 보유량 텍스트의 Transform을 기준으로 팝업을 표시한다.
            popup.RectTransform.position = popupTemplate.rectTransform.position + Vector3.up * (riseDistance * progress);

            if (progress >= 1f)
            {
                popup.Active = false;
                popup.Text.gameObject.SetActive(false);
            }
        }
    }

    private PopupState FindAvailablePopup()
    {
        for (int index = 0; index < popupPool.Count; index++)
        {
            if (!popupPool[index].Active)
            {
                return popupPool[index];
            }
        }

        return null;
    }

    private void CreatePopup()
    {
        TextMeshProUGUI popup = Instantiate(popupTemplate, popupRoot);
        popup.gameObject.name = "CoinRewardPopup";
        popup.gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor;
        popup.gameObject.SetActive(false);
        popupPool.Add(new PopupState(popup));
    }

    private sealed class PopupState
    {
        public readonly TextMeshProUGUI Text;
        public readonly RectTransform RectTransform;
        public bool Active;
        public float Time;
        public PopupState(TextMeshProUGUI text)
        {
            Text = text;
            RectTransform = text.rectTransform;
        }
    }
}
