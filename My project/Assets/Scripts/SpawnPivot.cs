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
    [SerializeField] private Transform[] enemySummonPos;
    [SerializeField] private GameObject[] enemys;
    [SerializeField] private GameObject target;
    [SerializeField] private float spawnDelay;

    private Player player;
    private float currentTime;

    private List<List<GameObject>> enemyList = new List<List<GameObject>>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
                GameObject enemy = Instantiate(enemys[i],transform.position,Quaternion.identity);
                enemyList[i].Add(enemy);
                enemy.SetActive(false);
            }
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
