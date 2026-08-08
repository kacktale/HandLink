using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class FirstRunTutorial : MonoBehaviour
{
    private const string LanguagePreferenceKey = "GameLanguage";
    private const string CompletionPreferenceKey = "SevenStepInteractiveTutorialCompleted";
    private const float StepDisplayDuration = 3f;
    private const float StaminaCompletionThreshold = 0.8f;
    private const float InterfaceFocusScaleMultiplier = 1.6f;
    private const float InterfaceFocusSmoothSpeed = 8f;
    private const float CameraFocusSizeMultiplier = 0.58f;
    private const float CameraFocusSmoothSpeed = 6f;
    private const float CenterInputRadiusRatio = 0.18f;
    private const float PerfectJudgementDistance = 1.5f;
    private const float CoinPickupDistance = 0.7f;
    private const float CoinPickupDuration = 6f;
    private const float TutorialSpecialEnemyTravelSpeed = 1.2f;

    private GameObject messagePanel;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI bodyText;
    private TextMeshProUGUI progressText;
    private Button skipButton;
    private TextMeshProUGUI skipButtonText;
    private RectTransform touchTarget;
    private RectTransform touchPointer;
    private Player player;
    private SpawnPivot spawnPivot;
    private UIAnimate uiAnimate;
    private TutorialStep currentStep = TutorialStep.WaitingForGame;
    private GameObject tutorialEnemy;
    private GameObject tutorialCoin;
    private int tutorialEnemyIndex;
    private int tutorialEnemyIntroductionLineIndex;
    private int tutorialEnemyActionLineIndex;
    private readonly GameObject[] tutorialEnemyShowcase = new GameObject[3];
    private TMP_FontAsset font;
    private float stepActionTime;
    private float coinExpiryTime;
    private Vector2 coinDropPosition;
    private bool isSubscribed;
    private Canvas canvas;
    private RectTransform canvasRect;
    private GameObject spotlightRoot;
    private RectTransform worldFocusProxyRoot;
    private readonly List<Image> worldFocusProxyImages = new List<Image>();
    private SpriteRenderer[] focusedSpriteRenderers = Array.Empty<SpriteRenderer>();
    private RectTransform staminaGauge;
    private Transform focusedTransform;
    private Vector3 focusedTransformScale;
    private bool isInterfaceFocused;
    private Transform interfaceFocusTransform;
    private Vector3 interfaceFocusTargetScale;
    private float completeFocusReleaseTime;
    private Camera tutorialCamera;
    private Vector3 baseCameraPosition;
    private float baseCameraOrthographicSize;
    private Transform cameraFocusTransform;
    private RectTransform promotedUiTransform;
    private Transform promotedUiParent;
    private int promotedUiSiblingIndex;
    private Vector2 promotedUiAnchorMin;
    private Vector2 promotedUiAnchorMax;
    private Vector2 promotedUiAnchoredPosition;
    private Vector2 promotedUiSizeDelta;
    private Vector2 promotedUiPivot;
    private Quaternion promotedUiLocalRotation;
    private Vector3 promotedUiLocalScale;

    private void Awake()
    {
        BuildUserInterface();
    }

    private void Start()
    {
        player = Player.Instance;
        spawnPivot = SpawnPivot.Instance;
        tutorialCamera = Camera.main;
        if (tutorialCamera != null)
        {
            baseCameraPosition = tutorialCamera.transform.position;
            baseCameraOrthographicSize = tutorialCamera.orthographicSize;
        }

        if (TryGetComponent(out uiAnimate))
        {
            staminaGauge = uiAnimate.staminaPos;
        }

        SubscribeToPlayer();
        if (PlayerPrefs.GetInt(CompletionPreferenceKey, 0) == 1)
        {
            enabled = false;
        }
    }

    private void OnDisable()
    {
        if (isSubscribed && player != null)
        {
            player.PerfectCoinDropRequested -= HandlePerfectCoinDrop;
            isSubscribed = false;
        }

        if (player != null)
        {
            player.SetTutorialMovementLocked(false);
        }

        spawnPivot?.SetTutorialMode(false);
        uiAnimate?.SetTutorialMode(false);
        messagePanel?.SetActive(false);
        SetTouchGuideVisible(false);

        RemoveTutorialObjects();
        ResetCameraFocus();
    }

    private void OnDestroy()
    {
        skipButton?.onClick.RemoveListener(SkipTutorial);
    }

private void Update()
    {
        UpdateCameraFocus();
        UpdateInterfaceFocus();
        UpdateSpotlight();
        UpdateTouchGuide();

        if (currentStep == TutorialStep.Completed)
        {
            if (Time.unscaledTime >= completeFocusReleaseTime &&
                interfaceFocusTransform == null)
            {
                enabled = false;
            }
            return;
        }

        if (player == null)
        {
            player = Player.Instance;
            SubscribeToPlayer();
            return;
        }

        if (spawnPivot == null)
        {
            spawnPivot = SpawnPivot.Instance;
            return;
        }

        if (currentStep == TutorialStep.WaitingForGame)
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.IsGameplayActive &&
                uiAnimate != null &&
                uiAnimate.IsInGameUiReady)
            {
                BeginTutorial();
            }
            return;
        }

        if (Time.unscaledTime < stepActionTime)
        {
            return;
        }

        player.SetTutorialMovementLocked(false);

        switch (currentStep)
        {
            case TutorialStep.HoldScreen:
                if (IsScreenHeld()) BeginStep(TutorialStep.MoveFingerToCenter);
                break;
            case TutorialStep.MoveFingerToCenter:
                if (IsFingerNearScreenCenter()) BeginStep(TutorialStep.StaminaIntroduction);
                break;
            case TutorialStep.StaminaIntroduction:
                BeginStep(TutorialStep.StaminaAction);
                break;
            case TutorialStep.StaminaAction:
                if (player.StaminaNormalized <= StaminaCompletionThreshold) BeginEnemyIntroduction();
                break;
            case TutorialStep.EnemyIntroduction:
                if (tutorialEnemyIntroductionLineIndex == 0)
                {
                    tutorialEnemyIntroductionLineIndex = 1;
                    BeginStep(TutorialStep.EnemyIntroduction);
                }
                else
                {
                    tutorialEnemyActionLineIndex = 0;
                    BeginStep(TutorialStep.EnemyApproach);
                }
                break;
            case TutorialStep.EnemyApproach:
                if (tutorialEnemyIndex > 0 && tutorialEnemyActionLineIndex == 0)
                {
                    tutorialEnemyActionLineIndex = 1;
                    BeginStep(TutorialStep.EnemyApproach);
                    break;
                }

                if (tutorialEnemy != null && !tutorialEnemy.activeInHierarchy)
                {
                    if (tutorialEnemyIndex < 2)
                    {
                        SelectTutorialEnemy(tutorialEnemyIndex + 1);
                        BeginCurrentEnemyIntroduction();
                    }
                    else
                    {
                        BeginStep(TutorialStep.JudgementIntroduction);
                    }
                }
                break;
            case TutorialStep.JudgementIntroduction:
                BeginStep(TutorialStep.PerfectJudgement);
                break;
            case TutorialStep.PerfectJudgement:
                BeginStep(TutorialStep.PerfectEnemyDefeat);
                SpawnPerfectJudgementEnemy();
                SetFocusForStep(TutorialStep.PerfectEnemyDefeat);
                break;
            case TutorialStep.ProgressionIntroduction:
                CompleteTutorial();
                break;
        }
    }

    public bool ReplayTutorial()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || gameManager.CurrentState != GameState.MainMenu)
        {
            return false;
        }

        player = Player.Instance;
        spawnPivot = SpawnPivot.Instance;
        if (player == null || spawnPivot == null)
        {
            return false;
        }

        PlayerPrefs.SetInt(CompletionPreferenceKey, 0);
        PlayerPrefs.Save();
        currentStep = TutorialStep.WaitingForGame;
        tutorialEnemyIndex = 0;
        tutorialEnemyIntroductionLineIndex = 0;
        tutorialEnemyActionLineIndex = 0;
        stepActionTime = 0f;
        RemoveTutorialObjects();
        messagePanel.SetActive(false);
        enabled = true;
        SubscribeToPlayer();

        if (gameManager.StartGame())
        {
            return true;
        }

        PlayerPrefs.SetInt(CompletionPreferenceKey, 1);
        PlayerPrefs.Save();
        enabled = false;
        return false;
    }

    private void BeginTutorial()
    {
        spawnPivot.SetTutorialMode(true);
        if (uiAnimate != null)
        {
            uiAnimate.SetTutorialMode(true);
        }

        messagePanel.SetActive(true);
        BeginStep(TutorialStep.HoldScreen);
    }

private void BeginEnemyIntroduction()
    {
        tutorialEnemyIndex = 0;
        SpawnCurrentTutorialEnemy();
        BeginCurrentEnemyIntroduction();
    }

    private void BeginCurrentEnemyIntroduction()
    {
        tutorialEnemyIntroductionLineIndex = 0;
        tutorialEnemyActionLineIndex = 0;
        BeginStep(TutorialStep.EnemyIntroduction);
    }

private void SpawnCurrentTutorialEnemy()
    {
        if (player == null || spawnPivot == null)
        {
            return;
        }

        for (int index = 0; index < tutorialEnemyShowcase.Length; index++)
        {
            if (tutorialEnemyShowcase[index] != null)
            {
                tutorialEnemyShowcase[index].SetActive(false);
            }

            Vector2 position = GetTutorialShowcasePosition(index);
            Vector2 targetPosition = spawnPivot.TutorialTargetPosition;
            tutorialEnemyShowcase[index] = index switch
            {
                0 => spawnPivot.SpawnTutorialEnemy(position, targetPosition, 0f),
                1 => spawnPivot.SpawnTutorialSpecialEnemy(
                    SpecialEnemyType.Pulse,
                    position,
                    targetPosition,
                    0f),
                _ => spawnPivot.SpawnTutorialSpecialEnemy(
                    SpecialEnemyType.HeartHealer,
                    position,
                    targetPosition,
                    0f)
            };
        }

        SelectTutorialEnemy(0);
    }

private Vector2 GetTutorialShowcasePosition(int index)
    {
        float cameraSize = baseCameraOrthographicSize > 0f
            ? baseCameraOrthographicSize
            : (tutorialCamera != null ? tutorialCamera.orthographicSize : 5f);
        float halfWidth = cameraSize * (tutorialCamera != null ? tutorialCamera.aspect : 0.56f);
        Vector2 center = new Vector2(baseCameraPosition.x, baseCameraPosition.y);
        return index switch
        {
            0 => center + new Vector2(-halfWidth * 0.44f, cameraSize * 0.24f),
            1 => center + new Vector2(halfWidth * 0.44f, cameraSize * 0.24f),
            _ => center + new Vector2(0f, cameraSize * 0.50f)
        };
    }

private void SelectTutorialEnemy(int index)
    {
        tutorialEnemyIndex = index;
        for (int enemyIndex = 0; enemyIndex < tutorialEnemyShowcase.Length; enemyIndex++)
        {
            GameObject enemy = tutorialEnemyShowcase[enemyIndex];
            if (enemy == null || !enemy.activeInHierarchy)
            {
                continue;
            }

            bool isCurrentEnemy = enemyIndex == tutorialEnemyIndex;
            bool allowPlayerContact = isCurrentEnemy && tutorialEnemyIndex == 0;
            foreach (Collider2D collider in enemy.GetComponents<Collider2D>())
            {
                collider.enabled = allowPlayerContact;
            }

            SpecialEnemy specialEnemy = enemy.GetComponent<SpecialEnemy>();
            if (specialEnemy != null)
            {
                specialEnemy.enabled = false;
            }

            if (isCurrentEnemy && tutorialEnemyIndex > 0)
            {
                Enemy enemyComponent = enemy.GetComponent<Enemy>();
                if (enemyComponent != null)
                {
                    enemyComponent.targetPos = spawnPivot.TutorialTargetPosition;
                    enemyComponent.speed = 0f;
                }
            }
        }

        tutorialEnemy = tutorialEnemyShowcase[tutorialEnemyIndex];
    }

    private void StartCurrentSpecialEnemyTravel()
    {
        if (tutorialEnemyIndex == 0 || tutorialEnemy == null)
        {
            return;
        }

        Enemy enemyComponent = tutorialEnemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.targetPos = spawnPivot.TutorialTargetPosition;
            enemyComponent.speed = TutorialSpecialEnemyTravelSpeed;
        }

        SpecialEnemy specialEnemy = tutorialEnemy.GetComponent<SpecialEnemy>();
        if (specialEnemy != null)
        {
            specialEnemy.enabled = true;
        }
    }



    private void BeginStep(TutorialStep nextStep)
    {
        currentStep = nextStep;
        bool isSpecialEnemyInstruction = nextStep == TutorialStep.EnemyApproach && tutorialEnemyIndex > 0;
        bool allowTutorialMovement = IsImmediateInputStep(nextStep) &&
            (nextStep != TutorialStep.EnemyApproach || tutorialEnemyIndex == 0);
        player.SetTutorialMovementLocked(!allowTutorialMovement);

        if (isSpecialEnemyInstruction && tutorialEnemyActionLineIndex == 0)
        {
            StartCurrentSpecialEnemyTravel();
        }

        bool requiresDisplayDelay = RequiresThreeSecondDisplay(nextStep) || isSpecialEnemyInstruction;
        stepActionTime = Time.unscaledTime + (requiresDisplayDelay ? StepDisplayDuration : 0f);
        ShowMessage(nextStep);
        SetFocusForStep(nextStep);
    }

    private void SpawnPerfectJudgementEnemy()
    {
        Vector2 targetPosition = spawnPivot.TutorialTargetPosition;
        tutorialEnemy = spawnPivot.SpawnTutorialEnemy(
            targetPosition + Vector2.right * PerfectJudgementDistance,
            targetPosition,
            0f);
    }

private bool HandlePerfectCoinDrop(Vector3 position)
    {
        if (currentStep != TutorialStep.PerfectEnemyDefeat)
        {
            return false;
        }

        const int perfectCoinReward = 1;
        player.Progression.AddCoin(perfectCoinReward);
        if (UiManager.instance != null)
        {
            UiManager.instance.ShowCoinReward(position, perfectCoinReward);
        }

        BeginStep(TutorialStep.ProgressionIntroduction);
        return true;
    }

    private void SkipTutorial()
    {
        GameAudio.Instance?.PlayButton();
        CompleteTutorial();
    }

    private void CreateTutorialCoin()
    {
        RemoveTutorialCoin();
        tutorialCoin = new GameObject("TutorialCoin", typeof(TextMeshPro));
        tutorialCoin.transform.position = coinDropPosition;
        tutorialCoin.transform.localScale = Vector3.one * 0.65f;

        TextMeshPro coinText = tutorialCoin.GetComponent<TextMeshPro>();
        coinText.font = font;
        coinText.text = "C";
        coinText.fontSize = 6f;
        coinText.alignment = TextAlignmentOptions.Center;
        coinText.color = new Color(1f, 0.78f, 0.12f, 1f);
        coinText.raycastTarget = false;
    }

    private void UpdateCoinPickup()
    {
        if (tutorialCoin != null && Vector2.Distance(player.transform.position, tutorialCoin.transform.position) <= CoinPickupDistance)
        {
            player.Progression.AddCoin(1);
            UiManager.instance.ShowCoinReward(tutorialCoin.transform.position, 1);
            RemoveTutorialCoin();
            CompleteTutorial();
            return;
        }

        if (Time.unscaledTime >= coinExpiryTime)
        {
            CreateTutorialCoin();
            BeginStep(TutorialStep.CoinPickup);
            coinExpiryTime = stepActionTime + CoinPickupDuration;
        }
    }

private void CompleteTutorial()
    {
        RemoveTutorialObjects();
        PlayerPrefs.SetInt(CompletionPreferenceKey, 1);
        PlayerPrefs.Save();
        messagePanel.SetActive(false);
        player.SetTutorialMovementLocked(false);
        spawnPivot.SetTutorialMode(false);
        if (uiAnimate != null)
        {
            uiAnimate.SetTutorialMode(false);
        }

        currentStep = TutorialStep.Completed;
        completeFocusReleaseTime = Time.unscaledTime + 0.45f;
    }

    private void SubscribeToPlayer()
    {
        if (isSubscribed || player == null)
        {
            return;
        }

        player.PerfectCoinDropRequested += HandlePerfectCoinDrop;
        isSubscribed = true;
    }

private void RemoveTutorialObjects()
    {
        ClearFocus();

        foreach (GameObject showcaseEnemy in tutorialEnemyShowcase)
        {
            if (showcaseEnemy != null)
            {
                showcaseEnemy.SetActive(false);
            }
        }

        if (tutorialEnemy != null)
        {
            tutorialEnemy.SetActive(false);
            tutorialEnemy = null;
        }

        RemoveTutorialCoin();
    }

    private void RemoveTutorialCoin()
    {
        if (tutorialCoin == null)
        {
            return;
        }

        Destroy(tutorialCoin);
        tutorialCoin = null;
    }

    private static bool IsImmediateInputStep(TutorialStep step)
    {
        return step == TutorialStep.HoldScreen ||
               step == TutorialStep.MoveFingerToCenter ||
               step == TutorialStep.StaminaAction ||
               step == TutorialStep.EnemyApproach ||
               step == TutorialStep.PerfectEnemyDefeat ||
               step == TutorialStep.CoinPickup;
    }

    private static bool IsScreenHeld()
    {
        return PointerInputService.Read().IsGameplayPressed;
    }

    private static bool IsFingerNearScreenCenter()
    {
        if (!PointerInputService.TryGetScreenPosition(out Vector2 inputPosition))
        {
            return false;
        }

        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        float allowedDistance = Mathf.Min(Screen.width, Screen.height) * CenterInputRadiusRatio;
        return Vector2.Distance(inputPosition, screenCenter) <= allowedDistance;
    }

    private static bool RequiresThreeSecondDisplay(TutorialStep step)
    {
        return step == TutorialStep.StaminaIntroduction ||
               step == TutorialStep.EnemyIntroduction ||
               step == TutorialStep.JudgementIntroduction ||
               step == TutorialStep.PerfectJudgement ||
               step == TutorialStep.ProgressionIntroduction;
    }

    private void SetFocusForStep(TutorialStep step)
    {
        Transform nextFocus = step switch
        {
            TutorialStep.StaminaIntroduction => staminaGauge,
            TutorialStep.EnemyIntroduction => tutorialEnemy != null ? tutorialEnemy.transform : null,
            TutorialStep.JudgementIntroduction => spawnPivot != null ? spawnPivot.TutorialTargetTransform : null,
            TutorialStep.PerfectEnemyDefeat => tutorialEnemy != null ? tutorialEnemy.transform : null,
            _ => null
        };

        SetFocus(nextFocus);
    }

private void SetFocus(Transform nextFocus)
    {
        ClearFocus();
        if (nextFocus == null)
        {
            return;
        }

        focusedTransform = nextFocus;
        isInterfaceFocused = nextFocus is RectTransform;
        if (!isInterfaceFocused)
        {
            cameraFocusTransform = nextFocus;
            ConfigureWorldFocusProxy(nextFocus);
            spotlightRoot?.SetActive(true);
            return;
        }

        PromoteFocusedUi((RectTransform)nextFocus);

        focusedTransformScale = nextFocus.localScale;

        float yFocusMultiplier = nextFocus == staminaGauge
            ? InterfaceFocusScaleMultiplier * 2f
            : InterfaceFocusScaleMultiplier;

        interfaceFocusTransform = nextFocus;
        interfaceFocusTargetScale = new Vector3(
            focusedTransformScale.x * InterfaceFocusScaleMultiplier,
            focusedTransformScale.y * yFocusMultiplier,
            focusedTransformScale.z);
    }

    private void ClearFocus()
    {
        if (interfaceFocusTransform != null)
        {
            interfaceFocusTransform.localScale = focusedTransformScale;
            interfaceFocusTransform = null;
            RestorePromotedUi();
        }

        focusedTransform = null;
        isInterfaceFocused = false;
        cameraFocusTransform = null;
        focusedSpriteRenderers = Array.Empty<SpriteRenderer>();
        if (worldFocusProxyRoot != null)
        {
            worldFocusProxyRoot.gameObject.SetActive(false);
        }
        if (spotlightRoot != null)
        {
            spotlightRoot.SetActive(false);
        }
    }

    private void UpdateCameraFocus()
    {
        if (tutorialCamera == null || !tutorialCamera.orthographic)
        {
            return;
        }

        Vector3 targetPosition = baseCameraPosition;
        float targetSize = baseCameraOrthographicSize;
        if (cameraFocusTransform != null)
        {
            targetPosition = cameraFocusTransform.position;
            targetPosition.z = baseCameraPosition.z;
            targetSize = baseCameraOrthographicSize * CameraFocusSizeMultiplier;
        }

        float blend = 1f - Mathf.Exp(-CameraFocusSmoothSpeed * Time.unscaledDeltaTime);
        tutorialCamera.transform.position = Vector3.Lerp(
            tutorialCamera.transform.position,
            targetPosition,
            blend);
        tutorialCamera.orthographicSize = Mathf.Lerp(
            tutorialCamera.orthographicSize,
            targetSize,
            blend);
    }

    private void ResetCameraFocus()
    {
        if (tutorialCamera == null || !tutorialCamera.orthographic)
        {
            return;
        }

        cameraFocusTransform = null;
        tutorialCamera.transform.position = baseCameraPosition;
        tutorialCamera.orthographicSize = baseCameraOrthographicSize;
    }

private void UpdateInterfaceFocus()
    {
        float blend = 1f - Mathf.Exp(-InterfaceFocusSmoothSpeed * Time.unscaledDeltaTime);

        if (interfaceFocusTransform != null)
        {
            interfaceFocusTransform.localScale = Vector3.Lerp(
                interfaceFocusTransform.localScale,
                interfaceFocusTargetScale,
                blend);
        }

    }

    private void BuildSpotlight()
    {
        if (canvasRect == null)
        {
            return;
        }

        spotlightRoot = new GameObject(
            "TutorialFocusOverlay",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        RectTransform overlayRect = spotlightRoot.GetComponent<RectTransform>();
        overlayRect.SetParent(transform, false);
        SetStretch(overlayRect);

        Image overlayImage = spotlightRoot.GetComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.82f);
        overlayImage.raycastTarget = false;

        GameObject proxyObject = new GameObject(
            "TutorialWorldFocusProxy",
            typeof(RectTransform));
        worldFocusProxyRoot = proxyObject.GetComponent<RectTransform>();
        worldFocusProxyRoot.SetParent(transform, false);
        SetStretch(worldFocusProxyRoot);
        proxyObject.SetActive(false);
        spotlightRoot.SetActive(false);
    }

    private static void SetStretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void BuildTouchGuide()
    {
        TextMeshProUGUI targetText = CreateOverlayText(
            "TutorialTouchTarget",
            "◎",
            104f,
            new Color(0.35f, 0.85f, 1f, 0.9f));
        touchTarget = targetText.rectTransform;

        TextMeshProUGUI pointerText = CreateOverlayText(
            "TutorialTouchPointer",
            "●",
            54f,
            new Color(1f, 0.82f, 0.22f, 0.95f));
        touchPointer = pointerText.rectTransform;
        SetTouchGuideVisible(false);
    }

    private TextMeshProUGUI CreateOverlayText(
        string objectName,
        string value,
        float fontSize,
        Color color)
    {
        GameObject textObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(transform, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(160f, 160f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;
        text.raycastTarget = false;
        text.SetText(value);
        return text;
    }

    private void UpdateTouchGuide()
    {
        bool shouldShow = currentStep == TutorialStep.HoldScreen ||
                          currentStep == TutorialStep.MoveFingerToCenter;
        if (touchTarget == null || touchPointer == null || canvas == null)
        {
            return;
        }

        touchTarget.gameObject.SetActive(shouldShow);
        if (!shouldShow)
        {
            touchPointer.gameObject.SetActive(false);
            return;
        }

        float pulse = 1f + Mathf.Sin(Time.unscaledTime * 5f) * 0.1f;
        touchTarget.localScale = Vector3.one * pulse;

        PointerInputSnapshot snapshot = PointerInputService.Read();
        touchPointer.gameObject.SetActive(snapshot.IsGameplayPressed);
        if (!snapshot.IsGameplayPressed || canvasRect == null)
        {
            return;
        }

        Camera canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                snapshot.ScreenPosition,
                canvasCamera,
                out Vector2 localPoint))
        {
            touchPointer.anchoredPosition = localPoint;
        }
    }

    private void SetTouchGuideVisible(bool visible)
    {
        touchTarget?.gameObject.SetActive(visible);
        touchPointer?.gameObject.SetActive(false);
    }

    private void UpdateSpotlight()
    {
        if (spotlightRoot == null || tutorialCamera == null || !spotlightRoot.activeSelf)
        {
            return;
        }

        if (focusedSpriteRenderers.Length == 0 || worldFocusProxyRoot == null)
        {
            return;
        }

        float canvasScale = canvas != null ? Mathf.Max(0.0001f, canvas.scaleFactor) : 1f;
        float canvasUnitsPerWorldUnit = tutorialCamera.pixelHeight /
            (tutorialCamera.orthographicSize * 2f * canvasScale);
        Camera canvasCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        for (int index = 0; index < focusedSpriteRenderers.Length; index++)
        {
            SpriteRenderer source = focusedSpriteRenderers[index];
            Image proxy = worldFocusProxyImages[index];
            bool visible = source != null && source.enabled && source.gameObject.activeInHierarchy && source.sprite != null;
            proxy.gameObject.SetActive(visible);
            if (!visible)
            {
                continue;
            }

            proxy.sprite = source.sprite;
            proxy.color = source.color;
            RectTransform proxyRect = proxy.rectTransform;
            Vector2 screenPosition = tutorialCamera.WorldToScreenPoint(source.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                canvasCamera,
                out Vector2 localPosition);
            proxyRect.anchoredPosition = localPosition;

            Vector3 lossyScale = source.transform.lossyScale;
            Vector2 spriteSize = source.sprite.bounds.size;
            proxyRect.sizeDelta = new Vector2(
                spriteSize.x * Mathf.Abs(lossyScale.x) * canvasUnitsPerWorldUnit,
                spriteSize.y * Mathf.Abs(lossyScale.y) * canvasUnitsPerWorldUnit);
            proxyRect.pivot = new Vector2(
                source.sprite.pivot.x / source.sprite.rect.width,
                source.sprite.pivot.y / source.sprite.rect.height);
            proxyRect.localRotation = Quaternion.Euler(0f, 0f, source.transform.eulerAngles.z);
            proxyRect.localScale = new Vector3(source.flipX ? -1f : 1f, source.flipY ? -1f : 1f, 1f);
        }
    }

    private void ConfigureWorldFocusProxy(Transform target)
    {
        focusedSpriteRenderers = target.GetComponentsInChildren<SpriteRenderer>(true);
        Array.Sort(focusedSpriteRenderers, CompareSpriteRenderers);
        EnsureWorldFocusProxyCapacity(focusedSpriteRenderers.Length);

        for (int index = 0; index < worldFocusProxyImages.Count; index++)
        {
            worldFocusProxyImages[index].gameObject.SetActive(index < focusedSpriteRenderers.Length);
        }

        worldFocusProxyRoot.gameObject.SetActive(true);
    }

    private void EnsureWorldFocusProxyCapacity(int requiredCount)
    {
        while (worldFocusProxyImages.Count < requiredCount)
        {
            GameObject proxyObject = new GameObject(
                $"FocusSprite{worldFocusProxyImages.Count}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            RectTransform rect = proxyObject.GetComponent<RectTransform>();
            rect.SetParent(worldFocusProxyRoot, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);

            Image image = proxyObject.GetComponent<Image>();
            image.raycastTarget = false;
            image.preserveAspect = false;
            worldFocusProxyImages.Add(image);
        }
    }

    private static int CompareSpriteRenderers(SpriteRenderer left, SpriteRenderer right)
    {
        int layerComparison = SortingLayer.GetLayerValueFromID(left.sortingLayerID)
            .CompareTo(SortingLayer.GetLayerValueFromID(right.sortingLayerID));
        return layerComparison != 0
            ? layerComparison
            : left.sortingOrder.CompareTo(right.sortingOrder);
    }

    private void PromoteFocusedUi(RectTransform target)
    {
        RestorePromotedUi();
        promotedUiTransform = target;
        promotedUiParent = target.parent;
        promotedUiSiblingIndex = target.GetSiblingIndex();
        promotedUiAnchorMin = target.anchorMin;
        promotedUiAnchorMax = target.anchorMax;
        promotedUiAnchoredPosition = target.anchoredPosition;
        promotedUiSizeDelta = target.sizeDelta;
        promotedUiPivot = target.pivot;
        promotedUiLocalRotation = target.localRotation;
        promotedUiLocalScale = target.localScale;

        target.SetParent(transform, true);
        target.SetSiblingIndex(worldFocusProxyRoot.GetSiblingIndex() + 1);
        spotlightRoot.SetActive(true);
        worldFocusProxyRoot.gameObject.SetActive(false);
        focusedTransformScale = target.localScale;
    }

    private void RestorePromotedUi()
    {
        if (promotedUiTransform == null || promotedUiParent == null)
        {
            promotedUiTransform = null;
            return;
        }

        promotedUiTransform.SetParent(promotedUiParent, false);
        promotedUiTransform.SetSiblingIndex(promotedUiSiblingIndex);
        promotedUiTransform.anchorMin = promotedUiAnchorMin;
        promotedUiTransform.anchorMax = promotedUiAnchorMax;
        promotedUiTransform.anchoredPosition = promotedUiAnchoredPosition;
        promotedUiTransform.sizeDelta = promotedUiSizeDelta;
        promotedUiTransform.pivot = promotedUiPivot;
        promotedUiTransform.localRotation = promotedUiLocalRotation;
        promotedUiTransform.localScale = promotedUiLocalScale;
        promotedUiTransform = null;
        promotedUiParent = null;
    }

    private void ShowMessage(TutorialStep step)
    {
        bool isKorean = CurrentLanguage == GameLanguage.Korean;
        skipButtonText?.SetText(isKorean ? "건너뛰기" : "SKIP");
        if (step == TutorialStep.HoldScreen)
        {
            titleText.SetText(isKorean ? "튜토리얼" : "TUTORIAL");
            bodyText.SetText(isKorean ? "일단 화면을 꾹 눌러보세요" : "Press and hold the screen first.");
            progressText.SetText(string.Empty);
            return;
        }

        if (step == TutorialStep.MoveFingerToCenter)
        {
            titleText.SetText(isKorean ? "튜토리얼" : "TUTORIAL");
            bodyText.SetText(isKorean ? "손가락을 화면 가운데로 모아보세요" : "Move your finger to the center of the screen.");
            progressText.SetText(string.Empty);
            return;
        }

        bodyText.fontSizeMin = 26f;
        bodyText.fontSizeMax = 46f;
        if (step == TutorialStep.EnemyIntroduction)
        {
            titleText.SetText(isKorean ? "적 소개" : "ENEMY INTRODUCTION");
            string enemyIntroKorean = tutorialEnemyIndex switch
            {
                0 => tutorialEnemyIntroductionLineIndex == 0 ? "이것은 일반 적입니다." : "플레이어를 향해 다가옵니다.",
                1 => tutorialEnemyIntroductionLineIndex == 0 ? "이것은 펄스 적입니다." : "빨간색으로 깜빡인 뒤 파동 공격을 합니다.",
                _ => tutorialEnemyIntroductionLineIndex == 0 ? "이것은 회복 적입니다." : "심장에 닿으면 체력을 1 회복합니다."
            };
            string enemyIntroEnglish = tutorialEnemyIndex switch
            {
                0 => tutorialEnemyIntroductionLineIndex == 0 ? "This is a Normal Enemy." : "It moves toward the player.",
                1 => tutorialEnemyIntroductionLineIndex == 0 ? "This is a Pulse Enemy." : "It flashes red, then attacks with a pulse.",
                _ => tutorialEnemyIntroductionLineIndex == 0 ? "This is a Healer Enemy." : "It restores 1 health when it reaches the heart."
            };
            bodyText.SetText(isKorean ? enemyIntroKorean : enemyIntroEnglish);
            progressText.SetText("3 / 7");
            return;
        }

        if (step == TutorialStep.EnemyApproach)
        {
            titleText.SetText(isKorean ? "튜토리얼" : "TUTORIAL");
            string koreanInstruction = tutorialEnemyIndex switch
            {
                0 => "일반 적에게 가까이 가서 처치해보세요.",
                1 => tutorialEnemyActionLineIndex == 0
                    ? "펄스 적이 심장 쪽에 도착하게 두세요."
                    : "파동에 닿으면 체력을 잃습니다.",
                _ => tutorialEnemyActionLineIndex == 0
                    ? "회복 적이 심장 쪽에 도착하게 두세요."
                    : "도착하면 체력이 1 회복됩니다."
            };
            string englishInstruction = tutorialEnemyIndex switch
            {
                0 => "Move closer to the Normal Enemy and defeat it.",
                1 => tutorialEnemyActionLineIndex == 0
                    ? "Let the Pulse Enemy reach the heart."
                    : "Touching its pulse costs health.",
                _ => tutorialEnemyActionLineIndex == 0
                    ? "Let the Healer Enemy reach the heart."
                    : "It restores 1 health on arrival."
            };
            bodyText.SetText(isKorean ? koreanInstruction : englishInstruction);
            progressText.SetText("4 / 7");
            return;
        }

        if (step == TutorialStep.PerfectEnemyDefeat)
        {
            titleText.SetText(isKorean ? "튜토리얼" : "TUTORIAL");
            bodyText.SetText(isKorean ? "퍼펙트 판정으로 적을 처치하면 코인이 자동 지급됩니다." : "Defeat the enemy with a PERFECT judgement to receive a coin automatically.");
            progressText.SetText("7 / 7");
            return;
        }

        if (step == TutorialStep.ProgressionIntroduction)
        {
            titleText.SetText(isKorean ? "코인과 업그레이드" : "COINS & UPGRADES");
            bodyText.SetText(isKorean
                ? "획득한 코인은 게임 종료 후 상점에서 능력을 강화할 때 사용합니다."
                : "Use earned coins in the shop after a run to upgrade your abilities.");
            progressText.SetText("7 / 7");
            return;
        }

        string koreanText = step switch
        {
            TutorialStep.StaminaIntroduction => "이것은 스테미나입니다",
            TutorialStep.StaminaAction => "손가락 움직여 스테미나를 80퍼 이하로 만들어보세요",
            TutorialStep.EnemyIntroduction => "이것은 적입니다",
            TutorialStep.EnemyApproach => "손가락을 움직여 적에 가까이 가보세요",
            TutorialStep.JudgementIntroduction => "거리에 따라 판정이 달라집니다.",
            TutorialStep.PerfectJudgement => "퍼펙트 판정으로 적을 처치하면 코인이 자동 지급됩니다.",
            TutorialStep.CoinPickup => "퍼펙트 판정으로 적을 처치하면 코인이 자동 지급됩니다.",
            _ => string.Empty
        };
        string englishText = step switch
        {
            TutorialStep.StaminaIntroduction => "This is stamina.",
            TutorialStep.StaminaAction => "Move your finger until stamina is below 80%.",
            TutorialStep.EnemyIntroduction => "This is an enemy.",
            TutorialStep.EnemyApproach => "Move your finger closer to the enemy.",
            TutorialStep.JudgementIntroduction => "Your judgement changes with distance.",
            TutorialStep.PerfectJudgement => "Defeating an enemy with a PERFECT judgement grants a coin automatically.",
            TutorialStep.CoinPickup => "Defeat the enemy with a PERFECT judgement to receive a coin automatically.",
            _ => string.Empty
        };

        titleText.SetText(isKorean ? "튜토리얼" : "TUTORIAL");
        bodyText.SetText(isKorean ? koreanText : englishText);
        int stepNumber = GetStepNumber(step);
        if (stepNumber == 0)
        {
            progressText.SetText(string.Empty);
        }
        else
        {
            progressText.SetText("{0} / 7", stepNumber);
        }
    }



    private void BuildUserInterface()
    {
        font = GetComponentInChildren<TextMeshProUGUI>(true)?.font ?? TMP_Settings.defaultFontAsset;
        if (font == null)
        {
            Debug.LogError("FirstRunTutorial could not find a TMP font asset.", this);
            return;
        }

        canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas != null
            ? canvas.transform as RectTransform
            : null;
        BuildSpotlight();

        messagePanel = new GameObject("TutorialMessage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform panelRect = messagePanel.GetComponent<RectTransform>();
        panelRect.SetParent(transform, false);
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0f, -228f);
        panelRect.sizeDelta = new Vector2(-56f, 245f);
        Image panelImage = messagePanel.GetComponent<Image>();
        panelImage.color = new Color(0.025f, 0.045f, 0.09f, 0.96f);
        panelImage.raycastTarget = false;

        titleText = CreateText("Title", messagePanel.transform, new Vector2(0f, -48f), new Vector2(860f, 58f), 34f, font);
        bodyText = CreateText("Body", messagePanel.transform, new Vector2(0f, -130f), new Vector2(860f, 60f), 28f, font);
        progressText = CreateText("Progress", messagePanel.transform, new Vector2(0f, -192f), new Vector2(200f, 40f), 24f, font);
        ConfigureAutoSize(titleText, 30f, 50f);
        ConfigureAutoSize(bodyText, 26f, 46f);
        ConfigureAutoSize(progressText, 22f, 34f);

        skipButton = CreateButton(
            "Skip",
            messagePanel.transform,
            new Vector2(-20f, -18f),
            new Vector2(170f, 52f),
            CurrentLanguage == GameLanguage.Korean ? "건너뛰기" : "SKIP",
            24f,
            font);
        RectTransform skipRect = skipButton.GetComponent<RectTransform>();
        skipRect.anchorMin = Vector2.one;
        skipRect.anchorMax = Vector2.one;
        skipRect.pivot = Vector2.one;
        skipButtonText = skipButton.GetComponentInChildren<TextMeshProUGUI>(true);
        skipButton.onClick.AddListener(SkipTutorial);

        BuildTouchGuide();
        messagePanel.SetActive(false);
    }

    private static Button CreateButton(
        string objectName,
        Transform parent,
        Vector2 position,
        Vector2 size,
        string label,
        float fontSize,
        TMP_FontAsset textFont)
    {
        GameObject buttonObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button));
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.28f, 0.42f, 0.62f, 1f);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        TextMeshProUGUI text = CreateText(
            "Label",
            buttonObject.transform,
            Vector2.zero,
            size,
            fontSize,
            textFont);
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        text.raycastTarget = false;
        text.SetText(label);
        return button;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, Vector2 position, Vector2 size, float fontSize, TMP_FontAsset textFont)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = textFont;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private static void ConfigureAutoSize(TextMeshProUGUI text, float minimumSize, float maximumSize)
    {
        text.enableAutoSizing = true;
        text.fontSizeMin = minimumSize;
        text.fontSizeMax = maximumSize;
    }

    private GameLanguage CurrentLanguage => (GameLanguage)PlayerPrefs.GetInt(LanguagePreferenceKey, (int)GameLanguage.Korean);

    private static int GetStepNumber(TutorialStep step)
    {
        return step switch
        {
            TutorialStep.StaminaIntroduction => 1,
            TutorialStep.StaminaAction => 2,
            TutorialStep.EnemyIntroduction => 3,
            TutorialStep.EnemyApproach => 4,
            TutorialStep.JudgementIntroduction => 5,
            TutorialStep.PerfectJudgement => 6,
            TutorialStep.PerfectEnemyDefeat => 7,
            TutorialStep.ProgressionIntroduction => 7,
            TutorialStep.CoinPickup => 7,
            _ => 0
        };
    }

    private enum TutorialStep
    {
        WaitingForGame,
        HoldScreen,
        MoveFingerToCenter,
        StaminaIntroduction,
        StaminaAction,
        EnemyIntroduction,
        EnemyApproach,
        JudgementIntroduction,
        PerfectJudgement,
        PerfectEnemyDefeat,
        ProgressionIntroduction,
        CoinPickup,
        Completed
    }
}
