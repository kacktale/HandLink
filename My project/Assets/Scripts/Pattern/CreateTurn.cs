using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateTurn : MonoBehaviour
{
    public GameObject preWarnObj;
    public GameObject lineBullet;
    public GameObject Bullet;

    public Transform preWarnParant;
    public Transform lineBulletParant;
    public Transform BulletParant;

    private List<GameObject> preWarnList = new List<GameObject>();
    private List<GameObject> lineList = new List<GameObject>();
    private List<GameObject> BulletList = new List<GameObject>();
    private List<GameObject> warnPos = new List<GameObject>();

    public Vector2 preWarnX;
    public Vector2 preWarnY;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateDummyBullet();
        StartCoroutine(DownLeftLine());
    }

    void CreateDummyBullet()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject dummy = Instantiate(preWarnObj, transform.position, Quaternion.identity, preWarnParant);
            dummy.SetActive(false);
            preWarnList.Add(dummy);
        }
        for (int i = 0; i < 20; i++)
        {
            GameObject dummy = Instantiate(lineBullet, transform.position, Quaternion.identity, lineBulletParant);
            dummy.SetActive(false);
            lineList.Add(dummy);
        }
        for (int i = 0; i < 20; i++)
        {
            GameObject dummy = Instantiate(Bullet, transform.position, Quaternion.identity, BulletParant);
            dummy.SetActive(false);
            BulletList.Add(dummy);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    GameObject GetObjectPool(int type)
    {
        GameObject pool = null;
        switch (type)
        {
            case 0:
                for (int i = 0; i < preWarnList.Count; i++)
                {
                    if (!preWarnList[i].activeInHierarchy)
                    {
                        pool = preWarnList[i];
                        pool.SetActive(true);
                        return pool;
                    }
                }
                pool = Instantiate(preWarnObj, transform.position, Quaternion.identity, preWarnParant);
                break;
            case 1:
                for (int i = 0; i < lineList.Count; i++)
                {
                    if (!lineList[i].activeInHierarchy)
                    {
                        pool = lineList[i];
                        pool.SetActive(true);
                        return pool;
                    }
                }
                pool = Instantiate(lineBullet, transform.position, Quaternion.identity, lineBulletParant);
                break;
            case 2:
                for (int i = 0; i < BulletList.Count; i++)
                {
                    if (!BulletList[i].activeInHierarchy)
                    {
                        pool = BulletList[i];
                        pool.SetActive(true);
                        return pool;
                    }
                }
                pool = Instantiate(Bullet, transform.position, Quaternion.identity, BulletParant);
                break;
        }
        return pool;
    }

    IEnumerator DownLeftLine()
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject preWarnx = GetObjectPool(0);
            preWarnx.transform.rotation = Quaternion.Euler(0, 0, 90);
            preWarnx.transform.position = preWarnX;
            GameObject preWarny = GetObjectPool(0);
            preWarny.transform.position = preWarnY;

            warnPos.Add(preWarnx);
            warnPos.Add(preWarny);
            preWarnX -= new Vector2(0.5f, 0);
            preWarnY += new Vector2(0, 0.5f);
            yield return new WaitForSeconds(0.2f);
        }
        yield return new WaitForSeconds(0.5f);
        for (int i = 0;i<warnPos.Count;i+=2)
        {
            GameObject preWarnx = GetObjectPool(1);
            preWarnx.transform.rotation = Quaternion.Euler(0, 0, 90);
            preWarnx.transform.position = warnPos[i].transform.position;
            GameObject preWarny = GetObjectPool(1);
            preWarny.transform.position = warnPos[i+1].transform.position;

            warnPos[i].SetActive(false);
            warnPos[i+1].SetActive(false);
        }
        warnPos = new List<GameObject>();
    }
}
