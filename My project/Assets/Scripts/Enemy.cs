using UnityEngine;

public class Enemy : MonoBehaviour
{
    private const float HitDistance = 1f;

    public float speed;

    public Vector2 targetPos;
    public float maxScore;

    public float[] judgeDistance;
    public Color[] judgeColor;

    private Player player;
    private float[] baseJudgeDistance;

    private void Awake()
    {
        baseJudgeDistance = (float[])judgeDistance.Clone();
    }

    private void Start()
    {
        player = Player.Instance;
        RefreshJudgeDistance();
    }

    public void RefreshJudgeDistance()
    {
        if (player == null)
        {
            player = Player.Instance;
        }

        if (player == null || baseJudgeDistance == null)
        {
            return;
        }

        float upgradeValue = player.GetUpgradeValue(UpgradeType.Judgement);
        for (int index = 0; index < judgeDistance.Length; index++)
        {
            judgeDistance[index] = baseJudgeDistance[index] + upgradeValue;
        }
    }
    private void FixedUpdate()
    {
        if (!player.gameStarted)
        {
            return;
        }

        transform.position += transform.right * Time.fixedDeltaTime * speed;
        float distance = Vector2.Distance(targetPos, transform.position);
        if (distance <= HitDistance)
        {
            SpecialEnemy specialEnemy = GetComponent<SpecialEnemy>();
            if (specialEnemy != null && specialEnemy.TryHandleHeartArrival())
            {
                return;
            }

            gameObject.SetActive(false);
            player.Damage();
        }
    }

    public long Calculate(out Color judgementColor, out bool isPerfect)
    {
        judgementColor = Color.white;
        isPerfect = false;
        float distance = Vector2.Distance(targetPos, transform.position);
        long currentScore = 0L;
        for (int i = 0; i < judgeDistance.Length; i++)
        {
            if (distance <= judgeDistance[i])
            {
                currentScore = (long)System.Math.Round(
                    (judgeDistance.Length - i) * (double)maxScore /
                    judgeDistance.Length,
                    System.MidpointRounding.AwayFromZero);
                judgementColor = judgeColor[i];
                isPerfect = i == 0;
                return currentScore;
            }
        }
        return 0L;
    }

    public bool TryGetJudgementColor(float distance, out Color judgementColor)
    {
        judgementColor = Color.white;

        for (int index = 0; index < judgeDistance.Length; index++)
        {
            if (distance <= judgeDistance[index])
            {
                judgementColor = judgeColor[index];
                return true;
            }
        }

        return false;
    }

    public void SetTravelDuration(Vector2 targetPosition, float travelDuration)
    {
        targetPos = targetPosition;
        float travelDistance = Mathf.Max(0f, Vector2.Distance(transform.position, targetPos) - HitDistance);
        speed = travelDistance / Mathf.Max(0.01f, travelDuration);
    }
}
