using System;
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
            if (!isPc)
            {
                transform.position = touch.position;
            }
            else
            {
                transform.position = mousePosition;
            }
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            score += collision.gameObject.GetComponent<Enemy>().Caculate();
            if(score > 0) stamina = Math.Min(100,stamina+30);
            collision.gameObject.SetActive(false);
        }
    }

    void TurnOffInvincible() => invincible = false;
}
