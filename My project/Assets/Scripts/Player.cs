using UnityEngine;
using UnityEngine.InputSystem;

public class Player : InputAxis
{
    public Player Instance;
    public int hp;
    public bool invincible = false;

    public void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (!isPc) transform.position = touch.position;
        else transform.position = mousePosition;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet") || collision.gameObject.CompareTag("DontDestroyBullet"))
        {
            if (invincible) return;
            hp--;

            if(collision.gameObject.CompareTag("Bullet")) Destroy(collision.gameObject);
            invincible = true;
            Invoke("TurnOffInvincible", 0.8f);
        }
    }

    void TurnOffInvincible() => invincible = false;
}
