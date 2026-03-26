using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : InputAxis
{
    public static Player Instance;
    public int hp;
    public bool invincible = false;

    public float score;

    public float stamina = 100;
    public float stunTime = 1.5f;

    private bool isStun = false;
    private float currentTime;
    private UiManager uiManager;

    public void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        uiManager = UiManager.instance;
        for (int i = 0; i < hp; i++)
        {
            GameObject obj = Instantiate(uiManager.hartObj, transform.position, Quaternion.identity, uiManager.hartUI);
            uiManager.harts.Add(obj);
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (!isStun)
        {
            if (!isPc) transform.position = touch.position;
            else transform.position = mousePosition;
            stamina -= MathF.Abs(distanseValue.x + distanseValue.y) / 2;
            if(stamina <= 0)
            {
                currentTime = 0;
                isStun = true;
                stamina = 0;
            }
        }
        else
        {
            if(!gameStarted) return;
            currentTime += Time.deltaTime;
            stamina =  100 * currentTime / stunTime;
            if(currentTime >= stunTime)
            {
                isStun = false;
                currentTime = 0;
                stamina = 100;
            }
        }
    }

    public void Damage()
    {
        hp--;
        uiManager.harts[hp].SetActive(false);
        if(hp <= 0)
        {
            gameStarted = false;
            uiManager.bestScore = Mathf.Max(uiManager.bestScore, score);
            uiManager.bestScoreTxt.SetText($"{(int)uiManager.bestScore}");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            SpriteRenderer judge = SpawnPivot.Instance.FindJudge();
            judge.gameObject.transform.position = collision.gameObject.transform.position;
            score += collision.gameObject.GetComponent<Enemy>().Caculate(judge);
            uiManager.currentScoreText.SetText($"{(int)score}");
            if (score > 0) stamina = Math.Min(100,stamina+30);
            collision.gameObject.SetActive(false);
        }
    }

    void TurnOffInvincible() => invincible = false;
}
