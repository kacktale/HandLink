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

    public TextMeshProUGUI ScoreUI;

    public GameObject[] hpUI;

    private bool isStun = false;
    private float currentTime;

    public void Awake()
    {
        Instance = this;
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
        hpUI[hp].SetActive(false);
        if(hp <= 0)
        {
            gameStarted = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            SpriteRenderer judge = SpawnPivot.Instance.FindJudge();
            judge.gameObject.transform.position = collision.gameObject.transform.position;
            score += collision.gameObject.GetComponent<Enemy>().Caculate(judge);
            ScoreUI.SetText($"{(int)score}");
            if (score > 0) stamina = Math.Min(100,stamina+30);
            collision.gameObject.SetActive(false);
        }
    }

    void TurnOffInvincible() => invincible = false;
}
