using UnityEngine;
using UnityEngine.UI;

public class UIAnimate : InputAxis
{
    public Image pausePannel;
    public RectTransform pauseUpUI;
    public RectTransform pauseDownUI;
    public RectTransform IngameUiPos;
    public RectTransform staminaUI;
    public RectTransform staminaPos;

    public Vector3[] pos;

    private bool animateDone = false;
    private Player player;

    public void Start()
    {
        player = Player.Instance;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        if (animateDone)
        {
            IngameUiPos.anchoredPosition -= distanseValue * 2f;
        }

        if (gameStarted && !animateDone)
        {
            float alpha = Mathf.Lerp(pausePannel.color.a, 0, Time.deltaTime / 0.1f);
            pausePannel.color = new Color(pausePannel.color.r, pausePannel.color.g, pausePannel.color.b, alpha);

            pauseUpUI.anchoredPosition = Vector3.Lerp(pauseUpUI.anchoredPosition, pos[3], Time.deltaTime / 0.1f);
            pauseDownUI.anchoredPosition = Vector3.Lerp(pauseDownUI.anchoredPosition, pos[4], Time.deltaTime / 0.1f);
            IngameUiPos.anchoredPosition = Vector3.Lerp(IngameUiPos.anchoredPosition, pos[5], Time.deltaTime / 0.1f);
            staminaPos.anchoredPosition = Vector3.Lerp(staminaPos.anchoredPosition, pos[6], Time.deltaTime / 0.1f);
        }
        else if(!gameStarted)
        {
            float alpha = Mathf.Lerp(pausePannel.color.a, 0.8f, Time.deltaTime / 0.1f);
            pausePannel.color = new Color(pausePannel.color.r, pausePannel.color.g, pausePannel.color.b, alpha);

            pauseUpUI.anchoredPosition = Vector3.Lerp(pauseUpUI.anchoredPosition, pos[0], Time.deltaTime / 0.1f);
            pauseDownUI.anchoredPosition = Vector3.Lerp(pauseDownUI.anchoredPosition, pos[1], Time.deltaTime / 0.1f);
            IngameUiPos.anchoredPosition = Vector3.Lerp(IngameUiPos.anchoredPosition, pos[2], Time.deltaTime / 0.1f);
            staminaPos.anchoredPosition = Vector3.Lerp(staminaPos.anchoredPosition, pos[7], Time.deltaTime / 0.1f);
        }

        if (Vector3.Distance(IngameUiPos.anchoredPosition, pos[5]) <= 8.3f) animateDone = true;
        if (Vector3.Distance(IngameUiPos.anchoredPosition, pos[2]) <= 400.3f) animateDone = false;

        staminaUI.anchorMax = new Vector2 (player.stamina/100, 0);
    }
}
