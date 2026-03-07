using NUnit.Framework;
using System.Collections.Generic;
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

    private float currentTime;

    private List<List<GameObject>> enemyList = new List<List<GameObject>>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.Rotate(new Vector3(0,0,1)); //Rotate Pivot

        currentTime += Time.deltaTime;

        if (currentTime >= spawnDelay)
        {
            int summonPos = Random.Range(0, enemySummonPos.Length);
            int enemyType = Random.Range(0,100);

        }
    }
}
