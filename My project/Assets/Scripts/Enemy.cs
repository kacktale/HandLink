using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;

    public Vector2 targetPos;
    public float maxScore;

    public float[] judgeDistance = new float[6];

    private Player player;

    private void Start()
    {
        player = Player.Instance;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if(!player.gameStarted) return;
        transform.position += transform.right * Time.deltaTime * speed;
    }

    public float Caculate()
    {
        float distance = Vector2.Distance(targetPos, transform.position);
        float currentScore = 0;
        for (int i = 0; i < judgeDistance.Length; i++)
        {
            if (distance <= judgeDistance[i])
            {
                currentScore = (judgeDistance.Length - i) * maxScore / judgeDistance.Length;
                return currentScore;
            }
        }
        return currentScore;
    }
}
