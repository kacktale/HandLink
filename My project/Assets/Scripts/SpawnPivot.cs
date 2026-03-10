using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*ToDo : 
 * Rotate
 * Enemy Spawn
 * Enemy LookAt Target
 * SpawnDelay
 */
public class SpawnPivot : MonoBehaviour
{
    public static SpawnPivot Instance { get; private set; }

    [SerializeField] private Transform[] enemySummonPos;
    [SerializeField] private GameObject[] enemys;
    [SerializeField] private GameObject judgeObj;
    [SerializeField] private GameObject target;
    [SerializeField] private float spawnDelay;
    [SerializeField] private Transform poolParant;

    private Player player;
    private float currentTime;

    private List<List<GameObject>> enemyList = new List<List<GameObject>>();
    private List<SpriteRenderer> judgeObjs = new List<SpriteRenderer>();

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        player = Player.Instance;
        for (int i = 0; i < enemys.Length; i++)
        {
            enemyList.Add(new List<GameObject>());
        }
        SummonDummyEnemy();
    }

    void SummonDummyEnemy()
    {
        for (int i = 0; i < enemys.Length; i++)
        {
            for(int j = 0; j < 80; j++)
            {
                GameObject enemy = Instantiate(enemys[i],transform.position,Quaternion.identity, poolParant);
                enemyList[i].Add(enemy);
                enemy.SetActive(false);
            }
        }
        for (int i = 0;i < 80; i++)
        {
            SpriteRenderer judge = Instantiate(judgeObj, transform.position, Quaternion.identity, poolParant).GetComponent<SpriteRenderer>();
            judgeObjs.Add(judge);
            judge.gameObject.SetActive(false);
        }
    }

    public GameObject FindObj(int tag)
    {
        GameObject enemy;
        for (int i = 0; i < enemyList[tag].Count; i++)
        {
            if (!enemyList[tag][i].activeInHierarchy)
            {
                enemy = enemyList[tag][i];
                enemy.SetActive(true);
                return enemy;
            }
        }
        enemy = Instantiate(enemys[tag], transform.position, Quaternion.identity);
        enemyList[tag].Add(enemy);
        return enemy;
    }

    public SpriteRenderer FindJudge()
    {
        SpriteRenderer judge;
        for (int i = 0; i < judgeObjs.Count; i++)
        {
            if (!judgeObjs[i].gameObject.activeInHierarchy)
            {
                judge = judgeObjs[i];
                judge.gameObject.SetActive(true);
                return judge;
            }
        }
        judge = Instantiate(judgeObj, transform.position, Quaternion.identity, poolParant).GetComponent<SpriteRenderer>();
        judgeObjs.Add(judge);
        return judge;
    }

    // Update is called once per frame
    void Update()
    {
        if (!player.gameStarted) return;
        gameObject.transform.Rotate(new Vector3(0,0,1)); //Rotate Pivot

        currentTime += Time.deltaTime;

        if (currentTime >= spawnDelay)
        {
            int summonPos = Random.Range(0, enemySummonPos.Length);
            int enemyType = Random.Range(0, enemys.Length);

            GameObject enemy = FindObj(enemyType);
            enemy.transform.position = enemySummonPos[summonPos].position;

            Vector2 newPos = target.transform.position - enemy.transform.position;
            float rotZ = Mathf.Atan2(newPos.y, newPos.x) * Mathf.Rad2Deg;
            enemy.transform.rotation = Quaternion.Euler(0, 0, rotZ);

            currentTime = 0;
        }
    }
}
