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
    public Image releaseCountdownFill;
    public GameObject releaseCountdownRoot;
    [SerializeField, Min(0.1f)] private float releaseTimeoutDuration = 3f;

    public Vector3[] pos;

    private bool animateDone = false;
    private bool isReadyToPlay = false;
    private bool isTutorialMode;
    private bool wasScreenHeld;
    private float releaseElapsedTime;
    private Player player;
    private GameManager gameManager;

    public bool IsInGameUiReady => isReadyToPlay && animateDone;

    private void Awake()
    {
        player = Player.Instance;
        gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.StateChanged += HandleStateChanged;
            ApplyState(gameManager.CurrentState);
        }
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.StateChanged -= HandleStateChanged;
        }
    }

    // Update is called once per frame
public override void Update()
    {
        if (!isReadyToPlay)
        {
            return;
        }

        base.Update();

        // Tutorial mode still needs to reflect live stamina while movement input is being demonstrated.
        if (player != null && staminaUI != null)
        {
            staminaUI.anchorMax = new Vector2(player.StaminaNormalized, 0f);
        }

        if (isTutorialMode)
        {
            return;
        }

        UpdateReleaseTimeout();

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
        else if (!gameStarted)
        {
            float alpha = Mathf.Lerp(pausePannel.color.a, 0.8f, Time.deltaTime / 0.1f);
            pausePannel.color = new Color(pausePannel.color.r, pausePannel.color.g, pausePannel.color.b, alpha);

            pauseUpUI.anchoredPosition = Vector3.Lerp(pauseUpUI.anchoredPosition, pos[0], Time.deltaTime / 0.1f);
            pauseDownUI.anchoredPosition = Vector3.Lerp(pauseDownUI.anchoredPosition, pos[1], Time.deltaTime / 0.1f);
            IngameUiPos.anchoredPosition = Vector3.Lerp(IngameUiPos.anchoredPosition, pos[2], Time.deltaTime / 0.1f);
            staminaPos.anchoredPosition = Vector3.Lerp(staminaPos.anchoredPosition, pos[7], Time.deltaTime / 0.1f);
        }

        if (gameStarted && Vector3.Distance(IngameUiPos.anchoredPosition, pos[5]) <= 8.3f)
        {
            animateDone = true;
        }

        if (!gameStarted && Vector3.Distance(IngameUiPos.anchoredPosition, pos[2]) <= 400.3f)
        {
            animateDone = false;
        }

        if (player != null && staminaUI != null)
        {
            staminaUI.anchorMax = new Vector2(player.StaminaNormalized, 0f);
        }
    }

    public void StopForGameOver()
    {
        ResetReleaseTimeout();
        releaseCountdownRoot.SetActive(false);
        pausePannel.gameObject.SetActive(false);
        enabled = false;
    }

    public void EnterMainMenu()
    {
        isReadyToPlay = false;
        gameStarted = false;
        ResetReleaseTimeout();
        releaseCountdownRoot.SetActive(false);
        pausePannel.gameObject.SetActive(false);
        enabled = false;
    }

    public void BeginGame()
    {
        if (player == null)
        {
            player = Player.Instance;
            if (player == null)
            {
                Debug.LogError("UIAnimate requires an active Player.", this);
                enabled = false;
                return;
            }
        }

        enabled = true;
        isReadyToPlay = true;
        gameStarted = true;
        ResetReleaseTimeout();
        releaseCountdownRoot.SetActive(!isTutorialMode);
        pausePannel.gameObject.SetActive(!isTutorialMode);
    }

    public void SetTutorialMode(bool enabledForTutorial)
    {
        isTutorialMode = enabledForTutorial;
        ResetReleaseTimeout();

        if (enabledForTutorial)
        {
            releaseCountdownRoot.SetActive(false);
            pausePannel.gameObject.SetActive(false);
            return;
        }

        if (isReadyToPlay && GameManager.Instance != null && GameManager.Instance.IsGameplayActive)
        {
            releaseCountdownRoot.SetActive(true);
            pausePannel.gameObject.SetActive(true);
        }
    }

    private void UpdateReleaseTimeout()
    {
        if (!isReadyToPlay || GameManager.Instance == null || !GameManager.Instance.IsGameplayActive)
        {
            ResetReleaseTimeout();
            releaseCountdownRoot.SetActive(false);
            return;
        }

        if (gameStarted)
        {
            releaseElapsedTime = 0f;
            wasScreenHeld = true;
            releaseCountdownRoot.SetActive(false);
            SetReleaseCountdownProgress(1f);
            return;
        }

        if (!wasScreenHeld)
        {
            releaseCountdownRoot.SetActive(false);
            return;
        }

        releaseElapsedTime += Time.unscaledDeltaTime;
        releaseCountdownRoot.SetActive(true);
        SetReleaseCountdownProgress(
            1f - (releaseElapsedTime / releaseTimeoutDuration));
        if (releaseElapsedTime >= releaseTimeoutDuration)
        {
            ResetReleaseTimeout();
            GameManager.Instance.EndGame();
        }
    }

    private void ResetReleaseTimeout()
    {
        releaseElapsedTime = 0f;
        wasScreenHeld = false;
        if (releaseCountdownFill != null)
        {
            SetReleaseCountdownProgress(1f);
        }
    }

    private void SetReleaseCountdownProgress(float progress)
    {
        float clampedProgress = Mathf.Clamp01(progress);
        releaseCountdownFill.fillAmount = clampedProgress;

        RectTransform fillRect = releaseCountdownFill.rectTransform;
        Vector2 anchorMax = fillRect.anchorMax;
        anchorMax.x = clampedProgress;
        fillRect.anchorMax = anchorMax;
    }

    private void HandleStateChanged(GameState previousState, GameState nextState)
    {
        ApplyState(nextState);
    }

    private void ApplyState(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                BeginGame();
                break;

            case GameState.GameOver:
                StopForGameOver();
                break;

            case GameState.MainMenu:
            case GameState.Shop:
                EnterMainMenu();
                break;
        }
    }
}
