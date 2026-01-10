using UnityEngine;

public class StarCreate : InputAxis
{
    public GameObject stars;
    public Transform starParant;
    public Vector3 minSpawnPos;
    public Vector3 maxSpawnPos;
    public float starMoveSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i< 40; i++)
        {
            GameObject star = Instantiate(stars,transform.position,Quaternion.identity,starParant);
            float x = Random.Range(minSpawnPos.x,maxSpawnPos.x);
            float y = Random.Range(minSpawnPos.y, maxSpawnPos.y);
            star.transform.localPosition = new Vector3(x,y,0);
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (!gameStarted) return;
        Vector3 downpos = new Vector3(0,1,0) * Time.deltaTime * starMoveSpeed;
        Vector3 pos = distanseValue * 0.02f;
        transform.position -= pos + downpos;
        if(transform.position.y <= -15f)
        {
            transform.position = new Vector3(0,15f,0);
        }
    }
}
