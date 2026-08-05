using System;
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
    private const float CameraFocusSizeMultiplier = 0.58f;
    private const float CameraFocusSmoothSpeed = 6f;
    private const float InterfaceFocusSmoothSpeed = 8f;
    private const float CenterInputRadiusRatio = 0.18f;
    private const float PerfectJudgementDistance = 1.5f;
    private const float CoinPickupDistance = 0.7f;
    private const float CoinPickupDuration = 6f;

    private GameObject messagePanel;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI bodyText;
    private TextMeshProUGUI progressText;
    private Player player;
    private SpawnPivot spawnPivot;
    private UIAnimate uiAnimate;
    private TutorialStep currentStep = TutorialStep.WaitingForGame;
    private GameObject tutorialEnemy;
    private GameObject tutorialCoin;
    private TMP_FontAsset font;
    private float stepActionTime;
    private float coinExpiryTime;
    private Vector2 coinDropPosition;
    private bool isSubscribed;
    private Canvas canvas;
    private RectTransform canvasRect;
    private GameObject spotlightRoot;
    private RectTransform[] dimBlocks;
    private RectTransform staminaGauge;
    private Transform focusedTransform;
    private Vector3 focusedTransformScale;
    private bool isInterfaceFocused;
    private Transform interfaceFocusTransform;
    private Vector3 interfaceFocusTargetScale;
    private Transform interfaceReleaseTransform;
    private Vector3 interfaceReleaseTargetScale;
    private float completeFocusReleaseTime;
    private Camera tutorialCamera;
    private Vector3 baseCameraPosition;
    private float baseCameraOrthographicSize;
    private Transform cameraFocusTransform;

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

        RemoveTutorialObjects();
        ResetCameraFocus();
    }

private void Update()
    {
        UpdateCameraFocus();
        UpdateInterfaceFocus();

        if (currentStep == TutorialStep.Completed)
        {
            if (Time.unscaledTime >= completeFocusReleaseTime &&
                interfaceFocusTransform == null &&
                interfaceReleaseTransform == null)
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
                BeginStep(TutorialStep.EnemyApproach);
                break;
            case TutorialStep.EnemyApproach:
                if (tutorialEnemy != null && !tutorialEnemy.activeInHierarchy) BeginStep(TutorialStep.JudgementIntroduction);
                break;
            case TutorialStep.JudgementIntroduction:
                BeginStep(TutorialStep.PerfectJudgement);
                break;
            case TutorialStep.PerfectJudgement:
                BeginStep(TutorialStep.PerfectEnemyDefeat);
                SpawnPerfectJudgementEnemy();
                SetFocusForStep(TutorialStep.PerfectEnemyDefeat);
                break;
        }
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
        Vector2 enemyPosition = player.transform.position + Vector3.up * 3.5f;
        tutorialEnemy = spawnPivot.SpawnTutorialEnemy(enemyPosition, enemyPosition + Vector2.up * 10f, 0f);
        BeginStep(TutorialStep.EnemyIntroduction);
    }

    private void BeginStep(TutorialStep nextStep)
    {
        currentStep = nextStep;
        player.SetTutorialMovementLocked(!IsImmediateInputStep(nextStep));
        stepActionTime = Time.unscaledTime + (RequiresThreeSecondDisplay(nextStep) ? StepDisplayDuration : 0f);
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

        CompleteTutorial();
        return true;
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
        ClearFocus();
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
               step == TutorialStep.PerfectJudgement;
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
        if (nextFocus is RectTransform)
        {
            if (interfaceReleaseTransform == nextFocus)
            {
                focusedTransformScale = interfaceReleaseTargetScale;
                interfaceReleaseTransform = null;
            }
            else
            {
                focusedTransformScale = nextFocus.localScale;
            }

            float yFocusMultiplier = nextFocus == staminaGauge
                ? InterfaceFocusScaleMultiplier * 2f
                : InterfaceFocusScaleMultiplier;

            isInterfaceFocused = true;
            interfaceFocusTransform = nextFocus;
            interfaceFocusTargetScale = new Vector3(
                focusedTransformScale.x * InterfaceFocusScaleMultiplier,
                focusedTransformScale.y * yFocusMultiplier,
                focusedTransformScale.z);
            return;
        }

        isInterfaceFocused = false;
        cameraFocusTransform = nextFocus;
    }

private void ClearFocus()
    {
        if (interfaceFocusTransform != null)
        {
            interfaceReleaseTransform = interfaceFocusTransform;
            interfaceReleaseTargetScale = focusedTransformScale;
            interfaceFocusTransform = null;
        }

        focusedTransform = null;
        isInterfaceFocused = false;
        cameraFocusTransform = null;
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

        Vector3 desiredPosition = baseCameraPosition;
        float desiredSize = baseCameraOrthographicSize;
        if (cameraFocusTransform != null)
        {
            desiredPosition = cameraFocusTransform.position;
            desiredPosition.z = baseCameraPosition.z;
            desiredSize = baseCameraOrthographicSize * CameraFocusSizeMultiplier;
        }

        float blend = 1f - Mathf.Exp(-CameraFocusSmoothSpeed * Time.unscaledDeltaTime);
        tutorialCamera.transform.position = Vector3.Lerp(tutorialCamera.transform.position, desiredPosition, blend);
        tutorialCamera.orthographicSize = Mathf.Lerp(tutorialCamera.orthographicSize, desiredSize, blend);
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

        if (interfaceReleaseTransform != null)
        {
            interfaceReleaseTransform.localScale = Vector3.Lerp(
                interfaceReleaseTransform.localScale,
                interfaceReleaseTargetScale,
                blend);

            if (Vector3.Distance(interfaceReleaseTransform.localScale, interfaceReleaseTargetScale) <= 0.01f)
            {
                interfaceReleaseTransform.localScale = interfaceReleaseTargetScale;
                interfaceReleaseTransform = null;
            }
        }
    }

    private void ResetCameraFocus()
    {
        if (tutorialCamera == null || !tutorialCamera.orthographic)
        {
            return;
        }

        tutorialCamera.transform.position = baseCameraPosition;
        tutorialCamera.orthographicSize = baseCameraOrthographicSize;
    }

    private void BuildSpotlight()
    {
        if (canvasRect == null)
        {
            return;
        }

        spotlightRoot = new GameObject("TutorialSpotlight", typeof(RectTransform));
        RectTransform spotlightRect = spotlightRoot.GetComponent<RectTransform>();
        spotlightRect.SetParent(transform, false);
        spotlightRect.anchorMin = Vector2.zero;
        spotlightRect.anchorMax = Vector2.one;
        spotlightRect.offsetMin = Vector2.zero;
        spotlightRect.offsetMax = Vector2.zero;

        dimBlocks = new RectTransform[4];
        dimBlocks[0] = CreateDimBlock("Top");
        dimBlocks[1] = CreateDimBlock("Bottom");
        dimBlocks[2] = CreateDimBlock("Left");
        dimBlocks[3] = CreateDimBlock("Right");
        spotlightRoot.SetActive(false);
    }

    private RectTransform CreateDimBlock(string name)
    {
        GameObject block = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = block.GetComponent<RectTransform>();
        rect.SetParent(spotlightRoot.transform, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        Image image = block.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.7f);
        image.raycastTarget = false;
        return rect;
    }

    private void UpdateSpotlight()
    {
        if (focusedTransform == null || spotlightRoot == null || !TryGetFocusRect(out Rect focusRect))
        {
            return;
        }

        const float padding = 90f;
        Rect canvasBounds = canvasRect.rect;
        float xMin = Mathf.Clamp(focusRect.xMin - padding, canvasBounds.xMin, canvasBounds.xMax);
        float xMax = Mathf.Clamp(focusRect.xMax + padding, canvasBounds.xMin, canvasBounds.xMax);
        float yMin = Mathf.Clamp(focusRect.yMin - padding, canvasBounds.yMin, canvasBounds.yMax);
        float yMax = Mathf.Clamp(focusRect.yMax + padding, canvasBounds.yMin, canvasBounds.yMax);

        SetDimBlock(dimBlocks[0], new Vector2(0f, (yMax + canvasBounds.yMax) * 0.5f), new Vector2(canvasBounds.width, canvasBounds.yMax - yMax));
        SetDimBlock(dimBlocks[1], new Vector2(0f, (yMin + canvasBounds.yMin) * 0.5f), new Vector2(canvasBounds.width, yMin - canvasBounds.yMin));
        SetDimBlock(dimBlocks[2], new Vector2((xMin + canvasBounds.xMin) * 0.5f, 0f), new Vector2(xMin - canvasBounds.xMin, canvasBounds.height));
        SetDimBlock(dimBlocks[3], new Vector2((xMax + canvasBounds.xMax) * 0.5f, 0f), new Vector2(canvasBounds.xMax - xMax, canvasBounds.height));
    }

    private bool TryGetFocusRect(out Rect focusRect)
    {
        Camera canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        if (focusedTransform is RectTransform focusRectTransform)
        {
            Vector3[] corners = new Vector3[4];
            focusRectTransform.GetWorldCorners(corners);
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            foreach (Vector3 corner in corners)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvasCamera, corner);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, canvasCamera, out Vector2 localPoint);
                min = Vector2.Min(min, localPoint);
                max = Vector2.Max(max, localPoint);
            }

            focusRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            return true;
        }

        Camera worldCamera = Camera.main;
        if (worldCamera == null)
        {
            focusRect = default;
            return false;
        }

        Vector2 screenPosition = worldCamera.WorldToScreenPoint(focusedTransform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, canvasCamera, out Vector2 localPosition);
        focusRect = new Rect(localPosition - Vector2.one * 110f, Vector2.one * 220f);
        return true;
    }

    private static void SetDimBlock(RectTransform block, Vector2 position, Vector2 size)
    {
        block.gameObject.SetActive(size.x > 0f && size.y > 0f);
        block.anchoredPosition = position;
        block.sizeDelta = size;
    }

    private void ShowMessage(TutorialStep step)
    {
        bool isKorean = CurrentLanguage == GameLanguage.Korean;
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

        bodyText.fontSizeMin = step == TutorialStep.EnemyIntroduction ? 18f : 26f;
        bodyText.fontSizeMax = step == TutorialStep.EnemyIntroduction ? 38f : 46f;
        if (step == TutorialStep.EnemyIntroduction)
        {
            titleText.SetText(isKorean ? "튜토리얼" : "TUTORIAL");
            bodyText.SetText(isKorean
                ? "적은 세 종류입니다.\n일반 적은 플레이어를 향해 다가옵니다.\n빨간 펄스 적은 가까이에서 파동 공격, 초록 회복 적은 도착 시 체력을 회복합니다."
                : "There are three enemy types.\nNormal enemies approach you. Red Pulse enemies attack nearby; green Healer enemies restore health when they arrive.");
            progressText.SetText("3 / 7");
            return;
        }

        if (step == TutorialStep.EnemyApproach)
        {
            titleText.SetText(isKorean ? "튜토리얼" : "TUTORIAL");
            bodyText.SetText(isKorean ? "손가락을 움직여 적에게 다가가 처치해보세요" : "Move your finger to the enemy and defeat it.");
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
        progressText.SetText(stepNumber == 0 ? string.Empty : $"{stepNumber} / 7");
    }

private void BuildUserInterface()
    {
        font = GetComponentInChildren<TextMeshProUGUI>(true)?.font ?? TMP_Settings.defaultFontAsset;
        if (font == null)
        {
            Debug.LogError("FirstRunTutorial could not find a TMP font asset.", this);
            return;
        }

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
        messagePanel.SetActive(false);
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
        CoinPickup,
        Completed
    }
}
