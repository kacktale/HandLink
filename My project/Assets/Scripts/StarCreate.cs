using UnityEngine;
using UnityEngine.Serialization;

public class StarCreate : InputAxis
{
    public GameObject stars;
    [FormerlySerializedAs("starParant")]
    public Transform starParent;
    public Vector3 minSpawnPos;
    public Vector3 maxSpawnPos;
    public float starMoveSpeed;
    private void Start()
    {
        for (int i = 0; i < 40; i++)
        {
            GameObject star = Instantiate(stars, transform.position, Quaternion.identity, starParent);
            float x = Random.Range(minSpawnPos.x, maxSpawnPos.x);
            float y = Random.Range(minSpawnPos.y, maxSpawnPos.y);
            star.transform.localPosition = new Vector3(x, y, 0f);
        }
    }

    public override void Update()
    {
        if (Player.Instance == null || !Player.Instance.gameStarted)
        {
            return;
        }

        base.Update();
        if (!gameStarted)
        {
            return;
        }

        Vector3 downwardMovement = Vector3.up * (Time.deltaTime * starMoveSpeed);
        Vector3 pointerMovement = distanceValue * 0.02f;
        transform.position -= pointerMovement + downwardMovement;
        if (transform.position.y <= -15f)
        {
            transform.position = new Vector3(0f, 15f, 0f);
        }
    }
}
